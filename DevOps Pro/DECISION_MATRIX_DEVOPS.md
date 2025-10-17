# 📊 **DECISION MATRIX - Quand Choisir Quel Service AWS**

## 🎯 **Guide d'utilisation**
Cette matrice aide à **choisir le service AWS optimal** selon des critères objectifs et mesurables :
- **💰 Cost-Effective** = meilleur ratio prix/performance
- **⚡ Least Effort** = configuration minimale, managed service
- **🎮 Full Control** = customization maximale
- **📦 Zero Ops** = aucune gestion d'infrastructure

---

## 1️⃣ **COMPUTE - EC2 vs Lambda vs Fargate vs Batch**

### **Comparaison détaillée**

| Critère | **EC2** | **Lambda** | **Fargate** | **Batch** |
|---------|---------|-----------|------------|----------|
| **Startup time** | 2-5 min | 50-200ms (cold start) | 15-30 sec | 2-5 min |
| **Max runtime** | Illimité | 15 min stricte | Illimité | Illimité |
| **Min cost/mois** | $10-50 (t3.micro) | $0 (free tier) | $0.01-0.05/hour | $0 (just pay time) |
| **Cost model** | Per-hour (instance running) | Per 100ms + GB-sec (invocation) | Per task/second | Per core-hour |
| **Management overhead** | ⭐⭐⭐⭐ (OS patching, security) | ⭐ (fully managed) | ⭐⭐ (container images) | ⭐⭐ (Docker + config) |
| **Scaling automation** | ASG (minutes de lag) | Auto (<1 sec) | ECS auto-scaling (10-30 sec) | Auto (minutes) |
| **Memory flexibility** | Fix per instance type | 128MB-10GB | 0.5GB-120GB | Flexible per job |
| **Networking** | Full VPC control | Simplified (Lambda execution role) | Full VPC control | VPC + IAM role |
| **Ideal for** | Long-running always-on | Event-driven, variable traffic | Containerized but stateless | Data processing batch |

### **Cost Scenarios (monthly estimate)**

#### **Scenario A: Web API 1000 req/day, 100ms per request**
- **EC2**: t3.small = $20/mo (24/7) + network + storage
- **Lambda**: 1000 × 365 × 100ms = 36.5M GB-sec = ~$1.50/mo ✅ **Cost-Effective**
- **Fargate**: $0.0423/hour × 24 × 30 = $30/mo (always running)
- **Winner**: λ Lambda (99% cheaper than 24/7 EC2)

#### **Scenario B: 24/7 API Server, consistent 1000 req/sec**
- **EC2**: c5.xlarge = $150/mo ✅ **Cost-Effective**
- **Lambda**: 1000 × 86400 × 100ms = 8.64B GB-sec = $350/mo (too expensive!)
- **Fargate**: $0.0423 × 1 vCPU × 730h = $30/mo (4 tasks) ✅
- **Winner**: EC2 (predictable load, lower cost with Reserved Instances -40%)

#### **Scenario C: Docker containerized app, variable traffic (0-100 tasks)**
- **EC2**: ECS cluster + 10 instances = $150-300/mo
- **Lambda**: Not applicable (no Docker full control)
- **Fargate**: Pay-per-task = $30-300/mo depending on scale ✅ **Zero-Ops**
- **Winner**: Fargate (no cluster management)

#### **Scenario D: Batch job - process 1TB data, 30 min processing**
- **EC2**: c5.18xlarge × 1 = $3/hour × 0.5 = $1.50
- **Lambda**: Not possible (15 min timeout!)
- **Fargate**: $0.0423 × 2 vCPU × 0.5h = $0.042 ✅ **Cost-Effective**
- **Batch**: Spot instances = $0.30-0.50 ✅ **Most Cheap** (but needs setup)
- **Winner**: Batch with Spot (automatic)

### **Decision Tree**

```
Q1: Exécution < 15 minutes ?
├─ NON → EC2 ou Fargate
│  ├─ Q2: Container Docker ?
│  │  ├─ OUI → Fargate (Zero-Ops ✅)
│  │  └─ NON → EC2 (Full Control)
│  │
└─ OUI → Lambda ou Batch ?
   ├─ Q3: Invocations = variable/rare ?
   │  ├─ OUI → Lambda (Cost-Effective ✅)
   │  └─ NON → Batch si parallel, Lambda si simple
```

### **Recommendation Matrix**

| Cas d'usage | **Meilleur choix** | **Coût/mois** | **Setup** | **Gestion** |
|-------------|-------------------|---------------|----------|-----------|
| Startup MVP, traffic variable | Lambda | $0-10 | 10 min | ⭐ Zero-Ops |
| E-commerce 24/7, 1K req/sec | EC2 + ASG | $100-300 | 30 min | ⭐⭐⭐⭐ (ops) |
| Microservice Docker variable | Fargate + ALB | $50-200 | 30 min | ⭐⭐ (image mgmt) |
| Data pipeline batch (2h) | Batch + Spot | $2-20 | 20 min | ⭐⭐ (config) |
| ML training distributed | EC2 GPU + ASG | $500-2000/mo | 45 min | ⭐⭐⭐⭐ (scaling) |

---

## 2️⃣ **STORAGE - S3 vs EBS vs EFS vs FSx**

### **Comparaison détaillée par cas d'usage**

| Critère | **S3** | **EBS** | **EFS** | **FSx (Linux)** | **FSx (Windows)** |
|---------|--------|--------|--------|-----------------|------------------|
| **Type données** | Objects (files) | Block (disk-like) | NFS files | POSIX files + HPC | SMB/CIFS (Windows) |
| **Capacity** | Illimité | 16 TB max | Illimité | 100s TB | 100s TB |
| **Access pattern** | Sequential/random | Random | Random/sequential | Low-latency workloads | Windows apps |
| **Multi-instance access** | Oui (API) | Non (1 instance) | Oui (multiple) | Oui (multiple) | Oui (Windows) |
| **Cost per GB/mois** | $0.023 | $0.10-0.20 | $0.30 | $1.50+ | $2.00+ |
| **Throughput** | 100s MB/s | 250-16000 MB/s | 500MB/s | Up to 3.1 GB/s | Up to 2 GB/s |
| **Latency** | Milliseconds | < 1ms | < 1ms | <1ms | <1ms |
| **Durability** | 99.999999999% | Snapshot-based | Redundant (Multi-AZ) | Replicated | Replicated |
| **Management overhead** | ⭐ (zero) | ⭐⭐⭐ (snapshots, monitoring) | ⭐⭐ (Auto-scaling) | ⭐⭐⭐ (maintenance) | ⭐⭐⭐⭐ (Windows mgmt) |
| **Backup complexity** | Easy (versioning, CRR) | Snapshot-based (manual) | Point-in-time restore | AWS Backup integration | AWS Backup integration |

### **Cost Scenarios (monthly for 1TB)**

#### **Scenario A: Data lake (archive, infrequent access)**
- **S3 Standard**: 1TB = $23/mo + requests (~$1) = ~$24/mo ✅ **Cost-Effective**
- **S3 Glacier**: 1TB = $4/mo (for archives) ✅✅ **Most Cheap**
- **EBS**: 1TB = $100-200/mo (continuous cost!)
- **EFS**: 1TB = $300/mo + bursting
- **Winner**: S3 Glacier (archive), S3 Standard (active)

#### **Scenario B: EC2 database root volume (1TB)**
- **EBS gp3**: 1TB = $100/mo + snapshots = $110/mo ✅ **Necessary**
- **EFS**: Not suitable (block storage needed)
- **S3**: Not suitable (EC2 needs local volume)
- **Winner**: EBS (only option for EC2 storage)

#### **Scenario C: Shared NFS for 10 instances, 2TB dataset**
- **EBS**: Not possible (1 instance only)
- **EFS**: 2TB = $600/mo + standard bursting ✅ **Multi-instance**
- **S3**: API-based sharing = application complexity
- **FSx (Linux)**: 2TB = $3000+/mo (overkill for this)
- **Winner**: EFS (simplest for multi-instance NFS)

#### **Scenario D: HPC scratch space, 100TB, 10K IOPS**
- **EBS**: Cost prohibitive + single-instance limit
- **EFS**: High cost + bursting limitations
- **FSx for Lustre**: 100TB = $150k/mo (specialized HPC) ✅ **Industry Standard**
- **Winner**: FSx for Lustre (only viable for HPC)

### **Decision Tree**

```
Q1: Type de données ?
├─ Objects/files (unstructured)
│  └─ S3 (✅ Cost-Effective)
│     ├─ Access frequent ? → Standard
│     └─ Archive ? → Glacier
│
├─ Block storage (EC2 volume)
│  └─ EBS (✅ Necessary)
│
├─ Shared filesystem (NFS)
│  ├─ Multi-instance + Linux ?
│  │  └─ EFS (✅ Multi-access)
│  └─ Windows SMB ?
│     └─ FSx Windows (✅ Windows native)
│
└─ HPC / Ultra-high performance ?
   └─ FSx Lustre (✅ Specialized)
```

### **Recommendation Matrix**

| Cas d'usage | **Meilleur choix** | **Coût/1TB** | **Setup** | **Gestion** |
|-------------|-------------------|-------------|----------|-----------|
| Data lake (cold) | S3 Glacier | $4 | 5 min | ⭐ Zero-Ops |
| Data warehouse (warm) | S3 Standard | $23 | 5 min | ⭐⭐ (versioning) |
| EC2 database root | EBS gp3 | $100 | 2 min | ⭐⭐⭐ (snapshots) |
| Multi-instance NFS | EFS | $300 | 10 min | ⭐⭐ (auto-scaling) |
| Windows file share | FSx Windows | $2000+ | 20 min | ⭐⭐⭐⭐ (AD integration) |
| HPC scratch | FSx Lustre | $150K/100TB | 30 min | ⭐⭐⭐⭐ (specialized) |

---

## 3️⃣ **DATABASE - RDS vs Aurora vs DynamoDB vs Redshift**

### **Comparaison détaillée par cas d'usage**

| Critère | **RDS** | **Aurora** | **DynamoDB** | **Redshift** |
|---------|---------|-----------|--------------|--------------|
| **Type DB** | SQL managed (MySQL, PostgreSQL, Oracle) | SQL AWS-native (MySQL/PostgreSQL compat) | NoSQL (Key-Value) | SQL Data warehouse |
| **Query pattern** | OLTP (transactions) | OLTP (high-performance) | OLTP (real-time) | OLAP (analytics) |
| **Read latency** | 10-100ms | 1-10ms | <5ms | 100ms-5s |
| **Throughput** | 10K-100K ops/sec | 100K-1M ops/sec | 1M+ ops/sec (provisioned) | 10-100M rows/query |
| **Data volume** | 40TB max | 128TB per region | Terabytes (partition key dependent) | Petabytes |
| **Consistency** | Strong ACID | Strong ACID | Eventual (with strong read after write) | Strong ACID |
| **Cost model** | Per instance type | Per instance type | Per-request or provisioned capacity | Per node + storage |
| **Auto-scaling** | Manual (replicas) | Automatic (read replicas) | Automatic (provisioned/on-demand) | Manual |
| **Setup complexity** | 15-20 min | 15-20 min | 5-10 min | 30+ min |
| **Backup restore** | Automated snapshots | Automated backup (point-in-time) | On-demand + continuous | Automated snapshots |
| **Global replication** | Manual (read replicas) | Aurora Global DB (milliseconds) | Global Tables (milliseconds) | Manual (snapshots) |
| **Maintenance overhead** | ⭐⭐⭐ (parameter tuning) | ⭐⭐ (auto-scaling) | ⭐ (fully managed) | ⭐⭐⭐⭐ (cluster) |

### **Cost Scenarios (monthly)**

#### **Scenario A: Web app, 100 concurrent users, 1000 queries/sec, 50GB data**
- **RDS db.t3.medium** ($50-70/mo) ✅ **Cost-Effective**
- **Aurora db.t3.medium** ($80-100/mo)
- **DynamoDB on-demand** (1000 writes, 1000 reads/s = $150-200/mo)
- **Redshift dc2.large** cluster minimum = $2000+/mo
- **Winner**: RDS (adequate for OLTP, cheapest)

#### **Scenario B: High-traffic application, 100K ops/sec, variable load**
- **RDS**: Cannot scale (single instance bottleneck)
- **Aurora**: db.r5.2xlarge auto-scale = $400-600/mo ✅ **Auto-scaling**
- **DynamoDB provisioned**: 100K = $40K+/mo (too much!)
- **DynamoDB on-demand**: 100K ops = $300-500/mo ✅ **Cheaper alternative**
- **Winner**: Aurora (ACID + auto-scaling) OR DynamoDB (if non-relational)

#### **Scenario C: Real-time mobile app, 10M users, unpredictable spikes**
- **RDS**: Manual scaling → not suitable
- **Aurora**: Auto-scaling but may cost $2K+/mo at peak
- **DynamoDB on-demand**: 1M writes/reads daily avg = $50/mo, spikes handled ✅ **Zero-Ops**
- **Winner**: DynamoDB on-demand (unpredictable = serverless better)

#### **Scenario D: Analytics, 100TB historical data, queries < 5 min acceptable**
- **RDS**: Cannot store 100TB efficiently
- **Aurora**: Theoretically 128TB limit but expensive ($10K+/mo)
- **DynamoDB**: Not designed for analytics queries (scan operations slow)
- **Redshift**: dc2.large 2-node cluster = $2000/mo for 160GB, scales to petabytes ✅ **Designed for analytics**
- **Winner**: Redshift (OLAP workload, cost-effective for bulk)

### **Decision Tree**

```
Q1: Type d'accès ?
├─ OLTP (transactions, <100ms latency)
│  ├─ SQL relational ?
│  │  ├─ Scale high-performance required ?
│  │  │  ├─ OUI → Aurora (✅ High-scale + ACID)
│  │  │  └─ NON → RDS (✅ Cost-Effective)
│  │  │
│  │  └─ Unpredictable spikes ?
│  │     └─ OUI → DynamoDB on-demand (✅ Zero-Ops)
│  │
│  └─ NoSQL key-value ?
│     └─ DynamoDB (✅ Real-time)
│
└─ OLAP (analytics, queries tolerate latency)
   └─ Redshift (✅ Designed for analytics)
```

### **Recommendation Matrix**

| Cas d'usage | **Meilleur choix** | **Coût/mois** | **Setup** | **Gestion** |
|-------------|-------------------|--------------|----------|-----------|
| E-commerce OLTP | RDS (PostgreSQL) | $50-100 | 15 min | ⭐⭐⭐ (tuning) |
| SaaS high-scale | Aurora | $200-1000 | 15 min | ⭐⭐ (monitoring) |
| Mobile app variable | DynamoDB on-demand | $50-500 | 10 min | ⭐ (auto-managed) |
| Real-time leaderboard | DynamoDB provisioned | $100-500 | 10 min | ⭐ (provisioning) |
| Business intelligence | Redshift | $2000-10K | 30 min | ⭐⭐⭐⭐ (cluster) |
| Time-series data (IoT) | DynamoDB + TTL | $100-300 | 10 min | ⭐ (TTL cleanup) |

---

## 4️⃣ **NETWORKING - VPC vs Route 53 vs CloudFront vs API Gateway**

### **Comparaison détaillée par cas d'usage**

| Critère | **VPC** | **Route 53** | **CloudFront** | **API Gateway** |
|---------|---------|-------------|----------------|-----------------|
| **Purpose** | Network isolation & connectivity | DNS + routing logic | Content delivery globally | API management & scaling |
| **Scope** | Single region (or peering) | Global | 200+ edge locations globally | Multi-region capable |
| **Latency impact** | Milliseconds (local) | DNS resolution 100ms | Reduces to 10-100ms (edge) | Minimal (edge endpoint) |
| **Cost model** | NAT Gateway ($32-45/mo) | $0.50 per million queries | $0.085 per GB out | $3.50 per million requests |
| **Setup complexity** | 20-30 min (CIDR, subnets) | 5 min (record creation) | 15 min (distribution) | 10 min (resource/methods) |
| **Cache/acceleration** | None (network layer) | TTL-based DNS | Object caching (1 year) | Caching + throttling |
| **Auto-scaling** | Manual (VPC peering) | Automatic (DNS routing) | Automatic (edge) | Automatic (API Gateway) |
| **DDoS protection** | Security Groups + NACLs | Built-in (basic) | Shield Standard included | WAF integration available |
| **Failover capability** | Route 53 integration (external) | ✓ Health checks + failover | ✓ Origin failover | ✓ Lambda fallback |
| **Management overhead** | ⭐⭐⭐⭐ (subnets, routes, peering) | ⭐ (DNS records) | ⭐⭐ (origins, behaviors) | ⭐⭐ (stages, auth) |

### **Cost Scenarios**

#### **Scenario A: Single region deployment with internal traffic**
- **VPC**: One-time setup, operational cost = $32/mo (NAT) for private instances ✅ **Necessary**
- **Route 53**: $0.50/million queries ($0.50/mo minimal) ✅ **Necessary**
- **CloudFront**: Not needed (single region, internal)
- **API Gateway**: Not needed if internal endpoints
- **Winner**: VPC + Route 53 (foundational, minimal cost)

#### **Scenario B: Static website, 100GB/month global users**
- **VPC + Route 53**: Manual failover = $32/mo (NAT, if needed)
- **CloudFront**: 100GB × $0.085 = $8.50 ✅ **Cost-Effective**
- **API Gateway**: Not applicable (static content)
- **Winner**: CloudFront (massive latency reduction for $8.50)

#### **Scenario C: REST API, 1 billion requests/month global**
- **ALB + EC2**: ~$20/mo + EC2 costs = $150-300/mo
- **API Gateway**: 1B × $3.50/M = $3500/mo (ouch!)
- **API Gateway + CloudFront caching**: 1B × $3.50/M = $3500 (if not cached)
- **If 80% cacheable**: 200M = $700/mo ✅ **Cache efficiency matters**
- **Winner**: API Gateway (only solution for global APIs without building failover)

#### **Scenario D: Multi-region failover with Route 53**
- **VPC + EC2 primary**: $150-300/mo
- **VPC + EC2 secondary** (passive): $150-300/mo
- **Route 53 + health checks**: $0.50/million queries
- **Total**: $300-600/mo but **automatic failover** ✅ **HA required**
- **Winner**: Route 53 health checks enable failover

### **Decision Tree**

```
Q1: Besoin global de distribution ?
├─ Static content (images, CSS, JS) ?
│  └─ CloudFront (✅ Cost-Effective + fast)
│
├─ Dynamic API requests ?
│  └─ API Gateway (✅ Managed scaling)
│     └─ Cacher responses ? → CloudFront + API GW
│
├─ DNS + failover logic ?
│  └─ Route 53 (✅ Health checks + failover)
│
└─ Internal networking ?
   └─ VPC (✅ Necessary foundation)
```

### **Recommendation Matrix**

| Cas d'usage | **Services** | **Coût/mois** | **Setup** | **Gestion** |
|-------------|-------------|--------------|----------|-----------|
| Simple website | VPC + Route 53 | $1 | 10 min | ⭐ (DNS only) |
| Static site global | CloudFront + S3 | $10-50 | 15 min | ⭐ (edge mgmt) |
| REST API global | API Gateway + Lambda | $50-500 | 20 min | ⭐⭐ (caching) |
| APIs + caching | API GW + CloudFront | $100-1000 | 25 min | ⭐⭐⭐ (cache rules) |
| Multi-region failover | Route 53 + ALB | $100-200 | 30 min | ⭐⭐⭐ (health checks) |
| Enterprise CDN | CloudFront + WAF + Shield | $500+ | 45 min | ⭐⭐⭐⭐ (DDoS mgmt) |

---

## 5️⃣ **MESSAGE & EVENTS - SQS vs SNS vs EventBridge vs Kinesis**

### **Comparaison détaillée par pattern d'utilisation**

| Critère | **SQS** | **SNS** | **EventBridge** | **Kinesis Streams** |
|---------|---------|---------|-----------------|----------------------|
| **Pattern** | Queue (pull-based) | Pub-Sub (push-based) | Event routing (pattern-based) | Data streaming (ordered) |
| **Message throughput** | 300-3000 msg/s | 100s msg/s | 10000s events/s | 1000s records/s per shard |
| **Latency** | Seconds (polling) | Sub-second | <1 sec | <1 sec |
| **Delivery model** | Pull (consumer polls) | Push (subscriber notified) | Push (target invoked) | Push (stream processing) |
| **Message retention** | 14 days (configurable) | Immediate (no storage) | Event routing only | 24 hours (1 year option) |
| **Ordering guarantee** | FIFO queues | ❌ | ❌ | ✓ (per shard) |
| **Consumer groups** | Visibility timeout (4h default) | Subscriptions | Targets (Lambda, SNS, SQS, etc.) | Consumer groups (DynamoDB) |
| **Replay capability** | ❌ (consumed = gone) | ❌ | ❌ | ✓ (stream replay) |
| **Deduplication** | ✓ (FIFO) | ❌ | ⭐ (limited) | ❌ |
| **Fan-out capability** | Via SNS subscription | ✓ (multiple subscribers) | ✓ (multiple targets) | Via Kinesis Firehose |
| **Cost model** | Per request (million/mo = $0.40) | Per request (million/mo = $0.50) | Per event + invocations | Per shard-hour + PUT |
| **Setup complexity** | 5 min | 5 min | 10 min | 15 min |
| **Management overhead** | ⭐⭐ (visibility timeout tuning) | ⭐ (fully managed) | ⭐ (pattern matching) | ⭐⭐ (shard capacity planning) |

### **Cost Scenarios (monthly for 1M events/day = 30M/month)**

#### **Scenario A: Task queue (1M tasks/day, worker processes)**
- **SQS Standard**: 30M × $0.40/M = $12/mo ✅ **Cost-Effective**
- **SQS FIFO** (ordered): 30M × $0.50/M = $15/mo (for order guarantee)
- **SNS**: 30M × $0.50/M = $15/mo (not ideal for queue pattern)
- **EventBridge**: 30M × $1/M (events) + Lambda invocations = $50+/mo
- **Winner**: SQS Standard (designed for async tasks)

#### **Scenario B: Fanout notifications (1 event → 10 subscribers)**
- **SQS**: Not suitable (would need 10 separate queues)
- **SNS + 10 subscriptions**: 30M × $0.50/M = $15/mo ✅ **Fanout built-in**
- **EventBridge + 10 targets**: 30M × $1/M events = $30/mo (not cost-effective)
- **Winner**: SNS (fanout is native)

#### **Scenario C: Event-driven architecture (200+ event types, pattern matching)**
- **SQS**: Would need 200+ queues (not practical)
- **SNS**: No pattern matching (topics only)
- **EventBridge**: 30M events × $1/M = $30/mo ✅ **Pattern filtering**
- **Winner**: EventBridge (only option for complex routing)

#### **Scenario D: Real-time streaming (stock prices, 100K events/sec, 24/7)**
- **SQS**: 100K/s × 86400s = 8.6B/day = too expensive + not real-time
- **SNS**: Not designed for streaming
- **EventBridge**: 8.6B/day × $1/M = $8600/mo (still expensive)
- **Kinesis**: 100K/s = 100 shards = $50 × 24 × 30 = $36K/mo (expected for streaming) ✅ **Industry standard**
- **Winner**: Kinesis (only option for sustained high throughput streaming)

#### **Scenario E: Historical data replay (need to reprocess same events)**
- **SQS/SNS**: Once consumed/delivered = gone (no replay)
- **EventBridge**: No storage
- **Kinesis**: Replay events for 24 hours (up to 1 year with extension) ✅ **Replay capability**
- **Winner**: Kinesis (only option for replay)

### **Decision Tree**

```
Q1: Pattern de communication ?
├─ Task queue (producer → workers) ?
│  ├─ Order matters ? 
│  │  ├─ OUI → SQS FIFO (✅ ordered)
│  │  └─ NON → SQS Standard (✅ cheap)
│  │
├─ Notifications (1 event → many) ?
│  └─ SNS (✅ fanout native)
│
├─ Event routing (pattern matching) ?
│  └─ EventBridge (✅ only option)
│
└─ Real-time streaming (high throughput) ?
   └─ Kinesis (✅ designed for streaming)
```

### **Recommendation Matrix**

| Cas d'usage | **Meilleur choix** | **Coût/mois** | **Setup** | **Gestion** |
|-------------|-------------------|---------------|----------|-----------|
| Background jobs | SQS Standard | $10-50 | 5 min | ⭐⭐ (DLQ setup) |
| Ordered processing | SQS FIFO | $15-50 | 10 min | ⭐⭐⭐ (throughput) |
| Multi-target notifications | SNS | $15-50 | 5 min | ⭐ (fully managed) |
| Complex event routing | EventBridge | $30-200 | 15 min | ⭐⭐ (rule mgmt) |
| IoT streaming | Kinesis Streams | $1K-10K | 20 min | ⭐⭐⭐⭐ (shard scaling) |
| Real-time analytics | Kinesis + Firehose | $2K-20K | 25 min | ⭐⭐⭐ (delivery config) |

---

## 6️⃣ **ANALYTICS - Athena vs QuickSight vs EMR vs Glue**

### **Comparaison détaillée par cas d'usage**

| Critère | **Athena** | **QuickSight** | **EMR** | **Glue** |
|---------|-----------|----------------|--------|----------|
| **Purpose** | SQL queries on S3 | BI/Visualization | Big data processing | ETL/Data prep |
| **Compute model** | Serverless (pay-per-query) | Serverless (pay-per-session) | Cluster (pay-per-hour) | Serverless (pay-per-DPU) |
| **Data size** | Petabytes (S3) | Millions of rows | Petabytes (distributed) | Terabytes (ETL) |
| **Query latency** | 10 sec - 5 min | Real-time (cached) | 5 min - 1 hour | 5 min - 30 min |
| **Processing** | SQL only (presto) | SQL + AI/ML | Spark/Hadoop/Presto | Spark-based |
| **Complexity** | Simple ad-hoc | Simple BI queries | Complex transformations | Medium transformations |
| **Cost/month** | $5-50 (per GB scanned) | $12-1000 (per user) | $500-5000 (cluster) | $0.44-4.40 per DPU-hour |
| **Setup time** | 2 min (query S3) | 15 min (connection) | 30 min (cluster) | 15 min (jobs) |
| **Management** | ⭐ (query then forget) | ⭐⭐ (dashboards) | ⭐⭐⭐⭐ (cluster ops) | ⭐⭐⭐ (job monitoring) |
| **Learning curve** | Easy (SQL) | Easy (UI) | Hard (Spark/Hadoop) | Medium (visual UI) |

### **Cost Scenarios**

#### **Scenario A: Ad-hoc SQL queries, 100GB data in S3**
- **Athena**: 100GB scanned × $5/TB = $0.50 per query ✅ **Cost-Effective**
- **QuickSight**: $12/user/month minimum
- **EMR**: Cluster minimum 1 day = $50-100
- **Glue**: 100GB ÷ 100GB per DPU/hour = 1 DPU-hour = $0.44
- **Winner**: Athena (pay per query, no setup)

#### **Scenario B: Daily BI dashboards for 50 users**
- **Athena + Tableau**: $0.50 × 50 queries = $25/day + Tableau license
- **QuickSight**: $12 × 50 users = $600/month ✅ **All-in-one**
- **EMR cluster running 24/7**: $50-100/month (just compute)
- **Winner**: QuickSight (purpose-built BI, all included)

#### **Scenario C: ETL pipeline, transform 1TB raw data daily to cleaned**
- **Athena**: Not suitable (query tool, not transformer)
- **Glue ETL**: 1TB ÷ 100GB per DPU-hour = 10 DPU-hours = $4.40/day ✅ **Cost-Effective**
- **EMR Spark cluster**: 1TB processing = $20-50/day (cluster + compute)
- **Winner**: Glue (designed for ETL, simpler)

#### **Scenario D: Complex ML data prep, 100TB distributed processing**
- **Athena**: SQL limited
- **Glue**: 100TB ÷ 100GB per DPU = 1000 DPU-hours = $440/run ✅
- **EMR Spark cluster**: 100 nodes × $0.50/hour × 10 hours = $500/run ✅ (plus overhead)
- **Winner**: Glue or EMR (similar cost, Glue simpler)

### **Decision Tree**

```
Q1: Task type ?
├─ One-off SQL queries ?
│  └─ Athena (✅ Cheapest, instant)
│
├─ BI dashboards ?
│  └─ QuickSight (✅ All-in-one, per-user)
│
├─ ETL data transformation ?
│  ├─ Simple (SQL-based) ?
│  │  └─ Glue (✅ Visual, simple)
│  └─ Complex (Spark code) ?
│     ├─ Custom code ? → EMR (✅ Full control)
│     └─ Preference ? → Glue (✅ Managed)
│
└─ ML prep + Hadoop ecosystem ?
   └─ EMR (✅ Flexible, industry standard)
```

### **Recommendation Matrix**

| Cas d'usage | **Meilleur choix** | **Coût/mois** | **Setup** | **Gestion** |
|-------------|-------------------|--------------|----------|-----------|
| Ad-hoc queries | Athena | $5-50 | 2 min | ⭐ (zero) |
| Internal BI | QuickSight | $100-500 | 20 min | ⭐⭐ (dashboards) |
| Data pipeline | Glue | $50-500 | 15 min | ⭐⭐⭐ (job mgmt) |
| Spark/Hadoop jobs | EMR | $500-2000 | 30 min | ⭐⭐⭐⭐ (cluster ops) |
| Real-time analytics | Kinesis + Glue | $500-1000 | 25 min | ⭐⭐⭐ (streaming) |

---

## 7️⃣ **DEPLOYMENT STRATEGY - Blue-Green vs Canary vs Rolling**

### **Comparaison détaillée par contrainte**

| Critère | **Blue-Green** | **Canary** | **Rolling** |
|---------|----------------|-----------|-------------|
| **Downtime** | 0 (zero) | 0 (zero) | 0 (zero) |
| **Resource requirement** | 2x compute + storage | 1.2x-1.5x | 1x (no extra) |
| **Rollback time** | <1 sec (DNS switch) | <1 sec (traffic shift) | 30-60 min (inverse order) |
| **Risk if deployment fails** | Medium (after 100% switch) | Very low (only 5-10% affected) | Medium (phased rollout) |
| **Initial validation time** | 10-30 min (test GREEN env) | 0 (tests live) | 0 (tests live) |
| **Time to reach 100%** | <1 sec | 30-60 min | 10-30 min |
| **Traffic shift** | Instant | Progressive (5%→25%→50%→100%) | Batch (1-2 instances at a time) |
| **Monitoring during deploy** | Pre-deploy validation | Real-time metrics (errors, latency) | Real-time health checks |
| **Testing capability** | Full environment testing before | Testing under real load | Testing under real load |
| **Cost** | $$$$ (2x environment) | $$$ (temporary 1.3x) | $$ (no extra) |
| **Complexity** | High (ALB, dual environments) | Medium (CodeDeploy + metrics) | Low (simple ASG update) |
| **Ideal for** | Critical (banking, e-commerce) | Large deployments (safe testing) | Non-critical (low traffic) |
| **AWS tooling** | CodeDeploy Blue-Green, ALB | CodeDeploy Canary, CloudWatch | CodeDeploy Rolling, ASG |

### **Cost Scenarios**

#### **Scenario A: e-commerce during Black Friday, 5000 requests/sec**
- **Blue-Green**: 2 ALBs + 2x ASG = extra $200-300/deploy ✅ **Worth the safety**
- **Canary**: Same resources during test phase, simpler mgmt
- **Rolling**: Cheapest but risky (losing 5% capacity = potential failures)
- **Decision**: Blue-Green (critical business + budget OK)

#### **Scenario B: Internal company dashboard, 10 users, updates weekly**
- **Blue-Green**: Overkill (expensive, complex)
- **Canary**: Medium overhead
- **Rolling**: Perfectly adequate ✅ **Cost-Effective**
- **Decision**: Rolling (low traffic, frequent updates acceptable)

#### **Scenario C: Mobile app backend, 100K daily users, updates daily**
- **Blue-Green**: $300+/mo (too expensive for daily deploys)
- **Canary**: $50-100/mo + monitoring ✅ **Safe, cost-effective**
- **Rolling**: Cheapest but risky at scale
- **Decision**: Canary (balance cost + safety)

### **Decision Tree**

```
Q1: Downtime tolerable ?
├─ NON (critical apps) ?
│  ├─ Budget unlimited → Blue-Green (✅ Fast rollback)
│  └─ Budget limited → Canary (✅ Progressive, safe)
│
└─ OUI (non-critical) ?
   ├─ Traffic > 1000 req/sec ?
   │  └─ Canary (✅ Monitor carefully)
   └─ Traffic < 1000 req/sec ?
      └─ Rolling (✅ Cost-Effective)
```

### **Recommendation Matrix**

| Cas d'usage | **Meilleur choix** | **Coût/deploy** | **Rollback time** | **Gestion** |
|-------------|-------------------|-----------------|-------------------|-----------|
| Production critical | Blue-Green | $200-300 | <1 sec | ⭐⭐⭐⭐ (ALB mgmt) |
| High-traffic app | Canary | $50-100 | <1 sec | ⭐⭐⭐ (monitoring) |
| Low-traffic app | Rolling | $0 | 30 min | ⭐⭐ (health checks) |
| Frequent updates | Rolling | $0 | 30 min | ⭐⭐ (ASG tuning) |
| Safe testing | Canary | $50-100 | <1 sec | ⭐⭐⭐ (metrics choice) |

---

## 8️⃣ **IaC - CloudFormation vs CDK vs Terraform**

| Critère | **CloudFormation** | **CDK** | **Terraform** |
|---------|-------------------|--------|--------------|
| **Language** | YAML/JSON | Python/TypeScript/Java | HCL |
| **AWS coverage** | 100% | 100% (via CloudFormation) | 95% |
| **Multi-cloud** | ❌ | ❌ | ✓ |
| **Learning curve** | Moyen | Moyen | Facile |
| **Reusability** | Modules limités | Constructs excellents | Modules bons |
| **CI/CD friendly** | ✓ | ✓ | ✓ |
| **Drift detection** | ✓ (AWS native) | ❌ (via terraform) | ✓ |
| **Community** | AWS focused | Growing | Très grand |
| **Cost** | Free | Free | Free (+ Terraform Cloud $$$) |
| **Best for** | AWS-only projects | Reusable constructs | Multi-cloud |

### **Decision Logic**
- **Si AWS-only + constructs réutilisables** → CDK
- **Si AWS-only simple** → CloudFormation
- **Si multi-cloud** → Terraform

---

## 9️⃣ **SECURITY - WAF vs Shield vs GuardDuty vs Inspector vs Macie**

| Critère | **WAF** | **Shield** | **GuardDuty** | **Inspector** | **Macie** |
|---------|---------|-----------|---------------|--------------|---------
| **Type menace** | Application layer | DDoS network layer | Account threats | Vulnerabilities | Data exposure |
| **Detection** | Rule-based | Signature + behavior | ML + intelligence | Scan + database | ML pattern matching |
| **Cost** | $/rules | Standard (free), Advanced ($$$) | $/log analysis | $/assessment | $/scan |
| **Auto response** | ❌ (manual block) | ✓ (auto mitigation) | ❌ (alert only) | ❌ (report) | Alert + event |
| **Integration** | CloudFront, ALB, API GW | CloudFront, Route 53, ALB, Global Accelerator | Multi-service | EC2, Lambda, ECR | S3 |
| **Use case** | App security | DDoS protection | Threat detection | Vuln scanning | PII/sensitive data |

### **Decision Logic**
- **Si attaques web (SQL injection, XSS)** → WAF
- **Si protection DDoS** → Shield
- **Si threat detection avancée** → GuardDuty
- **Si vulnerabilities scanning** → Inspector
- **Si données sensibles découverte** → Macie

---

## 🔟 **MONITORING - CloudWatch vs X-Ray vs CloudTrail**

| Critère | **CloudWatch** | **X-Ray** | **CloudTrail** |
|---------|----------------|----------|-----------------|
| **Purpose** | Metrics, logs, alarms | Distributed tracing | Audit trail |
| **Data type** | Metrics, logs | Traces, segments | API calls |
| **Granularity** | Per service | Per request | Per AWS API |
| **Real-time** | Yes | Yes | Yes (with lag) |
| **Retention** | Default 15 months | Default 30 days | CloudTrail Events default 90 days (S3 unlimited) |
| **Cost** | Metrics + log ingestion | Per traces sampled | $$$ (per log) |
| **Debug capability** | Good (logs) | Excellent (trace) | Excellent (audit) |
| **Use case** | App monitoring | Performance debugging | Compliance audit |

### **Decision Logic**
- **Si dashboards + alertes** → CloudWatch
- **Si request tracing + performance** → X-Ray
- **Si compliance + audit trail** → CloudTrail

---

## ✅ **QUICK DECISION CHECKLISTS**

### **Choosing Compute**
```
1. Combien de temps le job prend-il ?
   < 15 min → Lambda
   < 1 heure → Batch ou Lambda
   > 1 heure → EC2 ou Fargate

2. Quelle fréquence ?
   Rare/on-demand → Lambda
   Scheduled → Batch, EventBridge + Lambda
   Always-on → EC2

3. Budget / Traffic pattern ?
   Variable → Lambda, Fargate
   Constant → EC2 (Reserved Instance)
   Bursty → Spot instances + Lambda
```

### **Choosing Storage**
```
1. Quoi stocker ?
   Objects/files non-structured → S3
   Block storage pour EC2 → EBS
   Shared filesystem → EFS
   Managed file server (Windows) → FSx

2. Combien d'instances accès ?
   1 instance → EBS
   N instances → EFS ou S3
```

### **Choosing Database**
```
1. Type de données ?
   SQL structured → RDS, Aurora
   Key-value → DynamoDB
   Analytics massive → Redshift

2. Performance requis ?
   <10ms latency → Aurora, DynamoDB
   >100ms OK → Redshift, RDS
```

---

## 📚 **RESSOURCES**

- AWS Pricing Calculator : https://calculator.aws
- AWS Well-Architected : https://aws.amazon.com/architecture/well-architected/
- AWS Service Quotas : https://docs.aws.amazon.com/general/latest/gr/aws_service_limits.html

---

**Prêt pour certification ? → Revise avec [GLOSSAIRE_AWS_DEVOPS.md](GLOSSAIRE_AWS_DEVOPS.md) et [PATTERNS_ARCHITECTURAUX_DEVOPS.md](PATTERNS_ARCHITECTURAUX_DEVOPS.md)**
