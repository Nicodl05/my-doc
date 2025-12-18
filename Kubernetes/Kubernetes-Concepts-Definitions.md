# Kubernetes — Définitions détaillées des concepts

But: fournir des définitions complètes et pratiques des principaux concepts Kubernetes pour préparation d'entretien.

Remarque: les commandes et extraits YAML fournis sont minimaux pour illustrer; adaptez selon votre cluster.

1) Pod
- Définition: unité atomique de déploiement dans Kubernetes. Un Pod contient un ou plusieurs containers qui partagent le même réseau (IP, port space) et les mêmes volumes.
- But: regrouper containers qui doivent cohabiter (ex: sidecar pattern).
- Comportement: cycle de vie géré par le contrôleur parent (Deployment, StatefulSet, DaemonSet, Job) ou directement par l'API.
- Champs clés: spec.containers[], spec.restartPolicy, spec.volumes, metadata.labels.
- Commandes: `kubectl get pod`, `kubectl describe pod`, `kubectl logs`.

2) Container
- Définition: processus isolé exécuté dans un Pod (ex: image Docker). Kubernetes ne gère pas le container directement mais via le container runtime (containerd, CRI-O).
- Points: chaque container a son propre filesystem mais partage le réseau et les volumes du Pod.

3) Node
- Définition: machine physique ou virtuelle qui exécute des Pods. Un Node exécute `kubelet`, `kube-proxy` et un runtime de conteneur.
- Rôle: héberger Pods, rapporter l'état au control plane, appliquer taints.
- Champs: capacity, allocatable, labels, taints.

4) Kubelet
- Définition: agent qui tourne sur chaque Node. Il surveille les Pods assignés, démarre/arrête containers, rapporte l'état au API Server.

5) ReplicaSet
- Définition: contrôleur qui garantit qu'un nombre donné de répliques d'un Pod (template) tourne en permanence.
- But: assurer haute disponibilité des Pods. Les ReplicaSets sont généralement gérés par un Deployment.
- Champs: spec.replicas, spec.selector, spec.template.
- Notes: un ReplicaSet seul gère uniquement le nombre de réplicas; il n'implémente pas de stratégie de déploiement (rolling update) — c'est le rôle du Deployment.

6) Deployment
- Définition: abstraction de niveau supérieur sur ReplicaSet; gère déclarativement l'état souhaité des Pods et supporte rolling updates/rollbacks.
- Champs clés: spec.replicas, spec.strategy (RollingUpdate/OnDelete), spec.template, revisionHistoryLimit.
- Comportement: crée/ met à jour ReplicaSets et gère la stratégie de mise à jour.

7) StatefulSet
- Définition: contrôleur pour workloads stateful; fournit identité réseau stable (nom ordinal), ordonnancement déterministe et support pour `volumeClaimTemplates`.
- Cas d'usage: bases de données, systèmes distribués nécessitant identité stable.

8) DaemonSet
- Définition: s'assure qu'une copie d'un Pod tourne sur chaque Node (ou subset via nodeSelector/taints). Utilisé pour agents de logging, monitoring.

9) Job & CronJob
- Job: exécute un ou plusieurs Pods jusqu'à complétion (batch). CronJob: scheduling récurrent d'un Job via cron expressions.

10) Volume
- Définition: abstraction de stockage persistante ou éphémère pour Pods. Types: emptyDir (ephemeral), hostPath, persistentVolumeClaim (PV dynamique), secret, configMap.

11) PersistentVolume (PV) & PersistentVolumeClaim (PVC)
- PV: ressource du cluster représentant une ressource de stockage provisionnée (statique ou dynamique via StorageClass).
- PVC: requête utilisateur pour du stockage (size, accessModes). Le control plane lie PVC→PV.
- StorageClass: décrit le provisioner (ex: aws-ebs, rook-ceph), paramètres dynamiques et reclaimPolicy.

12) ConfigMap
- Définition: objet API pour stocker des données de configuration non sensibles (paires clé/valeur, fichiers). Peut être monté en volume ou injecté en variables d'environnement.
- Bonnes pratiques: utiliser ConfigMap pour séparer config de l'image; ne pas stocker secrets.
- Exemple minimal:
```
apiVersion: v1
kind: ConfigMap
metadata:
  name: app-config
data:
  LOG_LEVEL: "info"
  APP_ENV: "staging"
```

13) Secret
- Définition: objet API pour stocker données sensibles (mots de passe, tokens). Les valeurs sont encodées en base64 dans l'API; chiffrement at-rest de etcd recommandé.
- Modes d'utilisation: monté en volume, injecté en env var, utilisé par imagePullSecrets.
- Exemples de bonnes pratiques: utiliser un secrets manager, restreindre accès via RBAC, activer encryptionConfig pour etcd.

14) Service
- Définition: abstraction réseau qui expose un ensemble de Pods (target endpoints) via une policy stable d'accès.
- Types de Service:
  - ClusterIP: accessible seulement dans le cluster (par défaut).
  - NodePort: expose un port statique sur chaque Node.
  - LoadBalancer: provisionne un LB externe (Cloud providers).
  - ExternalName: alias DNS vers un nom externe.
- Service selector: labels used to select endpoints; Headless Service (clusterIP: None) pour découverte DNS directe des pods.

15) Endpoints & EndpointSlice
- Endpoints: liste d'IP/ports correspondant aux Pods d'un Service. EndpointSlice: plus scalable, remplace progressivement Endpoints.

16) Ingress
- Définition: objet de routage L7 (HTTP/HTTPS). Nécessite un Ingress Controller (nginx, traefik, contour) qui implémente la ressource.
- Fonc.: fournit host/path routing, TLS termination, règles d'auth.

17) CNI (Container Network Interface)
- Définition: interface standard pour plugins réseau (Calico, Flannel, Cilium, Weave). Fournit le réseau entre Pods, implemente NetworkPolicy selon le plugin.

18) NetworkPolicy
- Définition: règle (namespace/podSelector) pour contrôler flux ingress/egress entre Pods. Fonctionne seulement si le CNI supporte NetworkPolicy.

19) DNS interne (CoreDNS)
- Définition: service DNS du cluster: résout services via `*.svc.cluster.local` et pods selon besoin.

20) Taints & Tolerations
- Taint (sur Node): marque un node pour repousser certains Pods (key=value:effect). Ex: `NoSchedule`, `PreferNoSchedule`, `NoExecute`.
- Toleration (sur Pod): permet à un Pod de tolérer un taint et d'être schedulé sur un node tainted.
- Usage: isoler nodes dédiés (GPU, stockage spécial) ou pour maintenance.

21) Node/Pod Affinity & Anti-affinity
- Node Affinity: contrainte de placement basée sur labels de Node (ex: zone, instance-type).
- Pod Affinity/Anti-affinity: contrainte pour placer pods proches/lointains d'autres pods (co-localisation ou dissémination).

22) Resource Requests & Limits, QoS
- Requests: ressources garanties au scheduler pour placer un Pod (CPU, memory).
- Limits: plafond d'utilisation. Si un container dépasse limit CPU il est throttled; dépasse memory -> OOMKill.
- QoS Classes:
  - Guaranteed: requests==limits pour tous les containers.
  - Burstable: requests < limits.
  - BestEffort: pas de requests ni de limits.

23) Probes: Liveness, Readiness, Startup
- Liveness: si échoue, kubelet redémarre le container (health check).
- Readiness: indique si le container reçoit du traffic Service; failing readiness retire le Pod des endpoints.
- Startup: utilisé pour apps lentes à démarrer; retarde les probes de readiness/liveness.

24) RBAC (Role-Based Access Control)
- Principes: Role/ClusterRole (ensemble de règles), RoleBinding/ClusterRoleBinding (liaison à users/serviceaccounts).
- Bonnes pratiques: principle of least privilege, utiliser ServiceAccounts pour pods.

25) ServiceAccount
- Définition: identité pour un Pod; lié à des tokens utilisés pour accéder à l'API.

26) Admission Controllers
- Définition: modules qui interceptent les requêtes API après authentification/authorization mais avant la persistance. Types: MutatingAdmissionWebhook, ValidatingAdmissionWebhook, built-in (NamespaceLifecycle, LimitRanger).

27) API Server
- Rôle: point d'entrée REST; valide, authentifie, autorise, passe par admission controllers, écrit dans etcd.

28) etcd
- Rôle: datastore clé-valeur distribuée, source de vérité pour l'état du cluster. Sauvegarde et chiffrement impératifs.

29) kube-proxy
- Rôle: implémente Service networking sur Node (iptables ou IPVS) et maintient règles pour diriger traffic vers endpoints.

30) Control Plane
- Composants: kube-apiserver, etcd, kube-scheduler, kube-controller-manager. Ils peuvent être HA ou single instance suivant l'install.

31) Helm & Kustomize (déclaratif)
- Helm: package manager pour Kubernetes; charts, templates, values.
- Kustomize: systeme d'overlays pour composer manifests YAML sans template engine.

32) GitOps
- Définition: pattern où le repo Git est la source de vérité et un controller (ArgoCD/Flux) synchronise l'état du cluster au repo.

33) Common Failure Modes & Indications
- CrashLoopBackOff: process exits; check logs, probes, OOM.
- ImagePullBackOff: problème auth ou image not found.
- Pending: scheduling impossible (ressources, taints, PVC non lié).

---

Exemples YAML rapides (Pod, Deployment, ReplicaSet, ConfigMap, Secret):

Pod minimal:
```
apiVersion: v1
kind: Pod
metadata:
  name: demo-pod
spec:
  containers:
  - name: nginx
    image: nginx:1.23
    ports:
    - containerPort: 80
```

Deployment minimal:
```
apiVersion: apps/v1
kind: Deployment
metadata:
  name: demo-deploy
spec:
  replicas: 3
  selector:
    matchLabels:
      app: demo
  template:
    metadata:
      labels:
        app: demo
    spec:
      containers:
      - name: nginx
        image: nginx:1.23
```

ReplicaSet minimal (généralement géré par Deployment):
```
apiVersion: apps/v1
kind: ReplicaSet
metadata:
  name: demo-rs
spec:
  replicas: 2
  selector:
    matchLabels:
      app: demo
  template:
    metadata:
      labels:
        app: demo
    spec:
      containers:
      - name: busy
        image: busybox
        command: ['sh','-c','sleep 3600']
```

ConfigMap & Secret minimal:
```
apiVersion: v1
kind: ConfigMap
metadata:
  name: demo-cm
data:
  GREETING: "bonjour"

---
apiVersion: v1
kind: Secret
metadata:
  name: demo-secret
type: Opaque
data:
  password: bXlwYXNzd29yZA==  # base64(mypassword)
```

---

