# Terraform — Définitions détaillées des concepts

But: fournir définitions complètes et exemples des principaux concepts Terraform pour préparation d'entretien.

1) HCL (HashiCorp Configuration Language)
- Langage déclaratif utilisé pour écrire fichiers Terraform (`.tf`). Lisible et conçu pour config d'infrastructure.

2) Provider
- Plugin qui traduit ressources Terraform vers API d'un fournisseur (ex: `hashicorp/aws`, `hashicorp/google`). Doit être configuré (`provider "aws" { region = var.region }`).

3) Resource
- Unité gérée par Terraform, ex: `aws_instance`, `azurerm_virtual_machine`.
- Bloc `resource "type" "name" { ... }` définit l'état désiré.

4) Data source
- `data` permet de lire informations existantes (ex: AMI lookup) sans les gérer.

5) Variables & Outputs
- `variable` déclare une entrée; `output` expose une valeur.
- Variables peuvent être passées via CLI (`-var`), fichiers (`.tfvars`), env vars (`TF_VAR_...`).

6) State
- Fichier JSON représentant l'état connu des ressources. Peut être local (`terraform.tfstate`) ou remote via backend.
- Important: contient parfois secrets; ne pas committer.

7) Backend
- Mécanisme de stockage du state (local, s3, gcs, azurerm, remote). Backends remote supportent souvent locking.

8) Locking
- Empêche écritures concurrentes du state (ex: DynamoDB lock pour S3 backend).

9) Workspaces
- Multiple states pour une même configuration; utile pour tests mais souvent moins idéal que per-env directories/backends.

10) Modules
- Unité réutilisable: `module "name" { source = "./modules/network" ... }`. Encouraged to version modules and keep interfaces small.

11) Lifecycle meta-arguments
- `lifecycle { create_before_destroy = true }` pour éviter downtime; `prevent_destroy` protège une ressource contre suppression accidentelle.

12) Count vs For_Each
- `count` génère instances indexées, `for_each` génère instances keyed; `for_each` better for maps/sets.

13) Provisioners
- `local-exec` and `remote-exec` : exécutés lors de create/destroy; use sparingly (not recommended for long-term config).

14) Interpolation & expressions
- `format()`, `join()`, `merge()`, `lookup()`; `try()` to handle optional values.

15) Sensitive values
- Mark variable `sensitive = true` to avoid printing values in CLI output; still stored in state unless using external secrets.

16) Terraform CLI commands (quick)
- `terraform init`, `terraform plan`, `terraform apply`, `terraform destroy`, `terraform fmt`, `terraform validate`, `terraform state`.

17) Remote state data source
- `terraform_remote_state` data source to read outputs from a remote state (useful for multi-stage stacks).

18) State management commands
- `terraform state list`, `terraform state show`, `terraform state mv`, `terraform state rm` for advanced manipulations.

19) Drift
- Quand la configuration réelle diffère du state; `terraform plan` détecte drift; `terraform refresh` updates local state from provider.

20) Provider versioning
- Pin providers in `required_providers` block and use `terraform` block to set required_version for stability.

21) Lock file
- `.terraform.lock.hcl` records provider checksums for reproducible installs.

22) Backward compatibility & upgrades
- `terraform 0.12upgrade` (historical); prefer reading changelogs and upgrade guides; test in isolated env.

23) Testing & linting
- `tflint`, `tfsec` for linting/security; `terratest` (Go) for integration tests; `kitchen-terraform` as alternative.

24) State encryption
- Encrypt state at rest (S3 + SSE-KMS, GCS CMEK) and restrict access with IAM.

25) Best practices
- Remote backend with locking; small modules; versioned modules; keep configs idempotent; keep secrets out of state or use external secrets stores.

---

Exemples rapides (main.tf, variables, outputs):

main.tf (extrait):
```
terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = ">= 4.0"
    }
  }
}

provider "aws" {
  region = var.region
}

resource "aws_vpc" "main" {
  cidr_block = var.vpc_cidr
  tags = { Name = "main-vpc" }
}
```

variables.tf (extrait):
```
variable "region" {
  type    = string
  default = "us-east-1"
}
variable "vpc_cidr" {
  type    = string
  default = "10.0.0.0/16"
}
```

outputs.tf (extrait):
```
output "vpc_id" {
  value = aws_vpc.main.id
}
```
