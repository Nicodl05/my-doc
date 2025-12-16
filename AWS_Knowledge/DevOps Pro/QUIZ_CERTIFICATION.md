# 🎓 **QUIZ PRATIQUES - AWS DevOps Pro**

## 🎯 **Format & Instructions**
**65 questions répartis sur 6 domaines d'examen.** Chaque question inclut :
- Question
- 4 réponses possibles
- Réponse correcte avec justification détaillée
- Pièges courants

---

## **DOMAIN 1: DEPLOYMENT, PROVISIONING & ORCHESTRATION**

### **Q1. Blue-Green Deployment**
Vous déployez une application critique e-commerce. Vous devez assurer zéro downtime et pouvoir revenir en arrière en secondes en cas de problème. Quelle stratégie de déploiement choisir ?

A) Rolling deployment  
B) Canary deployment  
C) **Blue-Green deployment** ✅  
D) All-at-once deployment

**Justification** : Blue-Green crée 2 environnements identiques. Vous déployez dans GREEN, testez complètement, puis switchez le trafic (0.5 sec avec ALB). Rollback = switch retour en 0.5 sec. Parfait pour e-commerce critique.
- Rolling = lent à rollback, déploiement progressif
- Canary = testage progressif mais risqué dès départ
- All-at-once = downtime inacceptable

**Piège** : Penser que Blue-Green économise les resources (non! 2x coûteux).

---

### **Q2. CodePipeline Multi-Stage**
Votre pipeline CodePipeline a 4 stages : Source → Build → Test → Deploy. Le stage Build échoue souvent. Comment déboguer efficacement ?

A) Consulter les logs CloudWatch du pipeline  
B) **Consulter les logs du CodeBuild job** ✅  
C) Consulter les logs CloudTrail  
D) Vérifier le fichier buildspec.yml

**Justification** : CodeBuild génère les logs détaillés dans CloudWatch Logs (namespace `/aws/codebuild/project-name`). CloudTrail = API calls seulement, pas les logs du build. buildspec.yml = config fichier (statique), pas l'exécution.

**Correct** : Logs CodeBuild + buildspec.yml pour comprendre.

---

### **Q3. CloudFormation Stack Deletion Policy**
Vous avez une CloudFormation stack avec base de données RDS. Vous supprimez la stack par erreur. Comment prévenir la suppression accidentelle à l'avenir ?

A) Ajouter DeletionPolicy=Retain au RDS resource  
B) Ajouter une policy IAM interdisant DeleteStack  
C) **Ajouter DeletionPolicy=Retain + Stack deletion protection** ✅  
D) Utiliser CloudFormation change sets

**Justification** :
- `DeletionPolicy=Retain` = garde la ressource RDS après suppression stack
- Stack deletion protection = empêche DELETE entièrement
- Combiner les deux = sécurité maximale

**Piège** : Penser que DeletionPolicy suffit (non, il y a aussi DeleteStack action).

---

### **Q4. CodeDeploy Deployment Groups**
Vous avez 100 instances EC2 en production. Comment déployer en garantissant min 80 instances saines pendant le déploiement ?

A) Deployment group avec MinimumHealthyHosts = 80  
B) **Deployment group avec MinimumHealthyHosts = (type: FLEET_PERCENT, value: 80)** ✅  
C) Utiliser Auto Scaling Group UpdatePolicy  
D) Utiliser Route 53 health checks

**Justification** : MinimumHealthyHosts garantit le nombre d'instances saines. FLEET_PERCENT = pourcentage (80% = 80 instances saines min).

**Piège** : Utiliser COUNT au lieu de FLEET_PERCENT pour scalabilité future.

---

### **Q5. EventBridge Rule Routing**
Vous avez des ordres de clients. OrderCreated events → Email Service ET Analytics Service. Vous utilisez EventBridge. Quelle approche ?

A) 1 topic SNS avec 2 subscribers  
B) 1 EventBridge rule avec 2 targets  
C) **1 EventBridge rule avec pattern matching, 1 target SNS with 2 subscribers** ✅  
D) 2 EventBridge rules, 1 par service

**Justification** : EventBridge = routing intelligent (pattern matching). SNS = distribution (push). Combiner = meilleur design.
- Option B marche mais moins flexible
- Option D = redondant

---

### **Q6. Multi-Region Deployment**
Vous devez déployer infrastructure identique en us-east-1 et eu-west-1. Meilleure approche ?

A) Créer 2 CloudFormation stacks manuellement  
B) **CloudFormation StackSets avec Target Accounts/Regions** ✅  
C) Utiliser CDK avec multi-region L3 constructs  
D) Script shell qui déploie 2 fois

**Justification** : StackSets = déployer même template sur multiple comptes/régions. Automation, versioning, gestion centralisée.

**Piège** : CDK marche aussi mais moins automatisé que StackSets.

---

### **Q7. Rollback Strategy**
Déploiement canary en cours : 5% trafic → v2. Soudain, erreurs augmentent. Comment rollback rapidement ?

A) Redéployer v1 avec CodeDeploy  
B) **ALB weighted target group : 5% → 0% pour v2** ✅  
C) Arrêter instances v2  
D) Route 53 failover

**Justification** : ALB = rollback en sec (juste changer weights). Redéployer = min + long. Arrêter instances = compliqué (ASG peut relancer). Route 53 = trop lent.

**Piège** : Penser que CodeDeploy rollback est plus rapide (non, recréer instances).

---

## **DOMAIN 2: CONFIGURATION MANAGEMENT & IaC (17%)**

### **Q8. VPC CIDR Planning**
Vous avez VPC avec CIDR 10.0.0.0/16. Vous voulez 4 subnets pour 4 AZ, chaque subnet 250 adresses IP. Architecture idéale ?

A) 4x /24 subnets (256 IPs each) = 10.0.0.0/24, 10.0.1.0/24, 10.0.2.0/24, 10.0.3.0/24  
B) **2x public /25 (128 IPs), 2x private /25** ✅  
C) 1x /24 per AZ, 1x /25 reserved  
D) 4x /23 subnets (512 IPs each)

**Justification** : 10.0.0.0/16 = 65536 IPs total.
- 4x /24 = ok mais peu d'overhead
- 2x public /25 + 2x private /25 = optimisé, flexible ✓
- /23 = trop grand (512 IPs gaspillé)

**Piège** : AWS réserve 5 IPs par subnet (network, gateway, broadcast, 2 AWS) = vrai /24 = 251 utilisables.

---

### **Q9. Security Group vs NACL**
Votre application reçoit trop de traffic DDoS sur port 80. Quelle solution ?

A) Security Group règle deny all inbound port 80  
B) **NACL règle deny inbound port 80 (stateless)** ✅  
C) IAM policy deny port 80  
D) Route 53 block

**Justification** :
- Security Group = instance level, mais STATEFUL (reply auto)
- NACL = subnet level, STATELESS (deny reply aussi)
- DDoS = NACL better car peuvent contrôler in+out explicitement
- IAM = permissions, pas traffic

**Piège** : Confondre SG stateful vs NACL stateless.

---

### **Q10. Auto Scaling Group Rolling Update**
ASG déploie v2 via launch template update. Vous configurez MinSize=5, DesiredCapacity=5, MaxSize=10. Combien d'instances max pendant rolling update avec batch 2 ?

A) 5 (toujours 5)  
B) 7 (5 + 2 batch)  
C) **10 (max capacity)** ✅  
D) 12 (max + batch)

**Justification** : ASG rolling update = créer nouvelles instances (v2) AVANT détruire anciennes (v1). Donc :
- T0 : 5x v1
- T1 : 5x v1 + 2x v2 = 7 (exceed desired)
- Jusqu'à MaxSize=10 possible

**Piège** : Penser que DesiredCapacity = limite (non, MaxSize = limite).

---

### **Q11. ECS Task Definition**
Vous déployez microservice Node.js sur ECS Fargate. Task doit avoir 512 MB memory. Quelle CPU assigner ?

A) 128 CPU units  
B) **256 CPU units** ✅  
C) 512 CPU units  
D) 1024 CPU units

**Justification** : Fargate CPU/memory combinations :
- 512 MB = supports 256 CPU units (min)
- 1 GB = 256-512 CPU
- 2 GB = 256-1024 CPU

**Piège** : Penser que CPU = MB (non, units différentes).

---

### **Q12. AWS Config Rule**
Vous voulez auditer que toutes les instances EC2 ont tag "Environment". Utiliser AWS Config Rule "required-tags" avec parameters. Quelle clé parameter ?

A) requiredTagKeys  
B) **tagKey** ✅  
C) requiredTags  
D) tag_keys

**Justification** : AWS managed rule "required-tags" utilise parameter `tagKey` (pas tagKeys plural).

**Piège** : Naming AWS Config parameters tricky, consulter documentation.

---

## **DOMAIN 3: RESILIENCE & HIGH AVAILABILITY**

### **Q13. RDS Multi-AZ Failover**
RDS instance primary en us-east-1a. Standby en us-east-1b (Multi-AZ). Primary crash. Quand failover terminé, connection string application CHANGE ?

A) Oui, new IP  
B) **Non, DNS alias reste identique** ✅  
C) Oui, endpoint AWS change  
D) Dépend du parameter group

**Justification** : RDS Multi-AZ utilise DNS alias. Failover = DNS pointe vers standby (CNAME change), mais application connection string identique.

**Piège** : Penser que failover = nouvelle endpoint (non, DNS transparente).

---

### **Q14. RTO vs RPO**
Vous avez critère : max 4 heures downtime acceptable, max 1 heure données perdues. Quelle stratégie backup ?

A) RTO = 4h, RPO = 1h → hourly snapshots  
B) **RTO = 4h, RPO = 1h → Backup toutes heures + standby region** ✅  
C) RTO = 1h, RPO = 4h → inverse  
D) Daily backup seulement

**Justification** :
- RTO (Recovery Time Objective) = combien temps pour restored? → 4h max → need standby
- RPO (Recovery Point Objective) = combien données perdus? → 1h max → backup every hour
- Combo = backup hourly + have standby (for RTO)

**Piège** : Confondre RTO/RPO.

---

### **Q15. S3 Cross-Region Replication**
Vous activez S3 CRR (Cross-Region Replication) vers eu-west-1. Objects existants en us-east-1 replicating automatiquement ?

A) Oui, tous objects replicate  
B) **Non, seulement futurs objects** ✅  
C) Oui après 24 heures  
D) Dépend de object size

**Justification** : S3 CRR = replicate seulement objets mis en place APRÈS activation CRR. Pour objets existants, utiliser batch replication.

**Piège** : Penser que CRR couvre historique (non!).

---

### **Q16. Elastic Load Balancer Health Checks**
ALB health check interval = 30 sec, timeout = 5 sec, healthy threshold = 2. Instance unhealthy. Combien de time avant deregistration ?

A) 5 sec  
B) 30 sec  
C) **60 sec (30 × 2)** ✅  
D) 95 sec

**Justification** : 
- Health check every 30 sec
- Healthy threshold = 2 successful checks required
- 2 failed checks → deregistration
- (2 failures) × (30 sec interval) = 60 sec max

---

### **Q17. Route 53 Health Checks**
Vous configurez Route 53 failover routing. Primary = eu-west-1, Secondary = us-east-1. Primary health check failed. Failover time expected ?

A) <1 sec  
B) **30 sec** ✅  
C) 5 min  
D) 15 min

**Justification** : Route 53 health check interval = 30 sec par défaut. Failover = quand health check détecte unhealthy.

**Piège** : Penser que failover instantané (non, 30 sec delay).

---

## **DOMAIN 4: SECURITY & COMPLIANCE**

### **Q18. IAM Policy Evaluation**
User a policy:
```json
{
  "Effect": "Allow",
  "Action": "s3:*",
  "Resource": "*"
}
```

Et policy:
```json
{
  "Effect": "Deny",
  "Action": "s3:GetObject",
  "Resource": "arn:aws:s3:::secure-bucket/*"
}
```

User peut GetObject de secure-bucket ?

A) Oui  
B) **Non (Deny wins)** ✅  
C) Oui si dans IP allowlist  
D) Dépend du bucket policy

**Justification** : IAM Deny ALWAYS wins vs Allow. Explicit deny > explicit allow.

**Piège** : Oublier que Deny est prioritaire.

---

### **Q19. Cross-Account IAM Role**
Account A user veut accéder resources Account B. Quelle approche ?

A) Create IAM user dans Account B avec credentials  
B) **Account A user assume cross-account role dans Account B** ✅  
C) Account A IAM user add à Account B IAM group  
D) Share AWS credentials

**Justification** : Assume role = meilleure pratique sécurité. No credentials sharing.

**Piège** : Penser que Assume role = seulement pour federation (non, cross-account aussi).

---

### **Q20. S3 Encryption Options**
S3 object doit être chiffré. Client veut gérer les clés. Quelle option ?

A) SSE-S3  
B) SSE-KMS  
C) **SSE-C (Server-Side Encryption with Customer-provided keys)** ✅  
D) Client-side encryption only

**Justification** :
- SSE-S3 = AWS manages keys
- SSE-KMS = AWS KMS manages keys (AWS still manages master)
- SSE-C = **Client provides key** → vrai customer-managed
- Client-side = avant S3 upload (separate from S3 encryption)

---

### **Q21. Secrets Manager vs Parameter Store**
Quel service pour database password avec auto-rotation toutes les 30 jours ?

A) **Secrets Manager** ✅  
B) Parameter Store  
C) Systems Manager Automation  
D) CloudFormation DynamicReferences

**Justification** :
- Secrets Manager = auto-rotation built-in
- Parameter Store = no built-in rotation
- Others = not for secrets storage

---

### **Q22. WAF Rule Order**
WAF a 2 rules :
1. Allow all requests
2. Block requests from IP 1.2.3.4

Requête from 1.2.3.4 result ?

A) **Blocked (rule order matters, first match wins if no explicit deny)** ✅  
B) Allowed  
C) Depends on priority setting  
D) Logged only

**Justification** : WAF évalue rules dans order. Premier match wins. Rule 1 (Allow all) = first match → ALLOWED. Pour bloquer, reverser order ou utiliser "block" rule BEFORE "allow all".

**Piège** : Penser que règles combinées (non, first match).

---

## **DOMAIN 5: MONITORING, LOGGING & REMEDIATION**

### **Q23. CloudWatch Custom Metric**
Application publie custom metric "OrderProcessingTime" toutes les minutes. Créer alarm si metric > 5 sec pendant 5 minutes. Quelle config ?

A) Period=60, EvaluationPeriods=1, Threshold=5000 (ms)  
B) **Period=60, EvaluationPeriods=5, Threshold=5000 (ms)** ✅  
C) Period=300, EvaluationPeriods=5, Threshold=5  
D) Period=60, EvaluationPeriods=1, Threshold=5 (seconds)

**Justification** :
- Period = 60 sec (collecté chaque minute)
- EvaluationPeriods = 5 (evaluer 5 periods = 5 min)
- Threshold = 5000 ms (CloudWatch = ms for custom metrics)

---

### **Q24. VPC Flow Logs**
VPC Flow Logs shows :
```
srcip=10.0.1.10 dstip=10.0.2.20 dstport=443 action=REJECT
```

Application peut pas connect. Cause possible ?

A) NACL deny outbound 443  
B) **Possible NACL deny outbound ou Security Group deny outbound** ✅  
C) Security Group allow 443 (contradiction)  
D) VPC peering issue

**Justification** : REJECT = network layer (NACL). Connection refused = application. Besoin debugger :
1. NACL rules (outbound 443)
2. Security Group rules (outbound 443)
3. Route tables

---

### **Q25. X-Ray Service Map**
X-Ray trace montre: App → API Gateway → Lambda → RDS. Lambda → RDS latency = 2 sec (vs avg 100ms). Cause ?

A) Lambda timeout  
B) **Possible RDS query slow (check RDS Enhanced Monitoring)** ✅  
C) Network latency (unlikely same AZ)  
D) X-Ray sampling missed events

**Justification** : X-Ray shows service map + latencies per leg. 2 sec sur RDS = query slow. Check RDS logs, query performance.

---

### **Q26. CloudWatch Alarm Action**
Alarm triggered → auto-remediation. Lambda executed OK mais instances not recovered. Quelle reason ?

A) Lambda not enough permissions  
B) **Lambda wrong VPC configuration (cannot access EC2 API endpoint)** ✅  
C) IAM role missing  
D) Alarm SNS topic misconfigured

**Justification** : Lambda dalam VPC = no access to public AWS APIs sauf via VPC endpoint. Lambda outside VPC = public API access.

---

### **Q27. CloudWatch Logs Insights Query**
Query tous errors last 1 hour de Lambda logs :
```
fields @timestamp, @message
| filter @message like /ERROR/
| stats count() as error_count
```

Quelle time window ?

A) `| filter @timestamp > ago(1h)`  
B) Console auto-select 1h when query runs  
C) **Utiliser console query time picker (indépendant de la query)** ✅  
D) Add `| filter @timestamp > now() - 3600`

**Justification** : CloudWatch Logs Insights time window = console picker (dropdown), pas dans query. Query = relatif à selected period.

---

## **DOMAIN 6: POLICIES, STANDARDS & AUTOMATION (10%)**

### **Q28. AWS Config Conformance Packs**
Vous voulez appliquer 5 Config Rules standards (required-tags, encrypted-volumes, etc.). Meilleure approche ?

A) Créer 5 individual Config Rules  
B) **Déployer Conformance Pack (packaged Config Rules)** ✅  
C) Utiliser CloudFormation custom resource  
D) AWS Systems Manager automation

**Justification** : Conformance Packs = pre-built, reusable collections de Config Rules. Simplifier.

---

### **Q29. Cost Allocation Tags**
Vous utilisez Cost Allocation Tags : Environment=prod, CostCenter=1234. Combien de temps avant tags visible dans Cost Explorer ?

A) Immédiat  
B) **24 heures après activation** ✅  
C) 1 semaine  
D) 1 mois

**Justification** : Cost Allocation Tags = activate via console, puis wait 24h avant visible in Cost Explorer/Billing.

---

### **Q30. Service Quotas**
Application crashes : "LimitExceededException - Lambda concurrent execution limit exceeded". Action ?

A) Utiliser AWS Trusted Advisor  
B) **Utiliser AWS Service Quotas pour increase limit** ✅  
C) Upgrade Lambda tier  
D) Créer autre Lambda account

**Justification** : Service Quotas = request limit increase (Lambda concurrent = default 1000). Request via console/API.

---

## **SCENARIO-BASED ADVANCED QUESTIONS**

### **Q31. Complete Deployment Scenario**
Scenarii : 
- E-commerce app (Peak: 10K requests/sec)
- Multi-AZ requirement
- Blue-Green deployment
- Must handle 10 min deployment window
- Rollback < 1 min
- Budget conscious

Meilleure architecture ?

A) EC2 + ELB + CodeDeploy rolling  
B) **EC2 + ALB + CodeDeploy Blue-Green + ASG** ✅  
C) Lambda (peak exceeds capacity)  
D) RDS + Fargate + CodePipeline

**Justification** :
- Lambda = max 1000 concurrent, 10K requests/sec = too much
- EC2 + ALB = proven for e-commerce ✓
- Blue-Green = 10 min deploy + <1 min rollback ✓
- ASG = Multi-AZ auto-scaling ✓
- CodeDeploy = integrate Blue-Green ✓

---

### **Q32. Disaster Recovery Architecture**
Critères :
- RPO = 1 hour
- RTO = 4 hours
- Budget limited
- Data in us-east-1

Meilleure solution ?

A) Active-passive with hourly snapshots + standby region  
B) **Hourly RDS backups to S3 + script restore (4h RTO acceptable)** ✅  
C) Multi-region active-active  
D) Daily backups only

**Justification** :
- RPO = 1 hour → hourly backups ✓
- RTO = 4 hours → script restore acceptable
- Budget = avoids active-active
- us-east-1 → S3 → restore to standby region

---

### **Q33. Multi-Account Security**
Vous avez 10 comptes AWS (dev, staging, prod × 3, analytics, shared services, etc.). Besoin de :
- Centralized logging (CloudTrail)
- Centralized IAM (identity federation)
- Cost tracking par account
- Automated compliance checking

Meilleure architecture ?

A) Manually manage IAM + CloudTrail per account  
B) **AWS Organizations + CloudTrail to S3 + IAM Identity Center + Config Aggregator** ✅  
C) Cross-account IAM roles + manual setup  
D) Separate AWS accounts (no orchestration)

**Justification** :
- Organizations = multi-account management ✓
- CloudTrail = centralized logging ✓
- Identity Center = federation (Okta, AD, etc.) ✓
- Config Aggregator = compliance check all accounts ✓

---

### **Q34. Microservices Deployment**
Vous avez 5 microservices (Node, Python, Go). Chacun deploy indépendamment, multi-AZ, auto-scaling. Infrastructure ?

A) 5 separate EC2 instances per service  
B) Docker + ECS + CodePipeline  
C) **Docker + ECS + ALB + separate CodePipeline per service** ✅  
D) EC2 + load balancer + manual deployment

**Justification** :
- Docker = versioning, reproducibility ✓
- ECS = orchestration ✓
- ALB = routing per service ✓
- Separate CodePipeline = independent deployment ✓
- Auto-scaling = per service ✓

---

### **Q35. Real-Time Monitoring Scenario**
Vous avez API critical. Besoin de :
- Real-time alertes si errors spike
- Corréler errors avec Lambda latency
- Auto-remediation si Lambda unhealthy
- Dashboard for on-call

Solution ?

A) CloudWatch Metrics + Alarms only  
B) **CloudWatch Logs + Logs Insights + X-Ray tracing + EventBridge automation** ✅  
C) Manual log review  
D) Third-party monitoring tool

**Justification** :
- CloudWatch Logs Insights = query errors real-time ✓
- X-Ray = trace latency sources ✓
- EventBridge = trigger Lambda auto-remediation ✓
- Cloudwatch Dashboard = visualization ✓

---

## 🎯 **ANSWER SUMMARY**

```
Domain 1 (Deployment): Q1-Q7 = 7 questions
Domain 2 (Configuration): Q8-Q12 = 5 questions
Domain 3 (Resilience): Q13-Q17 = 5 questions
Domain 4 (Security): Q18-Q22 = 5 questions
Domain 5 (Monitoring): Q23-Q27 = 5 questions
Domain 6 (Policies): Q28-Q30 = 3 questions
Advanced Scenarios: Q31-Q35 = 5 questions

Total: 35 questions
```

---

## 📊 **SCORING GUIDE**

- **32-35** : Ready for exam! 🟢
- **28-31** : Good, review weak areas 🟡
- **24-27** : Need more review 🟠
- **<24** : Focus on domains with low score 🔴

---

**🚀 You've got this! Go crush that certification! 💪**
