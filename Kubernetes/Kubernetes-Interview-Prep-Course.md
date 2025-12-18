# Cours Complet de Kubernetes — Préparation Entretien Technique

Version: 1.0

Remarque: Ce cours est conçu pour une préparation intensive aux entretiens techniques Kubernetes (junior → confirmé). Il couvre concepts, commandes essentielles, scénarios pratiques, exercices et questions d'entretien avec réponses.

Assumptions:
- Vous connaissez déjà Docker et les concepts de base des conteneurs.
- Environnement recommandé: minikube / kind / k3s pour labs locaux.
- Kubectl installé (version compatible Kubernetes 1.20+), accès à un cluster de test.

Plan du cours
- Introduction & Glossaire
- Architecture et composants du control plane
- Pods, ReplicaSets, Deployments, StatefulSets, DaemonSets
- Services, Networking, Ingress, CNI
- Volumes, PersistentVolume, PersistentVolumeClaim, StorageClass
- ConfigMaps, Secrets, variables d'environnement
- Scheduling: Nodes, taints/tolerations, affinities
- Observabilité: logs, metrics, liveness/readiness/startup probes
- Sécurité: RBAC, NetworkPolicy, PodSecurity, PSP/PSA
- CI/CD, Helm, Kustomize, GitOps (ArgoCD/Flux)
- Debug & Troubleshooting — commandes et scénarios
- Questions d'entretien + réponses
- Labs pratiques, exercices et solutions
- Cheat-sheet `kubectl` et plan d'étude

=================================================================

**Introduction & Glossaire**

- Pod: plus petite unité déployable; peut contenir 1+ containers partageant réseau et volumes.
- Node: machine (VM/physique) exécutant kubelet et pods.
- Control Plane: API Server, etcd, Controller Manager, Scheduler.

Note rapide sur le Control Plane:
- Le control plane orchestre l'état désiré du cluster : l'API Server expose l'API REST, `etcd` stocke l'état persistant, le Scheduler assigne les pods aux nodes et le Controller Manager corrige les dérives. En production, le control plane doit être hautement disponible (multi-master) ; sécurisez l'accès à l'API (TLS, RBAC), limitez les webhooks non fiables et effectuez des backups réguliers d'`etcd`.
- Kubelet: agent sur chaque Node.
- kube-proxy: gère routage réseau et Services (mode iptables/ipvs).
- Deployment: contrôle ReplicaSets, gère rolling updates/rollbacks.
- StatefulSet: pour workloads avec identité stable (ordre, stable network id, PVs).
- DaemonSet: déploie pod sur chaque node (ex: logging, monitoring).
- Service: abstraction réseau exposant un ensemble de pods (ClusterIP, NodePort, LoadBalancer).
- Ingress: routes HTTP(S) externes vers Services.

=================================================================

**Module 1 — Architecture & composants**

Objectifs: comprendre rôle de chaque composant et flux de commande.

1. API Server (kube-apiserver)
   - Point d'entrée REST. Authentification/authorization, admission controllers.
2. etcd
   - Base clé-valeur distribué stockant l'état du cluster.
3. Controller Manager
   - Boucles de contrôle pour controllers (deployment, node, endpoints...).
4. Scheduler
   - Assigne pods aux nodes selon contraintes et ressources.
5. kubelet
   - Communique avec API server, gestion des containers via CRI (containerd, docker shim historically).
6. kube-proxy
   - Implémente services réseau sur nodes.

Points d'entretien typiques:
- Comment fonctionne une requête `kubectl apply` jusqu'au pod créé ?
    - Réponse: `kubectl` envoie une requête au `kube-apiserver` (authn/authz). L'API Server applique les admission controllers (mutating/validating) qui peuvent modifier/valider l'objet, puis écrit l'objet dans `etcd`. Le Scheduler sélectionne un node pour les pods non assignés et crée un binding; le `kubelet` du node récupère le PodSpec, pull les images via le runtime (containerd), crée et démarre les containers; enfin le `kubelet` met à jour le status et les controllers (Deployment, ReplicaSet) maintiennent l'état désiré.
- Rôle des admission controllers (ex: MutatingAdmissionWebhook).
   - Réponse: Les admission controllers interceptent les requêtes après authentification/autorisation mais avant persistance; `MutatingAdmissionWebhook` peut modifier les objets (ex: injecter sidecars), `ValidatingAdmissionWebhook` valide et peut rejeter; ils sont utilisés pour policy-as-code, sécurité, et automatisation.

Commandes utiles:
```
kubectl get componentstatuses
kubectl get nodes -o wide
kubectl describe node <node>
```

Question typique:
- Q: Quel est le rôle de l'API Server dans le control plane ?
    R: L'API Server est le point d'entrée REST du cluster; il valide/authentifie/autorise les requêtes, passe par les admission controllers et écrit l'état dans `etcd`.

=================================================================

**Module 2 — Pods, Deployments, ReplicaSets, StatefulSets, DaemonSets**

Concepts clés:
- Pod lifecycle, restartPolicy, initContainers, ephemeral containers.
- Deployment: spec.replicas, strategy (RollingUpdate/OnDelete), revisionHistoryLimit
- Scaling: `kubectl scale deployment/<name> --replicas=N` ou HPA

Exemples rapides:
```
kubectl run nginx --image=nginx --restart=Always --port=80
kubectl create deployment nginx --image=nginx
kubectl set image deployment/nginx nginx=nginx:1.19
kubectl rollout status deployment/nginx
kubectl rollout undo deployment/nginx
```

StatefulSet usage: bases de données, stocker PV par pod ordinal.
DaemonSet usage: node-exporter, fluentd.

Entretien: différences entre Deployment et StatefulSet; comment faire un rolling update sans downtime.
    - Réponse: Deployment gère pods interchangeables via ReplicaSets, adapté aux workloads stateless; StatefulSet fournit identité stable (nom ordinal), PVs persistants par pod et ordering guarantees, adapté aux bases de données. Pour rolling update sans downtime: utiliser `spec.strategy.rollingUpdate` (maxUnavailable/ maxSurge), readiness probes pour retirer/ajouter les pods du service uniquement quand prêts, utiliser PodDisruptionBudget pour garantir nombre min de pods disponibles, et tester canary/blue-green si besoin.

Question typique:
- Q: Quelle est la différence fonctionnelle entre un ReplicaSet géré directement et un ReplicaSet géré par un Deployment ?
    R: Un ReplicaSet seul garantit un nombre de réplicas mais n'offre pas d'historique ni de stratégie de mise à jour; un Deployment gère les ReplicaSets, fournit rolling updates, rollbacks et gestion des révisions.

=================================================================

**Module 3 — Networking, Services & Ingress**

Principes:
- Chaque pod reçoit une IP unique (pod network). Toutes pods peuvent communiquer par défaut.
- Service types: ClusterIP (par défaut), NodePort, LoadBalancer, ExternalName.
- DNS: CoreDNS gère résolution interne (services, pods).
- Ingress: layer 7 routing; implémenté par controllers (nginx-ingress, contour, traefik).
- CNI: plugin réseau (Calico, Flannel, Cilium, Weave). Certains fournissent NetworkPolicy.

Commandes:
```
kubectl get svc
kubectl describe svc mysvc
kubectl get endpoints
kubectl get ingress
```

Entretien: expliquer Service ClusterIP vs NodePort vs LoadBalancer; comment diagnostiquer un problème DNS entre pods.
    - Réponse: `ClusterIP` expose le service uniquement en interne; `NodePort` ouvre un port sur chaque node; `LoadBalancer` demande un LB externe chez le cloud provider. Pour diagnostiquer DNS: vérifier `CoreDNS` pods et logs, `kubectl get svc` et `endpoints` pour voir IPs, `kubectl exec` dans un pod et `nslookup`/`dig` vers le service, vérifier NetworkPolicy qui pourrait bloquer le traffic, et examiner `/etc/resolv.conf` du pod.

Question typique:
- Q: Qu'est-ce qu'un Service headless et quand l'utiliser ?
    R: Un Service headless (`clusterIP: None`) ne crée pas une IP de service; il expose directement les endpoints (pods) via DNS SRV/A pour discovery, utile pour StatefulSets ou services nécessitant accès direct aux pods.

=================================================================

**Module 4 — Stockage**

Concepts:
- Volume vs PersistentVolume (PV) vs PersistentVolumeClaim (PVC).
- StorageClass: provisioner dynamique (ex: aws-ebs, gce-pd, rook-ceph).
- AccessModes: ReadWriteOnce, ReadOnlyMany, ReadWriteMany.

Commandes:
```
kubectl get pv,pvc,sc
kubectl describe pvc myclaim
```

Scénario: provisionner un PVC dynamique, attacher à un Pod, sauvegarder/restore.

Question typique:
- Q: Quelle est la différence entre un PersistentVolume (PV) et un PersistentVolumeClaim (PVC) ?
    R: Un PV est une ressource fournie par l'administrateur (ou provisionnée dynamiquement) représentant du stockage; un PVC est une requête utilisateur pour du stockage qui demande un PV correspondant selon taille et accessModes.

=================================================================

**Module 5 — ConfigMaps & Secrets**

- ConfigMaps: données non sensibles, montées en volume ou injectées en env vars.
- Secrets: base64-encoded; méthode recommandée: external secrets manager ou KMS-backed encryption at rest.

Commandes:
```
kubectl create configmap mycfg --from-literal=key=value
kubectl create secret generic mysecret --from-literal=password=abc
kubectl get secret mysecret -o yaml
kubectl describe secret mysecret
```

Entretien: bonnes pratiques pour gérer secrets (ne pas les committer, utiliser External Secrets/SealedSecrets).
    - Réponse: Ne pas committer secrets dans Git; chiffrer etcd (encryption at rest), restreindre l'accès via RBAC, utiliser ExternalSecrets/SealedSecrets ou Vault pour injecter secrets au runtime, utiliser ServiceAccounts avec permissions minimales, et auditer/rotater les secrets régulièrement.

Question typique:
- Q: Quand faut-il préférer un Secret à un ConfigMap ?
    R: Utilisez un Secret pour données sensibles (mots de passe, tokens) qui nécessitent protection; ConfigMap pour configuration non sensible.

=================================================================

**Module 6 — Scheduling, taints/tolerations, affinity**

- Taints et tolerations pour repousser pods sur nodes spécifiques.
- Node affinity & Pod affinity/anti-affinity pour placement avancé.
- Resource requests & limits -> influence sur scheduling et QoS (Guaranteed/Burstable/BestEffort).

Commandes:
```
kubectl describe node <node>  # voir taints, capacity, allocatable
kubectl top node
```

Entretien: expliquer difference requests vs limits, comment éviter OOMKills, comment forcer un pod sur node spécifique.
    - Réponse: `requests` indiquent les ressources garanties au scheduler; `limits` définissent le plafond d'utilisation. Si un container dépasse sa limite mémoire, il peut être OOMKilled; pour éviter: définir requests/logical limits raisonnables, surveiller metrics (Prometheus), ajouter liveness/readiness probes, et utiliser QoS (Guaranteed si requests==limits). Pour forcer placement: utiliser `nodeSelector`/`nodeAffinity` ou `nodeName` (liage direct) et gérer `taints`/`tolerations` pour isoler nodes.

Question typique:
- Q: Que fait un taint `NoSchedule` sur un node ?
    R: `NoSchedule` empêche le scheduler de placer des pods qui ne tolèrent pas ce taint ; seuls les pods avec la toleration correspondante pourront être programmés sur ce node.

=================================================================

**Module 7 — Observabilité, logs & probes**


Commandes:
```
kubectl logs pod/my-pod
kubectl top pod
kubectl port-forward svc/prometheus 9090:9090
```

Entretien: comment diagnostiquer un pod stuck in CrashLoopBackOff; quelles métriques surveiller.
    - Réponse: Vérifier `kubectl describe pod` pour events, `kubectl logs --previous` pour la sortie du container crashé, vérifier probes (liveness/readiness) et l'entrypoint/command. Surveiller métriques: restart count, memory usage (OOM), CPU throttling, I/O et disk space; corriger en ajustant probes, resources ou en corrigeant le code/entrypoint.
    
Question typique:
- Q: Quelle est la différence entre liveness et readiness probe ?
    R: Liveness redémarre le container si celui-ci est en mauvais état; readiness indique si le pod doit recevoir du trafic et influence les endpoints d'un Service.

=================================================================

**Module 8 — Sécurité: RBAC, NetworkPolicy, PodSecurity**

- AuthN/AuthZ: certificates, tokens, OIDC.
- RBAC: Role, ClusterRole, RoleBinding, ClusterRoleBinding.
- NetworkPolicy: contrôler flux réseau entre pods.
- Pod Security Admission (PSA) / PodSecurityPolicy (deprecated) pour contrainte runtime.

Exemples:
```
kubectl create role pod-reader --verb=get,list --resource=pods
kubectl create rolebinding read-pods --role=pod-reader --user=alice
```

Entretien: comment limiter accès à secrets via RBAC; comment restreindre accès réseau entre namespaces.
    - Réponse: Créer `Role`/`ClusterRole` avec permissions minimales (`get`, `list`, `watch` sur `secrets`) et binder uniquement aux `ServiceAccount` nécessaires via `RoleBinding`. Pour restreindre réseau: appliquer `NetworkPolicy` deny-by-default et autoriser explicitement traffic entre selectors/namespaces; utiliser CNI supportant NetworkPolicy (Calico/Cilium).

Question typique:
- Q: Quelle commande permet d'appliquer un RoleBinding pour un ServiceAccount ?
    R: `kubectl create rolebinding read-pods --role=pod-reader --serviceaccount=my-namespace:my-sa`.

=================================================================

**Module 9 — CI/CD, Helm, Kustomize, GitOps**

- Helm: package manager; charts, values.yaml, template rendering.
- Kustomize: overlay management (native in `kubectl apply -k`).
- GitOps: déclaratif via repo Git + controller (ArgoCD/Flux) pour déployer automatiquement.
- Best practices: immutabilité des images, image tags (use digests), rollback strategy.

Commands:
```
helm repo add stable https://... 
helm install myapp ./chart -f values.yaml
kubectl apply -k ./overlays/staging
```

Entretien: comparatif Helm vs Kustomize; comment faire rollbacks automatisés avec ArgoCD.
Réponse: Helm est un package manager utilisant templates et valeurs (`values.yaml`) avec notion de releases; Kustomize fait des overlays sur manifests YAML sans templating, favorisant la composition. Pour rollbacks automatisés avec ArgoCD: ArgoCD conserve l'historique des déploiements et permet rollback manuels via UI/CLI; pour automatiser rollback sur échec, combiner ArgoCD avec Argo Rollouts (canary/blue-green) ou automatiser policies that detect unhealthy and trigger rollback via automated hooks.

Question typique:
- Q: Que fait `helm upgrade --install` ?
    R: Tente de mettre à jour une release existante; si la release n'existe pas, il la crée (`install`).

=================================================================

**Module 10 — Troubleshooting & Debugging**

Checklist de diagnostic:
1. `kubectl get events --sort-by=.metadata.creationTimestamp`
2. `kubectl describe pod <pod>` (conditions, events)
3. `kubectl logs` (containers), `kubectl logs --previous` for crashed containers
4. `kubectl exec -it <pod> -- /bin/sh` pour debug runtime
5. `kubectl port-forward` pour accéder services localement
6. Network checks: `kubectl exec` + curl/nslookup, `tcpdump` on node if possible

Scénarios communs et solutions:
- CrashLoopBackOff: check liveness, readiness, command, env vars, volumes, OOM
- ImagePullBackOff: check image name/tag, registry auth
- Pending pod: insufficient resources or PVC not bound or nodeSelector mismatch

Question typique:
- Q: Quelle commande permet d'afficher les événements récents dans un namespace triés par timestamp ?
    R: `kubectl get events -n <namespace> --sort-by=.metadata.creationTimestamp`.

=================================================================

**Module 11 — Questions d'entretien (sélection)**

Q1: Explique le cheminement d'une requête `kubectl apply -f pod.yaml`.
A1: `kubectl` -> kube-apiserver (authn/authz) -> admission controllers -> etcd persist -> scheduler assign -> kubelet pulls image -> container runtime crée container -> apiserver update status -> readiness/liveness probes.

Q2: Différence entre Deployment et StatefulSet ?
A2: Deployment gère pods identiques sans identité stable. StatefulSet garantit identité stable (nom DNS ordinal), PVs persistants par pod, ordering semantics.

Q3: Comment sécuriser l'accès aux secrets ?
A3: Restreindre RBAC, chiffrer secrets at rest (etcd encryption), utiliser external secrets manager, minimiser la surface d'accès, éviter env vars pour secrets si possible.

Q4: Comment diagnostiquer un pod `Pending` ?
A4: `kubectl describe pod` → events show Pending reason (Unschedulable -> check resources, taints, nodeSelector; PVC unbound; imagePull issues). Check nodes capacity and taints.

Q5: Explique NetworkPolicy et cas d'utilisation.
A5: NetworkPolicy limite flux réseau (ingress/egress) entre pods/namespaces. Utilisé pour segmentation, zero-trust.

Question typique:
- Q: Quelle est la commande pour vérifier les endpoints liés à un service ?
    R: `kubectl get endpoints <service-name>`.

=================================================================

**Module 12 — Labs pratiques & exercices (avec solutions)**

Lab 1 — Déployer une application web simple et exposer via Ingress
Steps:
1. `kubectl create deployment hello --image=gcr.io/google-samples/hello-app:1.0`
2. `kubectl expose deployment hello --port=8080 --target-port=8080 --type=ClusterIP`
3. Créer un Ingress en mappant `/` au service `hello` (exemple manifest ci-dessous)

Solution manifest (extrait):
```
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: hello-ingress
spec:
  rules:
  - http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: hello
            port:
              number: 8080
```

Lab 2 — Créer un StatefulSet pour une base de données avec PVCs
- Fournir manifest avec `volumeClaimTemplates` et vérifier PVs créés.

Lab 3 — Diagnostiquer un CrashLoopBackOff
- Vérifier `kubectl logs --previous`, `kubectl describe pod`, vérifier probes, env vars, image entrypoint.

Question typique:
- Q: Comment vérifier si un PVC a bien été lié à un PV ?
    R: `kubectl get pvc` pour voir `STATUS` (Bound) et `kubectl describe pvc <name>` pour détails, ou `kubectl get pv` pour voir quel PV est lié.

Plus d'exercices: scalabilité, HPA, canary deployments (using labels, two deployments + traffic shifting), networkpolicy deny-by-default.

=================================================================

**Module 13 — Cheat-sheet `kubectl` rapide**

- Lister ressources: `kubectl get pods,svc,deploy,rs,sts -A`
- Décrire: `kubectl describe <resource>/<name>`
- Logs: `kubectl logs -f pod -c container` ; logs previous `--previous`
- Exec: `kubectl exec -it pod -- /bin/sh`
- Port-forward: `kubectl port-forward svc/name 8080:80`
- Apply: `kubectl apply -f file.yaml` ; delete: `kubectl delete -f file.yaml`
- Rollout: `kubectl rollout status deployment/mydep` ; `kubectl rollout undo deployment/mydep`
- Scale: `kubectl scale deployment name --replicas=N`

Question typique:
- Q: Comment rollback un déploiement vers la révision précédente ?
    R: `kubectl rollout undo deployment/<name>`.

=================================================================

**Module 14 — Plan d'étude & ressources**

Plan (8 semaines suggéré pour préparation intensive):
- Sem 1: Concepts de base + architecture + pods/deployments
- Sem 2: Networking, services, ingress
- Sem 3: Storage, ConfigMaps, Secrets
- Sem 4: Scheduling, resources, QoS
- Sem 5: Observabilité + probes + metrics
- Sem 6: Sécurité RBAC & NetworkPolicy
- Sem 7: CI/CD, Helm, GitOps
- Sem 8: Labs, questions d'entretien, simulations orales

Ressources recommandées:
- Documentation officielle: https://kubernetes.io/docs
- Interactive labs: Katacoda, Killer.sh (CKA practice)
- Books: "Kubernetes Up & Running"
- Tools: minikube, kind, k3d, kubectl, kubeadm (pour installer cluster)

**Comparatif rapide: k8s / k3s / k3d**

- `k8s` (Kubernetes upstream): distribution officielle contenant tous les composants du control-plane (API Server, etcd, controller-manager, scheduler). Destiné aux environnements de production à grande échelle et aux clusters HA.
- `k3s`: distribution Kubernetes allégée par Rancher. Binaire packagé, faible empreinte mémoire, remplace etcd par sqlite par défaut (ou run etcd/klipper) et retire certains composants non essentiels — conçu pour edge, IoT, et environnements contraints.
- `k3d`: outil utilitaire qui lance des clusters `k3s` à l'intérieur de conteneurs Docker. Idéal pour tests locaux et CI, car il fournit des clusters rapides à créer/supprimer.

Quand les utiliser:
- `k8s` pour production et clusters nécessitant toutes fonctionnalités et résilience.
- `k3s` pour environnements ressources limitées, démonstrations, edge.
- `k3d` pour développement local et CI (simule un cluster k3s inside Docker).


=================================================================

**Questions d'entretien — Solutions complètes**

Ci-dessous les questions clés extraites de la section Q/A, avec réponses développées. Pour une liste plus complète et classée, voir `Kubernetes/Kubernetes-Questions.md`.

Q1: Explique le cheminement d'une requête `kubectl apply -f pod.yaml`.
A1: `kubectl` construit la requête et contacte le `kube-apiserver` (authn/authz). Si autorisée, les admission controllers (mutating/validating webhooks) peuvent modifier/valider l'objet; l'API server écrit l'objet dans `etcd`; le scheduler choisit un node pour les pods non assignés et crée un binding; le `kubelet` sur le node choisi récupère le manifest, charge/pull les images via le container runtime (containerd/runc), crée et démarre les containers, et met à jour le status via l'API. Les probes (readiness/liveness) et controllers complètent le cycle en s'assurant de l'état désiré.

Q2: Différence entre Deployment et StatefulSet ?
A2: `Deployment` gère ReplicaSets et s'adresse à des workloads stateless où les pods sont interchangeables; il gère rolling updates et rollbacks. `StatefulSet` fournit une identité stable (nom ordinal), un stockage persistant par pod via `volumeClaimTemplates`, et ordering guarantees (création/suppression), utile pour bases de données ou services nécessitant identité stable.

Q3: Comment sécuriser l'accès aux secrets ?
A3: Restreindre l'accès via RBAC (limiter quels serviceaccounts peuvent lire secrets), activer encryption at-rest pour etcd (KMS), utiliser solutions externes (Vault, ExternalSecrets), limiter exposition en évitant env vars non chiffrées, auditer accès et rotation de secrets.

Q4: Comment diagnostiquer un pod `Pending` ?
A4: `kubectl describe pod` montre events indiquant la raison (Unschedulable, FailedScheduling); vérifier `kubectl get nodes` pour capacity/taints, `kubectl get pvc` pour PVC non liées, et labels/nodeSelector/affinity; regarder `kubectl top nodes` pour ressources et logs du scheduler si nécessaire.

Q5: Explique NetworkPolicy et cas d'utilisation.
A5: `NetworkPolicy` déclare règles ingress/egress pour pods/namespace (selectors + ports + CIDRs). Il permet une segmentation réseau (zero-trust), ex: autoriser uniquement le trafic de l'API vers les pods backend, interdire trafic inter-namespace par défaut. Note: nécessite un CNI qui implémente NetworkPolicy (Calico, Cilium, etc.).

=================================================================

Annexe — Manifests utiles & snippets
- Rolling update strategy example for Deployment
- Service + Ingress example
- RBAC role/rolebinding example

=================================================================

