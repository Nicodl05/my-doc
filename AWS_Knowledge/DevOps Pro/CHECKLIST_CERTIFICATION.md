# ✅ **CHECKLIST CERTIFICATION AWS DevOps Pro**

## 🎯 **Guide d'utilisation**
Cette checklist couvre **tous les domaines d'examen**. Pour chaque concept :
- ☐ = À revoir
- ✓ = Je comprends
- ✅ = Je peux l'expliquer et l'utiliser

---

## 🏗️ **DOMAIN 1: DEPLOYMENT, PROVISIONING, AND ORCHESTRATION (22%)**

### **Infrastructure as Code (IaC)**
- ☐ CloudFormation basics (templates, stacks, resources)
- ☐ CloudFormation parameters, outputs, metadata
- ☐ CloudFormation intrinsic functions (Ref, GetAtt, Join, Sub, ImportValue)
- ☐ CloudFormation drift detection & resolution
- ☐ CDK basics (Constructs L1, L2, L3)
- ☐ CDK Stacks & App concept
- ☐ Terraform vs CloudFormation trade-offs
- ☐ JSON vs YAML CloudFormation templates

### **Deployment Strategies**
- ☐ Blue-Green deployment architecture & AWS implementation
- ☐ Canary deployment & progressive traffic shift
- ☐ Rolling deployment & batch updates
- ☐ CodeDeploy AppSpec file configuration
- ☐ Rollback strategies & automatic rollback triggers
- ☐ Deployment lifecycle hooks (BeforeInstall, AfterInstall, etc.)
- ☐ In-place vs rolling vs blue-green in CodeDeploy
- ☐ Traffic control during deployment

### **Orchestration & Automation**
- ☐ CodePipeline stage management & actions
- ☐ Pipeline triggering (CodeCommit, CloudWatch Events, webhooks)
- ☐ CodeDeploy deployment groups & targets
- ☐ Multi-region deployments
- ☐ Step Functions for workflow orchestration
- ☐ EventBridge rules & event routing
- ☐ CloudFormation StackSets for multi-account/multi-region
- ☐ Systems Manager automation documents

### **🚨 Common Pitfalls**
- ❌ **Piège** : CloudFormation stack deletion without backup
  - ✅ **Solution** : Export important outputs, use DeletionPolicy=Retain
- ❌ **Piège** : Hardcoded values in CloudFormation templates
  - ✅ **Solution** : Use parameters + cross-stack references
- ❌ **Piège** : Blue-Green without validating GREEN environment first
  - ✅ **Solution** : Run smoke tests, check logs before traffic switch
- ❌ **Piège** : Rollback failures due to database schema changes
  - ✅ **Solution** : Make DB changes backward-compatible, test rollback

---

## 🛠️ **DOMAIN 2: CONFIGURATION MANAGEMENT AND IaC (17%)**

### **Configuration Management**
- ☐ Systems Manager Session Manager (connect to instances)
- ☐ Systems Manager Run Command (execute commands at scale)
- ☐ Systems Manager State Manager (maintain consistent configurations)
- ☐ Systems Manager Patch Manager (automated patching)
- ☐ Systems Manager OpsCenter & Explorer
- ☐ AWS Config rules & compliance checking
- ☐ AWS Config aggregator (multi-account/multi-region)
- ☐ Configuration drift remediation

### **Infrastructure Concepts**
- ☐ VPC architecture & CIDR planning
- ☐ Subnets (public vs private) & routing
- ☐ Security Groups (stateful) vs NACLs (stateless)
- ☐ Internet Gateway, NAT Gateway, NAT Instances
- ☐ VPC Endpoints (Gateway vs Interface)
- ☐ VPC Peering & Transit Gateway
- ☐ Auto Scaling Groups (launch templates, lifecycle)
- ☐ Load Balancers (ALB, NLB, CLB) & target groups

### **Container & Orchestration**
- ☐ ECS task definitions & container properties
- ☐ ECS services & task placement strategies
- ☐ Fargate vs EC2 launch types
- ☐ ECS Cluster architecture
- ☐ ECR (registry, repositories, image scanning)
- ☐ EKS basics (nodes, pods, cluster management)
- ☐ Docker basics (images, containers, registries)

### **🚨 Common Pitfalls**
- ❌ **Piège** : VPC subnets overlap with on-premises network
  - ✅ **Solution** : Plan CIDR carefully, use non-overlapping ranges
- ❌ **Piège** : Security Group allowing unrestricted access (0.0.0.0/0)
  - ✅ **Solution** : Use least privilege, restrict by specific IPs/SGs
- ❌ **Piège** : Auto Scaling Group rolling update with broken AMI
  - ✅ **Solution** : Test AMI, set min healthy percentage correctly
- ❌ **Piège** : Container memory limit too low causes OOMKill
  - ✅ **Solution** : Monitor memory usage, set appropriate limits

---

## 📊 **DOMAIN 3: RESILIENCE AND HIGH AVAILABILITY (15%)**

### **High Availability Architecture**
- ☐ Multi-AZ deployments & failover concepts
- ☐ Elastic Load Balancer failover & health checks
- ☐ Auto Scaling Group recovery & replace unhealthy instances
- ☐ RDS Multi-AZ with automatic failover
- ☐ Aurora Global Database & read replicas
- ☐ DynamoDB cross-region replication
- ☐ S3 cross-region replication (CRR)

### **Disaster Recovery**
- ☐ RTO (Recovery Time Objective) vs RPO (Recovery Point Objective)
- ☐ Backup strategies (point-in-time recovery, snapshots)
- ☐ EBS snapshots & AMI creation from snapshots
- ☐ RDS automated backups & manual snapshots
- ☐ DynamoDB backups & point-in-time recovery
- ☐ S3 versioning & cross-region replication (CRR)
- ☐ AWS Backup service centralized backup
- ☐ Route 53 health checks & failover routing

### **Resilience Patterns**
- ☐ Circuit Breaker pattern implementation
- ☐ Retry logic with exponential backoff
- ☐ Bulkhead pattern (isolation)
- ☐ Timeout handling
- ☐ Queue-based load leveling (SQS)
- ☐ Graceful degradation

### **Monitoring & Alerting**
- ☐ CloudWatch metrics & custom metrics
- ☐ CloudWatch alarms & SNS notifications
- ☐ CloudWatch Logs Insights & Log Groups
- ☐ Application Performance Monitoring (X-Ray)
- ☐ CloudTrail for audit & compliance
- ☐ VPC Flow Logs for network troubleshooting
- ☐ AWS Health Dashboard & Personal Health Dashboard

### **🚨 Common Pitfalls**
- ❌ **Piège** : RTO/RPO too aggressive for budget
  - ✅ **Solution** : Define realistic objectives based on business impact
- ❌ **Piège** : Single AZ deployment (appears as Multi-AZ but isn't)
  - ✅ **Solution** : Verify resource deployment across AZs
- ❌ **Piège** : Health check not configured properly on load balancer
  - ✅ **Solution** : Test health check path, adjust threshold/interval
- ❌ **Piège** : Database backup disabled to save costs
  - ✅ **Solution** : Automate backups, use backup retention policies

---

## 🔒 **DOMAIN 4: SECURITY AND COMPLIANCE (20%)**

### **Access Control & IAM**
- ☐ IAM Users, Groups, Roles, Policies
- ☐ IAM Policy conditions & wildcards
- ☐ Cross-account IAM roles & assume role
- ☐ IAM credential rotation
- ☐ MFA & hardware MFA keys
- ☐ Service roles (EC2, Lambda, RDS)
- ☐ Resource-based policies vs identity-based policies
- ☐ IAM permission boundaries

### **Secrets Management**
- ☐ Secrets Manager vs Parameter Store
- ☐ Automatic secret rotation
- ☐ Multi-region secrets
- ☐ Encryption with KMS
- ☐ Database credential management
- ☐ API key rotation

### **Encryption**
- ☐ Encryption at rest (S3, EBS, RDS, DynamoDB)
- ☐ Encryption in transit (TLS/SSL, HTTPS)
- ☐ KMS key management & rotation
- ☐ AWS CloudHSM for FIPS compliance
- ☐ S3 encryption options (SSE-S3, SSE-KMS, SSE-C)
- ☐ EBS encryption & snapshots

### **Network Security**
- ☐ VPC security groups & NACLs
- ☐ WAF rules & AWS Managed Rules
- ☐ Shield Standard vs Shield Advanced
- ☐ VPC Flow Logs & analysis
- ☐ Private Link & VPC Endpoints
- ☐ AWS Network Firewall

### **Threat Detection & Compliance**
- ☐ GuardDuty threat detection
- ☐ Inspector vulnerability scanning
- ☐ Macie for data discovery
- ☐ Security Hub for findings aggregation
- ☐ Config Rules for compliance checking
- ☐ CloudTrail for audit logging
- ☐ AWS Artifact & compliance documents

### **🚨 Common Pitfalls**
- ❌ **Piège** : Storing secrets in environment variables/code
  - ✅ **Solution** : Use Secrets Manager or Parameter Store
- ❌ **Piège** : Using root account for daily operations
  - ✅ **Solution** : Create IAM users with least privilege roles
- ❌ **Piège** : Overly permissive IAM policies (*:*)
  - ✅ **Solution** : Start restrictive, add permissions as needed
- ❌ **Piège** : Not rotating database credentials regularly
  - ✅ **Solution** : Use Secrets Manager auto-rotation every 30 days
- ❌ **Piège** : S3 bucket with public read access accidentally
  - ✅ **Solution** : Enable S3 Block Public Access, use bucket policies

---

## 📈 **DOMAIN 5: MONITORING, LOGGING, AND REMEDIATION (16%)**

### **Monitoring & Observability**
- ☐ CloudWatch namespaces, metrics, dimensions
- ☐ Custom metrics from applications
- ☐ CloudWatch Dashboard creation & widgets
- ☐ CloudWatch Alarms (Simple, Composite, Anomaly detection)
- ☐ SNS for alarm notifications
- ☐ EventBridge for event-driven monitoring
- ☐ Application Insights (automatic problem detection)
- ☐ Synthetic monitoring (CloudWatch Synthetics)

### **Logging**
- ☐ CloudWatch Logs (Log Groups, Streams, Filters)
- ☐ CloudWatch Logs Insights (queries)
- ☐ VPC Flow Logs setup & analysis
- ☐ ALB/NLB access logs
- ☐ CloudTrail for API logging
- ☐ Application logging best practices
- ☐ Log retention policies
- ☐ Centralized logging architecture

### **Distributed Tracing**
- ☐ X-Ray service map visualization
- ☐ X-Ray segments & subsegments
- ☐ X-Ray sampling & filtering
- ☐ X-Ray integration with Lambda, EC2, ECS
- ☐ X-Ray performance bottleneck identification
- ☐ Error tracking with X-Ray
- ☐ Service mapping with X-Ray

### **Remediation & Automation**
- ☐ CloudWatch alarms triggering Lambda
- ☐ EventBridge rules for auto-remediation
- ☐ Systems Manager automation for remediation
- ☐ Auto-scaling based on metrics
- ☐ Automatic instance recovery
- ☐ Step Functions for complex remediation workflows
- ☐ SNS for notifications & escalation

### **🚨 Common Pitfalls**
- ❌ **Piège** : Too many CloudWatch alarms causing alert fatigue
  - ✅ **Solution** : Baseline metrics, tune thresholds, use composite alarms
- ❌ **Piège** : Logs not retained long enough for investigation
  - ✅ **Solution** : Set appropriate retention (default 30 days → increase)
- ❌ **Piège** : X-Ray sampling too low, missing errors
  - ✅ **Solution** : Increase sample rate for critical services, 100% for errors
- ❌ **Piège** : Metrics alarm action misses edge case
  - ✅ **Solution** : Test alarms manually, simulate failure scenarios

---

## 💰 **DOMAIN 6: POLICIES AND STANDARDS AUTOMATION (10%)**

### **Compliance & Governance**
- ☐ AWS Config Rules for compliance automation
- ☐ Conformance Packs (packaged Config Rules)
- ☐ AWS Config aggregator (multi-account/multi-region)
- ☐ CloudFormation drift detection
- ☐ Service Quotas monitoring
- ☐ Trusted Advisor checks
- ☐ AWS Compliance Center & compliance documents

### **Cost Management**
- ☐ Cost Allocation Tags strategy
- ☐ AWS Budgets & cost forecasting
- ☐ Cost Explorer analysis & visualization
- ☐ Right-sizing recommendations
- ☐ Reserved Instances vs Savings Plans vs On-Demand
- ☐ Spot Instances for variable workloads
- ☐ AWS Compute Optimizer recommendations
- ☐ Lambda cost optimization

### **Automation Standards**
- ☐ Infrastructure as Code standards
- ☐ Naming conventions & tagging strategy
- ☐ Automated testing of infrastructure
- ☐ CI/CD pipeline standards
- ☐ Configuration management policies
- ☐ Security baseline enforcement
- ☐ Backup & disaster recovery standards

### **🚨 Common Pitfalls**
- ❌ **Piège** : No cost allocation tags, can't track expenses
  - ✅ **Solution** : Implement comprehensive tagging strategy
- ❌ **Piège** : Reserved Instances purchased but not utilized
  - ✅ **Solution** : Analyze usage before purchasing, use Savings Plans flexibility
- ❌ **Piège** : Config Rules too restrictive, blocking necessary changes
  - ✅ **Solution** : Balance automation with operational flexibility

---

## 🎯 **CERTIFICATION FOCUS AREAS**

### **Must Know (80% of exam)**
```
✓ IAM fundamentals (users, roles, policies)
✓ EC2 & Auto Scaling Groups
✓ Deployment strategies (Blue-Green, Canary)
✓ CodePipeline & CodeDeploy
✓ VPC networking & security
✓ RDS & DynamoDB
✓ S3 & storage
✓ CloudWatch monitoring
✓ High availability & disaster recovery
✓ CloudFormation & IaC
```

### **Should Know (15% of exam)**
```
✓ Lambda & serverless
✓ ECS & container orchestration
✓ KMS & encryption
✓ GuardDuty & threat detection
✓ X-Ray tracing
✓ Step Functions
✓ Kinesis & streaming
✓ SNS/SQS messaging
```

### **Nice to Know (5% of exam)**
```
✓ CDK advanced patterns
✓ Advanced networking (Transit Gateway, PrivateLink)
✓ EMR & big data
✓ Advanced IAM conditions
✓ Custom Config Rules
```

---

## 📋 **PRE-EXAM CHECKLIST (7 days before)**

### **Day 1-2: Review Domains 1 & 2**
- ☐ CloudFormation deep dive
- ☐ CodePipeline & CodeDeploy complete review
- ☐ VPC networking scenarios
- ☐ Practice 10 exam questions

### **Day 3-4: Review Domains 3 & 4**
- ☐ Multi-AZ & failover scenarios
- ☐ Backup & recovery strategies
- ☐ IAM policy evaluation
- ☐ Encryption scenarios
- ☐ Practice 10 exam questions

### **Day 5-6: Review Domains 5 & 6**
- ☐ CloudWatch & X-Ray monitoring
- ☐ Remediation automation
- ☐ Cost optimization
- ☐ Compliance & governance
- ☐ Practice 10 exam questions

### **Day 7: Final Review**
- ☐ Review weak areas
- ☐ Take full practice exam
- ☐ Review mistakes
- ☐ Get good sleep!

---

## 🏁 **EXAM DAY TIPS**

1. **Read questions carefully** - AWS loves trick questions with subtle wording
2. **Eliminate wrong answers** - Often 2 answers are clearly wrong
3. **Watch for "best practice"** - Not just what works, but what AWS recommends
4. **Time management** - 130 questions in 180 minutes = 1.4 min per question
5. **Flag & review** - Mark difficult questions, come back if time
6. **Trust your knowledge** - Don't overthink simple questions
7. **Scenario-based** - Focus on context (what's the constraint?)

---

## 📚 **RESOURCES FOR FINAL REVIEW**

- **AWS Documentation** : https://docs.aws.amazon.com/
- **AWS Whitepapers** : Well-Architected Framework, Security Pillar
- **AWS Developer Tools** : CodePipeline, CodeDeploy documentation
- **Practice Tests** : A Cloud Guru, Linux Academy, Examtopics
- **YouTube** : Stephane Maarek, Jon Bonso, ExamPro

---

**🎯 You're ready! Trust your preparation and go crush this exam! 💪**
