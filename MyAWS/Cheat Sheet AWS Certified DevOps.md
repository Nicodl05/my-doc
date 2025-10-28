# **Cheat Sheet AWS Certified DevOps Engineer - Professional (DOP-C01)**

---

## **📌 CI/CD & Déploiement**
### **1. AWS CodeCommit**
- **Rôle** : Repository Git privé et sécurisé pour le code source, intégré à AWS.
- **Cas d'usage + Pièges** :
  - **Utilisation** : Stocker le code d’une app avec contrôle d’accès via IAM.
  - **Piège** : Ne pas protéger les branches (ex: `master`) → Utiliser des **politiques IAM conditionnelles** (ex: `codecommit:GitPush` avec `aws:ResourceTag/branch: master`).
  - **Intégration** : Déclencher un pipeline CodePipeline après un `git push`.
- **Astuce pour l’examen** :
  - Si une question mentionne des **branches protégées**, pensez aux **politiques IAM** avec des conditions sur les tags de branche.

---

### **2. AWS CodeBuild**
- **Rôle** : Service de build serverless pour compiler, tester et packager le code.
- **Cas d'usage + Pièges** :
  - **Utilisation** : Exécuter des tests unitaires ou créer des images Docker.
  - **Piège** :
    - Utiliser des **variables d’environnement statiques** pour les credentials → Préférer les **rôles IAM**.
    - Oublier de configurer le **timeout** (max 8h) pour les builds longs.
  - **Intégration** : Pousser des images Docker dans **ECR** après un build réussi.
- **Astuce pour l’examen** :
  - Pour les builds Docker, utilisez toujours le **driver `awslogs`** pour envoyer les logs à CloudWatch.

---

### **3. AWS CodeDeploy**
- **Rôle** : Déploiement automatisé d’applications (EC2, Lambda, ECS) avec rollback.
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Déployer une nouvelle version d’une app sur un **Auto Scaling Group** avec un **déploiement canary**.
    - Intégration avec **ALB** pour basculer le trafic progressivement.
  - **Piège** :
    - Confondre `BeforeAllowTraffic` (vérification pré-déploiement) et `AfterAllowTraffic` (post-déploiement).
    - Oublier de configurer les **health checks** pour les rollbacks automatiques.
- **Astuce pour l’examen** :
  - Pour les déploiements **Lambda**, utilisez `LambdaCanary10Percent5Minutes` pour un déploiement progressif.

---

### **4. AWS CodePipeline**
- **Rôle** : Orchestration des étapes CI/CD (source → build → test → déploiement).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Chaîner CodeCommit → CodeBuild → CodeDeploy pour un pipeline complet.
    - Ajouter des **étapes manuelles** (ex: approbation avant production).
  - **Piège** :
    - Exécuter les étapes en **série** alors qu’elles pourraient être parallèles → Utilisez `runOrder`.
    - Stocker les artefacts dans **ECR** au lieu de **S3** → CodePipeline nécessite S3.
- **Astuce pour l’examen** :
  - Pour les **déploiements multi-régions**, utilisez **CloudFormation StackSets** dans une étape finale.

---

### **5. Amazon ECR**
- **Rôle** : Registry Docker privé pour stocker et gérer les images conteneurisées.
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Stocker des images Docker pour ECS/EKS.
    - Intégration avec CodeBuild pour pousser des images après un build.
  - **Piège** :
    - Utiliser le tag `latest` → Impossible de tracker les versions → Préférer les **hashes SHA**.
    - Oublier de configurer les **politiques de lifecycle** pour nettoyer les anciennes images.
- **Astuce pour l’examen** :
  - Pour les déploiements ECS, référencez toujours l’image par son **SHA** (ex: `123456789.dkr.ecr.us-east-1.amazonaws.com/my-app@sha256:abc123`).

---

### **6. Amazon ECS**
- **Rôle** : Orchestration de conteneurs Docker (sur EC2 ou Fargate).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Déployer une app conteneurisée avec un **Auto Scaling Group** pour les tâches.
    - Utiliser **Fargate** pour éviter de gérer les instances EC2.
  - **Piège** :
    - Oublier de configurer le **log driver `awslogs`** pour les logs CloudWatch.
    - Ne pas mettre à jour le **task definition** avant un déploiement.
- **Astuce pour l’examen** :
  - Pour les déploiements **blue/green**, utilisez **CodeDeploy avec ECS**.

---

### **7. AWS Lambda**
- **Rôle** : Exécution de code serverless (max 15 min, déclenché par événements).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Traiter des fichiers uploadés dans S3.
    - Déclencher une remédiation après une alerte CloudWatch.
  - **Piège** :
    - Utiliser Lambda pour des tâches **>15 min** → Préférer **Step Functions** ou **Fargate**.
    - Oublier de configurer le **timeout** et la **mémoire** (impacte les coûts).
- **Astuce pour l’examen** :
  - Pour les **workflows longs**, combinez Lambda avec **Step Functions**.

---

## **📌 Infrastructure as Code (IaC)**
### **1. AWS CloudFormation**
- **Rôle** : Déploiement d’infrastructure via des templates (JSON/YAML).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Créer un stack complet (VPC, EC2, RDS) en une seule commande.
    - Utiliser **StackSets** pour déployer dans plusieurs comptes/régions.
  - **Piège** :
    - Oublier les **dépendances** entre ressources → Utilisez `DependsOn`.
    - Ne pas gérer les **mises à jour** → Utilisez `UpdatePolicy` pour les ASG.
- **Astuce pour l’examen** :
  - Pour les **Auto Scaling Groups**, utilisez `AutoScalingRollingUpdate` pour les rolling updates.

---

### **2. AWS OpsWorks**
- **Rôle** : Gestion de configurations avec Chef/Puppet (stacks, layers, recipes).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Automatiser la configuration d’un cluster Cassandra.
    - Déployer des apps PHP/Node.js avec des **lifecycle hooks** (`setup`, `configure`).
  - **Piège** :
    - Utiliser `setup` pour les mises à jour dynamiques → Préférer `configure`.
    - Oublier d’installer l’**agent OpsWorks** sur les instances.
- **Astuce pour l’examen** :
  - Pour les **mises à jour dynamiques**, utilisez toujours le hook `configure`.

---

### **3. AWS Elastic Beanstalk**
- **Rôle** : Déploiement simplifié d’applications (PaaS) avec scaling automatique.
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Déployer une app Node.js avec un **Load Balancer** et Auto Scaling.
    - Utiliser des **Worker Environments** pour traiter des tâches asynchrones (SQS).
  - **Piège** :
    - Modifier l’ALB directement via la console → Utilisez `.ebextensions`.
    - Oublier de configurer les **health checks** pour les déploiements.
- **Astuce pour l’examen** :
  - Pour les **redirections HTTP→HTTPS**, utilisez `aws:elbv2:listener:default` dans `.ebextensions`.

---

### **4. AWS Systems Manager (SSM)**
- **Rôle** : Gestion centralisée des instances (run commands, patching, inventaire).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Exécuter des commandes à distance sur des instances EC2 (`Run Command`).
    - Automatiser les **patches** avec **Patch Manager**.
  - **Piège** :
    - Oublier d’installer l’**agent SSM** sur les instances on-premise.
    - Utiliser **Parameter Store** pour les secrets → Préférer **Secrets Manager**.
- **Astuce pour l’examen** :
  - Pour les **inventaires**, utilisez **SSM Inventory** + export vers S3.

---

## **📌 Monitoring & Logging**
### **1. Amazon CloudWatch**
- **Rôle** : Métriques, logs, alarmes et dashboards pour surveiller les ressources.
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Surveiller la CPU/mémoire des instances EC2.
    - Créer des **alarms** pour déclencher des actions (ex: scaling, notifications).
  - **Piège** :
    - Confondre **CloudWatch Events** (événements) et **CloudTrail** (audit des API calls).
    - Oublier d’activer le **detailed monitoring** pour les instances EC2.
- **Astuce pour l’examen** :
  - Pour les **logs**, utilisez **CloudWatch Logs** + **metric filters** pour créer des alarmes.

---

### **2. AWS CloudTrail**
- **Rôle** : Audit des appels API (qui a fait quoi, quand, où).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Traquer les appels API pour la conformité (ex: qui a supprimé un bucket S3 ?).
    - Intégration avec **CloudWatch Events** pour déclencher des alertes.
  - **Piège** :
    - Ne pas activer CloudTrail dans **toutes les régions** → Utilisez AWS Organizations.
    - Oublier de vérifier l’**intégrité des logs** → Utilisez `aws cloudtrail validate-logs`.
- **Astuce pour l’examen** :
  - CloudTrail est **obligatoire** pour l’audit, mais ne surveille pas les **ressources** (utilisez **Config**).

---

### **3. AWS Config**
- **Rôle** : Audit de conformité des ressources (ex: "Tous les buckets S3 sont-ils chiffrés ?").
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Vérifier que toutes les instances EC2 ont des tags obligatoires.
    - Créer des **règles personnalisées** (Lambda) pour des audits spécifiques.
  - **Piège** :
    - Confondre Config (conformité) et **CloudTrail** (audit des API calls).
    - Oublier de créer un **aggregator** pour les comptes multi-AWS.
- **Astuce pour l’examen** :
  - Pour les **règles personnalisées**, utilisez **Lambda** + **SNS** pour les notifications.

---

### **4. AWS X-Ray**
- **Rôle** : Tracing des requêtes pour analyser les performances (latence, erreurs).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Debugger une app Lambda qui appelle plusieurs services (ex: DynamoDB, S3).
    - Identifier les **goulots d’étranglement** dans une architecture microservices.
  - **Piège** :
    - Oublier d’activer le **tracing** dans le code (SDK requis).
    - Ne pas configurer les **sampling rules** pour limiter les coûts.
- **Astuce pour l’examen** :
  - X-Ray est utile pour le **debugging**, mais pas pour le **monitoring** (utilisez **CloudWatch**).

---

## **📌 Résilience & Scalabilité**
### **1. Auto Scaling**
- **Rôle** : Scaling automatique des instances EC2 (basé sur CPU, mémoire, etc.).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Scaler une app web pendant les pics de trafic (ex: Black Friday).
    - Utiliser des **Scheduled Actions** pour le scaling prédictif.
  - **Piège** :
    - Oublier de suspendre les **scale-out** pendant un déploiement CodeDeploy.
    - Confondre **Canary** (Lambda/ECS) et **Rolling** (EC2).
- **Astuce pour l’examen** :
  - Pour les **déploiements**, utilisez `AutoScalingRollingUpdate` dans CloudFormation.

---

### **2. ALB/NLB**
- **Rôle** : Répartition de charge (HTTP/TCP) avec health checks.
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Répartir le trafic entre plusieurs instances EC2.
    - Configurer des **redirections HTTP→HTTPS**.
  - **Piège** :
    - Ne pas configurer les **health checks** pour les rollbacks CodeDeploy.
    - Oublier de vérifier la **connectivité backend** (ex: DB) dans les health checks.
- **Astuce pour l’examen** :
  - Pour les **déploiements blue/green**, utilisez **ALB + CodeDeploy**.

---

### **3. Amazon RDS & Aurora**
- **Rôle** : Bases de données relationnelles managées (MySQL, PostgreSQL).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Déployer une base MySQL avec **Multi-AZ** pour la haute disponibilité.
    - Utiliser **Aurora Global Database** pour les apps globales.
  - **Piège** :
    - Oublier de tester les **failovers** en staging.
    - Confondre **Multi-AZ** (haute disponibilité) et **Read Replicas** (scalabilité en lecture).
- **Astuce pour l’examen** :
  - Pour les **upgrades majeurs**, utilisez un **Read Replica** + promotion.

---

### **4. Amazon Route 53**
- **Rôle** : DNS avec routing intelligent (failover, géolocalisation).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Basculer le trafic entre deux régions en cas de failover.
    - Utiliser des **health checks** pour router le trafic vers des instances saines.
  - **Piège** :
    - Oublier de configurer le **TTL** pour les failovers rapides.
    - Utiliser Route 53 pour le **load balancing** → Préférer **ALB**.
- **Astuce pour l’examen** :
  - Pour les **failovers**, utilisez **Route 53 + health checks**.

---

## **📌 Sécurité & Conformité**
### **1. IAM**
- **Rôle** : Gestion des permissions (utilisateurs, rôles, politiques).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Créer des **rôles IAM** pour les instances EC2 (pas de credentials statiques).
    - Utiliser des **politiques conditionnelles** (ex: `aws:ResourceTag/branch: master`).
  - **Piège** :
    - Donner des permissions trop larges (ex: `AdministratorAccess`).
    - Oublier d’activer **MFA** pour les comptes root.
- **Astuce pour l’examen** :
  - Pour les **politiques**, utilisez toujours le **principe de least privilege**.

---

### **2. AWS KMS**
- **Rôle** : Chiffrement des données (clés symétriques/asymétriques).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Chiffrer des buckets S3 ou des volumes EBS.
    - Gérer les clés de chiffrement pour RDS.
  - **Piège** :
    - Oublier de configurer les **grants** pour les comptes cross-AWS.
    - Utiliser des **clés par défaut** → Créez des clés dédiées.
- **Astuce pour l’examen** :
  - KMS est **obligatoire** pour le chiffrement, mais ne gère pas les **secrets** (utilisez **Secrets Manager**).

---

### **3. AWS Secrets Manager**
- **Rôle** : Stockage et rotation automatique des secrets (mots de passe, API keys).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Stocker les credentials RDS avec rotation automatique.
    - Intégration avec Lambda pour récupérer les secrets.
  - **Piège** :
    - Utiliser **Parameter Store** pour les secrets → Préférer Secrets Manager.
    - Oublier de configurer la **rotation** pour les credentials.
- **Astuce pour l’examen** :
  - Secrets Manager est **payant**, mais indispensable pour les **secrets sensibles**.

---

### **4. AWS GuardDuty & Macie**
- **Rôle** :
  - **GuardDuty** : Détection des menaces (comportements malveillants).
  - **Macie** : Détection des données sensibles (PII) dans S3.
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Détecter des **credentials exposées** avec GuardDuty.
    - Scanner les buckets S3 pour des **données PII** avec Macie.
  - **Piège** :
    - Confondre GuardDuty (menaces) et **Inspector** (vulnérabilités).
    - Oublier d’activer Macie dans **toutes les régions**.
- **Astuce pour l’examen** :
  - GuardDuty est **gratuit pendant 30 jours**, puis payant.

---

## **📌 Incident Response & Automation**
### **1. AWS Step Functions**
- **Rôle** : Orchestration de workflows complexes (états, retries, parallélisme).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Automatiser une **remédiation** après une alerte (ex: redémarrer une instance).
    - Coordonner des **étapes Lambda** pour un workflow long (>15 min).
  - **Piège** :
    - Utiliser Lambda seul pour des workflows longs → Préférer Step Functions.
    - Oublier les **états `Catch`** pour gérer les erreurs.
- **Astuce pour l’examen** :
  - Step Functions est **idéal** pour les workflows avec **logique conditionnelle**.

---

### **2. AWS SSM Automation**
- **Rôle** : Automatisation de tâches (ex: redémarrage d’instances).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Créer un **runbook** pour redémarrer des instances défaillantes.
    - Automatiser des **patches** sur un parc d’instances.
  - **Piège** :
    - Oublier de tester les **documents Automation** avant production.
    - Confondre SSM Automation et **Step Functions** (SSM est plus limité).
- **Astuce pour l’examen** :
  - SSM Automation est **idéal** pour les tâches **linéaires** (ex: redémarrage).

---

### **3. Amazon SNS**
- **Rôle** : Notifications (emails, SMS, Lambda) pour les alertes.
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Envoyer des alertes après un échec de déploiement CodePipeline.
    - Intégration avec **CloudWatch Alarms** pour les notifications.
  - **Piège** :
    - Oublier de configurer les **abonnements** (ex: email, Lambda).
    - Utiliser SNS pour des **workflows complexes** → Préférer Step Functions.
- **Astuce pour l’examen** :
  - SNS est **simple et économique**, mais ne gère pas la logique métier.

---

## **📌 Stockage & Bases de Données**
### **1. Amazon S3**
- **Rôle** : Stockage d’objets (fichiers, logs, backups).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Stocker des logs ou des artefacts de build (CodePipeline).
    - Configurer des **lifecycle policies** pour archiver les anciens fichiers.
  - **Piège** :
    - Oublier d’activer le **versioning** pour les backups.
    - Ne pas configurer les **ACLs** ou **bucket policies** pour la sécurité.
- **Astuce pour l’examen** :
  - S3 est **durable (11 9s)**, mais pas un système de fichiers (utilisez **EFS**).

---

### **2. Amazon EBS**
- **Rôle** : Stockage en bloc pour les instances EC2.
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Attacher un volume EBS à une instance EC2 pour le stockage persistant.
    - Créer des **snapshots** pour les backups.
  - **Piège** :
    - Oublier de chiffrer les volumes avec **KMS**.
    - Ne pas configurer les **snapshots automatiques**.
- **Astuce pour l’examen** :
  - EBS est **local à une AZ** → Utilisez des snapshots pour la **résilience**.

---

### **3. Amazon EFS**
- **Rôle** : Système de fichiers partagé (compatible NFS).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Partager des fichiers entre plusieurs instances EC2.
    - Utiliser avec **ECS** pour le stockage persistant des conteneurs.
  - **Piège** :
    - Oublier de configurer les **security groups** pour l’accès réseau.
    - Confondre EFS (partagé) et **EBS** (local à une instance).
- **Astuce pour l’examen** :
  - EFS est **coûteux** mais indispensable pour le **stockage partagé**.

---

### **4. Amazon DynamoDB**
- **Rôle** : Base de données NoSQL serverless (clé-valeur, documents).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Stocker des données de session pour une app web.
    - Utiliser **DAX** pour réduire la latence des lectures.
  - **Piège** :
    - Oublier de configurer les **capacités** (on-demand vs provisionné).
    - Ne pas utiliser les **Global Tables** pour les apps multi-régions.
- **Astuce pour l’examen** :
  - DynamoDB est **serverless**, mais nécessite une **bonne modélisation des données**.

---

### **5. Amazon RDS & Aurora**
- **Rôle** : Bases de données relationnelles managées.
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Déployer une base MySQL avec **Multi-AZ** pour la haute disponibilité.
    - Utiliser **Aurora Serverless** pour les charges variables.
  - **Piège** :
    - Oublier de tester les **failovers** en staging.
    - Confondre **Multi-AZ** (haute disponibilité) et **Read Replicas** (scalabilité en lecture).
- **Astuce pour l’examen** :
  - Pour les **upgrades majeurs**, utilisez un **Read Replica** + promotion.

---

## **📌 Réseau & Conteneurs**
### **1. Amazon VPC**
- **Rôle** : Réseau privé virtuel (sous-réseaux, ACLs, route tables).
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Isoler des ressources dans un réseau privé.
    - Configurer des **NACLs** et **Security Groups** pour la sécurité.
  - **Piège** :
    - Oublier de configurer les **route tables** pour le trafic internet.
    - Ne pas utiliser des **subnets privés** pour les bases de données.
- **Astuce pour l’examen** :
  - VPC est **obligatoire** pour la sécurité réseau.

---

### **2. AWS Direct Connect**
- **Rôle** : Connexion dédiée entre un datacenter et AWS.
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Réduire la latence pour les apps hybrides.
    - Utiliser avec **VPC** pour étendre un réseau local.
  - **Piège** :
    - Oublier de configurer les **BGP sessions** pour le routage.
    - Confondre Direct Connect et **VPN** (Direct Connect est plus rapide).
- **Astuce pour l’examen** :
  - Direct Connect est **coûteux** mais offre une **bande passante garantie**.

---

### **3. Amazon API Gateway**
- **Rôle** : Création et gestion d’APIs REST/HTTP.
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Créer une API pour une app serverless (Lambda).
    - Configurer des **stages** (v1, v2) pour le versioning.
  - **Piège** :
    - Oublier de configurer les **throttling limits** pour éviter les abus.
    - Ne pas utiliser les **mapping templates** pour transformer les requêtes.
- **Astuce pour l’examen** :
  - API Gateway est **obligatoire** pour exposer des Lambda comme APIs.

---

### **4. Amazon EKS**
- **Rôle** : Kubernetes managé pour l’orchestration de conteneurs.
- **Cas d'usage + Pièges** :
  - **Utilisation** :
    - Déployer des apps conteneurisées avec Kubernetes.
    - Intégration avec **IAM** pour les permissions.
  - **Piège** :
    - Oublier de configurer les **node groups** pour le scaling.
    - Confondre EKS et **ECS** (EKS est plus complexe mais plus flexible).
- **Astuce pour l’examen** :
  - EKS est **coûteux** mais indispensable pour les **apps Kubernetes**.

---

## **💡 Astuces Générales pour l’Examen**
1. **Lire attentivement les questions** :
   - Repérer les mots-clés comme **"minimal cost"**, **"high availability"**, ou **"automated rollback"**.
   - Éliminer les réponses avec des **solutions manuelles** (ex: "redémarrer manuellement").

2. **Gérer son temps** :
   - **2 min par question** : Si vous bloquez, marquez la question et passez.
   - **Revenir aux questions marquées** à la fin.

3. **Connaître les différences clés** :
   - **CloudWatch** (métriques/logs) vs **CloudTrail** (audit des API calls).
   - **Lambda** (tâches courtes) vs **Step Functions** (workflows longs).
   - **Parameter Store** (configurations) vs **Secrets Manager** (secrets sensibles).

4. **Pratiquer avec des exams blancs** :
   - **Tutorials Dojo** et **Jon Bonso** sont les meilleurs pour s’entraîner.
   - **Relire les explications** pour chaque réponse fausse.

5. **Se concentrer sur les services clés** :
   - **CI/CD** : CodeCommit, CodeBuild, CodeDeploy, CodePipeline.
   - **IaC** : CloudFormation, OpsWorks, Elastic Beanstalk.
   - **Monitoring** : CloudWatch, CloudTrail, Config.
   - **Résilience** : Auto Scaling, ALB, RDS, Aurora.
   - **Sécurité** : IAM, KMS, Secrets Manager, GuardDuty.

---
**Besoin de précisions sur un service en particulier ?** 😊
*(Exemples : "Step Functions pour les workflows EBS", "OpsWorks pour Cassandra", etc.)*