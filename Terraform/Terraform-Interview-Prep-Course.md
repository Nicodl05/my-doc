# Cours Complet Terraform — Préparation Entretien Technique

Version: 1.0

Remarque: Ce cours couvre Terraform pour provisionner et gérer l'infrastructure (IaC). Il est orienté préparation d'entretien (concepts, commandes, patterns, bonnes pratiques, scénarios, labs).

Assumptions:
- Connaissances de base en cloud (AWS/Azure/GCP) et CLI.
- Terraform v1.0+ recommandé (idéalement 1.3+ ou 1.5 pour features récentes).

Plan du cours
- Introduction & Glossaire
- Architecture Terraform: providers, core, state, backends
- Configuration HCL: resources, data, variables, outputs, locals
- Modules: structure, registry, composition
- State management: backends (S3/GCS/Azure), locking, remote state
- Workspaces & env strategies
- Lifecycle, provisioners, meta-arguments
- Expressions & interpolation, for_each, count
- Provisioning patterns: immutable infra, blue/green, canary
- Testing & CI: `terraform validate`, `plan`, `fmt`, `tflint`, `terratest`
- Security & secrets: protect state, sensitive variables, avoid hardcoding
- Drift detection & state reconciliation
- Troubleshooting & debugging
- Labs pratiques: AWS simple infra, module composition, remote state
- Questions d'entretien + réponses
- Cheat-sheet terraform commands & CI examples

=================================================================

**Introduction & Glossaire**

- Terraform: outil IaC déclaratif développé par HashiCorp. Écrit en HCL (HashiCorp Configuration Language).
- Provider: plugin qui traduit ressources Terraform vers API du fournisseur (AWS, GCP, Azure, kubernetes...).
- Resource: unité de gestion (ex: `aws_instance`, `azurerm_resource_group`).
- State: fichier (local ou remote) représentant l'état actuel connu des ressources gérées.
- Backend: mécanisme de stockage du state (local, S3, GCS, AzureRM, remote). Supporte locking.

=================================================================

**Module 1 — Architecture & flux**

1. `terraform init` installe providers et configure le backend.
2. `terraform plan` compare configuration vs state et affiche actions.
3. `terraform apply` exécute les actions pour atteindre l'état désiré.
4. `terraform destroy` supprime les ressources gérées.

Best practices: gardez le state remote et verrouillé en backend; ne committez jamais le state contenant secrets.

=================================================================

**Module 2 — HCL: resources, variables, outputs, locals**

- `variable`: déclarer entrées; `default`, `type`, `sensitive`.
- `output`: exposer valeurs (ex: IPs, ARNs).
- `locals`: expressions locales pour éviter duplication.
- `data` source: lire ressources externes sans les gérer.

Exemple:
```
resource "aws_instance" "web" {
  ami           = var.ami_id
  instance_type = var.instance_type
  tags = { Name = "web" }
}
```

=================================================================

**Module 3 — Modules**

- Modules: unités réutilisables; peuvent provenir du Registry, d'un repo Git, ou d'un chemin local.
- Structure recommandée: `main.tf`, `variables.tf`, `outputs.tf`, `README.md`.
- Versionner modules et utiliser `source` avec une version tag (git or registry).

=================================================================

**Module 4 — State & Backend**

- Stocker state dans un backend remote (S3 + DynamoDB lock pour AWS) pour collaboration.
- Verrouillage: DynamoDB (AWS) ou Cloud Storage Locks pour éviter write contention.
- Sensitive data: marquer variables `sensitive = true`, chiffrer backend (ex: S3 + KMS).

Commands:
```
terraform init
terraform plan -out=tfplan
terraform apply tfplan
terraform fmt
terraform validate
```

=================================================================

**Module 5 — Workspaces & env strategies**

- Workspaces: multiple states for same config (not a full multi-environment solution). Prefer folder-per-env or multiple backend states for production.

=================================================================

**Module 6 — Lifecycle & meta-arguments**

- `count` vs `for_each` for resource repetition.
- `depends_on`, `lifecycle { create_before_destroy, prevent_destroy }`.
- `provisioner`: `local-exec` / `remote-exec` (use sparingly).

=================================================================

**Module 7 — Expressions & iteration**

- `for_each` supports maps/sets; `count` uses index-based.
- `merge()`, `concat()`, `lookup()` utiles pour composition.

=================================================================

**Module 8 — CI/CD, testing & best practices**

- CI pipeline steps: `terraform fmt`, `terraform init -backend=false`, `terraform validate`, `terraform plan -out=plan`, `terraform show -json plan` for PR checks, push `plan` for apply job.
- Tools: `tflint`, `tfsec` (security), `terratest` for integration tests.

=================================================================

**Module 9 — Security**

- Ne jamais stocker secrets en clair dans variables.tf or state; use KMS/ Vault for secrets and encrypt backend.
- Lockdown access to state S3 buckets and DynamoDB lock table.

=================================================================

**Module 10 — Troubleshooting & drift**

- `terraform refresh` updates state from provider.
- `terraform plan` montre drift.
- `terraform state` subcommands for state inspection and moving resources.

=================================================================

**Module 11 — Labs pratiques**

Lab 1: Provisionner VPC + 2 subnets + EC2 instance (AWS) with remote S3 backend + DynamoDB lock.
Lab 2: Ecrire un module `network` réutilisable et l'utiliser dans deux environnements.
Lab 3: Simuler drift (modifier ressource manuellement) et corriger via Terraform.

=================================================================

**Module 12 — Questions d'entretien & trucs rapides**

- Je fournirai un fichier séparé `Terraform/Terraform-Questions.md` avec Q/A (junior → senior).

**Questions d'entretien — Solutions complètes**

Junior (fondamentaux)
- Q1: Qu'est-ce que Terraform ?
  A1: Outil IaC déclaratif pour provisionner et gérer l'infrastructure via HCL.
- Q2: Que fait `terraform init` ?
  A2: Initialise le répertoire, télécharge les providers et configure le backend.
- Q3: Qu'est-ce que le state Terraform ?
  A3: Fichier JSON représentant l'état actuel géré; peut être local ou remote.
- Q4: Comment passer des variables à Terraform ?
  A4: Via `-var`, fichiers `.tfvars`, variables d'environnement `TF_VAR_` ou `terraform.tfvars`.
- Q5: Quelle commande pour formater la config ?
  A5: `terraform fmt`.

Intermédiaire (pratique & patterns)
- Q6: Pourquoi utiliser un backend remote ?
  A6: Collaboration, state partagé et verrouillage pour éviter conflits concurrentiels.
- Q7: Différence entre `count` et `for_each` ?
  A7: `count` produit instances indexées; `for_each` produit instances keyed (meilleur pour maps).
- Q8: Comment gérer secrets dans Terraform ?
  A8: Utiliser variables `sensitive=true`, stocker secrets dans Vault/KMS et référencer via provider, chiffrer backend.
- Q9: Qu'est-ce qu'un module ?
  A9: Bloc réutilisable d'infrastructure; peut être local, git ou registry.
- Q10: Que fait `terraform plan -out=plan` ?
  A10: Génère et enregistre le plan binaire qui peut être appliqué ensuite via `terraform apply plan`.

Senior (opérations & architecture)
- Q11: Comment mettre en place un backend S3 avec locking ?
  A11: Utiliser un bucket S3 pour le state et une table DynamoDB pour le lock; configurer le backend `s3` avec `dynamodb_table` et gérer IAM permissions.
- Q12: Comment traiter le drift ?
  A12: Exécuter `terraform plan` pour détecter le drift, puis `terraform apply` pour corriger; pour changements manuels complexes, utiliser `terraform import` ou modifier le state avec `terraform state`.
- Q13: Expliquez `terraform state mv` et cas d'utilisation.
  A13: Permet renommer/déplacer une ressource dans le state (p.ex. lors de refactorisation vers un module), évitant la recréation.
- Q14: Comment versionner providers et pourquoi ?
  A14: Pin providers dans `required_providers` pour garantir installations reproductibles et éviter breaking changes lors d'upgrades.
- Q15: Quelles sont les bonnes pratiques pour modules partagés ?
  A15: Petits API (inputs/outputs), versioning sémantique, tests (terratest), documentation et exemples d'usage.

=================================================================