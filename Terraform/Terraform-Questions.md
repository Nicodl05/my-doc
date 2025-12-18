# Terraform — Questions d'entretien (avec réponses concises)

Objectif: questions classées par niveau pour révision rapide.

---

**Junior (fondamentaux)**

Q1: Qu'est-ce que Terraform ?
A1: Outil IaC déclaratif pour provisionner et gérer l'infrastructure via HCL.

Q2: Que fait `terraform init` ?
A2: Initialise le répertoire, télécharge les providers et configure le backend.

Q3: Qu'est-ce que le state Terraform ?
A3: Fichier JSON représentant l'état actuel géré; peut être local ou remote.

Q4: Comment passer des variables à Terraform ?
A4: Via `-var`, fichiers `.tfvars`, variables d'environnement `TF_VAR_` ou `terraform.tfvars`.

Q5: Quelle commande pour formater la config ?
A5: `terraform fmt`.

---

**Intermédiaire (pratique & patterns)**

Q6: Pourquoi utiliser un backend remote ?
A6: Collaboration, state partagé et verrouillage pour éviter conflits concurrentiels.

Q7: Différence entre `count` et `for_each` ?
A7: `count` produit instances indexées; `for_each` produit instances keyed (meilleur pour maps).

Q8: Comment gérer secrets dans Terraform ?
A8: Utiliser variables `sensitive=true`, stocker secrets dans Vault/KMS et référencer via provider, chiffrer backend.

Q9: Qu'est-ce qu'un module ?
A9: Bloc réutilisable d'infrastructure; peut être local, git ou registry.

Q10: Que fait `terraform plan -out=plan` ?
A10: Génère et enregistre le plan binaire qui peut être appliqué ensuite via `terraform apply plan`.

---

**Senior (opérations & architecture)**

Q11: Comment mettre en place un backend S3 avec locking ?
A11: Utiliser S3 bucket pour state et DynamoDB table pour le lock; config backend `s3` avec `dynamodb_table`.

Q12: Comment traiter le drift ?
A12: Exécuter `terraform plan` pour détecter drift, `terraform apply` pour corriger ou manuellement reconciler et importer si nécessaire.

Q13: Expliquez `terraform state mv` et cas d'utilisation.
A13: Permet renommer / déplacer ressources dans le state (ex: refactorisation ou changement de module source).

Q14: Comment versionner providers et pourquoi ?
A14: Pin providers in `required_providers` to ensure reproducible installs and avoid breaking changes.

Q15: Quelles sont les bonnes pratiques pour modules partagés ?
A15: Petite surface d'inputs/outputs, versioning, tests (terratest), documentation et examples.

---