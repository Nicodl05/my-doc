## **📌 Fiche Récapitulative Ultime : AWS DevOps Pro (DOP-C02)**
*(Avec pièges détaillés, cas d'usage, et astuces pour l'examen)*

---

## **1. Pièges Fréquents par Service (À Cocher Mentalement)**
### **🔹 CodePipeline / CodeDeploy**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| Utiliser **Lambda pour attendre la fin d'un build CodeBuild**           | Lambda a un timeout max de 15 min.                                                      | Utiliser **CloudWatch Events** pour déclencher une action après le build.       |
| **Déployer directement en production sans test**                        | Violation du principe de SDLC (ex: pas de staging).                                      | Toujours inclure une étape de **staging + tests automatiques**.                |
| **Oublier les rollbacks automatiques**                                  | Risque de downtime si le déploiement échoue.                                            | Configurer **CloudWatch Alarms** + **CodeDeploy Auto-Rollback**.                  |
| **Utiliser `latest` comme tag Docker dans ECR**                        | Impossible de tracker les versions.                                                      | Utiliser des **hashes SHA** ou des tags sémantiques (ex: `v1.2.3`).               |
| **Ne pas suspendre Auto Scaling pendant un déploiement**               | Risque de scaling pendant le déploiement → instances avec des versions différentes. | Suspendre **AZRebalance**, **AlarmNotification** via `HANDLE_PROCS=true`.         |

---
### **🔹 CloudFormation**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Utiliser `DBEngineVersion` au lieu de `EngineVersion`**              | Propriété incorrecte dans le template.                                                   | Toujours vérifier la [doc officielle](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/aws-properties-rds-database-instance.html). |
| **Ne pas gérer les dépendances entre stacks**                          | Risque de création dans le mauvais ordre (ex: VPC avant les instances).               | Utiliser `DependsOn` ou `Outputs`/`Exports`.                                        |
| **Supprimer un bucket S3 non vide**                                      | CloudFormation échoue si le bucket contient des objets.                              | Utiliser un **Custom Resource (Lambda)** pour vider le bucket avant suppression.  |
| **Oublier `UpdatePolicy` pour les Auto Scaling Groups**                | Risque de replacement complet au lieu d'un rolling update.                             | Configurer `AutoScalingRollingUpdate` avec `MinInstancesInService`.              |

---
### **🔹 Auto Scaling & Load Balancing**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Utiliser `Canary` pour des déploiements EC2**                        | `Canary` ne s'applique qu'à **Lambda/ECS**, pas à EC2.                                  | Pour EC2, utiliser un **petit deployment group** + approval manuelle.             |
| **Ne pas configurer de health checks avancés**                          | Health checks basiques (ex: HTTP 200) ne vérifient pas la DB.                          | Étendre les health checks pour **vérifier la connectivité DB**.                 |
| **Oublier de tagger les instances pour les deployment groups**         | CodeDeploy ne sait pas quelles instances mettre à jour.                                | Toujours tagger les instances avec `DeploymentGroup: <nom>`.                   |
| **Utiliser `AllAtOnce` pour un environnement critique**                | Downtime garanti.                                                                         | Préférer **Linear** ou **Canary** (si applicable).                                 |

---
### **🔹 Monitoring & Logging**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Stocker des logs CloudWatch >15 mois**                                  | CloudWatch ne conserve pas les logs aussi longtemps.                                   | Exporter vers **S3 + Athena** ou **Elasticsearch**.                                |
| **Utiliser CloudTrail sans l'activer dans toutes les régions**        | Perte de visibilité sur les appels API dans certaines régions.                        | Activer CloudTrail **multi-régions** via Organizations.                            |
| **Confondre CloudWatch Events et CloudTrail**                            | CloudWatch Events = événements système ; CloudTrail = audit des API calls.             | Utiliser **CloudTrail pour l'audit**, **Events pour les actions automatiques**.   |
| **Oublier d'activer les métriques détaillées pour EC2**                | Métriques basiques (ex: CPU) ne suffisent pas pour le debugging.                       | Activer **CloudWatch Agent** ou **détaillé monitoring**.                          |

---
### **🔹 Sécurité & Conformité**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Donner un accès IAM User à une instance EC2**                        | Risque de fuite des credentials.                                                          | Toujours utiliser un **IAM Role**.                                               |
| **Stocker des secrets dans Parameter Store**                            | Pas de rotation automatique.                                                            | Utiliser **Secrets Manager** pour les mots de passe DB.                           |
| **Ne pas activer MFA pour les comptes root/IAM**                       | Violation des bonnes pratiques de sécurité.                                             | **Activer MFA** + **SCP** via AWS Organizations.                                  |
| **Oublier de chiffrer les volumes EBS**                                 | Non-conforme aux standards de sécurité (ex: PCI DSS).                                   | Utiliser **KMS** + **politiques IAM** pour forcer le chiffrement.                 |

---
### **🔹 RDS & Bases de Données**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Utiliser Multi-AZ pour éviter le downtime lors d'un upgrade RDS**    | Multi-AZ ne prévient pas le downtime pour les upgrades majeurs.                         | Utiliser un **Read Replica** + promotion pour les upgrades critiques.             |
| **Ne pas configurer de backup automatisés**                             | Risque de perte de données.                                                              | Configurer **snapshots automatiques** + **retention period**.                   |
| **Oublier de tester les failovers RDS**                                  | Le failover peut échouer en production.                                                 | Tester les failovers en **staging** avec `RebootDBInstance`.                     |
| **Utiliser DynamoDB pour des requêtes SQL complexes**                  | DynamoDB est NoSQL.                                                                       | Utiliser **Aurora** ou **RDS** pour les requêtes SQL.                            |

---
### **🔹 Step Functions & Lambda**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Utiliser Lambda pour des workflows >15 min**                           | Timeout max de 15 min.                                                                    | Utiliser **Step Functions** pour les workflows longs.                             |
| **Oublier de gérer les erreurs dans Step Functions**                   | Risque de blocage du workflow.                                                            | Toujours inclure des **états `Catch`** et **retry policies**.                   |
| **Ne pas versionner les fonctions Lambda**                              | Impossible de rollback.                                                                  | Utiliser des **aliases** (ex: `PROD`, `STAGING`) + **versions**.                 |
| **Utiliser API Gateway + Lambda pour 10k requêtes/s**                   | Limite de 1k requêtes/s pour Lambda.                                                    | Utiliser **API Gateway + Step Functions** ou **ALB**.                            |

---

## **2. Cas d'Usage Classiques (À Reconnaître)**
### **🔸 Déploiements Blue/Green**
- **Scénario** : Déployer une nouvelle version d'une app **sans downtime**.
- **Services** : CodeDeploy (pour EC2/ECS/Lambda) + **ALB** (pour le traffic shifting).
- **Piège** : Oublier de **tester la nouvelle version** avant de basculer le traffic.
- **Solution** :
  1. Déployer sur un nouveau fleet (ex: nouveau ASG).
  2. Vérifier les **health checks** et **métriques CloudWatch**.
  3. Basculer le traffic via **ALB listener rules** ou **Route53 weighted routing**.

---
### **🔸 Gestion des Secrets**
- **Scénario** : Stocker et rotater automatiquement un mot de passe RDS.
- **Services** : **Secrets Manager** (pour la rotation) + **IAM Roles** (pour l'accès).
- **Piège** : Utiliser **Parameter Store** (pas de rotation auto).
- **Solution** :
  ```yaml
  # CloudFormation pour Secrets Manager
  MyRDSSecret:
    Type: AWS::SecretsManager::Secret
    Properties:
      GenerateSecretString:
        SecretStringTemplate: '{"username": "admin"}'
        GenerateStringKey: "password"
        PasswordLength: 16
        ExcludeCharacters: '"@/\'
  ```

---
### **🔸 Audit de Conformité**
- **Scénario** : Vérifier que **toutes les instances EC2 ont des tags obligatoires**.
- **Services** : **AWS Config** (règles personnalisées) + **CloudWatch Events** (alertes).
- **Piège** : Utiliser **CloudTrail** (ne vérifie pas les tags).
- **Solution** :
  1. Créer une **règle Config personnalisée** (Lambda) pour vérifier les tags.
  2. Configurer un **aggregator** pour les comptes multi-AWS.
  3. Déclencher une **remédiation automatique** via SSM Automation.

---
### **🔸 Scaling Prédictif**
- **Scénario** : Scaler une app **avant un pic de traffic prévu** (ex: Black Friday).
- **Services** : **Auto Scaling Scheduled Actions** + **Target Tracking**.
- **Piège** : Utiliser seulement **Target Tracking** (réactif, pas prédictif).
- **Solution** :
  ```yaml
  # CloudFormation pour Scheduled Action
  ScaleUpAction:
    Type: AWS::AutoScaling::ScheduledAction
    Properties:
      AutoScalingGroupName: !Ref MyASG
      DesiredCapacity: 10
      Recurrence: "0 8 * * 1-5" # Lundi-Vendredi à 8h
  ```

---
### **🔸 Détection de Fuites de Credentials**
- **Scénario** : Être alerté si des **credentials AWS sont exposées sur GitHub**.
- **Services** : **AWS Health API** (événement `AWS_RISK_CREDENTIALS_EXPOSED`) + **Step Functions**.
- **Piège** : Utiliser **CloudTrail** (ne détecte pas les fuites externes).
- **Solution** :
  1. Créer un **CloudWatch Event** pour l'événement `AWS_RISK_CREDENTIALS_EXPOSED`.
  2. Déclencher un **Step Function workflow** qui :
     - Désactive les credentials via **IAM API**.
     - Envoie une alerte via **SNS**.
     - Audit les appels récents via **CloudTrail**.

---
### **🔸 Migration de Bases de Données**
- **Scénario** : Migrer une base **MySQL RDS vers Aurora** avec **minimal downtime**.
- **Services** : **AWS DMS** (Data Migration Service) + **Read Replica**.
- **Piège** : Arrêter l'ancienne base trop tôt.
- **Solution** :
  1. Créer un **Read Replica Aurora** depuis RDS.
  2. Utiliser **DMS** pour synchroniser les données.
  3. Basculer les apps vers Aurora une fois la synchro terminée.

---
### **🔸 Logs Centralisés**
- **Scénario** : Agréguer les logs **multi-comptes** dans un compte central.
- **Services** : **CloudWatch Logs Subscriptions** + **Kinesis Firehose** + **S3/Athena**.
- **Piège** : Utiliser **CloudTrail** (ne stocke pas les logs applicatifs).
- **Solution** :
  1. Configurer un **log destination** dans le compte central.
  2. Créer une **subscription filter** dans chaque compte source.
  3. Envoyer les logs vers **Kinesis Firehose** → **S3** → **Athena** pour les requêtes.

---

## **3. Astuces pour l'Examen**
### **🔎 Comment Repérer les Mauvaises Réponses ?**
| **Indice dans la Question**               | **Réponse à Éliminer**                          | **Pourquoi ?**                                                                 |
|-------------------------------------------|--------------------------------------------------|-------------------------------------------------------------------------------|
| **"Minimal cost"**                        | Réponses avec EC2 (vs Lambda/Fargate)           | Lambda/Fargate sont moins chers pour des tâches courtes.                     |
| **"High availability"**                   | Réponses sans Multi-AZ ou Auto Scaling          | Toujours vérifier la redondance.                                             |
| **"Automated rollback"**                  | Réponses sans CloudWatch Alarms                 | CodeDeploy a besoin d'alarmes pour les rollbacks.                          |
| **"Least privilege"**                     | Réponses avec `AdministratorAccess`             | Toujours limiter les permissions.                                             |
| **"Audit trail"**                          | Réponses sans CloudTrail ou Config               | Ces services sont obligatoires pour l'audit.                                |
| **"Real-time processing"**               | Réponses avec Batch (vs Kinesis/Lambda)         | Batch n'est pas temps réel.                                                   |
| **"Cross-account"**                       | Réponses sans IAM Roles ou Resource Access Manager | Les comptes AWS nécessitent des rôles ou RAM pour partager des ressources. |

---
### **⏳ Gestion du Temps**
- **2 min par question** : Si tu bloques, passe et reviens plus tard.
- **Marquer les questions** avec des **flags** pour les revoir.
- **Éliminer 2 réponses** clairement fausses pour augmenter tes chances.

---
### **📝 Notes Rapides à Retenir**
| **Service**       | **Cas d'Usage**                          | **Piège à Éviter**                          |
|-------------------|------------------------------------------|---------------------------------------------|
| **CodeDeploy**    | Déploiements EC2/ECS/Lambda              | Oublier les **hooks** (`ValidateService`). |
| **Step Functions** | Workflows longs (>15 min)                | Utiliser Lambda à la place.                |
| **Secrets Manager** | Rotation auto des secrets               | Utiliser Parameter Store.                  |
| **Aurora Global DB** | Bases globales low-latency             | Confondre avec DynamoDB Global Tables.     |
| **SSM Automation** | Runbooks pour remédiation automatique | Oublier les **permissions IAM**.           |

---
## **4. Ressources pour Pratiquer**
### **📚 Exams Blancs Recommandés**
1. **Tutorials Dojo** ([Lien](https://portal.tutorialsdojo.com/)) :
   - Exams réalistes avec explications détaillées.
2. **Jon Bonso / Udemy** ([Lien](https://www.udemy.com/course/aws-certified-devops-engineer-professional-practice-exams/)) :
   - 6 exams blancs + 390 questions.
3. **AWS Skill Builder** ([Lien](https://explore.skillbuilder.aws/)) :
   - Exams officiels AWS (payants mais hautement recommandés).

---
### **🛠 Labs Pratiques**
1. **AWS Workshops** ([Lien](https://workshops.aws/)) :
   - Labs gratuits sur CodePipeline, Step Functions, etc.
2. **Qwiklabs** ([Lien](https://www.qwiklabs.com/)) :
   - Scénarios réels (ex: "Automate a CI/CD Pipeline").

---
### **📖 Documentation Officielle**
- **FAQs AWS** : [Lien](https://aws.amazon.com/faqs/) (à lire pour chaque service clé).
- **Whitepapers** :
  - [Well-Architected Framework](https://aws.amazon.com/architecture/well-architected/)
  - [Security Best Practices](https://aws.amazon.com/whitepapers/)

---
## **5. Checklist Finale avant l'Examen**
✅ **Relire les pièges** pour chaque service (cf. tableau ci-dessus).
✅ **Faire 2-3 exams blancs** en conditions réelles (timer à 2 min/question).
✅ **Revoir les FAQs** de CodeDeploy, CloudFormation, Auto Scaling, IAM.
✅ **Comprendre les différences** :
   - CloudWatch Events vs CloudTrail.
   - Lambda vs Step Functions.
   - Parameter Store vs Secrets Manager.
✅ **Préparer une feuille de notes** avec :
   - Les **UpdatePolicy** pour CloudFormation.
   - Les **hooks** de CodeDeploy (`AfterInstall`, `ValidateService`).
   - Les **métriques CloudWatch** clés (`CPUUtilization`, `MillisBehindLatest`).

---
Voici une **version enrichie et structurée** de ta fiche récapitulative, incluant les **dernières questions (55 à 75)** avec leurs **pièges**, **bonnes pratiques**, et **cas d'usage**. J'ai aussi ajouté des **tableaux synthétiques** pour faciliter la mémorisation et des **astuces pour l'examen**.

---

## **📌 Fiche Récapitulative Ultime : AWS DevOps Pro (DOP-C02)**
*(Avec pièges détaillés, cas d'usage, et astuces pour l'examen)*

---

## **1. Pièges et Bonnes Pratiques par Service (Nouveaux Ajouts)**
### **🔹 OpsWorks (Configuration Management)**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Utiliser `setup` au lieu de `configure`** pour mettre à jour un cluster Cassandra. | `setup` ne s'exécute qu'au **premier lancement** de l'instance.                  | Utiliser le **hook `configure`** pour les mises à jour dynamiques (ex: ajout/suppression d'instances). |
| **Utiliser Lambda + SSH** pour mettre à jour les IPs des instances.     | Lambda ne peut pas **SSH** directement dans les instances EC2.                   | Utiliser **OpsWorks + Chef/Puppet** pour gérer les configurations dynamiques.     |
| **Utiliser User Data** pour des mises à jour dynamiques.                | User Data ne s'exécute qu'au **premier démarrage**.                                  | Préférer **OpsWorks** ou **SSM Run Command** pour les mises à jour continues.       |

---
### **🔹 Elastic Beanstalk (Déploiements)**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Modifier directement l'ALB** via la console AWS.                       | Les changements seront **écrasés** par Beanstalk.                                      | Utiliser **`.ebextensions`** pour configurer l'ALB (ex: redirections HTTP→HTTPS).  |
| **Déployer un Worker Tier dans le même environnement que le Web Tier**. | Risque de **conflits de ressources** et de latence.                                      | Créer un **environnement séparé** pour les Workers (ex: traitement asynchrone via SQS). |
| **Utiliser `container_commands` pour des tâches de configuration**.     | `container_commands` s'exécute **après** le déploiement de l'app.                     | Utiliser **`commands`** pour les tâches pré-déploiement (ex: installation de paquets). |

---
### **🔹 SSM (Gestion des Configurations)**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Utiliser SSM Parameter Store pour stocker des secrets**.              | Pas de **rotation automatique** des secrets.                                              | Utiliser **Secrets Manager** pour les mots de passe et clés API.                  |
| **Ne pas installer l'agent SSM** sur les instances on-premise.         | Impossible de gérer les instances **hybrides** sans l'agent.                          | Toujours installer l'agent SSM via **User Data** ou **Ansible/Chef**.              |
| **Utiliser SSM Automation pour des workflows complexes**.               | SSM Automation est limité aux **tâches linéaires**.                                       | Utiliser **Step Functions** pour les workflows avec logique conditionnelle.       |

---
### **🔹 Step Functions (Orchestration)**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Utiliser Lambda pour des workflows >15 min**.                         | Timeout max de **15 min** pour Lambda.                                                   | Utiliser **Step Functions** pour les workflows longs (ex: sauvegardes EBS multi-régions). |
| **Oublier les `Catch` et `Retry`** dans les workflows.                 | Risque de **blocage** en cas d'erreur.                                                   | Toujours inclure des **états `Catch`** et des **politiques de retry**.           |
| **Ne pas auditer les exécutions** de Step Functions.                   | Impossible de **retracer** les erreurs.                                                   | Utiliser **CloudTrail + CloudWatch Logs** pour l'audit.                            |

---
### **🔹 CloudTrail & Config (Audit)**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Utiliser CloudTrail sans l'activer dans toutes les régions**.       | Perte de visibilité sur les **appels API cross-régions**.                               | Activer CloudTrail **multi-régions** via AWS Organizations.                      |
| **Vérifier l'intégrité des logs CloudTrail avec S3 Versioning**.       | Ne garantit pas que les logs n'ont pas été **altérés**.                                  | Utiliser **CloudTrail Log File Validation** (`aws cloudtrail validate-logs`).     |
| **Utiliser AWS Config pour détecter les fuites de credentials**.        | Config ne **surveille pas les appels API**.                                              | Utiliser **CloudTrail + AWS Health** (événement `AWS_RISK_CREDENTIALS_EXPOSED`).   |

---
### **🔹 ECS & ECR (Conteneurs)**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Utiliser `latest` comme tag Docker**.                                  | Impossible de **tracker les versions**.                                                  | Utiliser des **hashes SHA** ou des tags sémantiques (ex: `v1.2.3`).                |
| **Ne pas configurer le `awslogs` driver** pour les logs ECS.            | Les logs ne sont pas **centralisés** dans CloudWatch.                                     | Toujours inclure `awslogs` dans la **task definition**.                      |
| **Oublier de mettre à jour le `task definition`** avant un déploiement. | Risque de déployer une **ancienne version** du conteneur.                                 | Toujours **pousser une nouvelle révision** dans ECR avant le déploiement.         |

---
### **🔹 CodePipeline (CI/CD)**
| **Piège**                                                                 | **Pourquoi c'est faux ?**                                                                 | **Bonne Pratique**                                                                 |
|--------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------|
| **Exécuter les tests en série** dans CodePipeline.                      | Augmente le **temps total** du pipeline.                                                  | Utiliser le même **`runOrder`** pour exécuter les tests en **parallèle**.          |
| **Ne pas utiliser `BeforeAllowTraffic`** pour les déploiements Lambda. | Risque de **servir du traffic** avant que le backend ne soit prêt.                     | Toujours vérifier la **disponibilité du backend** avant de basculer le traffic.    |
| **Stocker les artefacts dans ECR au lieu de S3**.                       | CodePipeline **nécessite S3** pour les artefacts.                                       | Toujours utiliser **S3** pour les artefacts de build.                           |

---
## **2. Cas d'Usage Classiques (Questions 55-75)**
### **🔸 Question 55 : Mise à jour dynamique d'un cluster Cassandra (OpsWorks)**
- **Scénario** : Un cluster Cassandra sur EC2 doit mettre à jour automatiquement la liste des IPs des autres instances.
- **Piège** : Utiliser **Lambda + SSH** ou **User Data**.
- **Solution** :
  - Utiliser **OpsWorks** avec un **Chef Cookbook** sur l'événement `configure`.
  - Le hook `configure` s'exécute à chaque **ajout/suppression d'instance**.
  ```yaml
  # .ebextensions/cassandra.config
  commands:
    update_ips:
      command: "chef-solo -c /etc/chef/solo.rb -j /etc/chef/dna.json"
      cwd: "/tmp"
  ```

---
### **🔸 Question 56 : Configuration ALB via `.ebextensions` (Elastic Beanstalk)**
- **Scénario** : Ajouter une redirection HTTP→HTTPS à un ALB géré par Beanstalk.
- **Piège** : Modifier l'ALB directement via la console AWS.
- **Solution** :
  - Créer un fichier `.ebextensions/alb.config` avec `option_settings` :
  ```yaml
  option_settings:
    aws:elbv2:listener:default:
      Rules:
        - Priority: 1
          Conditions:
            - Field: "host-header"
              Values: ["example.com"]
          Actions:
            - Type: "redirect"
              RedirectConfig:
                Protocol: "HTTPS"
                Port: "443"
  ```

---
### **🔸 Question 58 : Inventaire des logiciels avec SSM**
- **Scénario** : Générer une liste des logiciels installés sur toutes les instances EC2.
- **Piège** : Utiliser **AWS Inspector** ou **Lambda seul**.
- **Solution** :
  1. Installer l'**agent SSM** sur toutes les instances.
  2. Utiliser **SSM Inventory** pour collecter les métadonnées et les envoyer à **S3**.
  3. Créer un **CloudWatch Event** pour notifier les instances non conformes.

---
### **🔸 Question 60 : Logs ECS vers CloudWatch**
- **Scénario** : Centraliser les logs des conteneurs ECS dans CloudWatch.
- **Piège** : Utiliser un **sidecar conteneur** avec le CloudWatch Agent.
- **Solution** :
  - Configurer le **driver `awslogs`** dans la task definition :
  ```json
  "logConfiguration": {
    "logDriver": "awslogs",
    "options": {
      "awslogs-group": "/ecs/my-app",
      "awslogs-region": "us-east-1",
      "awslogs-stream-prefix": "ecs"
    }
  }
  ```
  - Donner un **IAM Role** à l'instance EC2 pour écrire dans CloudWatch.

---
### **🔸 Question 64 : Gestion des versions d'API (API Gateway + Lambda)**
- **Scénario** : Supporter des clients anciens et nouveaux avec une API Gateway.
- **Piège** : Créer une **nouvelle fonction Lambda** pour la v2.
- **Solution** :
  1. Déployer une **nouvelle version** de la fonction Lambda.
  2. Créer un **nouveau stage v2** dans API Gateway.
  3. Utiliser un **mapping template** pour ajouter `"color": "none"` aux requêtes v1.

---
### **🔸 Question 67 : Notifications OpsWorks (Auto-Healing)**
- **Scénario** : Recevoir des notifications Slack quand OpsWorks remplace une instance.
- **Piège** : Utiliser **SNS directement** ou **CloudTrail**.
- **Solution** :
  - Créer un **CloudWatch Event** pour l'événement `aws.opsworks` avec `initiated_by: auto-healing`.
  - Cibler une **Lambda** qui envoie une notification à Slack.

---
### **🔸 Question 70 : Sauvegardes EBS Multi-Régions (Step Functions)**
- **Scénario** : Créer des snapshots EBS, les copier dans une autre région, et notifier par email.
- **Piège** : Utiliser **Lambda seul** ou **SSM Automation**.
- **Solution** :
  1. Créer un **workflow Step Functions** avec :
     - Une étape pour créer le snapshot.
     - Une étape pour le copier dans une autre région (avec gestion des erreurs).
     - Une étape pour envoyer un email via SNS.
  2. Déclencher le workflow via **CloudWatch Events**.

---
### **🔸 Question 74 : Mise à jour des AMIs Golden (SSM + CloudFormation)**
- **Scénario** : Mettre à jour 100 environnements Beanstalk avec une nouvelle AMI golden chaque semaine.
- **Piège** : Modifier manuellement chaque template CloudFormation.
- **Solution** :
  1. Stocker l'**AMI ID** dans **SSM Parameter Store**.
  2. Référencer ce paramètre dans les templates CloudFormation :
     ```yaml
     Parameters:
       GoldenAMI:
         Type: AWS::SSM::Parameter::Value<AWS::EC2::Image::Id>
         Default: /myapp/golden-ami
     ```
  3. Utiliser un **CloudWatch Event** pour déclencher une mise à jour hebdomadaire.

---
## **3. Astuces pour l'Examen (Nouveaux Ajouts)**
### **🔎 Comment Repérer les Mauvaises Réponses ?**
| **Indice dans la Question**               | **Réponse à Éliminer**                          | **Pourquoi ?**                                                                 |
|-------------------------------------------|--------------------------------------------------|-------------------------------------------------------------------------------|
| **"Minimal effort"**                      | Réponses avec des **scripts manuels** (ex: CRON sur EC2). | Préférer les services **serverless** (Step Functions, Lambda).              |
| **"Highly available"**                    | Réponses sans **Multi-AZ** ou **Auto Scaling**. | Toujours vérifier la redondance et la scalabilité.                        |
| **"Automated remediation"**              | Réponses sans **CloudWatch Alarms** ou **SSM Automation**. | Ces services sont **obligatoires** pour la remédiation automatique.      |
| **"Cross-account"**                       | Réponses sans **IAM Roles** ou **RAM**.          | Les comptes AWS nécessitent des **rôles** ou **Resource Access Manager**.   |
| **"Real-time processing"**                | Réponses avec **Batch** ou **EC2 seul**.        | Utiliser **Kinesis**, **Lambda**, ou **Step Functions**.                   |

---
### **⏳ Gestion du Temps (Stratégie)**
1. **2 min par question** : Si tu bloques, **marque la question** et passe.
2. **Éliminer 2 réponses** clairement fausses pour augmenter tes chances.
3. **Revenir aux questions marquées** à la fin.
4. **Vérifier les réponses "trop simples"** (ex: "Utiliser EC2 pour tout").

---
### **📝 Notes Rapides à Retenir (Nouveaux Ajouts)**
| **Service**               | **Cas d'Usage**                          | **Piège à Éviter**                          | **Bonne Pratique**                          |
|---------------------------|------------------------------------------|---------------------------------------------|---------------------------------------------|
| **OpsWorks**              | Gestion de clusters (ex: Cassandra).     | Utiliser `setup` pour les mises à jour.     | Utiliser le hook `configure`.              |
| **Elastic Beanstalk**     | Déploiements Web/Worker.                | Modifier l'ALB directement.                | Utiliser `.ebextensions`.                  |
| **SSM Inventory**         | Audit des logiciels installés.          | Utiliser AWS Inspector.                    | Utiliser **SSM Inventory + S3**.           |
| **Step Functions**        | Workflows longs/complexes.               | Utiliser Lambda seul.                       | Toujours inclure `Catch` et `Retry`.        |
| **ECS Logs**              | Centralisation des logs.                 | Oublier le driver `awslogs`.                | Configurer `awslogs` dans la task def.     |
| **API Gateway**           | Versioning d'API.                        | Créer une nouvelle Lambda pour la v2.       | Utiliser des **stages + mapping templates**. |

---
## **4. Ressources pour Pratiquer**
### **📚 Exams Blancs Recommandés**
1. **Tutorials Dojo** : [6 exams blancs](https://portal.tutorialsdojo.com/) avec explications détaillées.
2. **Jon Bonso / Udemy** : [390 questions](https://www.udemy.com/course/aws-certified-devops-engineer-professional-practice-exams/).
3. **AWS Skill Builder** : [Exams officiels](https://explore.skillbuilder.aws/) (payants mais réalistes).

---
### **🛠 Labs Pratiques**
1. **AWS Workshops** : [Labs gratuits](https://workshops.aws/) (ex: CI/CD avec CodePipeline).
2. **Qwiklabs** : [Scénarios réels](https://www.qwiklabs.com/) (ex: "Automate a CI/CD Pipeline").

---
### **📖 Documentation Officielle**
- **FAQs AWS** : [À lire pour chaque service](https://aws.amazon.com/faqs/).
- **Whitepapers** :
  - [Well-Architected Framework](https://aws.amazon.com/architecture/well-architected/)
  - [Security Best Practices](https://aws.amazon.com/whitepapers/)

---
## **5. Checklist Finale avant l'Examen**
✅ **Relire les pièges** pour OpsWorks, Elastic Beanstalk, SSM, Step Functions.
✅ **Faire 2-3 exams blancs** en conditions réelles (timer strict).
✅ **Revoir les FAQs** de CodeDeploy, CloudFormation, Auto Scaling, IAM.
✅ **Comprendre les différences** :
   - `setup` vs `configure` (OpsWorks).
   - `awslogs` vs `sidecar` (ECS).
   - `BeforeAllowTraffic` vs `AfterAllowTraffic` (CodeDeploy).
✅ **Préparer une feuille de notes** avec :
   - Les **hooks** de CodeDeploy (`BeforeInstall`, `AfterAllowTraffic`).
   - Les **mappings** pour API Gateway (ex: redirections HTTP→HTTPS).
   - Les **politiques IAM** pour SSM/Secrets Manager.

---
### **💡 Dernière Question pour Toi, Nicolas :**
**Quels sont les 2-3 sujets où tu veux des exercices ciblés ?**
*(Exemples : "Step Functions pour les workflows EBS", "OpsWorks pour Cassandra", "Elastic Beanstalk + ALB", etc.)*
Je peux te fournir :
- Des **scénarios détaillés** pour t'entraîner.
- Des **exemples de questions d'examen** sur ces topics.
- Des **labs pratiques** pour renforcer tes connaissances.

*(Réponds avec tes priorités, et je te prépare un plan sur mesure !)* 😊