# 🚀 AWS DevOps Pro - Services de Calcul
## EC2, Lambda, Fargate, Batch

---

## 📋 **Vue d'ensemble des Services de Calcul**

### **Rôle dans l'Architecture DevOps**
Les services de calcul forment la base de toute infrastructure cloud. Le choix entre EC2, Lambda, Fargate et Batch dépend des besoins en **scalabilité**, **gestion opérationnelle** et **coûts**.

### **Interactions Clés avec Autres Services**
- **Stockage** : EBS pour EC2, EFS pour containers
- **Réseau** : VPC, Load Balancers, API Gateway
- **Sécurité** : IAM roles, Security Groups
- **Monitoring** : CloudWatch, X-Ray
- **CI/CD** : CodePipeline, CodeDeploy

---

## 🔧 **Amazon EC2 - Serveurs Virtuels**

**Définition :** Amazon Elastic Compute Cloud (EC2) est un service web qui fournit une capacité de calcul redimensionnable dans le cloud. Il est conçu pour faciliter l'accès à l'informatique à la demande pour les développeurs. EC2 offre un contrôle total sur les ressources informatiques et permet de lancer autant de serveurs virtuels que nécessaire, de configurer la sécurité et le réseau, et de gérer le stockage. Les instances EC2 peuvent être lancées en quelques minutes et offrent une variété de types d'instances optimisés pour différents cas d'usage, de l'informatique générale aux applications nécessitant des performances élevées.

### **Quand Choisir EC2 ?**

#### **Scénarios Idéaux**
- **Applications stateful** nécessitant persistance des données
- **Workloads nécessitant** contrôle total sur l'OS et les logiciels
- **Bases de données** relationnelles (RDS utilise EC2 sous le capot)
- **Applications legacy** difficiles à containeriser

#### **Types d'Instances et Cas d'Usage**
- **T3/T4g (Burstable)** : Applications web légères, développement
- **C6g (Compute)** : Calcul intensif, gaming, ML training
- **R6g (Memory)** : Bases de données, cache, big data
- **I4i (Storage)** : Data warehouses, NoSQL, logs processing

### **Auto Scaling - Scaling Horizontal**

#### **Pourquoi Auto Scaling ?**
- **Haute disponibilité** : Remplace automatiquement les instances défaillantes
- **Elasticité** : S'adapte à la demande sans intervention manuelle
- **Optimisation coût** : Réduit la capacité pendant les périodes creuses

#### **Interactions avec Autres Services**
- **Load Balancers** : Distribue le trafic entre instances
- **CloudWatch** : Métriques pour déclencher le scaling
- **RDS** : Scaling des bases de données en lecture
- **EFS** : Stockage partagé entre instances

#### **Limitations et Pièges**
- **Cold starts** : Pas de scaling instantané
- **State management** : Les données locales sont perdues
- **Configuration drift** : Instances peuvent diverger sans automatisation

### **EC2 dans un Pipeline DevOps**

#### **Scénario : Application Web Traditionnelle**
```
Internet → CloudFront → ALB → Auto Scaling Group (EC2) → RDS
```

**Pourquoi cette architecture ?**
- **ALB** gère la répartition de charge et le SSL termination
- **Auto Scaling** maintient la disponibilité
- **CloudFront** accélère la livraison du contenu statique
- **RDS** fournit la base de données managée

#### **Problèmes Courants**
- **Scaling vertical limité** : Instance types ont des limites
- **Maintenance OS** : Patchs de sécurité à gérer
- **Configuration consistency** : AMI baking requis

---

## ⚡ **AWS Lambda - Serverless Functions**

**Définition :** AWS Lambda est un service de calcul serverless qui exécute du code en réponse à des événements et gère automatiquement les ressources informatiques sous-jacentes. Il permet aux développeurs d'exécuter du code sans provisionner ni gérer de serveurs. Lambda prend en charge plusieurs langages de programmation (Node.js, Python, Java, Go, .NET, Ruby) et peut être déclenché par divers événements AWS ou sources externes. Le service met automatiquement à l'échelle les ressources en fonction de la demande, de zéro à des milliers d'exécutions simultanées, et facture uniquement le temps de calcul consommé à la milliseconde près.

### **Quand Choisir Lambda ?**

#### **Scénarios Idéaux**
- **Event-driven processing** : Réactions à des événements
- **API backends** : Microservices légers
- **Data processing** : ETL, transformations
- **Chatbots et webhooks** : Réponses rapides et stateless

#### **Avantages Théoriques**
- **Zero administration** : AWS gère l'infrastructure
- **Auto-scaling automatique** : De 0 à 1000+ exécutions simultanées
- **Pay-per-use** : Facturation à la milliseconde
- **Intégration native** : Triggers depuis 200+ services AWS

### **Interactions avec l'Écosystème AWS**

#### **Avec les Services de Stockage**
- **S3** : Traitement automatique des uploads (redimensionnement images)
- **DynamoDB** : Streams pour processing temps réel
- **EFS** : Accès à des fichiers partagés

#### **Avec les Services de Messaging**
- **SQS** : Processing asynchrone des messages
- **SNS** : Notifications et fan-out
- **EventBridge** : Routage d'événements complexes

#### **Avec les Services Réseau**
- **API Gateway** : Exposition HTTP des fonctions
- **VPC** : Accès aux ressources privées (avec limitations)

### **Limitations Critiques**

#### **Timeouts et Ressources**
- **Maximum 15 minutes** d'exécution
- **Mémoire limitée** à 10GB
- **Pas de persistance** des données locales

#### **Cold Starts**
- **Impact sur latence** pour les fonctions peu utilisées
- **Provisioned Concurrency** comme solution partielle
- **Plus critique** pour les langages compilés (Java, .NET)

#### **Vendor Lock-in**
- **Runtime spécifique** AWS Lambda
- **Migration difficile** vers d'autres providers
- **Dépendances** aux services AWS

### **Lambda dans les Architectures DevOps**

#### **Scénario : API Serverless**
```
API Gateway → Lambda → DynamoDB
```

**Pourquoi cette architecture ?**
- **API Gateway** gère l'authentification et le rate limiting
- **Lambda** scale automatiquement avec la demande
- **DynamoDB** fournit la persistance NoSQL

#### **Scénario : Pipeline de Traitement de Données**
```
S3 Upload → Lambda → Glue → Redshift
```

**Pourquoi cette architecture ?**
- **Lambda** transforme les données brutes
- **Glue** orchestre le pipeline ETL
- **Redshift** stocke les données analytiques

---

## 🐳 **AWS Fargate - Containers Serverless**

**Définition :** AWS Fargate est un moteur de calcul serverless pour conteneurs qui fonctionne avec Amazon Elastic Container Service (ECS) et Amazon Elastic Kubernetes Service (EKS). Il élimine le besoin de provisionner et de gérer des serveurs ou des clusters pour exécuter des conteneurs. Fargate alloue automatiquement la quantité appropriée de ressources informatiques pour exécuter les conteneurs, gère le scaling automatique, et facture uniquement pour les ressources vCPU et mémoire utilisées par les tâches en cours d'exécution. Il prend en charge les applications conteneurisées sans nécessiter de connaissances approfondies sur l'infrastructure sous-jacente.

### **Quand Choisir Fargate ?**

#### **Scénarios Idéaux**
- **Applications containerisées** existantes
- **Microservices** avec scaling automatique
- **Batch processing** containerisé
- **Migration depuis** Kubernetes ou Docker Compose

#### **Avantages sur EC2**
- **Pas de gestion EC2** : AWS gère les instances
- **Scaling automatique** des tâches
- **Sécurité améliorée** : Isolation par tâche
- **Coût optimisé** : Pay per task, not per server

### **Interactions dans l'Écosystème**

#### **Avec ECS et EKS**
- **ECS** : Orchestration simple pour AWS
- **EKS** : Kubernetes managé pour complexité
- **Service Discovery** : Load balancing automatique

#### **Avec les Services de Stockage**
- **EFS** : Stockage partagé entre containers
- **S3** : Stockage objet pour données statiques
- **FSx** : Stockage haute performance

#### **Avec les Services Réseau**
- **ALB/NLB** : Load balancing des services
- **Cloud Map** : Service discovery
- **VPC** : Isolation réseau

### **Comparaison Fargate vs Lambda**

#### **Fargate Gagne Quand :**
- **Long running tasks** (>15 minutes)
- **Besoin de contrôle** sur l'environnement runtime
- **Applications stateful** avec persistance
- **Utilisation de protocoles** autres que HTTP

#### **Lambda Gagne Quand :**
- **Short executions** (<5 min)
- **Event-driven** pur
- **Stateless** processing
- **Scaling très fréquent** (bursts)

### **Fargate dans les Architectures DevOps**

#### **Scénario : Application Microservices**
```
ALB → ECS Fargate Service → EFS (shared storage)
```

**Pourquoi cette architecture ?**
- **ALB** route vers les services appropriés
- **Fargate** scale chaque service indépendamment
- **EFS** permet le partage de fichiers entre instances

#### **Scénario : Migration Lift-and-Shift**
```
Legacy App → Container → ECS Fargate → RDS
```

**Pourquoi cette architecture ?**
- **Containerisation** préserve l'application originale
- **Fargate** élimine la gestion infrastructure
- **RDS** modernise la base de données

---

## 🔄 **AWS Batch - Traitement par Lots**

**Définition :** AWS Batch est un service de calcul par lots entièrement managé qui planifie, exécute et met à l'échelle des charges de travail de calcul par lots sur la plateforme AWS. Il optimise la distribution et l'utilisation des ressources informatiques en fonction de la quantité et de l'échelle des travaux par lots soumis. Batch peut s'exécuter sur des instances EC2 ou AWS Fargate, et fournit des files d'attente de tâches, des priorités, des dépendances et des tentatives automatiques. Le service est particulièrement adapté aux workloads de calcul intensif qui peuvent être parallélisés et ne nécessitent pas d'interaction en temps réel.

### **Quand Choisir Batch ?**

#### **Scénarios Idéaux**
- **Calcul intensif** périodique (nuit, fin de mois)
- **Traitement de gros volumes** de données
- **Simulations scientifiques** ou financières
- **Rendering** vidéo ou 3D

#### **Avantages sur les Autres Services**
- **Optimisé pour batch** : Files d'attente intelligentes
- **Scaling automatique** basé sur la charge
- **Multi-AZ** pour haute disponibilité
- **Intégration native** avec S3 et DynamoDB

### **Interactions avec l'Infrastructure**

#### **Avec les Services de Calcul**
- **EC2** : Pour workloads nécessitant GPU
- **Fargate** : Pour containers serverless
- **Spot Instances** : Réduction coût de 70-90%

#### **Avec les Services de Stockage**
- **S3** : Input/output des jobs
- **EFS** : Stockage partagé entre jobs
- **FSx** : Stockage haute performance

#### **Avec les Services d'Orchestration**
- **Step Functions** : Orchestration de workflows complexes
- **EventBridge** : Scheduling des jobs
- **Lambda** : Pré/post-processing

### **Limitations et Considérations**

#### **Latence**
- **Pas temps réel** : Minutes à heures pour completion
- **Scheduling** : Pas de déclenchement instantané
- **Dependencies** : Gestion complexe pour workflows parallèles

#### **Coûts**
- **Pay per use** mais utilisation variable
- **Spot Instances** recommandées pour optimiser
- **Monitoring** nécessaire pour optimisation

### **Batch dans les Architectures DevOps**

#### **Scénario : Traitement de Données Nocturne**
```
EventBridge (cron) → Batch Job → S3 → Redshift
```

**Pourquoi cette architecture ?**
- **EventBridge** déclenche à heure fixe
- **Batch** utilise Spot pour coût optimisé
- **Redshift** reçoit les données transformées

#### **Scénario : Pipeline ML**
```
S3 (new data) → Lambda → Batch (training) → SageMaker
```

**Pourquoi cette architecture ?**
- **Lambda** déclenche le retraining
- **Batch** exécute l'entraînement sur GPU
- **SageMaker** déploie le modèle

---

## 🎯 **Scénarios de Certification DevOps Pro**

### **"Migration d'une Application Monolithique"**

**Situation :** Application legacy sur EC2 unique point de défaillance

**Solution :**
- **Phase 1 :** Migration vers ECS Fargate (containerisation)
- **Phase 2 :** Décomposition en microservices Lambda
- **Phase 3 :** Auto-scaling et multi-AZ

**Pourquoi pas directement Lambda ?**
- **Dépendances complexes** difficiles à résoudre
- **Base de données** nécessite refactoring
- **Testing** plus simple avec containers

### **"Optimisation d'un Pipeline de CI/CD"**

**Situation :** Builds lents et coûteux

**Solution :**
- **EC2 Spot** pour les agents de build
- **Lambda** pour les tests légers
- **Batch** pour les tests d'intégration lourds

**Interactions critiques :**
- **CodeBuild** orchestre les étapes
- **S3** stocke les artefacts
- **CloudWatch** monitore les performances

### **"Traitement de Données Temps Réel vs Batch"**

**Temps réel (Kinesis + Lambda) :**
- **Latence** : Secondes
- **Coût** : Pay per execution
- **Complexité** : Faible

**Batch (S3 + Batch) :**
- **Latence** : Minutes/heures
- **Coût** : Optimisé avec Spot
- **Complexité** : Plus élevée

**Choix dépend de :** SLA, volume de données, coût acceptable

---

## 🔄 **Comparaisons et Trade-offs**

### **EC2 vs Fargate vs Lambda**

| Critère | EC2 | Fargate | Lambda |
|---------|-----|---------|--------|
| **Contrôle** | Complet | Moyen | Faible |
| **Scaling** | Manuel/Auto | Automatique | Automatique |
| **Cold Start** | Aucun | Faible | Élevé |
| **Timeout** | Illimité | Illimité | 15 min |
| **Coût** | Fixe | Par tâche | Par execution |
| **Maintenance** | Élevée | Faible | Aucune |

### **Batch vs Lambda pour Processing**

**Batch gagne pour :**
- **Long running** (>15 min)
- **Ressources lourdes** (GPU, mémoire)
- **Orchestration complexe**
- **Coût optimisé** avec Spot

**Lambda gagne pour :**
- **Short tasks** (<5 min)
- **Event-driven** pur
- **Stateless** processing
- **Intégration** native

---

## 🚨 **Pièges Courants et Solutions**

### **Over-Engineering**
- **Problème :** Utiliser Lambda pour tout
- **Conséquence :** Cold starts, timeouts, vendor lock-in
- **Solution :** Choisir le bon outil pour chaque workload

### **Sous-Estimation des Coûts**
- **Problème :** Lambda semble gratuit mais...
- **Conséquence :** Facturation par request + durée
- **Solution :** Calculer TCO incluant monitoring et debugging

### **Architecture Hybride Complexe**
- **Problème :** Mélanger EC2, Fargate et Lambda
- **Conséquence :** Complexité opérationnelle
- **Solution :** Stratégie claire par couche d'architecture

---

**🎯 Comprendre les interactions théoriques entre services de calcul permet de concevoir des architectures robustes et optimisées !**