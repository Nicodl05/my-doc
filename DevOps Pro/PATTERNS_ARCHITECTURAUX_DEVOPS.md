# 🏗️ **PATTERNS ARCHITECTURAUX DevOps AWS**

## 🎯 **Vue d'ensemble**
Les patterns architecturaux sont des solutions éprouvées pour problèmes communs. Cette section couvre **10 patterns essentiels** pour l'exam DevOps Pro avec schémas visuels et explications courtes.

---

## 1️⃣ **BLUE-GREEN DEPLOYMENT**

### 📊 **Schéma**
```
Trafic Production (100%)
        ↓
    Route 53 / ALB
        ↓
    ┌─────────────────┐
    │  BLUE (V1)      │  ← Actuellement actif
    │  Prod OK        │
    └─────────────────┘
    
DÉPLOIEMENT :
    ┌─────────────────┐
    │  GREEN (V2)     │  ← Nouveau déploiement
    │  Tests OK       │
    └─────────────────┘
    
SWITCH :
    Route 53 → GREEN (100% trafic)
    
ROLLBACK (si problème) :
    Route 53 → BLUE (instant)
```

### **Caractéristiques**
- ✅ **Zéro downtime** : Switch instantané
- ✅ **Rollback facile** : Switch retour en secondes
- ✅ **Test complet** : Valider GREEN avant switch
- ❌ **Coûteux** : 2x ressources running
- ❌ **Complexe** : Gestion 2 envs identiques

### **Quand utiliser**
- Application critique (e-commerce, banking)
- Changements importants (migration DB structure, API breaking)
- Team DevOps mature

### **Implémentation AWS**
- ALB + 2 Target Groups (Blue / Green)
- CodeDeploy avec blueGreenDeploymentConfiguration
- Route 53 weighted routing ou ALB listener rules
- Auto Scaling Groups pour chaque env

### **AWS Services**
- **ALB/NLB** : Router trafic
- **Route 53** : DNS failover
- **CodeDeploy** : Orchestrer déploiement
- **Auto Scaling** : Scale out/in selon besoin

---

## 2️⃣ **CANARY DEPLOYMENT**

### 📊 **Schéma**
```
Temps →
├─ T0 : V1 = 100%, V2 = 0%
│
├─ T1 : V1 = 95%, V2 = 5% (Canaries)
│  Monitorer : error rate, latency, business metrics
│  ✓ Si OK → continue
│  ✗ Si NOK → rollback V2
│
├─ T2 : V1 = 75%, V2 = 25%
│  Monitorer metrics
│
├─ T3 : V1 = 50%, V2 = 50% (50-50)
│  Double validation
│
├─ T4 : V1 = 0%, V2 = 100% (Complet switch)
└─ DONE : V2 = production, V1 = backup
```

### **Caractéristiques**
- ✅ **Risque très faible** : Testable sous charge réelle
- ✅ **Rollback rapide** : Peu d'utilisateurs affectés
- ✅ **Monitoring intégré** : Catch bugs tôt
- ⚠️ **Complexité moderée** : 2 versions running mais traffic shift progressif
- ⚠️ **Temps déploiement** : 30 min - 1h complet switch

### **Quand utiliser**
- Changements importants mais pas critical
- Product avec fort trafic (détecter bugs souvent)
- API changes avec backward compatibility
- ML models (tester accuracy en production)

### **Implémentation AWS**
- **ALB** + weighted target groups (5%, 25%, 50%, 100%)
- **CodeDeploy** avec AllAtOnce + manual validation
- **CloudWatch** alarms pour metrics (errors, latency)
- **Lambda** ou **CodeBuild** pour traffic shift automation

### **AWS Services**
- **ALB/NLB** : Traffic shifting
- **CloudWatch** : Metrics & alarms
- **Lambda** : Shift logic
- **CodeDeploy** : Orchestration
- **SNS/Email** : Notifications validation

---

## 3️⃣ **ROLLING DEPLOYMENT**

### 📊 **Schéma**
```
Batch size = 2 instances

Initial : [V1] [V1] [V1] [V1]  (4 instances)
          
Step 1 :  [V2] [V2] [V1] [V1]  (2 → V2, 2 running)
          
Step 2 :  [V2] [V2] [V2] [V2]  (all → V2)
          
If error at Step 1 : Rollback [V1] [V1] [V1] [V1]
```

### **Caractéristiques**
- ✅ **Zéro downtime** : Always running instances
- ✅ **Ressources fixes** : Pas de 2x coût comme Blue-Green
- ✅ **Simple** : Configuration directe CodeDeploy
- ❌ **Lent** : Plusieurs batches = 10-30 min
- ❌ **Rollback lent** : Même process inverse
- ⚠️ **Validation lente** : Attendre fin avant savoir si OK

### **Quand utiliser**
- Changements mineurs (config, patches)
- Haute disponibilité critique
- Budget limité (pas 2x ressources)
- Downtime toléré 1-2 instances

### **Implémentation AWS**
- Auto Scaling Groups + rolling update
- CodeDeploy OneAtATime ou HalfAtATime
- Healthchecks pour validation
- Min/desired/max capacity management

### **AWS Services**
- **Auto Scaling Groups**
- **CodeDeploy**
- **CloudWatch** (healthchecks)
- **Elastic Load Balancing** (connection draining)

---

## 4️⃣ **SHADOW DEPLOYMENT (Dark Launch)**

### 📊 **Schéma**
```
User Request
    ↓
    ├─→ Production (V1) → Response à User ✓
    │
    └─→ V2 (Shadow) → Log Response (non utilisée) 📝
    
Comparer :
    Response V1 vs Response V2 (offline)
    Error rate V2, Performance V2
    
Si OK → Promouvoir V2 à production
Si NOK → Fix V2, retry
```

### **Caractéristiques**
- ✅ **Zéro risque** : V2 pas utilisée, utilisateurs insensibles
- ✅ **Test production-like** : Vraie charge, vraies données
- ✅ **Flexible** : V2 peut être algo/schema différent
- ❌ **Coûteux** : 2x compute resource
- ❌ **Non détectable** : Certains bugs (state mutation) invisibles
- ⏱️ **Long** : Doit courir parallèle longtemps

### **Quand utiliser**
- Changements algorithme critique (ML, pricing)
- Migration DB schema (tester performance)
- Réfactoring internals (API identique)
- Zéro tolérance pour bugs utilisateur-visible

### **Implémentation AWS**
- Application fork requêtes → V2 en background
- Lambda async invoke pour V2
- SQS pour queuer V2 processing
- Logs V1 vs V2 → CloudWatch → Athena analysis
- Comparaison offline (batch job)

### **AWS Services**
- **Lambda** (invoke async)
- **SQS** (queue)
- **CloudWatch Logs** (capture)
- **S3** (store responses)
- **Athena** (analyze)

---

## 5️⃣ **A/B TESTING DEPLOYMENT**

### 📊 **Schéma**
```
100% Trafic
    ↓
┌─────────────┐
│ Route Logic │
│ (Random %)  │
└─────────────┘
    ↓
    ├─→ 50% → V1 (Contrôle)
    │
    └─→ 50% → V2 (Variant)
    
Mesurer :
    Conversion rate V1 vs V2
    Engagement V1 vs V2
    Revenue V1 vs V2
    
Statistique décision : Quel variant gagne ?
```

### **Caractéristiques**
- ✅ **Mesurable** : A/B test statistically significant
- ✅ **Business-driven** : Basé business metrics, pas technique
- ✅ **Non-risqué** : 50-50 split = faible impact pire scénario
- ❌ **Pas réellement déploiement** : C'est un test utilisateur
- ❌ **Complexe** : Nécessite analytics sophistiquée
- ⏱️ **Long** : Semaines pour stat significance

### **Quand utiliser**
- Décisions product (UI, features)
- Pricing/promo testing
- User experience experiments
- Marketing campaigns

### **Implémentation AWS**
- ALB weighted target groups (50-50)
- User ID → consistent variant (session affinity)
- CloudWatch custom metrics (conversions)
- QuickSight dashboard pour monitoring
- Lambda calcul statistique significance

### **AWS Services**
- **ALB** : Traffic split
- **CloudWatch** : Metrics custom
- **QuickSight** : Dashboard metrics
- **Lambda** : Calcul statistique
- **DynamoDB** : User variant mapping

---

## 6️⃣ **N-TIER ARCHITECTURE**

### 📊 **Schéma**
```
Users / Internet
    ↓
┌─────────────────────────────────────┐
│        PRESENTATION TIER             │
│  CloudFront + S3 (Static)            │
│  CloudFront + ALB (Dynamic HTML)     │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│         APPLICATION TIER             │
│  ALB (Load Balancer)                 │
│  Auto Scaling Group (EC2/ECS)        │
│  API endpoints                       │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│          DATA TIER                   │
│  RDS (SQL) / DynamoDB (NoSQL)        │
│  ElastiCache (Cache)                 │
│  S3 (Object storage)                 │
└─────────────────────────────────────┘
```

### **Caractéristiques**
- ✅ **Scalable** : Chaque tier scale indépendamment
- ✅ **Maintenance facile** : Separation of concerns
- ✅ **Flexible** : Changer tech stack per tier
- ✓ **Sécurité** : Multi-layer defense
- ❌ **Complexe** : Plus de services à gérer
- ❌ **Latency** : Hops entre tiers = latency

### **Quand utiliser**
- Applications enterprise (grande équipe)
- Haute disponibilité requis
- Scalability prévisible (E-commerce, SaaS)
- Multi-tenant applications

### **Implémentation AWS**
- Tier 1 : CloudFront + S3/ALB
- Tier 2 : ALB + ASG (EC2 ou ECS)
- Tier 3 : RDS + ElastiCache + S3
- Networking : VPC, subnets, security groups per tier
- Monitoring : CloudWatch per tier

### **AWS Services**
- **CloudFront** : CDN
- **ALB/NLB** : Load balancing
- **Auto Scaling Groups**
- **RDS/DynamoDB/ElastiCache**
- **VPC Security Groups / NACLs**

---

## 7️⃣ **MICROSERVICES ARCHITECTURE**

### 📊 **Schéma**
```
API Gateway
    ↓
    ├→ User Service (ECS)      ↔ User DB
    ├→ Order Service (ECS)     ↔ Order DB
    ├→ Payment Service (Lambda) → Payment API
    ├→ Notification Service (SQS/SNS)
    └→ Analytics Service (Kinesis → Athena)
    
Communication :
    Sync : REST/gRPC (API Gateway)
    Async : SQS/SNS/EventBridge
```

### **Caractéristiques**
- ✅ **Independent scaling** : Service peut scale alone
- ✅ **Independent deployment** : Service déploie indépendante
- ✅ **Tech flexibility** : Service peut être Python ou Go
- ✅ **Fault isolation** : Service crash ≠ cascade failure
- ❌ **Complexe** : Distributed tracing, debugging difficile
- ❌ **Operational overhead** : Many services = many configs
- ❌ **Network latency** : Hops entre services

### **Quand utiliser**
- Large teams (chaque team = service)
- Évolution indépendante services
- Scale selective (certain service plus load)
- Polyglotte tech teams

### **Implémentation AWS**
- Service = ECS task ou Lambda function
- API Gateway pour facade
- SQS/SNS/EventBridge pour async
- DynamoDB per service (data isolation)
- Service discovery : ECS service discovery
- Monitoring : CloudWatch + X-Ray tracing

### **AWS Services**
- **API Gateway**
- **ECS/Lambda** (per service)
- **SQS/SNS/EventBridge**
- **DynamoDB** (per service)
- **X-Ray** (tracing)
- **CloudWatch** (logs)

---

## 8️⃣ **SERVERLESS ARCHITECTURE**

### 📊 **Schéma**
```
Event Source
    ↓
┌─────────────────────────┐
│  API Gateway            │  ← REST endpoint
├─────────────────────────┤
│  Lambda                 │  ← Compute
├─────────────────────────┤
│  DynamoDB               │  ← Data
│  S3 / SQS / Kinesis     │  ← Storage / Messaging
└─────────────────────────┘

Zero servers to manage = AWS gère scaling/HA
```

### **Caractéristiques**
- ✅ **No ops** : AWS gère servers, scaling, patching
- ✅ **Pay per execution** : $$$ si pas traffic
- ✅ **Auto-scaling** : Instant scaling 0 → 1000 requests
- ✅ **Simple deployment** : Upload code, done
- ❌ **Max timeout** : Lambda = 15 min max
- ❌ **Cold starts** : Premier appel = latency +1-2 sec
- ❌ **Vendor lock-in** : AWS specific

### **Quand utiliser**
- Low-traffic apps (startup, internal tools)
- Event-driven workloads (IoT, webhooks)
- Batch processing (data transformation)
- Microservices (déjà distributed)
- Burstable traffic (analytics, reports)

### **Implémentation AWS**
- API Gateway → Lambda → DynamoDB
- S3 → Lambda (file processing)
- EventBridge → Lambda (scheduled)
- SQS → Lambda (async)
- Cognito → Lambda (auth)

### **AWS Services**
- **Lambda**
- **API Gateway**
- **DynamoDB**
- **S3**
- **EventBridge/CloudWatch Events**
- **Cognito** (auth)
- **X-Ray** (tracing)

---

## 9️⃣ **EVENT-DRIVEN ARCHITECTURE**

### 📊 **Schéma**
```
Event Producer              Event Consumer
(Source)                    (Handler)

Order Created               ├→ Email Service
    ↓                       ├→ Analytics
[EventBridge]               ├→ Warehouse
    ↓                       ├→ Notification
Order Processing Started
    ↓
[EventBridge]
    ↓
Multiple Subscribers
(Asynchronous execution)
```

### **Caractéristiques**
- ✅ **Loosely coupled** : Producers ≠ know consumers
- ✅ **Scalable** : Add consumers sans changer producer
- ✅ **Resilient** : Consumer down ≠ affect producer
- ✅ **Asynchronous** : Non-blocking, faster response
- ⚠️ **Complex debugging** : Trace execution flow
- ⚠️ **Eventual consistency** : État ≠ immédiatement consistent

### **Quand utiliser**
- Multi-step processes (order → payment → shipping)
- Notifications (email, SMS, push)
- Data replication (DB → analytics)
- User activity tracking
- Microservices communication

### **Implémentation AWS**
- EventBridge = central event bus
- Pattern matching pour routing
- SNS/SQS pour delivery
- Lambda pour processing
- DynamoDB streams pour CDC (Change Data Capture)
- Kinesis pour high-throughput events

### **AWS Services**
- **EventBridge** : Event bus
- **SNS/SQS** : Messaging
- **Lambda** : Processing
- **DynamoDB Streams** : CDC
- **Kinesis** : Streaming
- **CloudWatch Events** : Scheduled events

---

## 🔟 **CIRCUIT BREAKER PATTERN**

### 📊 **Schéma**
```
Client → Service A → (AppLogic) → Service B

Scénario normal :
    ✓ Closed state : Traffic flows
    ✓ Service B répond OK

Scénario panne :
    ✗ Service B fail / timeout
    ✗ Open state : Block requests (fast fail)
    ✗ Return cached/default response

Recovery :
    ⏳ Half-open state : Try 1 request
    ✓ If OK → Closed (resume)
    ✗ If NOK → Open (continue blocking)
```

### **Caractéristiques**
- ✅ **Fail fast** : Don't hammer failing service
- ✅ **Graceful degradation** : Continue with default/cache
- ✅ **Recovery automatic** : Detect when service back
- ✅ **Cascading failure prevention** : Stop propagation
- ⚠️ **Complexity** : More logic in application
- ⚠️ **Consistency** : Cached data may be stale

### **Quand utiliser**
- External API calls (3rd party, fragile)
- Microservices (isolate failure)
- High-availability requirement
- Must handle partial failures gracefully

### **Implémentation AWS**
- Lambda + decorator (boto3 retry)
- Application code logic
- DynamoDB/ElastiCache para caching response
- CloudWatch metrics pour state transitions
- SNS alerts on state change

### **AWS Services**
- **Lambda** (implement logic)
- **DynamoDB/ElastiCache** (caching)
- **CloudWatch** (monitoring)
- **SNS** (alerts)
- **API Gateway** (front layer)

---

## 📋 **RÉSUMÉ & DECISION TREE**

### **Quel pattern choisir ?**

| Question | Réponse | Pattern |
|----------|---------|---------|
| **Zéro downtime requis ?** | Oui | Blue-Green ou Rolling |
| **Déploiement fréquent** | Oui | Canary ou Rolling |
| **Test prod-like** | Oui | Shadow deployment |
| **Budget limité** | Oui | Rolling ou Canary |
| **Business metrics test** | Oui | A/B testing |
| **Architecture complexe** | Oui | N-Tier ou Microservices |
| **No ops / Serverless** | Oui | Serverless |
| **Multi-step workflow** | Oui | Event-driven |
| **External APIs fragile** | Oui | Circuit Breaker |

---

**Prêt ? → Voir [DECISION_MATRIX.md](DECISION_MATRIX_DEVOPS.md) pour comparaisons services**
