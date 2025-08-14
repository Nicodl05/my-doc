

## **Amazon EC2 (Elastic Compute Cloud)**

### **Définition**  
Amazon EC2 est un service de calcul basé sur le cloud qui permet de louer et gérer des serveurs virtuels, appelés instances, pour héberger des applications. Il offre une flexibilité et une élasticité pour ajuster les ressources selon les besoins de l'utilisateur.

---

### **Concepts clés à maîtriser**

#### **1. Scalabilité**
- **Définition** : La capacité à ajuster les ressources d'un système en fonction de la charge de travail.  
- **Types** :
  - **Scalabilité verticale** : Augmentation des capacités d'une instance unique (plus de CPU, RAM, etc.).
  - **Scalabilité horizontale** : Ajout ou suppression d'instances pour répartir la charge.  
- **Exemple** :
  - Une application e-commerce peut augmenter la puissance d'une instance (vertical) ou ajouter plusieurs serveurs en cas de pic de trafic (horizontal).

---

#### **2. Zones de disponibilité (AZ - Availability Zones)**
- **Définition** : Datacenters physiques distincts, interconnectés au sein d'une région AWS.  
- **Caractéristiques** :
  - Chaque région AWS contient plusieurs zones de disponibilité.
  - Les zones sont isolées pour éviter les pannes simultanées.  
- **Exemple** : 
  - Une application déployée dans la région **"eu-west-1"** peut utiliser plusieurs zones comme **"eu-west-1a"** et **"eu-west-1b"** pour garantir une haute disponibilité.

---

#### **3. Auto Scaling**
- **Définition** : Service qui ajuste automatiquement le nombre d'instances EC2 en fonction de critères prédéfinis, comme la charge CPU ou le trafic.  
- **Avantages** :
  - Réduit les coûts en diminuant les ressources inutilisées.
  - Améliore les performances en ajoutant des ressources pendant les pics de trafic.  
- **Exemple** : 
  - Une entreprise configure l'Auto Scaling pour ajouter une instance lorsque l'utilisation du CPU dépasse 80 %, et en supprimer une lorsque l'utilisation descend en dessous de 20 %.

---

#### **4. Elastic Load Balancer (ELB)**
- **Définition** : Service qui répartit automatiquement le trafic entrant entre plusieurs instances EC2.  
- **Fonctions** :
  - Améliore la disponibilité et la tolérance aux pannes.
  - Prend en charge plusieurs protocoles : HTTP, HTTPS, TCP.  
- **Exemple** :
  - Un site web utilisant plusieurs instances EC2 derrière un ELB garantit que les utilisateurs sont dirigés vers une instance disponible, même si une autre est hors ligne.

---

#### **5. Elastic Block Store (EBS)**
- **Définition** : Un service de stockage persistant utilisé avec les instances EC2.  
- **Caractéristiques** :
  - Attaché à une seule instance à la fois, mais peut être détaché et réutilisé.
  - Différents types de disques disponibles :
    - **SSD** : Pour des besoins rapides (lecture/écriture intensive).
    - **HDD** : Pour des transferts séquentiels de données.  
- **Exemple** :
  - Une base de données MySQL hébergée sur EC2 utilise un volume EBS comme disque principal pour stocker les données.

---

#### **6. Security Groups**
- **Définition** : Pare-feu virtuel pour les instances EC2, qui contrôle le trafic entrant et sortant basé sur des règles définies par l'utilisateur.  
- **Fonctions** :
  - Limiter l'accès à l'instance selon les adresses IP ou les ports.
  - Les règles sont **stateful** : si une connexion est autorisée en entrée, elle est automatiquement autorisée en sortie.  
- **Exemple** :
  - Un serveur web utilise un Security Group pour autoriser uniquement le trafic HTTP (port 80) et SSH (port 22) depuis des adresses IP spécifiques.

---

#### **7. Elastic IP**
- **Définition** : Une adresse IP publique fixe attribuée à une instance EC2.  
- **Utilité** :
  - Maintient une adresse IP constante, même si l'instance sous-jacente change.  
- **Exemple** :
  - Une application web critique attribue une Elastic IP pour s'assurer que ses utilisateurs peuvent toujours y accéder à la même adresse, même après un redémarrage de l'instance.

---

#### **8. Amazon Machine Images (AMI)**
- **Définition** : Modèle qui contient la configuration d’une instance EC2 (système d’exploitation, applications, et paramètres).  
- **Fonctions** :
  - Permet de lancer des instances rapidement avec des configurations prédéfinies.
  - Possibilité de créer des AMI personnalisées.  
- **Exemple** :
  - Une entreprise crée une AMI avec un serveur web Apache préconfiguré et l’utilise pour déployer rapidement plusieurs instances.

---

### **Étapes pratiques d’utilisation d’EC2**

1. **Création d'une instance EC2**
   - Choisissez une **AMI** (ex. : Ubuntu 22.04).  
   - Sélectionnez un **type d’instance** (ex. : t2.micro).  
   - Configurez le **Security Group** pour autoriser les ports nécessaires (ex. : 80 et 22).  

2. **Ajout de stockage**
   - Attachez un volume **EBS** pour le stockage persistant.  

3. **Assignation d'une Elastic IP**
   - Associez une Elastic IP à l'instance pour lui donner une adresse publique statique.  

4. **Surveillance et gestion**
   - Configurez **Auto Scaling** pour gérer la charge automatiquement.  
   - Ajoutez un **ELB** pour équilibrer le trafic entre plusieurs instances.  

---

### **Exemple concret**

#### **Contexte** :  
Une startup lance une plateforme e-commerce et veut garantir des performances optimales même pendant des pics de trafic.

#### **Solution avec EC2** :
1. **Architecture** :
   - 3 instances EC2 réparties dans des zones de disponibilité différentes.  
   - Elastic Load Balancer pour gérer le trafic entrant.  
   - Auto Scaling configuré pour ajouter ou supprimer des instances selon le trafic.  

2. **Configuration** :
   - AMI Ubuntu Server avec un serveur web préinstallé.  
   - Volumes EBS pour stocker les données des commandes.  
   - Security Group autorisant les ports HTTP (80) et HTTPS (443).  

3. **Résultat** :
   - Pendant les soldes, Auto Scaling ajoute des instances pour répondre à la demande, et l'ELB répartit le trafic entre elles. Après les soldes, les ressources inutilisées sont automatiquement supprimées pour réduire les coûts.

---

### **Récapitulatif des termes**

| Terme            | Fonction                                                                                                                                 |
|-------------------|-----------------------------------------------------------------------------------------------------------------------------------------|
| **Scalabilité**   | Ajuster les ressources (verticalement ou horizontalement) pour répondre à la charge.                                                   |
| **AZ (Zone de disponibilité)** | Datacenters physiques isolés dans une région AWS.                                                                           |
| **Auto Scaling**  | Service ajustant automatiquement le nombre d'instances en fonction des besoins.                                                        |
| **ELB (Elastic Load Balancer)** | Répartit le trafic entre plusieurs instances pour améliorer la disponibilité et les performances.                           |
| **EBS (Elastic Block Store)** | Stockage persistant attaché aux instances EC2.                                                                              |
| **Security Groups** | Contrôle le trafic réseau autorisé pour les instances.                                                                                |
| **Elastic IP**    | Adresse IP publique fixe associée à une instance.                                                                                      |
| **AMI (Amazon Machine Image)** | Modèle préconfiguré pour lancer des instances rapidement.                                                                    |
