# 📘 Glossaire Complet AWS DevOps - Services de Certification

> Guide exhaustif des 120+ services AWS pour la certification DevOps Engineer Professional

---

## 📊 **ANALYTICS**

### **Amazon Athena**
Service de requêtes SQL serverless sur données S3. Facturation par TB scanné ($5/TB). Supporte formats Parquet, ORC, JSON, CSV. Idéal pour analyses ad-hoc, logs exploration, data lake queries sans infrastructure.

### **Amazon EMR (Elastic MapReduce)**
Plateforme big data managée pour Hadoop, Spark, Presto, HBase. Clusters transients (job-based) ou persistants. Supporte Spot Instances pour économies jusqu'à 90%. Idéal pour ETL massif, ML training, analyse logs.

### **Amazon Kinesis Data Firehose**
Service livraison streaming data vers S3, Redshift, OpenSearch, Splunk. Serverless, auto-scaling, buffer automatique. Transformations Lambda possibles. Facturation par GB ingéré. Latence 60 secondes minimum.

### **Amazon Kinesis Data Streams**
Streaming temps-réel, ingestion massive de données (logs, events, IoT). Shards manuels (1MB/s ou 1000 records/s par shard). Retention 24h-365 jours. Consommateurs custom (Lambda, EC2, Kinesis Analytics).

### **Amazon OpenSearch Service**
Service managé pour OpenSearch (fork Elasticsearch). Recherche full-text, logs analytics, visualisation Kibana/OpenSearch Dashboards. Multi-AZ, snapshots automatiques. Idéal pour APM, SIEM, analyse logs CloudWatch.

### **Amazon QuickSight**
Business Intelligence serverless. Dashboards interactifs, ML Insights (anomalies, forecasting). SPICE engine in-memory. Facturation par session ($0.30) ou per-user ($12/mois). Intégration Athena, RDS, Redshift, S3.

---

## 🔗 **APPLICATION INTEGRATION**

### **Amazon AppFlow**
Service ETL SaaS-to-AWS sans code. Connecteurs: Salesforce, Slack, ServiceNow, Google Analytics, S3, Redshift. Flux triggered, scheduled ou event-driven. Transformations, filtres, encryption automatique. Facturation par flow run.

### **Amazon EventBridge**
Bus d'événements serverless. Routage événements entre services AWS, applications custom, SaaS (Zendesk, Datadog). Règles basées sur patterns JSON. Event replay, archive. Facturation $1/million événements. Remplace CloudWatch Events.

---

## 💻 **COMPUTE**

### **AWS App Runner**
Service PaaS pour déployer containers/code source (Python, Node.js) directement. Auto-scaling, HTTPS, load balancing inclus. Alternative simple à ECS/EKS. Facturation par vCPU/RAM utilisés + requêtes. Idéal pour APIs, web apps simples.

### **Amazon EC2 (Elastic Compute Cloud)**
Serveurs virtuels, contrôle complet OS. Types: General (t3, m6), Compute (c6), Memory (r6), Storage (i4), GPU (p4, g5). Pricing: On-Demand, Reserved (économie 72%), Spot (économie 90%), Savings Plans.

### **Amazon EC2 Auto Scaling**
Ajustement automatique capacité EC2. Scaling policies: Target Tracking (CPU 70%), Step Scaling (paliers), Scheduled, Predictive. Intégration ELB health checks. Launch Templates, warm pools, lifecycle hooks.

### **EC2 Image Builder**
Service automatisation création/maintenance AMIs et container images. Pipelines avec tests validation. Gestion patches, hardening sécurité. Distribution multi-régions/comptes. Suppression versions obsolètes automatique.

### **AWS Elastic Beanstalk**
PaaS pour déployer apps web (Java, .NET, Node, Python, Ruby, Go, Docker). Gestion automatique infrastructure (EC2, ALB, ASG, RDS). Configuration `.ebextensions`. Rolling, Blue-Green, Immutable deployments. CLI `eb`.

### **AWS Serverless Application Repository**
Marketplace d'applications serverless prêtes à déployer. Templates SAM publics/privés. Intégration CloudFormation. Déploiement 1-click. Idéal pour réutiliser architectures Lambda, Step Functions, API Gateway.

---

## 📦 **CONTAINERS**

### **AWS App2Container (A2C)**
Outil migration applications legacy (Java, .NET) vers containers. Analyse app, génère Dockerfile, crée pipeline CI/CD CodePipeline. Déploie vers ECS/EKS. CLI tool local. Supporte Windows/Linux.

### **AWS Copilot**
CLI pour déployer apps conteneurisées sur ECS/Fargate/App Runner. Abstractions simplifiées (environments, services, jobs). Génère infrastructure CloudFormation. Commands: `copilot init`, `copilot deploy`. Alternative à ECS CLI.

### **Amazon ECR (Elastic Container Registry)**
Registry Docker images privé AWS. Intégration IAM, encryption at-rest (KMS), scan vulnérabilités, lifecycle policies. Réplication cross-region/cross-account. Facturation par GB stocké + data transfer.

### **Amazon ECS (Elastic Container Service)**
Orchestration containers Docker. Task Definitions (CPU, RAM, image), Services (desired count, ALB), Clusters. Launch types: EC2 (contrôle infrastructure) ou Fargate (serverless). Rolling updates, Blue-Green via CodeDeploy.

### **Amazon EKS (Elastic Kubernetes Service)**
Kubernetes managé AWS. Control plane géré (HA Multi-AZ). Worker nodes: EC2, Fargate, Spot. Add-ons: ALB Controller, CSI drivers, Calico. Intégration IAM, CloudWatch Container Insights. Version K8s supportées 3 ans.

### **Amazon EKS Distro**
Distribution Kubernetes upstream utilisée par EKS. Open-source, déployable on-premises, autre cloud. Même versions/patches qu'EKS. Pour hybrid/multi-cloud avec cohérence EKS.

### **AWS Fargate**
Compute serverless pour containers ECS/EKS. Pas de gestion EC2 (instances, patching, scaling). Facturation par vCPU-seconde + GB-seconde. Configurations: 0.25 vCPU-2GB → 16 vCPU-120GB. Idéal workloads variables.

### **Red Hat OpenShift Service on AWS (ROSA)**
OpenShift managé conjointement par AWS et Red Hat. Kubernetes enterprise-grade, developer console, CI/CD intégré. Facturation horaire nodes + AWS infrastructure. Support Red Hat 24/7. Alternative à EKS pour OpenShift users.

---

## 🗄️ **DATABASE**

### **Amazon Aurora**
Base relationnelle cloud-native compatible MySQL/PostgreSQL. 5x perf MySQL, 3x PostgreSQL. Storage auto-scaling (10GB-128TB), 15 read replicas, failover <30s. Aurora Serverless v2: auto-scaling instantané ACU (0.5-128).

### **Amazon Aurora Serverless v2**
Variante Aurora avec auto-scaling instantané capacité compute (ACU). Scaling granularité 0.5 ACU, latence <1s. Facturation par ACU-seconde utilisé. Idéal workloads variables, dev/test, applications multi-tenant.

### **AWS DMS (Database Migration Service)**
Migration bases de données vers AWS. Homogène (Oracle→Oracle) ou hétérogène (Oracle→Aurora). CDC (Change Data Capture) pour réplication continue. Schema Conversion Tool (SCT) pour conversions complexes. Facturation par instance replication.

### **Amazon DocumentDB (MongoDB compatibility)**
Base NoSQL compatible MongoDB API. Architecture distribuée, 3 copies données 3 AZs. Scaling read replicas (15 max), storage auto-scaling (10GB-64TB). Backups automatiques, PITR 35 jours. Idéal migrations MongoDB.

### **Amazon DynamoDB**
Base NoSQL serverless, latence millisecondes. Tables, items, attributs. Clés: partition key + sort key. Modes: On-Demand (pay-per-request) ou Provisioned (RCU/WCU). Global Tables (réplication multi-région), Streams, DAX (cache).

### **Amazon ElastiCache**
Service cache in-memory managé. Redis (persistance, pub/sub, Lua, clustering) ou Memcached (simple, multi-thread, sharding). Multi-AZ, read replicas, backups Redis. Latence sub-milliseconde. Idéal session store, gaming leaderboards.

### **Amazon MemoryDB for Redis**
Base in-memory durable compatible Redis. Persistance multi-AZ, PITR, durabilité données. Plus performant qu'ElastiCache Redis (durable writes). Idéal workloads nécessitant Redis + durabilité (cache, session, real-time analytics).

### **Amazon RDS (Relational Database Service)**
Bases relationnelles managées: MySQL, PostgreSQL, MariaDB, Oracle, SQL Server. Multi-AZ (failover automatique), read replicas, backups automatiques, PITR 35 jours. Scaling vertical, storage auto-scaling. Patching automatique.

### **Amazon Redshift**
Data warehouse OLAP, analyse petabytes données. Architecture columnar, compression, MPP (massively parallel). RA3 instances (storage/compute découplés), Redshift Serverless (auto-scaling). Spectrum (requêtes S3 direct). ML in-database.

---

## 🛠️ **DEVELOPER TOOLS**

### **AWS CLI**
Interface ligne commande AWS. Installation: pip, apt, brew. Configuration: `aws configure` (access key, secret, region). Commandes: `aws s3 cp`, `aws ec2 describe-instances`. Profiles multiples, output JSON/YAML/text/table.

### **AWS CDK (Cloud Development Kit)**
IaC avec langages programmation (TypeScript, Python, Java, C#, Go). Constructs (L1 CloudFormation, L2 abstractions, L3 patterns). Génère CloudFormation. Commands: `cdk init`, `cdk synth`, `cdk deploy`, `cdk diff`. CDK Pipelines pour CI/CD.

### **AWS CloudShell**
Terminal navigateur AWS pré-configuré. 1GB storage persistent, outils pré-installés (AWS CLI, Python, Node, git). Pas de frais. Timeout 20 min inactivité. Idéal scripts quick, tests CLI, pas besoin credentials locales.

### **AWS CodeArtifact**
Registry artifacts/packages managé. Support: Maven, Gradle, npm, yarn, pip, NuGet, twine. Upstream repositories (npmjs, PyPI, Maven Central) avec caching. Intégration CodeBuild, IAM authentication. Facturation par GB stocké.

### **AWS CodeBuild**
Service CI/CD managé pour builds. Environnements Docker (standard images AWS ou custom). Buildspec.yml: phases install, pre_build, build, post_build. Intégration GitHub, CodeCommit, S3. Artifacts vers S3. Logs CloudWatch. Facturation par minute build.

### **AWS CodeDeploy**
Service déploiement automatisé vers EC2, Lambda, ECS. Stratégies: In-Place, Blue-Green. Deployment configs: OneAtATime, HalfAtATime, AllAtOnce, custom. AppSpec file (hooks). Rollback automatique si alarmes CloudWatch. Intégration CodePipeline.

### **Amazon CodeGuru**
Suite ML pour revue code et profiling. **CodeGuru Reviewer**: détection bugs, vulnérabilités, best practices lors PRs GitHub/CodeCommit/Bitbucket. **CodeGuru Profiler**: analyse performance runtime, heap summary, CPU hotspots, recommandations optimisations.

### **AWS CodePipeline**
Orchestration CI/CD multi-stages. Stages: Source (CodeCommit, GitHub, S3), Build (CodeBuild), Test, Deploy (CodeDeploy, CloudFormation, ECS). Actions parallèles, approbations manuelles, webhooks. Intégration 3rd-party (Jenkins, Terraform Cloud).

### **AWS CodeStar**
Service unifié pour démarrer projets software. Templates pré-configurés (web apps, Lambda, Alexa skills). Génère pipeline CodePipeline complet. Dashboard projet (code, build, deploy). Gestion équipe IAM. Alternative: utiliser SAM ou CDK directement.

### **AWS FIS (Fault Injection Simulator)**
Service chaos engineering managé. Expériences contrôlées: arrêt EC2, throttling API, latency réseau, stress CPU. Templates pré-construits, stop conditions (alarmes CloudWatch). Rollback automatique. Logs CloudWatch. Valide résilience applications.

### **AWS SDKs and Tools**
SDKs officiels: JavaScript, Python (Boto3), Java, .NET, Ruby, Go, PHP, C++. IDE toolkits: VS Code, IntelliJ, PyCharm, Visual Studio. Mobile SDKs: iOS, Android, React Native. IoT Device SDKs. Authentication IAM, STS.

### **AWS X-Ray**
Distributed tracing pour applications microservices. Segments/subsegments par requête, service map visuel. Intégration: Lambda (automatique), ECS (daemon), EC2 (agent). Annotations, metadata custom. Sampling rules. Analyse latences, erreurs, bottlenecks.

---

## ⚙️ **MANAGEMENT AND GOVERNANCE**

### **AWS Auto Scaling**
Service unifié scaling multi-services (EC2, ECS, DynamoDB, Aurora). Plans scaling, predictive scaling. Dashboard centralisé. Alternative: utiliser auto-scaling natif par service (plus granulaire).

### **AWS CloudFormation**
IaC AWS natif, templates YAML/JSON. Stacks, change sets (preview changes), StackSets (multi-comptes/régions). Drift detection, rollback automatique, DeletionPolicy (Retain, Snapshot, Delete). Nested stacks, cross-stack references (Exports).

### **AWS CloudTrail**
Audit API calls AWS (qui, quoi, quand, où). Logs vers S3, intégration CloudWatch Logs. Event history 90 jours gratuit. Trails pour retention long-terme. Insights pour anomalies. Intégrité logs (digest files). Conformité, forensics.

### **Amazon CloudWatch**
Monitoring métriques, logs, alarmes. Métriques AWS services + custom. Alarmes (seuils, anomaly detection). Dashboards, Logs Insights (queries), Events (maintenant EventBridge). Agent CloudWatch (métriques OS, logs apps). Retention logs configurable.

### **Amazon CloudWatch Logs**
Collecte/stockage logs applications, services AWS. Log groups, log streams. Metric filters (métriques custom depuis logs). Subscriptions (vers Kinesis, Lambda, OpenSearch). Insights queries (SQL-like). Export S3 (archives), Kinesis (streaming).

### **AWS Compute Optimizer**
Recommandations ML pour rightsizing EC2, EBS, Lambda, ECS Fargate, ASG. Analyse utilisation CloudWatch (14 jours min). Métriques: CPU, RAM, réseau, disk. Savings estimés. Export S3 pour reporting. Gratuit.

### **AWS Config**
Audit configurations ressources AWS. Configuration items (snapshot état ressource), timeline changements. Conformité rules (managed ou custom Lambda). Aggregators multi-comptes. Remediation automatique (SSM Automation). Compliance dashboards.

### **AWS Control Tower**
Gouvernance multi-comptes automatisée. Landing Zone (OUs, accounts, baselines). Guardrails (SCPs préventifs, Config rules détectifs). Account Factory (déploiement comptes standardisés). Dashboard conformité. Intégration Organizations, SSO, CloudTrail.

### **AWS Health**
Notifications santé services AWS et comptes. Personal Health Dashboard (événements affectant vos ressources). Service Health Dashboard (status global services). Intégration EventBridge pour automation réactions. Historique 90 jours.

### **AWS License Manager**
Gestion licences software (BYOL: Bring Your Own License). Suivi utilisation, enforcement règles, discovery licences existantes. Intégration SSM, EC2, Marketplace. Réduction coûts overprovisioning licences. Host resource groups.

### **Amazon Managed Grafana**
Service Grafana managé. Dashboards visualisation multi-sources (CloudWatch, Prometheus, X-Ray, OpenSearch, Redshift). Authentification IAM Identity Center. Alerting, annotations. Pas de gestion serveurs, scaling automatique. Facturation par user actif.

### **Amazon Managed Service for Prometheus**
Service Prometheus managé. Collecte métriques containers (ECS, EKS), storage long-terme, HA. Intégration Grafana. PromQL queries. Retention configurable. Facturation par metrics ingested + query processed. Alternative CloudWatch Container Insights.

### **AWS OpsWorks**
Service configuration management Chef/Puppet managé. Stacks, layers, instances, apps, deployments. Recipes Chef, cookbooks. Lifecycle events (setup, configure, deploy, undeploy, shutdown). Alternative moderne: Systems Manager, ECS, Kubernetes.

### **AWS Organizations**
Gestion centralisée multi-comptes AWS. OUs (Organizational Units) hiérarchiques. SCPs (Service Control Policies) restrictions permissions. Consolidated billing. Partage ressources (RAM). Service control policies inheritance. Root, management account.

### **AWS Proton**
Service gestion infrastructure pour apps containers/serverless. Templates standardisés (environments, services). Self-service developers, contrôle platform teams. Versioning templates, déploiements automatisés. Intégration CodePipeline. Idéal grandes organisations.

### **AWS Resilience Hub**
Service évaluation résilience applications. RTO/RPO objectives, analyse dépendances, tests résilience (simulations failures). Recommandations architecturales. Scoring résilience. Compliance reports. Intégration FIS pour chaos engineering.

### **AWS Service Catalog**
Catalogue produits AWS pré-approuvés pour self-service. Portfolios, produits (CloudFormation templates). Contraintes (launch, notifications, tags). Contrôle versions, gouvernance. Idéal organisations désirant standardisation tout en donnant autonomie devs.

### **AWS Systems Manager**
Suite gestion opérations centralisée. **Parameter Store** (config, secrets gratuit 10K params). **Session Manager** (SSH/RDP sans bastion). **Patch Manager** (patching automatisé). **Run Command** (exécution scripts fleet). **State Manager** (configuration drift), Automation.

### **AWS Trusted Advisor**
Service recommandations best practices AWS. 5 catégories: Cost Optimization, Performance, Security, Fault Tolerance, Service Limits. Checks: Basic (7 gratuits) ou Full (tous avec Support Business/Enterprise). Intégration EventBridge pour automation.

---

## 🌐 **NETWORKING AND CONTENT DELIVERY**

### **Amazon API Gateway**
Service création/publication APIs REST, HTTP, WebSocket. Intégrations: Lambda, HTTP backends, AWS services. Features: throttling, caching, API keys, usage plans, authorizers (Lambda, Cognito, IAM). Stages, canary deployments. Facturation par million requests.

### **AWS Client VPN**
VPN managé pour accès sécurisé ressources AWS/on-premises. OpenVPN protocol, client software. Authentication: Active Directory, certificates, SAML 2.0. Split-tunnel, full-tunnel. Facturation par endpoint-hour + connections.

### **Amazon CloudFront**
CDN global, 400+ edge locations. Distribution types: web, RTMP. Origins: S3, ALB, EC2, custom HTTP. Caching (TTL), invalidations, signed URLs/cookies. Lambda@Edge (serverless compute at edge). Shield Standard inclus. Georestrictions.

### **Elastic Load Balancing (ELB)**
Distribution trafic entrant multi-targets. Types: **ALB** (HTTP/HTTPS layer 7, path-based routing, host-based), **NLB** (TCP/UDP/TLS layer 4, ultra-performance), **GLB** (layer 3 gateway appliances virtuelles), **CLB** (legacy layer 4/7).

### **AWS PrivateLink**
Connectivité privée entre VPCs, services AWS, on-premises sans Internet. Interface VPC endpoints (ENI avec IP privée), Gateway VPC endpoints (S3, DynamoDB). Service Providers (exposer vos services). Sécurité, pas de NAT/IGW/VPN.

### **Amazon Route 53**
DNS managé, domain registration. Routing policies: Simple, Weighted (A/B), Latency (performance), Failover (DR), Geolocation (restriction géo), Geoproximity (bias), Multi-Value. Health checks (endpoints, CloudWatch alarms, calculated). DNSSEC support.

### **AWS Site-to-Site VPN**
Connexion VPN IPsec entre on-premises et VPC. Virtual Private Gateway (VGW côté AWS), Customer Gateway (CGW côté on-premises). 2 tunnels (HA), BGP routing. Throughput 1.25 Gbps par tunnel. Alternative: Direct Connect (dédié).

### **AWS Transit Gateway**
Hub réseau centralisé connectant VPCs, on-premises (VPN, Direct Connect). Routage transitive (VPC-to-VPC via TGW). Attachments: VPC, VPN, Direct Connect, peering TGW. Route tables, multicast support. Simplifie architectures hub-and-spoke.

### **Amazon VPC (Virtual Private Cloud)**
Réseau privé isolé dans AWS. Subnets (public/private), route tables, Internet Gateway (IGW), NAT Gateway/Instance. Security Groups (stateful instance-level), NACLs (stateless subnet-level). VPC Peering, VPC Endpoints, Flow Logs (monitoring trafic).

---

## 🔒 **SECURITY, IDENTITY, AND COMPLIANCE**

### **AWS Certificate Manager (ACM)**
Gestion certificats SSL/TLS gratuits. Provisioning, déploiement, renouvellement automatique. Intégration: CloudFront, ALB, API Gateway. Certificats publics (validation DNS/email) ou privés (CA interne). Wildcard support.

### **AWS CloudHSM**
HSM (Hardware Security Module) dédié FIPS 140-2 Level 3. Contrôle complet clés cryptographiques, vous gérez users/keys. Clusters HA Multi-AZ. Intégration KMS (custom key store), SSL/TLS offload, Oracle TDE. Conformité stricte.

### **Amazon Cognito**
Service authentification/autorisation users applications. **User Pools** (sign-up, sign-in, MFA, password reset), **Identity Pools** (credentials AWS temporaires via STS). Intégration: SAML, OIDC, social providers (Google, Facebook). JWT tokens.

### **Amazon Detective**
Investigation sécurité, analyse root cause incidents. Collecte automatique VPC Flow Logs, CloudTrail, GuardDuty findings. Visualisations graphiques relations entités. Timeline événements. ML détection patterns anormaux. Complément GuardDuty/Security Hub.

### **AWS Directory Service**
Active Directory managé AWS. Options: **Managed Microsoft AD** (AD natif dans VPC), **AD Connector** (proxy vers on-premises AD), **Simple AD** (Samba-based, small workloads). Intégration SSO, WorkSpaces, RDS. Trust relationships.

### **Amazon GuardDuty**
Détection menaces intelligente ML/threat intelligence. Analyse: CloudTrail events, VPC Flow Logs, DNS logs, EKS audit logs, S3 data events. Findings par severity. Intégration EventBridge (automation remédiations). Threat lists custom, suppression rules.

### **AWS IAM Identity Center (ex AWS SSO)**
Single Sign-On centralisé multi-comptes AWS, SaaS apps. User portal unique. Identity source: Identity Center directory, Active Directory, external IdP (Okta, Azure AD). Permission sets (policies par compte). MFA support. Remplace console switching accounts.

### **AWS IAM (Identity and Access Management)**
Service authentification/autorisation AWS. Users, groups, roles, policies JSON. Permissions: Allow/Deny, Explicit Deny wins. Condition keys. Principe moindre privilège. MFA, password policies. Access Analyzer (permissions non-utilisées), Credentials report.

### **Amazon Inspector**
Scanning automatisé vulnérabilités workloads. Cibles: EC2, containers (ECR), Lambda. CVEs, network exposure, CIS benchmarks. Agent SSM (EC2), agentless (ECR, Lambda). Findings par severity, risk score. Rescanning continu. Intégration Security Hub.

### **AWS KMS (Key Management Service)**
Gestion clés chiffrement, create/rotate/disable/audit. KMS keys (symmetric AES-256, asymmetric RSA/ECC). Intégration 100+ services AWS (encryption at-rest). Envelope encryption. CloudHSM backend (custom key stores). Grants, key policies, audit CloudTrail.

### **Amazon Macie**
Découverte/protection données sensibles S3 via ML. Détection: PII, financial data, credentials. Automated discovery jobs, findings par severity. Intégration EventBridge, Security Hub. Compliance (GDPR, PCI-DSS). Sensitive data inventory.

### **AWS Network Firewall**
Firewall stateful managé pour VPCs. Filtering: IPs, domains, protocols, stateful (traffic flows). IPS/IDS (Suricata rules). Logging (flow, alert). Centralized multi-VPC (via Transit Gateway). Alternative: 3rd-party appliances, Security Groups/NACLs.

### **AWS RAM (Resource Access Manager)**
Partage ressources AWS entre comptes/Organizations. Ressources: VPC subnets, Transit Gateway, Route 53 Resolver, License Manager. Pas de duplication, ownership centralisé. Réduction coûts, simplification architecture. Permissions IAM partagées.

### **AWS Secrets Manager**
Stockage sécurisé secrets (passwords, API keys, DB credentials). Rotation automatique (Lambda custom ou native RDS/Redshift/DocumentDB). Encryption KMS, audit CloudTrail. Versioning, staging labels. Facturation par secret + API calls. Alternative: SSM Parameter Store (gratuit).

### **AWS Security Hub**
Dashboard centralisé sécurité multi-comptes/régions. Agrégation findings: GuardDuty, Inspector, Macie, Firewall Manager, IAM Access Analyzer, Systems Manager. Standards: CIS Benchmarks, PCI-DSS, AWS Foundational Best Practices. Automated response (EventBridge).

### **AWS STS (Security Token Service)**
Service credentials temporaires IAM. AssumeRole (cross-account, federated), GetSessionToken (MFA), GetFederationToken. Expiration 15min-12h (roles), 36h (session tokens). Web identity federation (Cognito, OIDC). Reduced IAM proliferation, enhanced security.

### **AWS Shield**
Protection DDoS couches 3-7. **Standard** (gratuit, tous clients, SYN floods, UDP reflection). **Advanced** ($3000/mois, protection sophistiquée, DDoS Response Team 24/7, cost protection, integration WAF). CloudFront, Route 53, ALB/NLB protections.

### **AWS WAF (Web Application Firewall)**
Protection apps web layer 7. Rules: IP sets, geo-matching, rate limiting, SQL injection, XSS, size constraints, regex patterns. Managed rules (AWS, Marketplace). Intégration: CloudFront, ALB, API Gateway, AppSync. Logging S3/Kinesis Firehose.

---

## ⚡ **SERVERLESS**

### **AWS Lambda**
Compute serverless event-driven. Langages: Python, Node, Java, Go, C#, Ruby, custom runtimes. Timeout max 15 min, RAM 128MB-10GB, /tmp 512MB-10GB. Concurrency (reserved, provisioned). Triggers: S3, DynamoDB, Kinesis, EventBridge, API Gateway, ALB. Facturation 100ms.

### **AWS SAM (Serverless Application Model)**
Framework IaC serverless (extension CloudFormation). Template SAM simplifié: `AWS::Serverless::Function`, `AWS::Serverless::Api`, `AWS::Serverless::SimpleTable`. CLI: `sam init`, `sam build`, `sam deploy`, `sam local start-api` (test local). CodeDeploy intégré (canary).

### **Amazon SNS (Simple Notification Service)**
Pub/Sub messaging. Topics, subscriptions (SQS, Lambda, email, SMS, HTTP, Kinesis Firehose, mobile push). Message filtering, FIFO topics (ordering), delivery status logging. Retry policies, dead-letter queues. Fan-out pattern. Facturation $0.50/million publishes.

### **Amazon SQS (Simple Queue Service)**
File messages managée. Types: **Standard** (throughput illimité, at-least-once, best-effort ordering), **FIFO** (exactly-once, strict ordering, 300 TPS batch/3000 msg/s). Visibility timeout, dead-letter queues, long polling, delay queues. Retention 4 jours (max 14).

### **AWS Step Functions**
Orchestration workflows serverless. State machines (JSON ASL: Amazon States Language). States: Task, Choice, Parallel, Wait, Map, Pass, Succeed, Fail. Intégrations: Lambda, ECS, Batch, DynamoDB, SNS, SQS, Glue, SageMaker. Express (streaming) vs Standard (long-running).

---

## 💾 **STORAGE**

### **AWS Backup**
Service backup centralisé multi-services. Supported: EC2, EBS, RDS, Aurora, DynamoDB, EFS, FSx, Storage Gateway, DocumentDB. Backup plans (schedules, retention, lifecycle), Backup vaults (encryption KMS), cross-region/cross-account copy. Compliance reporting.

### **Amazon EBS (Elastic Block Store)**
Volumes bloc attachés EC2. Types: **gp3** (general SSD, 3000 IOPS baseline), **io2** (high-perf SSD, 64K IOPS, Multi-Attach), **st1** (throughput HDD, big data), **sc1** (cold HDD, archives). Snapshots incrémentaux S3, encryption KMS. Elastic Volumes (resize online).

### **AWS Elastic Disaster Recovery (CloudEndure)**
Service DR (Disaster Recovery) continu. Réplication block-level on-premises/cloud vers AWS. RTO minutes, RPO secondes. Automated failover, failback. Licensing inclus, facturation par serveur répliqué. Alternative: Pilot Light, Warm Standby, Multi-Site manual.

### **Amazon EFS (Elastic File System)**
Système fichiers NFS managé, shared storage multi-instances. Storage classes: Standard, Infrequent Access (IA). Lifecycle management (transition IA). Performance modes: General Purpose, Max I/O. Throughput modes: Bursting, Provisioned, Elastic. Encryption at-rest/in-transit.

### **Amazon FSx for Lustre**
Système fichiers haute performance HPC/ML. Intégration S3 (data repository). Déploiement: Scratch (temporaire, pas de réplication) ou Persistent (HA, réplication). Throughput 100s GB/s, millions IOPS. Idéal: genomics, seismic processing, video rendering.

### **Amazon FSx for NetApp ONTAP**
Système fichiers NetApp ONTAP managé AWS. Protocols: NFS, SMB, iSCSI. Features NetApp: snapshots, SnapMirror (réplication), FlexClone, deduplication, compression. Multi-AZ HA. Idéal migrations NetApp vers cloud.

### **Amazon FSx for OpenZFS**
Système fichiers OpenZFS managé. Protocol NFS v3/v4/v4.1/v4.2. Features: snapshots instantanés, cloning, compression (LZ4, zstd). Performance 1M IOPS, latency sub-milliseconde. Idéal workloads nécessitant OpenZFS features (media processing, databases).

### **Amazon FSx for Windows File Server**
Système fichiers Windows natif managé. Protocol SMB, intégration Active Directory. Features: DFS Namespaces, shadow copies, quotas. Multi-AZ HA, backups automatiques. SSD/HDD storage. Idéal migrations Windows file servers, .NET apps.

### **Amazon S3 (Simple Storage Service)**
Stockage objet illimité. Classes: **S3 Standard** (fréquent), **S3-IA** (infrequent access), **S3 One Zone-IA** (1 AZ), **Glacier Instant/Flexible/Deep Archive** (archives). Lifecycle policies, versioning, replication (CRR/SRR), encryption (SSE-S3, SSE-KMS, SSE-C). Event notifications.

### **Amazon S3 Glacier**
Stockage archives long-terme ultra-low-cost. Classes: **Glacier Instant Retrieval** (ms access, $4/TB/mois), **Glacier Flexible Retrieval** (mins-hours, $4/TB/mois), **Glacier Deep Archive** (12h retrieval, $1/TB/mois). Vault lock (compliance), lifecycle S3→Glacier.

### **AWS Storage Gateway**
Service hybrid cloud storage, intégration on-premises avec S3/EBS/Glacier. Types: **File Gateway** (NFS/SMB vers S3), **Volume Gateway** (iSCSI vers EBS, cached/stored modes), **Tape Gateway** (VTL backup vers Glacier). Cache local, async upload AWS.

---

## 📋 **Index Alphabétique**

**A**: ACM, API Gateway, App Runner, App2Container, AppFlow, Athena, Aurora, Auto Scaling, AWS Backup  
**B**: -  
**C**: CDK, Certificate Manager, Client VPN, CloudFormation, CloudFront, CloudHSM, CloudShell, CloudTrail, CloudWatch, CodeArtifact, CodeBuild, CodeDeploy, CodeGuru, CodePipeline, CodeStar, Cognito, Compute Optimizer, Config, Control Tower, Copilot  
**D**: Detective, Directory Service, DMS, DocumentDB, DynamoDB  
**E**: EBS, EC2, ECR, ECS, EFS, EKS, Elastic Beanstalk, Elastic Disaster Recovery, ElastiCache, ELB, EMR, EventBridge  
**F**: Fargate, FIS, FSx (Lustre, NetApp ONTAP, OpenZFS, Windows)  
**G**: GuardDuty  
**H**: Health  
**I**: IAM, IAM Identity Center, Inspector  
**K**: Kinesis (Data Firehose, Data Streams), KMS  
**L**: Lambda, License Manager  
**M**: Macie, Managed Grafana, Managed Service for Prometheus, MemoryDB  
**N**: Network Firewall  
**O**: OpenSearch Service, OpsWorks, Organizations  
**P**: PrivateLink, Proton  
**Q**: QuickSight  
**R**: RAM, RDS, Redshift, Resilience Hub, Route 53, ROSA  
**S**: S3, S3 Glacier, SAM, Secrets Manager, Security Hub, Service Catalog, Shield, SNS, SQS, STS, Step Functions, Storage Gateway, Systems Manager  
**T**: Transit Gateway, Trusted Advisor  
**V**: VPC  
**W**: WAF  
**X**: X-Ray

---

## 🎯 **Conseils d'utilisation pour la certification**

### **Priorités d'apprentissage**
1. **Core Services** (90% questions): EC2, Lambda, ECS, S3, RDS, VPC, IAM, CloudFormation, CodePipeline, CloudWatch
2. **Security** (20% exam): IAM, KMS, Secrets Manager, GuardDuty, WAF, Shield
3. **CI/CD** (22% exam): CodeCommit, CodeBuild, CodeDeploy, CodePipeline, Blue-Green/Canary
4. **Monitoring** (16% exam): CloudWatch, X-Ray, CloudTrail, Config

### **Services moins prioritaires** (bon à connaître)
- App2Container, Copilot, CodeStar, OpsWorks, Proton (nouveaux/moins adoptés)
- ROSA, EKS Distro (niches)
- AppFlow (intégration SaaS spécifique)

### **Comparaisons clés à maîtriser**
- **Compute**: EC2 vs Lambda vs Fargate vs Batch
- **Storage**: S3 vs EBS vs EFS vs FSx
- **Database**: RDS vs Aurora vs DynamoDB vs Redshift
- **Deployment**: Blue-Green vs Canary vs Rolling
- **IaC**: CloudFormation vs CDK vs Terraform (bien que Terraform hors scope officiel)
- **Monitoring**: CloudWatch vs X-Ray vs CloudTrail

### **Patterns architecturaux essentiels**
- Microservices avec ECS/EKS + ALB + DynamoDB
- Serverless avec Lambda + API Gateway + DynamoDB
- CI/CD avec CodePipeline + CodeBuild + CodeDeploy
- Monitoring avec CloudWatch + X-Ray + EventBridge
- Security avec IAM + KMS + Secrets Manager + GuardDuty

---

**Dernière mise à jour**: Novembre 2025  
**Version**: 2.0 - Certification AWS DevOps Engineer Professional
