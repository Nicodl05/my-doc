# Kubernetes — Questions d'entretien (avec réponses concises)

Objectif: questions classées par niveau pour révision rapide et préparation orale.

---

**Préambule: k8s vs k3s vs k3d**

- `k8s` (Kubernetes upstream): distribution officielle, composants complets (API Server, kubelet, controller-manager, scheduler, etcd). Utilisé en production pour clusters de toutes tailles.
- `k3s`: distribution Kubernetes légère développée par Rancher, empaquetée en binaire unique, remplace etcd par sqlite par défaut (ou etcd/klipper), optimisée pour edge, IoT et environnements contraints.
- `k3d`: outil qui exécute des clusters `k3s` dans Docker containers (utile pour labs locaux/CI). k3d = k3s in Docker.

---

**Junior (fondamentaux)**

Q1: Qu'est-ce qu'un Pod ?
A1: Plus petite unité déployable — un ou plusieurs containers partageant réseau et volumes.

Q1b: Qu'est-ce qu'un cluster ?
A1b: Un cluster Kubernetes est un ensemble coordonné de machines (nodes) géré par un control plane (API Server, etcd, controllers, scheduler) fournissant l'environnement d'exécution distribué pour déployer des pods, services et volumes.

Q1c: Qu'est-ce qu'un node ?
A1c: Un node est une machine (VM, serveur physique ou container dans k3d) qui exécute `kubelet`, `kube-proxy` et le runtime de containers; il fournit CPU, mémoire, stockage et réseau pour héberger des pods.

Q2: Différence entre Deployment et StatefulSet ?
A2: Deployment gère pods identiques sans identité stable; StatefulSet fournit identité réseau stable et PVs par pod.

Q3: À quoi sert un Service ClusterIP ?
A3: Exposer un ensemble de pods à l'intérieur du cluster par une IP stable.

Q4: Qu'est-ce qu'un Namespace ?
A4: Isolation logique pour ressources (noms, quotas, RBAC) au sein d'un cluster.

Q5: Comment lister les noeuds ?
A5: `kubectl get nodes`.

---

**Intermédiaire (réseau, stockage, sécurité)**

Q6: Qu'est-ce qu'une PersistentVolumeClaim ?
A6: Requête de stockage utilisée par un Pod; liée à un PV statique ou provisionnée dynamiquement via StorageClass.

Q7: Comment fonctionnent les liveness et readiness probes ?
A7: Liveness redémarre le container si échoue; readiness retire le Pod des endpoints si échoue.

Q8: Qu'est-ce que RBAC ?
A8: Role-Based Access Control — Roles/ClusterRoles + RoleBindings/ClusterRoleBindings pour autoriser actions sur ressources.

Q9: Que fait une NetworkPolicy ?
A9: Contrôle flux ingress/egress entre pods/namespaces si le CNI la supporte.

Q10: À quoi sert kube-proxy ?
A10: Implémente routing de Service sur chaque node (iptables ou IPVS).

---

**Avancé (opérations, HA, debugging)**

Q11: Comment diagnostiquer un pod Pending ?
A11: `kubectl describe pod` pour events (Insufficient cpu/memory, nodeSelector/taints mismatch, PVC unbound).

Q12: Quelle est la différence entre etcd et API Server ?
A12: API Server est point d'entrée REST; etcd est le datastore persistant de l'état du cluster.

Q13: Expliquez admission controllers et donnez un exemple.
A13: Modules interceptant requêtes API après authn/authz mais avant persistance (ex: MutatingAdmissionWebhook pour injecter sidecars).

Q14: Que sont PodDisruptionBudget et pourquoi l'utiliser ?
A14: PDB définit le nombre minimal de pods disponibles durant maintenances/evictions pour garantir disponibilité.

Q15: Comment faire un rollback d'un Deployment ?
A15: `kubectl rollout undo deployment/<name>`.

---

**Expert (architecture, autoscaling, sécurité)**

Q16: Comment fonctionne le scheduler Kubernetes ?
A16: Prend les pods non assignés, filtre nodes (taints, selectors, resource fit), score nodes (affinity, binpacking), choisit node, puis bind via API.

Q17: Que sont HorizontalPodAutoscaler et VerticalPodAutoscaler ?
A17: HPA scale replicas horizontalement selon métriques; VPA ajuste requests/limits verticalement.

Q18: Comment sécuriser etcd ?
A18: Chiffrement at-rest (etcd encryption), TLS mutual auth between API Server and etcd, backups réguliers, accès restreint.

Q19: Expliquez comment diagnostiquer un réseau pod-to-pod qui échoue.
A19: Vérifier CNI, NetworkPolicy, `kubectl exec` curl/nslookup, `kubectl get pods -o wide` pour IPs, logs du CNI, tcpdump sur node.

Q20: Que sont EndpointSlice et pourquoi sont-ils introduits ?
A20: Structure scalable pour représenter endpoints d'un service; améliore scalabilité et performance vs Endpoints.

---
