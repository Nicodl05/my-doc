# AWS Produits


### **1. Amazon EC2 (Elastic Compute Cloud)**

#### **1. Définition**  
Amazon EC2 est un service d'infrastructure en tant que service (IaaS) qui fournit des serveurs virtuels (instances) sur lesquels exécuter vos applications.

#### **2. Fonction principale**  
Héberger des machines virtuelles avec des systèmes d'exploitation configurables pour exécuter des applications, des services ou des bases de données.

#### **3. Fonctionnalités clés**  
- **Types d'instances** : Optimisées pour mémoire, calcul, stockage, etc.  
- **Auto Scaling** : Ajuste automatiquement le nombre d'instances en fonction de la charge.  
- **Elastic IP** : Fournit des adresses IP publiques persistantes.  
- **Spot Instances** : Utilisation à faible coût basée sur la disponibilité des ressources AWS.

#### **4. Informations générales à savoir**  
- Facturation basée sur l’utilisation (temps d’exécution de l’instance).  
- Large variété de systèmes d’exploitation (Windows, Linux, etc.).  
- Intégration avec d’autres services AWS comme S3, RDS, et VPC.

#### **5. Cas d'utilisation courants**  
- Héberger des applications web ou des bases de données.  
- Exécuter des simulations ou des calculs intensifs.  
- Tester et développer des applications dans un environnement isolé.

#### **6. Exemple concret d'utilisation**  
Une startup de e-commerce utilise EC2 pour héberger son site web. Elle configure une instance T3.medium avec Ubuntu pour déployer un serveur web Apache et héberger son application Node.js.

---

### **2. Amazon S3 (Simple Storage Service)**

#### **1. Définition**  
Amazon S3 est un service de stockage d’objets qui permet de stocker et récupérer des données à tout moment via Internet.

#### **2. Fonction principale**  
Stockage sécurisé et scalable de fichiers et objets (images, vidéos, sauvegardes, etc.).

#### **3. Fonctionnalités clés**  
- **Classes de stockage** : S3 Standard, S3 Intelligent-Tiering, S3 Glacier.  
- **Versioning** : Garde un historique des modifications d’un objet.  
- **Encryption** : Chiffrement des données au repos et en transit.  
- **S3 Bucket Policies** : Contrôle d’accès détaillé sur les données.

#### **4. Informations générales à savoir**  
- Taille maximale d’un fichier : 5 To.  
- Données accessibles via une URL ou une API REST.  
- Haute durabilité (11 nines : 99,999999999 %).

#### **5. Cas d'utilisation courants**  
- Stocker des fichiers volumineux (vidéos, archives).  
- Héberger des sites web statiques.  
- Effectuer des sauvegardes automatiques.

#### **6. Exemple concret d'utilisation**  
Une entreprise média utilise S3 pour stocker ses vidéos promotionnelles. Chaque fichier est accessible via une URL publique, et les vidéos rarement consultées sont déplacées vers S3 Glacier pour réduire les coûts.

---

### **3. AWS Lambda**

#### **1. Définition**  
AWS Lambda est un service de calcul sans serveur (Serverless) qui exécute du code en réponse à des événements, sans avoir besoin de gérer des serveurs.

#### **2. Fonction principale**  
Exécuter des fonctions spécifiques en réponse à des événements (par exemple, ajouter une image dans un bucket S3).

#### **3. Fonctionnalités clés**  
- **Runtime divers** : Supporte plusieurs langages (Python, Node.js, Java, etc.).  
- **Triggers** : Connecté à des événements S3, DynamoDB, CloudWatch, etc.  
- **Facturation** : Basée uniquement sur le temps d’exécution.  

#### **4. Informations générales à savoir**  
- Temps maximum d’exécution d’une fonction : 15 minutes.  
- Ne convient pas pour des applications longues ou persistantes.  
- S’intègre parfaitement avec d’autres services AWS.

#### **5. Cas d'utilisation courants**  
- Automatisation des tâches (par exemple : redimensionner des images).  
- Backend d’une API REST.  
- Traitement des flux de données en temps réel.

#### **6. Exemple concret d'utilisation**  
Une entreprise utilise Lambda pour traiter les images téléchargées par ses utilisateurs sur S3. Une fonction Lambda redimensionne automatiquement chaque image et la déplace dans un dossier “Optimisé”.

---

### **4. Amazon RDS (Relational Database Service)**

#### **1. Définition**  
Amazon RDS est un service de base de données relationnelle géré qui prend en charge plusieurs moteurs de bases de données comme MySQL, PostgreSQL, Oracle, et SQL Server.

#### **2. Fonction principale**  
Simplifier la configuration, la gestion et la mise à l'échelle des bases de données relationnelles.

#### **3. Fonctionnalités clés**  
- **Backup automatique** : Restauration point-in-time possible.  
- **Multi-AZ Deployment** : Réplication des données sur plusieurs zones de disponibilité.  
- **Read Replicas** : Amélioration des performances en distribuant les lectures.

#### **4. Informations générales à savoir**  
- Facturation en fonction de l’utilisation (taille des données et type d’instance).  
- Parfait pour des applications nécessitant des bases SQL.  
- Intégration native avec des outils comme CloudWatch pour la surveillance.

#### **5. Cas d'utilisation courants**  
- Héberger des bases de données pour des applications web.  
- Migrer des bases existantes vers le cloud.  
- Répliquer des données pour des analyses en temps réel.

#### **6. Exemple concret d'utilisation**  
Une application de réservation d'hôtels utilise Amazon RDS avec PostgreSQL pour stocker les données des clients, des réservations et des hôtels. Les sauvegardes automatiques garantissent la récupération rapide en cas de problème.

---

### **5. Amazon CloudFront**

#### **1. Définition**  
Amazon CloudFront est un service de réseau de diffusion de contenu (CDN) qui accélère la distribution de contenu à vos utilisateurs.

#### **2. Fonction principale**  
Distribuer rapidement des contenus statiques ou dynamiques via un réseau mondial de serveurs.

#### **3. Fonctionnalités clés**  
- **Points de présence (PoPs)** : Réseau mondial pour une faible latence.  
- **TLS/SSL** : Sécurise le contenu.  
- **Support Lambda@Edge** : Permet l’exécution de fonctions proches des utilisateurs.

#### **4. Informations générales à savoir**  
- Idéal pour les sites ayant des utilisateurs dispersés géographiquement.  
- Compatible avec S3 pour accélérer les sites web statiques.  
- Facturation basée sur le volume de données transférées.

#### **5. Cas d'utilisation courants**  
- Accélérer un site web ou une application mobile.  
- Diffuser des vidéos en streaming.  
- Distribuer des API à faible latence.

#### **6. Exemple concret d'utilisation**  
Un site d’e-commerce configure CloudFront pour servir ses images de produit à partir de S3, réduisant le temps de chargement pour les clients à travers le monde.

---

Ces cinq exemples constituent un socle pour ton apprentissage. Souhaites-tu approfondir une autre technologie ou continuer avec d'autres services ? 😊