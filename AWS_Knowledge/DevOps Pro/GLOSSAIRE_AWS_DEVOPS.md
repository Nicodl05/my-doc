# 📚 **GLOSSAIRE AWS DevOps - Termes Essentiels**

## 🎯 **Guide d'utilisation**
Ce glossaire contient les **50+ termes essentiels** pour réussir la certification AWS DevOps Engineer Professional. Chaque terme est expliqué en **2-3 lignes courtes** pour une compréhension rapide.

---

## 🏗️ **INFRASTRUCTURE & NETWORKING**

### **ARN (Amazon Resource Name)**
Identifiant unique global pour toute ressource AWS. Format : `arn:aws:service:region:account:resource`. Utilisé pour les permissions IAM, les références entre services, et l'audit CloudTrail.

### **IAM (Identity and Access Management)**
Service AWS de gestion des identités et permissions. Contrôle QUI (Users/Roles) peut faire QUOI (Actions) sur QUOI (Resources) et QUAND (Conditions).

### **IAM Role**
Identité AWS avec permissions (policies) attachées. Utilisé par les services AWS (EC2, Lambda, ECS) pour accéder à d'autres ressources sans exposer de credentials. Plus sécurisé que les IAM Users.

### **VPC (Virtual Private Cloud)**
Réseau isolé virtuellement dans AWS. Contient des subnets, security groups, NACLs. Chaque VPC = 1 région, les subnets = 1 AZ chacune.

### **Subnet**
Subdivisions du VPC avec une plage CIDR spécifique. Public subnet = connectée à internet gateway, Private subnet = pas d'accès internet direct (utilise NAT gateway).

### **Security Group**
Pare-feu au niveau instance/ENI. Contrôle le trafic entrant/sortant par port et protocole. **Stateful** = répond automatiquement si demande sortante (contrairement aux NACL).

### **NACL (Network Access Control List)**
Pare-feu au niveau subnet. Contrôle le trafic entrant/sortant. **Stateless** = doit explicitement autoriser retour du trafic. Plus granulaire que Security Group.

### **Internet Gateway**
Permet aux instances dans les subnets publics d'accéder à internet. Attacher à VPC + ajouter route 0.0.0.0/0 vers IGW dans route table.

### **NAT Gateway/Instance**
Permet aux instances privées d'accéder à internet (mais internet ne peut pas les initier). NAT Gateway = managé par AWS (coûteux), NAT Instance = auto-managé (moins cher).

### **Route Table**
Table de routage pour chaque subnet. Définit QUI va OÙ (destination CIDR → target). Exemple : 0.0.0.0/0 → IGW (pour accès internet).

### **Multi-AZ (Availability Zone)**
Déployer les ressources dans 2+ AZ différentes. Protège contre les pannes AZ. RDS Multi-AZ = failover automatique, EC2 = utiliser Auto Scaling Groups.

### **DNS (Domain Name System)**
Traduit domaines (example.com) en adresses IP. Route 53 = DNS service AWS. Important pour les alias vers ALB/CloudFront.

---

## 💾 **STORAGE & DATABASES**

### **S3 (Simple Storage Service)**
Stockage d'objets illimité. Organiser en buckets → clés (chemins). Versioning, encryption, lifecycle policies, replication disponibles.

### **EBS (Elastic Block Store)**
Stockage bloc persistant pour EC2. Attaché à UNE instance dans UNE AZ. Snapshots = copies à un moment T, utiles pour backups.

### **EFS (Elastic File System)**
Système de fichiers NFS managé. Accessible depuis plusieurs instances et AZ. Plupert cher que EBS mais partage de données plus facile.

### **RDS (Relational Database Service)**
Base de données managée SQL. Supporte MySQL, PostgreSQL, MariaDB, Oracle, SQL Server. Multi-AZ = haute disponibilité avec failover automatique.

### **Aurora**
Base de données relational AWS-native (compatible MySQL/PostgreSQL). Plus rapide, plus cher que RDS standard. Auto-scaling en lecture avec Read Replicas.

### **DynamoDB**
Base de données NoSQL managée. Partition key + sort key. Auto-scaling ou provisioned capacity. Accès très rapide pour workloads clés-valeurs.

### **Redshift**
Data warehouse columnar pour analytics. Requêtes SQL sur très gros volumes. Plus lent que DynamoDB mais plus cheap pour analytics massives.

---

## 🚀 **COMPUTE & CONTAINERIZATION**

### **EC2 (Elastic Compute Cloud)**
Serveurs virtuels configurables. Choisir type instance (t3.micro, m5.large, c5.xlarge). Payer à l'usage ou Reserved Instances (moins cher long terme).

### **Lambda**
Calcul serverless. Payer à l'execution (nombre d'appels × durée). Timeout max 15 min. Idéal pour microservices, webhook, traitement événementiel.

### **Fargate**
Conteneurs sans serveur. Déployer conteneurs Docker sans gérer EC2. Plus cher que ECS sur EC2 mais pas d'ops infra. Utiliser avec ECS ou EKS.

### **ECS (Elastic Container Service)**
Orchestrateur conteneurs AWS-native. Déployer et manager conteneurs. Sur EC2 (vous gérez l'infra) ou Fargate (serverless). Intégration CloudFormation native.

### **EKS (Elastic Kubernetes Service)**
Kubernetes managé AWS. Si équipe connaît Kubernetes. Plus complexe qu'ECS, mais plus portable et flexible.

### **Auto Scaling Group**
Scaler automatiquement EC2/containers selon charge. Définir min/desired/max capacité. Utiliser métriques CloudWatch (CPU, memory custom).

### **Launch Template**
Configuration d'instance EC2 (AMI, instance type, security group, storage). Utilisé par Auto Scaling Groups et spot instances.

---

## 🔄 **CI/CD & DEPLOYMENT**

### **CodeCommit**
Git repository managé AWS. Alternative à GitHub. Intégration native avec CodeBuild/CodePipeline/CodeDeploy.

### **CodeBuild**
Build service managé. Compiler code, run tests, build Docker images. Définir buildspec.yml pour instructions. Payer à l'utilisation.

### **CodePipeline**
Orchestrateur CI/CD. Chaîner Source → Build → Test → Deploy. Déclencheur automatique sur CodeCommit push. Multi-stage orchestration.

### **CodeDeploy**
Service de déploiement. Pousser code vers EC2/on-premises/Lambda. Stratégies : All-at-once, Half-at-time, One-at-a-time, Blue/Green. AppSpec file = config déploiement.

### **CodeArtifact**
Repository d'artefacts managé. Maven, npm, NuGet, PyPI. Stockage sécurisé dépendances. Alternatif à Nexus/Artifactory.

### **CodeGuru**
AI pour qualité code. CodeGuru Reviewer = détecte bugs/vulnérabilités. CodeGuru Profiler = optimise performance runtime.

### **CloudFormation**
Infrastructure as Code AWS. Décrire stack en YAML/JSON. Déployer, updater, deleter ressources atomiquement. Templates réutilisables.

### **CDK (Cloud Development Kit)**
IaC avec code (Python, TypeScript, Java). Générer CloudFormation automatiquement. Plus flexible que CloudFormation direct.

---

## 📊 **DEPLOYMENT STRATEGIES**

### **Blue-Green Deployment**
2 environnements identiques (blue=actuel, green=nouveau). Déployer dans green, tester, switch router traffic. Rollback instantané = switch vers blue.

### **Canary Deployment**
Déployer vers petit % du trafic (ex: 5% vers v2). Monitorer, si OK alors 25%, 50%, 100%. Si KO = rollback. Moins risqué que blue-green.

### **Rolling Deployment**
Mettre à jour instances 1 par 1 (ou par batch). Zéro downtime. Lent. Si v2 a bug = rollback lent. CodeDeploy = support rolling.

### **A/B Testing**
2 versions simultanées pour comparer. Basé sur % trafic ou user attribute. Mesurer conversion. Pas réellement un "deployment" mais test.

### **Shadow Deployment**
Copier 100% trafic vers v2 mais ne pas l'utiliser (juste log réponse). Tester v2 sous charge réelle. Zéro risque.

---

## 🔒 **SECURITY & COMPLIANCE**

### **Encryption at Rest**
Chiffrer données stockées (S3, EBS, RDS). AWS Key Management Service (KMS) = gérer clés de chiffrement.

### **Encryption in Transit**
Chiffrer données en transit (TLS/SSL). HTTPS pour APIs, SSL pour databases. ACM = gérer certificats SSL/TLS.

### **Secrets Manager**
Stocker secrets sécurisés (passwords, API keys, DB credentials). Rotation automatique, audit CloudTrail, fine-grained IAM.

### **WAF (Web Application Firewall)**
Protéger applications web contre attaques (SQL injection, XSS). Règles personnalisables. Déployer devant CloudFront, ALB, ou API Gateway.

### **Shield**
Protection DDoS AWS. Shield Standard = gratuit, automatique. Shield Advanced = payant, support 24/7 DDoS mitigation team.

### **GuardDuty**
Threat detection intelligent. Analyse CloudTrail, VPC Flow Logs, DNS logs. Machine learning pour détecter menaces. Output vers Security Hub.

### **Inspector**
Vulnerability scanner. Scan instances EC2, Lambda code, ECR images. Détecte CVEs connus. Intégrer dans CI/CD pipeline.

### **Macie**
Data security. Découvre données sensibles (PII, credentials) dans S3. ML pour classification automatique. Alertes sur accès suspect.

---

## 📈 **MONITORING & LOGGING**

### **CloudWatch**
Monitoring centralisé. Métriques (CPU, memory, custom), logs (centralisé), alarms. Dashboard pour visualisation. Alarmes déclenche actions (SNS, Lambda).

### **CloudTrail**
Audit trail. Enregistre QUI a fait QUOI (API calls) QUAND. Multi-account, multi-region possible. Importer vers S3 pour long-term storage.

### **X-Ray**
Distributed tracing. Voir path requête à travers services. Identifier bottleneck. Applicable Lambda, API Gateway, EC2, ECS.

### **VPC Flow Logs**
Capture détails trafic réseau. Quels IP communiquent, quels ports. Debugger connectivity issues. S3 ou CloudWatch Logs comme destination.

### **Application Insights**
Monitoring applicatif managé. Détecte anomalies, corrèle avec problèmes infrastructure. Plus haut niveau qu'CloudWatch.

---

## 🎯 **PERFORMANCE & OPTIMIZATION**

### **RTO (Recovery Time Objective)**
Temps maximum acceptable pour recourir après désastre. Mesure en heures/minutes. Cible = max X secondes downtime.

### **RPO (Recovery Point Objective)**
Quantité de données max acceptable à perdre. Mesure en temps (ex: perte max 1h de données). Détermine fréquence backups/replications.

### **MTTR (Mean Time To Repair)**
Temps moyen pour réparer une panne. Métrique de résilience. Automation réduit MTTR.

### **Latency**
Délai pour requête = réponse. Mesure en ms. Impactée par: distance géographique, complexité traitement, bottlenecks. CloudFront réduit latency.

### **Throughput**
Volume de requêtes/données traitées par unité temps. Mesure en requests/sec ou Mbps. Auto Scaling augmente throughput.

### **Availability**
Pourcentage de temps service fonctionne. 99.9% = max 43 min downtime/mois, 99.99% = max 4 min. Multi-AZ = plus haut availability.

---

## 💰 **COST OPTIMIZATION**

### **Reserved Instances (RI)**
Payer à l'avance pour instances EC2 1 ou 3 ans. ~40-60% discount. Capacity reservation. Flexible RIs = changeable AZ/instance type.

### **Savings Plans**
Engagement compute flexibly (1-3 ans). ~20-40% discount. Applicable à EC2, Fargate, Lambda. Plus flexible que RIs.

### **Spot Instances**
Instances EC2 sur capacité excédentaire. ~70-90% discount. Peut être interrompu. Ideal pour workloads fault-tolerant (batch, CI/CD, dev/test).

### **On-Demand**
Payer à l'usage par heure/seconde. Pas engagement. Plus cher mais flexible. Utiliser pour variabilité imprévisible.

### **Cost Allocation Tags**
Tagguer ressources pour tracer coûts par projet/équipe/environnement. CloudBilling Group By Tag. Charge-back facile.

---

## 🔀 **MESSAGING & EVENTS**

### **SQS (Simple Queue Service)**
Message queue. Producer envoie message → Queue → Consumer lit. Découpler composants. Pull-based (consumer poll). Max message = 15 min visibility timeout.

### **SNS (Simple Notification Service)**
Publish-Subscribe messaging. Publisher envoie → Topic → Subscribers reçoivent. Push-based. Delivery flexible (HTTP, email, Lambda, SQS).

### **EventBridge**
Event bus managé. Route events depuis sources (AWS services, custom apps) vers targets (Lambda, SNS, SQS, etc). Pattern matching pour filtrer.

### **Kinesis Data Streams**
Streaming temps réel. Producer envoie → Stream (Shards) → Consumer. Shard = unit de capacité. Applicable IoT, analytics temps réel.

### **Kinesis Firehose**
Streaming data delivery service. Auto-buffer et envoyer à S3, Redshift, ElasticSearch. Transformation Lambda possible. Moins complexe que Data Streams.

### **Step Functions**
Orchestrer workflows serverless. State machine. Chainer Lambda, SNS, SQS, API calls. Retry/error handling intégré.

---

## 📊 **ANALYTICS**

### **Athena**
SQL queries sur S3 (parquet, JSON, CSV). Serverless, payer par GB scannée. Idéal ad-hoc queries. Glue Catalog = metadata.

### **Glue**
ETL serverless. Glue Crawler = découvrir schema, Glue Jobs = transform data. Glue Catalog = central metadata repository.

### **EMR (Elastic MapReduce)**
Hadoop/Spark cluster managé. Big data processing. Long-running clusters ou transient jobs. Non-serverless mais flexible.

### **QuickSight**
BI tool managé. Dashboard, analytics. SPICE = in-memory cache. Intégration 40+ data sources. ML features (anomaly detection, forecasting).

---

## 🌐 **CONTENT DELIVERY & ROUTING**

### **CloudFront**
CDN global AWS. Cachet content aux edge locations (200+). Réduire latency global. DDoS protection intégrée.

### **Route 53**
DNS service AWS. Enregistrer domaines, routing policies (simple, weighted, latency-based, failover, geolocation). Health checks.

### **API Gateway**
API frontal managé. REST, HTTP, WebSocket APIs. Auth (API key, IAM, custom authorizers), throttling, CORS. Déployer stages (dev, prod).

### **ALB (Application Load Balancer)**
Balancer charges. Layer 7 (HTTP/HTTPS). Host-based ou path-based routing. Health checks auto. Fondation pour Auto Scaling.

### **NLB (Network Load Balancer)**
Balancer ultra-haute performance. Layer 4 (TCP/UDP). Ultra-low latency. Millions requests/sec. Gaming, IoT, non-HTTP protocols.

---

## 🛢️ **CACHE & OPTIMIZATION**

### **ElastiCache**
In-memory cache servicel. Redis ou Memcached. Réduire database load. Sessions store. Pub/sub patterns possible (Redis).

### **DAX (DynamoDB Accelerator)**
Cache in-memory pour DynamoDB. Microsecond latency. Transparently cache queries.

### **CloudFront Caching**
Cache static/dynamic content aux edge. TTL policies. Query string/headers handling. Invalidation possible.

---

## ✅ **RÉSUMÉ CERTIFICATION**

**À maîtriser avant l'examen :**
- ✓ IAM = fondation sécurité
- ✓ VPC/Networking = fondation infrastructure
- ✓ Multi-AZ/HA = résilience
- ✓ Deployment strategies = production readiness
- ✓ Monitoring = observabilité
- ✓ Cost optimization = business value
- ✓ Security = compliance

---

**Prêt pour certification ? → Voir [PATTERNS_ARCHITECTURAUX.md](PATTERNS_ARCHITECTURAUX_DEVOPS.md)**
