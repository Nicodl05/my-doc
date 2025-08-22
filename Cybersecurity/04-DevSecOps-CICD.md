# **DevSecOps - Fiche de Révision Entretien**

## **Concepts Fondamentaux**

### **Q: Qu'est-ce que "Shift-Left Security" ?**

**Définition :** Intégrer la sécurité dès les premières phases du développement, pas seulement en fin de cycle.

**Implémentation concrète :**
```
Traditional: Code → Build → Test → Deploy → Security
Shift-Left:  Security ← Code ← Build ← Test ← Deploy
```

**Benefits :**
- **Coût réduit** : Bug trouvé en dev = 10x moins cher qu'en prod
- **Time to market** : Pas de blocage en fin de cycle
- **Developer experience** : Feedback immédiat

### **Q: Quels sont les piliers du DevSecOps ?**

**Culture :** Shared responsibility pour la sécurité
**Automation :** Security gates automatisés dans CI/CD
**Measurement :** Métriques de sécurité continues
**Collaboration :** Dev, Sec, Ops travaillent ensemble

---

## **Pipeline CI/CD Sécurisé**

### **Q: Quelles vérifications de sécurité inclure dans un pipeline CI/CD ?**

**Security Gates par phase :**

1. **Pre-commit (IDE/Git hooks)**
   ```bash
   # Git pre-commit hook
   #!/bin/bash
   # Secret scanning
   trufflehog --regex --entropy=False .
   
   # Static analysis
   dotnet sonarscanner begin
   ```

2. **Build Phase**
   ```yaml
   # GitHub Actions example
   - name: Dependency Check
     uses: dependency-check/Dependency-Check_Action@main
     
   - name: SAST Scan
     uses: github/codeql-action/analyze@v2
     
   - name: Secret Scanning
     uses: trufflesecurity/trufflehog@main
   ```

3. **Container Phase**
   ```yaml
   - name: Container Scan
     uses: aquasecurity/trivy-action@master
     with:
       image-ref: 'myapp:${{ github.sha }}'
       severity: 'CRITICAL,HIGH'
   ```

4. **Deploy Phase**
   ```yaml
   - name: Infrastructure Scan
     uses: bridgecrewio/checkov-action@master
     with:
       directory: ./terraform
   ```

5. **Runtime (DAST)**
   ```yaml
   - name: DAST Scan
     uses: zaproxy/action-full-scan@v0.4.0
     with:
       target: 'https://test.myapp.com'
   ```

### **Q: Comment faire du "Security as Code" ?**

**Policy as Code avec OPA :**
```rego
# policy/security.rego
package kubernetes.security

# Deny privileged containers
deny[msg] {
    input.kind == "Pod"
    input.spec.containers[_].securityContext.privileged == true
    msg := "Privileged containers are not allowed"
}

# Require resource limits
deny[msg] {
    input.kind == "Pod"
    container := input.spec.containers[_]
    not container.resources.limits.memory
    msg := sprintf("Container %v missing memory limit", [container.name])
}
```

**Infrastructure Security avec Terraform :**
```hcl
# Enforce encryption
resource "aws_s3_bucket" "app_bucket" {
  bucket = "my-app-bucket"
  
  server_side_encryption_configuration {
    rule {
      apply_server_side_encryption_by_default {
        sse_algorithm = "AES256"
      }
    }
  }
}

# Deny public access
resource "aws_s3_bucket_public_access_block" "app_bucket_pab" {
  bucket = aws_s3_bucket.app_bucket.id
  
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}
```

---

## **Security Testing Automation**

### **Q: Quelle est la différence entre SAST, DAST, et IAST ?**

**SAST (Static Application Security Testing) :**
- **Quand** : Code source (white box)
- **Avantages** : Trouve vulnérabilités early, précis
- **Inconvénients** : Faux positifs, pas de contexte runtime
- **Outils** : SonarQube, CodeQL, Semgrep

**DAST (Dynamic Application Security Testing) :**
- **Quand** : Application running (black box)
- **Avantages** : Test réel, pas de faux positifs
- **Inconvénients** : Tardif, couverture limitée
- **Outils** : OWASP ZAP, Burp Suite

**IAST (Interactive Application Security Testing) :**
- **Quand** : Runtime avec instrumentation
- **Avantages** : Précis + contexte runtime
- **Inconvénients** : Performance impact
- **Outils** : Contrast Security, Synopsys

### **Q: Comment automatiser les tests de sécurité ?**

**Test Automation Strategy :**

1. **Unit Tests de sécurité**
   ```csharp
   [Test]
   public void PasswordHashing_ShouldUseSalt()
   {
       var password = "testpassword";
       var hash1 = _passwordService.HashPassword(password);
       var hash2 = _passwordService.HashPassword(password);
       
       // Même password, hash différents (salt unique)
       Assert.AreNotEqual(hash1, hash2);
   }
   ```

2. **Integration Tests**
   ```csharp
   [Test]
   public async Task API_WithoutAuth_ShouldReturn401()
   {
       var response = await _client.GetAsync("/api/secure-endpoint");
       Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
   }
   ```

3. **Security Load Testing avec K6**
   ```javascript
   import http from 'k6/http';
   import { check } from 'k6';
   
   export default function() {
       // Test SQL injection
       let maliciousPayload = "'; DROP TABLE users; --";
       let response = http.get(`https://api.test.com/users?id=${maliciousPayload}`);
       
       check(response, {
           'no_sql_injection': (r) => !r.body.includes('syntax error')
       });
   }
   ```

---

## **Container & Kubernetes Security**

### **Q: Comment sécuriser un Dockerfile ?**

**Secure Dockerfile Best Practices :**
```dockerfile
# ✅ Multi-stage build (image minimale)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["*.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ✅ Runtime image minimale
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# ✅ Créer utilisateur non-root
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser

# ✅ Installer uniquement le nécessaire
RUN apk add --no-cache ca-certificates && rm -rf /var/cache/apk/*

WORKDIR /app
COPY --from=build --chown=appuser:appgroup /app/publish .

# ✅ Changer vers utilisateur non-root
USER appuser

# ✅ Port non-privilégié
EXPOSE 8080

ENTRYPOINT ["dotnet", "MyApp.dll"]
```

**❌ Erreurs à éviter :**
- USER root (défaut)
- ADD au lieu de COPY
- Secrets dans les layers
- Packages inutiles installés

### **Q: Comment implémenter Pod Security Standards ?**

**Restricted Pod Security :**
```yaml
apiVersion: v1
kind: Pod
metadata:
  name: secure-app
spec:
  securityContext:
    runAsNonRoot: true
    runAsUser: 1000
    runAsGroup: 3000
    fsGroup: 2000
    seccompProfile:
      type: RuntimeDefault
      
  containers:
  - name: app
    image: myapp:latest
    securityContext:
      allowPrivilegeEscalation: false
      readOnlyRootFilesystem: true
      capabilities:
        drop:
        - ALL
      runAsNonRoot: true
      runAsUser: 1000
    
    resources:
      limits:
        memory: "128Mi"
        cpu: "500m"
      requests:
        memory: "64Mi" 
        cpu: "250m"
```

---

## **Secrets & Configuration Management**

### **Q: Comment gérer les secrets dans CI/CD ?**

**Secrets Management Strategy :**

1. **External Secret Stores**
   ```yaml
   # GitHub Actions avec Azure Key Vault
   - name: Get secrets from Key Vault
     uses: azure/get-keyvault-secrets@v1
     with:
       keyvault: "my-keyvault"
       secrets: 'database-password, api-key'
     id: secrets
   
   - name: Deploy with secrets
     env:
       DB_PASSWORD: ${{ steps.secrets.outputs.database-password }}
     run: ./deploy.sh
   ```

2. **Kubernetes External Secrets**
   ```yaml
   apiVersion: external-secrets.io/v1beta1
   kind: ExternalSecret
   metadata:
     name: app-secrets
   spec:
     secretStoreRef:
       name: vault-backend
       kind: SecretStore
     target:
       name: app-secrets
       creationPolicy: Owner
     data:
     - secretKey: password
       remoteRef:
         key: database
         property: password
   ```

3. **OIDC Authentication (no stored credentials)**
   ```yaml
   # GitHub Actions OIDC
   permissions:
     id-token: write
     contents: read
   
   steps:
   - name: Azure Login
     uses: azure/login@v1
     with:
       client-id: ${{ secrets.AZURE_CLIENT_ID }}
       tenant-id: ${{ secrets.AZURE_TENANT_ID }}
       subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
   ```

### **Q: Comment implémenter la rotation de secrets ?**

**Automated Secret Rotation :**
```csharp
public class SecretRotationService
{
    public async Task RotateSecretAsync(string secretName)
    {
        // 1. Generate new secret
        var newSecret = await GenerateNewSecretAsync();
        
        // 2. Store with new version
        await _keyVault.SetSecretAsync($"{secretName}-v2", newSecret);
        
        // 3. Update applications (rolling deployment)
        await UpdateApplicationSecretsAsync(secretName, "v2");
        
        // 4. Verify new secret works
        await VerifySecretHealthAsync(secretName, "v2");
        
        // 5. Deactivate old secret
        await _keyVault.UpdateSecretPropertiesAsync($"{secretName}-v1", 
            new SecretProperties { Enabled = false });
        
        // 6. Schedule deletion
        await ScheduleSecretDeletionAsync($"{secretName}-v1", TimeSpan.FromDays(7));
    }
}
```

---

## **Infrastructure as Code Security**

### **Q: Comment scanner l'Infrastructure as Code ?**

**IaC Security Scanning :**

1. **Terraform avec Checkov**
   ```bash
   # Scan Terraform files
   checkov -d . --framework terraform --output json
   
   # Custom policies
   checkov -d . --external-checks-dir ./custom-policies/
   ```

2. **CloudFormation avec cfn-lint**
   ```bash
   cfn-lint template.yaml
   cfn-guard validate --rules security-rules.guard --data template.yaml
   ```

3. **OPA Policies pour Kubernetes**
   ```rego
   package kubernetes.admission
   
   deny[msg] {
       input.request.kind.kind == "Pod"
       input.request.object.spec.hostNetwork == true
       msg := "Pods may not use host networking"
   }
   ```

### **Q: Comment implémenter GitOps sécurisé ?**

**Secure GitOps Workflow :**
```
Developer → PR → [Security Scan] → Merge → [Deploy] → Production
              ↓
    Policy Validation
    Secret Scanning  
    IaC Security Check
```

**ArgoCD Security Configuration :**
```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: secure-app
spec:
  source:
    repoURL: https://github.com/company/secure-app
    targetRevision: HEAD
    path: k8s/
  destination:
    server: https://kubernetes.default.svc
    namespace: production
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
    - CreateNamespace=false  # Prevent namespace creation
    - PrunePropagationPolicy=foreground
```

---

## **Compliance & Governance**

### **Q: Comment automatiser la compliance ?**

**Compliance as Code :**

1. **SOC 2 Automation**
   ```csharp
   public class SOC2ComplianceChecker
   {
       public async Task<bool> CheckCC61_LogicalAccess()
       {
           // Vérifier que tous les accès sont authentifiés
           var unauthenticatedEndpoints = await GetUnauthenticatedEndpointsAsync();
           return unauthenticatedEndpoints.Count == 0;
       }
       
       public async Task<bool> CheckCC62_EncryptionInTransit()
       {
           // Vérifier que toutes les communications utilisent TLS
           var unencryptedConnections = await GetUnencryptedConnectionsAsync();
           return unencryptedConnections.Count == 0;
       }
   }
   ```

2. **PCI DSS Requirements**
   ```bash
   # Check for credit card data patterns
   grep -r "\b4[0-9]{12}(?:[0-9]{3})?\b" ./src/
   
   # Verify encryption of stored data
   openssl enc -aes-256-cbc -d -in cardholder_data.enc
   ```

3. **GDPR Compliance**
   ```csharp
   public class GDPRComplianceService
   {
       public async Task<bool> VerifyDataProcessingLawfulness()
       {
           // Vérifier que chaque traitement a une base légale
           var processings = await GetDataProcessingsAsync();
           return processings.All(p => p.LegalBasis != null);
       }
   }
   ```

---

## **Monitoring & Incident Response**

### **Q: Comment monitorer la sécurité dans un pipeline DevOps ?**

**Security Monitoring Stack :**

1. **Pipeline Metrics**
   ```yaml
   # Prometheus metrics pour security gates
   security_scan_duration_seconds
   security_vulnerabilities_found_total{severity="critical"}
   deployment_security_score
   ```

2. **Runtime Security Monitoring**
   ```csharp
   // Security events dans les logs
   logger.LogWarning("Security event: {EventType} Pipeline: {Pipeline} Severity: {Severity}",
       "vulnerability_found", pipelineName, "HIGH");
   ```

3. **Alerting Rules**
   ```yaml
   # Prometheus alerting
   - alert: CriticalVulnerabilityFound
     expr: security_vulnerabilities_found_total{severity="critical"} > 0
     for: 0m
     labels:
       severity: critical
     annotations:
       summary: "Critical vulnerability found in pipeline"
   ```

### **Q: Comment gérer un incident de sécurité en production ?**

**Incident Response Process :**

1. **Detection & Alerting**
   - Automated monitoring détecte l'anomalie
   - Alert envoyée à l'équipe de sécurité

2. **Containment**
   ```bash
   # Isolation immédiate
   kubectl scale deployment compromised-app --replicas=0
   
   # Block malicious IPs
   kubectl apply -f network-policy-block.yaml
   ```

3. **Investigation**
   ```bash
   # Collect forensic data
   kubectl logs compromised-app-pod > incident-logs.txt
   kubectl describe pod compromised-app-pod > pod-details.txt
   ```

4. **Recovery**
   ```bash
   # Deploy clean version
   kubectl set image deployment/app container=app:clean-version
   
   # Verify health
   kubectl rollout status deployment/app
   ```

---

## **Questions Pièges Entretien**

### **Q: "DevSecOps ralentit-il le développement ?"**
**Réponse :** Non, si bien implémenté :
- **Shift-left** détecte problèmes plus tôt (moins cher à corriger)
- **Automation** élimine les goulots d'étranglement manuels
- **Feedback loops** rapides améliorent la qualité
- **Security debt** évitée = moins de hotfixes en production

### **Q: "Comment convaincre les développeurs d'adopter DevSecOps ?"**
**Stratégies :**
- **Developer experience** : Outils intégrés dans IDE
- **Education** : Formation sur les risques
- **Gamification** : Scores de sécurité, challenges
- **Responsibility sharing** : Pas de blâme, amélioration continue

### **Q: "Que faire si un scan trouve 1000 vulnérabilités ?"**
**Priorisation :**
1. **Triage** par criticité (CVSS score)
2. **Context** : Exploitable dans notre environnement ?
3. **Quick wins** : Patches faciles d'abord
4. **Acceptance** : Risk acceptance documenté pour le reste
5. **Technical debt** planning pour résoudre progressivement

---

## **Introduction à DevSecOps**

### Shift-Left Security
Le principe "Shift-Left" consiste à intégrer la sécurité dès les premières phases du développement.

```
Traditionnel:  Dev → Test → Deploy → Security Audit
DevSecOps:     Security ← → Dev ← → Test ← → Deploy
```

### Culture DevSecOps
```csharp
// Exemple de code review automatique avec sécurité
public class SecurityCodeReviewBot
{
    private readonly List<SecurityRule> _rules;
    
    public class SecurityRule
    {
        public string Name { get; set; }
        public string Pattern { get; set; }
        public SecurityLevel Level { get; set; }
        public string Message { get; set; }
    }
    
    public SecurityCodeReviewBot()
    {
        _rules = new List<SecurityRule>
        {
            new() 
            { 
                Name = "Hardcoded Password",
                Pattern = @"password\s*=\s*[""'][^""']+[""']",
                Level = SecurityLevel.Critical,
                Message = "Hardcoded password detected. Use secure configuration instead."
            },
            new() 
            { 
                Name = "SQL Injection Risk",
                Pattern = @"ExecuteRaw.*\+.*\$",
                Level = SecurityLevel.High,
                Message = "Potential SQL injection. Use parameterized queries."
            },
            new() 
            { 
                Name = "Weak Crypto",
                Pattern = @"MD5|SHA1|DES\b",
                Level = SecurityLevel.Medium,
                Message = "Weak cryptographic algorithm detected."
            }
        };
    }
    
    public async Task<SecurityReviewResult> ReviewCodeAsync(string filePath, string content)
    {
        var issues = new List<SecurityIssue>();
        
        foreach (var rule in _rules)
        {
            var matches = Regex.Matches(content, rule.Pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var lineNumber = GetLineNumber(content, match.Index);
                issues.Add(new SecurityIssue
                {
                    RuleName = rule.Name,
                    Level = rule.Level,
                    Message = rule.Message,
                    File = filePath,
                    Line = lineNumber,
                    Code = GetLineContent(content, lineNumber)
                });
            }
        }
        
        return new SecurityReviewResult
        {
            FilePath = filePath,
            Issues = issues,
            OverallRating = CalculateSecurityRating(issues)
        };
    }
}
```

---

## **Sécurité dans le pipeline CI/CD**

### GitHub Actions Security Pipeline

#### 1. Pipeline sécurisé complet
```yaml
name: Secure CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  security-scan:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    # 1. Secret scanning
    - name: Secret Scanning
      uses: trufflesecurity/trufflehog@main
      with:
        path: ./
        base: main
        head: HEAD
    
    # 2. Dependency check
    - name: Dependency Vulnerability Scan
      uses: dependency-check/Dependency-Check_Action@main
      with:
        project: 'my-project'
        path: '.'
        format: 'JSON'
    
    # 3. SAST (Static Application Security Testing)
    - name: SAST with CodeQL
      uses: github/codeql-action/init@v2
      with:
        languages: csharp
    
    - name: Build
      run: dotnet build
    
    - name: Perform CodeQL Analysis
      uses: github/codeql-action/analyze@v2
    
    # 4. Infrastructure as Code scanning
    - name: IaC Security Scan
      uses: bridgecrewio/checkov-action@master
      with:
        directory: ./infrastructure
        framework: terraform
        output_format: sarif
        output_file_path: checkov-report.sarif
    
    # 5. Container scanning
    - name: Build Docker Image
      run: |
        docker build -t ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }} .
    
    - name: Container Security Scan
      uses: aquasecurity/trivy-action@master
      with:
        image-ref: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }}
        format: 'sarif'
        output: 'trivy-results.sarif'
    
    - name: Upload SARIF results
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: trivy-results.sarif

  security-testing:
    needs: security-scan
    runs-on: ubuntu-latest
    steps:
    # 6. DAST (Dynamic Application Security Testing)
    - name: Deploy to Test Environment
      run: |
        # Deploy to isolated test environment
        kubectl apply -f k8s/test/ --namespace=security-testing
    
    - name: Wait for deployment
      run: kubectl wait --for=condition=available --timeout=300s deployment/app --namespace=security-testing
    
    - name: DAST with OWASP ZAP
      uses: zaproxy/action-full-scan@v0.4.0
      with:
        target: 'https://test.example.com'
        rules_file_name: '.zap/rules.tsv'
        cmd_options: '-a'
    
    # 7. Load testing avec monitoring sécurité
    - name: Security Load Testing
      run: |
        # Test de charge avec monitoring des métriques de sécurité
        k6 run --out prometheus load-test.js

  deploy:
    needs: [security-scan, security-testing]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
    - name: Deploy to Production
      run: |
        # Déploiement sécurisé avec vérifications
        ./scripts/secure-deploy.sh
```

#### 2. Policy as Code avec OPA
```yaml
# .github/workflows/policy-check.yml
name: Policy Validation

on:
  pull_request:
    paths:
    - 'policies/**'
    - 'k8s/**'

jobs:
  policy-validation:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Validate Policies
      uses: open-policy-agent/conftest-action@v0.1
      with:
        files: k8s/*.yaml
        policy: policies/
    
    - name: Security Policy Test
      run: |
        # Test des politiques de sécurité
        opa test policies/
```

### Azure DevOps Security Pipeline

```yaml
# azure-pipelines-security.yml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

stages:
- stage: SecurityScan
  displayName: 'Security Scanning'
  jobs:
  - job: StaticAnalysis
    displayName: 'Static Analysis'
    steps:
    - task: SonarCloudPrepare@1
      inputs:
        SonarCloud: 'SonarCloud'
        organization: 'myorg'
        scannerMode: 'MSBuild'
        projectKey: 'myproject'
    
    - task: DotNetCoreCLI@2
      inputs:
        command: 'build'
        projects: '**/*.csproj'
        arguments: '--configuration $(buildConfiguration)'
    
    - task: SonarCloudAnalyze@1
    
    - task: WhiteSource@21
      inputs:
        cwd: '$(System.DefaultWorkingDirectory)'
        projectName: 'MyProject'
    
    - task: CredScan@3
      inputs:
        toolMajorVersion: 'V2'
        scanFolder: '$(Build.SourcesDirectory)'
        debugMode: false

- stage: DynamicTesting
  displayName: 'Dynamic Security Testing'
  dependsOn: SecurityScan
  jobs:
  - deployment: DAST
    displayName: 'Dynamic Application Security Testing'
    environment: 'security-testing'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            inputs:
              azureSubscription: 'MySubscription'
              appType: 'webApp'
              appName: 'my-test-app'
          
          - task: SecurityCodeAnalysis-ToolInstaller@1
            inputs:
              toolset: 'all'
          
          - task: RunBinSkim@1
            inputs:
              InputType: 'Basic'
              Function: 'analyze'
```

---

## **Infrastructure as Code (IaC) Security**

### Terraform Security Scanning

#### 1. Configuration sécurisée
```hcl
# main.tf - Exemple de configuration sécurisée
terraform {
  required_version = ">= 1.0"
  
  backend "azurerm" {
    # Backend chiffré
    use_azuread_auth = true
  }
  
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

# Security group avec principe du moindre privilège
resource "azurerm_network_security_group" "web" {
  name                = "web-nsg"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  security_rule {
    name                       = "HTTPS"
    priority                   = 1001
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  # Bloquer tout le reste
  security_rule {
    name                       = "DenyAll"
    priority                   = 4096
    direction                  = "Inbound"
    access                     = "Deny"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }
}

# Key Vault avec configuration sécurisée
resource "azurerm_key_vault" "main" {
  name                = "kv-${random_string.suffix.result}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name           = "premium"

  # Sécurité renforcée
  purge_protection_enabled   = true
  soft_delete_retention_days = 90
  
  # Accès réseau restreint
  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
    
    ip_rules = [
      "203.0.113.0/24", # IP autorisées
    ]
    
    virtual_network_subnet_ids = [
      azurerm_subnet.private.id
    ]
  }
}
```

#### 2. Scanning automatique avec Checkov
```yaml
# .checkov.yml
framework:
  - terraform
  - kubernetes
  - dockerfile

skip-check:
  # Ignorer certains checks spécifiques avec justification
  - CKV_DOCKER_2  # HEALTHCHECK instruction manquante - géré par K8s

output: json
quiet: true
compact: true

# Custom checks
external-checks-dir: ./custom-policies/
```

#### 3. Policies personnalisées OPA/Rego
```rego
# policies/terraform/security.rego
package terraform.security

import future.keywords.if
import future.keywords.in

# Interdire les groupes de sécurité ouverts sur internet
deny[msg] if {
    resource := input.resource_changes[_]
    resource.type == "azurerm_network_security_rule"
    
    resource.change.after.source_address_prefix == "*"
    resource.change.after.destination_port_range in ["22", "3389", "1433", "5432"]
    
    msg := sprintf("Security rule '%s' allows dangerous port %s from any source", [
        resource.name, 
        resource.change.after.destination_port_range
    ])
}

# Forcer le chiffrement des disques
deny[msg] if {
    resource := input.resource_changes[_]
    resource.type == "azurerm_virtual_machine"
    
    not resource.change.after.storage_os_disk.encryption_settings
    
    msg := sprintf("Virtual machine '%s' does not have disk encryption enabled", [
        resource.name
    ])
}

# Valider la configuration Key Vault
warn[msg] if {
    resource := input.resource_changes[_]
    resource.type == "azurerm_key_vault"
    
    resource.change.after.soft_delete_retention_days < 90
    
    msg := sprintf("Key Vault '%s' should have soft delete retention >= 90 days", [
        resource.name
    ])
}
```

### Kubernetes Security

#### 1. Pod Security Standards
```yaml
# pod-security-policy.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: production
  labels:
    pod-security.kubernetes.io/enforce: restricted
    pod-security.kubernetes.io/audit: restricted
    pod-security.kubernetes.io/warn: restricted

---
# Security context sécurisé
apiVersion: apps/v1
kind: Deployment
metadata:
  name: secure-app
  namespace: production
spec:
  replicas: 3
  selector:
    matchLabels:
      app: secure-app
  template:
    metadata:
      labels:
        app: secure-app
    spec:
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
        runAsGroup: 3000
        fsGroup: 2000
        seccompProfile:
          type: RuntimeDefault
      
      containers:
      - name: app
        image: myapp:latest
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          capabilities:
            drop:
            - ALL
          runAsNonRoot: true
          runAsUser: 1000
        
        resources:
          limits:
            memory: "128Mi"
            cpu: "500m"
          requests:
            memory: "64Mi"
            cpu: "250m"
        
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        
        readinessProbe:
          httpGet:
            path: /ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

#### 2. Network Policies
```yaml
# network-policy.yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: secure-app-netpol
  namespace: production
spec:
  podSelector:
    matchLabels:
      app: secure-app
  policyTypes:
  - Ingress
  - Egress
  
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: ingress-nginx
    - podSelector:
        matchLabels:
          app: api-gateway
    ports:
    - protocol: TCP
      port: 8080
  
  egress:
  - to:
    - podSelector:
        matchLabels:
          app: database
    ports:
    - protocol: TCP
      port: 5432
  
  # DNS resolution
  - to: []
    ports:
    - protocol: UDP
      port: 53
```

---

## **Container et Image Security**

### Multi-stage Dockerfile sécurisé

```dockerfile
# Dockerfile sécurisé
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copier seulement les fichiers de projet d'abord (layer caching)
COPY ["*.csproj", "./"]
RUN dotnet restore

# Copier le reste du code
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Scan des vulnérabilités pendant le build
# RUN dotnet list package --vulnerable --include-transitive

# Runtime stage (image minimale)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# Créer un utilisateur non-root
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser

# Installer uniquement les packages nécessaires
RUN apk add --no-cache \
    ca-certificates \
    && rm -rf /var/cache/apk/*

WORKDIR /app

# Copier l'application avec les bonnes permissions
COPY --from=build --chown=appuser:appgroup /app/publish .

# Changer vers l'utilisateur non-root
USER appuser

# Exposer le port (non-privileged)
EXPOSE 8080

# Point d'entrée sécurisé
ENTRYPOINT ["dotnet", "MyApp.dll"]

# Metadata de sécurité
LABEL security.scan="enabled" \
      security.non-root="true" \
      security.no-secrets="true"
```

### Image scanning avec Trivy

```yaml
# .github/workflows/container-security.yml
name: Container Security

on:
  push:
    branches: [ main ]

jobs:
  scan:
    runs-on: ubuntu-latest
    steps:
    - name: Checkout
      uses: actions/checkout@v4
    
    - name: Build image
      run: docker build -t myapp:${{ github.sha }} .
    
    - name: Trivy vulnerability scan
      uses: aquasecurity/trivy-action@master
      with:
        image-ref: myapp:${{ github.sha }}
        format: 'sarif'
        output: 'trivy-results.sarif'
        severity: 'CRITICAL,HIGH'
    
    - name: Upload Trivy scan results
      uses: github/codeql-action/upload-sarif@v2
      if: always()
      with:
        sarif_file: 'trivy-results.sarif'
    
    - name: Trivy config scan
      uses: aquasecurity/trivy-action@master
      with:
        scan-type: 'config'
        format: 'sarif'
        output: 'trivy-config.sarif'
    
    - name: Docker Bench Security
      run: |
        docker run --rm --net host --pid host --userns host --cap-add audit_control \
          -e DOCKER_CONTENT_TRUST=$DOCKER_CONTENT_TRUST \
          -v /etc:/etc:ro \
          -v /var/lib:/var/lib:ro \
          -v /var/run/docker.sock:/var/run/docker.sock:ro \
          -v /usr/lib/systemd:/usr/lib/systemd:ro \
          -v /sys/fs/cgroup:/sys/fs/cgroup:ro \
          --label docker_bench_security \
          docker/docker-bench-security
```

### Image signing et verification

```bash
#!/bin/bash
# scripts/sign-image.sh

set -e

IMAGE_NAME="myapp"
IMAGE_TAG="${GITHUB_SHA}"
REGISTRY="ghcr.io/myorg"

# Build l'image
docker build -t ${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG} .

# Signer l'image avec Cosign
cosign sign --key cosign.key ${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}

# Attacher un SBOM (Software Bill of Materials)
syft ${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG} -o spdx-json > sbom.spdx.json
cosign attach sbom --sbom sbom.spdx.json ${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}

# Attacher l'attestation de vulnérabilités
grype ${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG} -o json > vulnerabilities.json
cosign attest --predicate vulnerabilities.json \
  --type vuln ${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}

echo "Image signed and attestations attached"
```

---

## **Secrets Management dans CI/CD**

### GitHub Secrets et OIDC

```yaml
# .github/workflows/deploy-with-oidc.yml
name: Deploy with OIDC

on:
  push:
    branches: [ main ]

permissions:
  id-token: write   # Nécessaire pour OIDC
  contents: read

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    # Authentification Azure avec OIDC (pas de secrets)
    - name: Azure Login
      uses: azure/login@v1
      with:
        client-id: ${{ secrets.AZURE_CLIENT_ID }}
        tenant-id: ${{ secrets.AZURE_TENANT_ID }}
        subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
    
    # Récupérer des secrets depuis Azure Key Vault
    - name: Get secrets from Key Vault
      uses: azure/get-keyvault-secrets@v1
      with:
        keyvault: "my-keyvault"
        secrets: 'database-connection-string, api-key'
      id: secrets
    
    - name: Deploy application
      env:
        DB_CONNECTION: ${{ steps.secrets.outputs.database-connection-string }}
        API_KEY: ${{ steps.secrets.outputs.api-key }}
      run: |
        # Déploiement avec secrets récupérés dynamiquement
        ./deploy.sh
```

### Vault Integration

```csharp
// Services/VaultSecretsService.cs
public class VaultSecretsService
{
    private readonly IVaultClient _vaultClient;
    private readonly ILogger<VaultSecretsService> _logger;
    
    public VaultSecretsService(IVaultClient vaultClient, ILogger<VaultSecretsService> logger)
    {
        _vaultClient = vaultClient;
        _logger = logger;
    }
    
    public async Task<string> GetSecretAsync(string path, string key)
    {
        try
        {
            var secret = await _vaultClient.V1.Secrets.KeyValue.V2
                .ReadSecretAsync(path, mountPoint: "secret");
            
            if (secret?.Data?.Data?.ContainsKey(key) == true)
            {
                return secret.Data.Data[key].ToString();
            }
            
            throw new KeyNotFoundException($"Secret key '{key}' not found at path '{path}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret from Vault: {Path}/{Key}", path, key);
            throw;
        }
    }
    
    public async Task<Dictionary<string, string>> GetSecretsAsync(string path)
    {
        var secret = await _vaultClient.V1.Secrets.KeyValue.V2
            .ReadSecretAsync(path, mountPoint: "secret");
        
        return secret?.Data?.Data?.ToDictionary(
            kvp => kvp.Key, 
            kvp => kvp.Value?.ToString() ?? string.Empty
        ) ?? new Dictionary<string, string>();
    }
}

// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Configuration Vault avec authentification Kubernetes
    services.AddSingleton<IVaultClient>(provider =>
    {
        var vaultAddr = Environment.GetEnvironmentVariable("VAULT_ADDR");
        var k8sRole = Environment.GetEnvironmentVariable("VAULT_K8S_ROLE");
        var jwtToken = File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
        
        var vaultClientSettings = new VaultClientSettings(vaultAddr, null);
        var vaultClient = new VaultClient(vaultClientSettings);
        
        // Authentification Kubernetes
        var authResponse = vaultClient.V1.Auth.Kubernetes
            .LoginAsync(k8sRole, jwtToken).Result;
        vaultClientSettings.AuthMethodInfo = new TokenAuthMethodInfo(authResponse.AuthInfo.ClientToken);
        
        return new VaultClient(vaultClientSettings);
    });
    
    services.AddScoped<VaultSecretsService>();
}
```

---

## **Security Testing Automation**

### SAST (Static Application Security Testing)

```csharp
// Tests/SecurityTests.cs
[TestFixture]
public class SecurityTests
{
    [Test]
    public void ScanForHardcodedSecrets()
    {
        var sourceFiles = Directory.GetFiles("../../../", "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("bin") && !f.Contains("obj"));
        
        var secretPatterns = new[]
        {
            @"password\s*=\s*[""'][^""']+[""']",
            @"connectionstring\s*=\s*[""'][^""']+[""']",
            @"secret\s*=\s*[""'][^""']+[""']",
            @"apikey\s*=\s*[""'][^""']+[""']"
        };
        
        var violations = new List<string>();
        
        foreach (var file in sourceFiles)
        {
            var content = File.ReadAllText(file);
            
            foreach (var pattern in secretPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    violations.Add($"Potential secret found in {file}");
                }
            }
        }
        
        Assert.IsEmpty(violations, $"Security violations found:\n{string.Join("\n", violations)}");
    }
    
    [Test]
    public void ValidateCryptographicPractices()
    {
        var sourceFiles = Directory.GetFiles("../../../", "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("bin") && !f.Contains("obj"));
        
        var weakCryptoPatterns = new[]
        {
            @"\bMD5\b",
            @"\bSHA1\b", 
            @"\bDES\b",
            @"\bTripleDES\b"
        };
        
        var violations = new List<string>();
        
        foreach (var file in sourceFiles)
        {
            var content = File.ReadAllText(file);
            
            foreach (var pattern in weakCryptoPatterns)
            {
                if (Regex.IsMatch(content, pattern))
                {
                    violations.Add($"Weak cryptography found in {file}: {pattern}");
                }
            }
        }
        
        Assert.IsEmpty(violations, $"Weak cryptography violations:\n{string.Join("\n", violations)}");
    }
}
```

### DAST (Dynamic Application Security Testing)

```yaml
# k6/security-load-test.js
import http from 'k6/http';
import { check } from 'k6';

export let options = {
  stages: [
    { duration: '1m', target: 10 },
    { duration: '2m', target: 50 },
    { duration: '1m', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],
    'security_headers_present': ['rate>0.95'],
  },
};

export default function() {
  // Test d'injection SQL
  let sqlInjectionPayloads = [
    "'; DROP TABLE users; --",
    "1' OR '1'='1",
    "admin'/*",
  ];
  
  sqlInjectionPayloads.forEach(payload => {
    let response = http.get(`https://api.example.com/users?id=${payload}`);
    check(response, {
      'no_sql_injection_success': (r) => r.status !== 200 || !r.body.includes('error'),
    });
  });
  
  // Test des headers de sécurité
  let response = http.get('https://api.example.com/');
  check(response, {
    'security_headers_present': (r) => {
      return r.headers['X-Content-Type-Options'] === 'nosniff' &&
             r.headers['X-Frame-Options'] !== undefined &&
             r.headers['Content-Security-Policy'] !== undefined;
    },
  });
  
  // Test XSS
  let xssPayloads = [
    '<script>alert("XSS")</script>',
    'javascript:alert("XSS")',
    '<img src=x onerror=alert("XSS")>',
  ];
  
  xssPayloads.forEach(payload => {
    let postResponse = http.post('https://api.example.com/comments', {
      comment: payload
    });
    check(postResponse, {
      'no_xss_reflection': (r) => !r.body.includes(payload),
    });
  });
}
```

### Infrastructure Testing avec InSpec

```ruby
# tests/security_spec.rb
describe port(22) do
  it { should_not be_listening }
end

describe port(443) do
  it { should be_listening }
end

describe file('/etc/ssl/private') do
  it { should be_directory }
  it { should be_mode 0700 }
  it { should be_owned_by 'root' }
end

describe kernel_parameter('net.ipv4.ip_forward') do
  its('value') { should eq 0 }
end

describe file('/proc/sys/kernel/dmesg_restrict') do
  its('content') { should match(/1/) }
end

# Test des politiques de mots de passe
describe file('/etc/pam.d/common-password') do
  its('content') { should match(/minlen=12/) }
  its('content') { should match(/retry=3/) }
end

# Vérification des permissions de fichiers critiques
['/etc/passwd', '/etc/shadow', '/etc/group'].each do |file|
  describe file(file) do
    it { should exist }
    it { should be_owned_by 'root' }
    it { should be_grouped_into 'root' }
  end
end

describe file('/etc/shadow') do
  it { should be_mode 0640 }
end
```

---

## **Compliance et Governance**

### Policy as Code avec OPA

```rego
# policies/security/mandatory_labels.rego
package kubernetes.security.mandatory_labels

import future.keywords.if

required_labels := {
    "security.level",
    "data.classification", 
    "owner.team",
    "backup.required"
}

violation[{"msg": msg}] if {
    required := required_labels[_]
    not input.metadata.labels[required]
    msg := sprintf("Missing required label: %v", [required])
}

violation[{"msg": msg}] if {
    input.metadata.labels["security.level"]
    not input.metadata.labels["security.level"] in {"public", "internal", "confidential", "restricted"}
    msg := "security.level must be one of: public, internal, confidential, restricted"
}
```

```rego
# policies/security/pod_security.rego
package kubernetes.security.pod_security

import future.keywords.if

violation[{"msg": msg}] if {
    input.kind == "Pod"
    input.spec.securityContext.runAsRoot == true
    msg := "Pods must not run as root"
}

violation[{"msg": msg}] if {
    input.kind == "Pod"
    container := input.spec.containers[_]
    container.securityContext.allowPrivilegeEscalation == true
    msg := sprintf("Container %v allows privilege escalation", [container.name])
}

violation[{"msg": msg}] if {
    input.kind == "Pod"
    container := input.spec.containers[_]
    not container.resources.limits.memory
    msg := sprintf("Container %v missing memory limit", [container.name])
}
```

### Compliance Scanning

```csharp
// Services/ComplianceService.cs
public class ComplianceService
{
    private readonly ILogger<ComplianceService> _logger;
    
    public class ComplianceCheck
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Framework { get; set; } // SOC2, ISO27001, PCI-DSS
        public Func<Task<bool>> Check { get; set; }
        public string Description { get; set; }
    }
    
    private readonly List<ComplianceCheck> _checks;
    
    public ComplianceService(ILogger<ComplianceService> logger)
    {
        _logger = logger;
        _checks = InitializeComplianceChecks();
    }
    
    private List<ComplianceCheck> InitializeComplianceChecks()
    {
        return new List<ComplianceCheck>
        {
            new()
            {
                Id = "SOC2-CC6.1",
                Name = "Encryption in Transit",
                Framework = "SOC2",
                Check = CheckEncryptionInTransit,
                Description = "All network communications must be encrypted"
            },
            new()
            {
                Id = "ISO27001-A.9.1.2", 
                Name = "Access Control Review",
                Framework = "ISO27001",
                Check = CheckAccessControlReview,
                Description = "Regular review of user access rights"
            },
            new()
            {
                Id = "PCI-DSS-3.4",
                Name = "PAN Encryption",
                Framework = "PCI-DSS", 
                Check = CheckPANEncryption,
                Description = "Primary Account Numbers must be encrypted"
            }
        };
    }
    
    public async Task<ComplianceReport> RunComplianceCheckAsync(string framework = null)
    {
        var checksToRun = string.IsNullOrEmpty(framework) 
            ? _checks 
            : _checks.Where(c => c.Framework == framework).ToList();
        
        var results = new List<ComplianceResult>();
        
        foreach (var check in checksToRun)
        {
            try
            {
                var passed = await check.Check();
                results.Add(new ComplianceResult
                {
                    CheckId = check.Id,
                    CheckName = check.Name,
                    Framework = check.Framework,
                    Passed = passed,
                    Timestamp = DateTime.UtcNow,
                    Details = passed ? "Check passed" : "Check failed"
                });
                
                _logger.LogInformation("Compliance check {CheckId} {Result}", 
                    check.Id, passed ? "PASSED" : "FAILED");
            }
            catch (Exception ex)
            {
                results.Add(new ComplianceResult
                {
                    CheckId = check.Id,
                    CheckName = check.Name,
                    Framework = check.Framework,
                    Passed = false,
                    Timestamp = DateTime.UtcNow,
                    Details = $"Check error: {ex.Message}"
                });
                
                _logger.LogError(ex, "Error running compliance check {CheckId}", check.Id);
            }
        }
        
        return new ComplianceReport
        {
            Framework = framework,
            Results = results,
            OverallCompliance = results.All(r => r.Passed),
            GeneratedAt = DateTime.UtcNow
        };
    }
    
    private async Task<bool> CheckEncryptionInTransit()
    {
        // Vérifier que tous les endpoints utilisent HTTPS
        var endpoints = await GetApplicationEndpointsAsync();
        return endpoints.All(e => e.StartsWith("https://"));
    }
    
    private async Task<bool> CheckAccessControlReview()
    {
        // Vérifier que les revues d'accès ont été effectuées récemment
        var lastReview = await GetLastAccessReviewDateAsync();
        return DateTime.UtcNow.Subtract(lastReview).TotalDays <= 90;
    }
    
    private async Task<bool> CheckPANEncryption()
    {
        // Vérifier le chiffrement des données de carte de crédit
        var dbConfig = await GetDatabaseConfigurationAsync();
        return dbConfig.EncryptionEnabled && dbConfig.EncryptionAlgorithm == "AES-256";
    }
}
```

---

## **Monitoring et Response**

### Security Monitoring Pipeline

```csharp
// Services/SecurityMonitoringService.cs
public class SecurityMonitoringService
{
    private readonly ILogger<SecurityMonitoringService> _logger;
    private readonly IMetricsCollector _metrics;
    private readonly IAlertManager _alertManager;
    
    public async Task MonitorSecurityEventsAsync()
    {
        // Métriques de sécurité en temps réel
        _metrics.Gauge("security.active_sessions").Set(await GetActiveSessionsCountAsync());
        _metrics.Gauge("security.failed_logins_per_minute").Set(await GetFailedLoginsPerMinuteAsync());
        _metrics.Gauge("security.privilege_escalations").Set(await GetPrivilegeEscalationsCountAsync());
        
        // Détection d'anomalies
        await DetectAnomalousActivityAsync();
        
        // Vérification de la conformité en continu
        await ContinuousComplianceMonitoringAsync();
    }
    
    private async Task DetectAnomalousActivityAsync()
    {
        var events = await GetRecentSecurityEventsAsync();
        
        foreach (var evt in events)
        {
            // Détection de brute force
            if (await IsBruteForceAttackAsync(evt))
            {
                await _alertManager.SendAlertAsync(new SecurityAlert
                {
                    Severity = AlertSeverity.High,
                    Type = "Brute Force Attack",
                    Description = $"Multiple failed login attempts detected for user {evt.UserId}",
                    Source = evt.IPAddress,
                    Timestamp = DateTime.UtcNow
                });
            }
            
            // Détection d'accès géographiquement anormal
            if (await IsGeographicalAnomalyAsync(evt))
            {
                await _alertManager.SendAlertAsync(new SecurityAlert
                {
                    Severity = AlertSeverity.Medium,
                    Type = "Geographical Anomaly",
                    Description = $"User {evt.UserId} accessed from unusual location",
                    Source = evt.IPAddress,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}

// Incident Response Automation
public class IncidentResponseService
{
    public async Task HandleSecurityIncidentAsync(SecurityIncident incident)
    {
        // 1. Classification automatique
        var classification = await ClassifyIncidentAsync(incident);
        
        // 2. Containment automatique si critique
        if (classification.Severity == IncidentSeverity.Critical)
        {
            await AutoContainmentAsync(incident);
        }
        
        // 3. Notification des équipes
        await NotifySecurityTeamAsync(incident, classification);
        
        // 4. Collection d'evidence
        await CollectForensicEvidenceAsync(incident);
        
        // 5. Lancement des playbooks
        await ExecuteResponsePlaybookAsync(incident.Type);
    }
    
    private async Task AutoContainmentAsync(SecurityIncident incident)
    {
        switch (incident.Type)
        {
            case IncidentType.DataBreach:
                await DisableAffectedAccountsAsync(incident.AffectedUsers);
                await BlockSuspiciousIPsAsync(incident.SourceIPs);
                break;
                
            case IncidentType.MalwareDetection:
                await IsolateAffectedSystemsAsync(incident.AffectedSystems);
                await UpdateSecurityRulesAsync(incident.Indicators);
                break;
                
            case IncidentType.PrivilegeEscalation:
                await RevokeElevatedPrivilegesAsync(incident.AffectedUsers);
                await ForcePasswordResetAsync(incident.AffectedUsers);
                break;
        }
    }
}
```

### Alerting Configuration

```yaml
# prometheus/security-alerts.yml
groups:
- name: security.rules
  rules:
  
  # Authentification
  - alert: HighFailedLoginRate
    expr: rate(authentication_failures_total[5m]) > 10
    for: 2m
    labels:
      severity: warning
      category: security
    annotations:
      summary: "High failed login rate detected"
      description: "{{ $value }} failed logins per second for the last 5 minutes"
  
  # Accès suspicieux
  - alert: SuspiciousAPIAccess
    expr: rate(api_requests_total{status=~"4..|5.."}[10m]) > 50
    for: 5m
    labels:
      severity: critical
      category: security
    annotations:
      summary: "Suspicious API access pattern detected"
      description: "High rate of 4xx/5xx responses: {{ $value }} req/sec"
  
  # Sécurité infrastructure
  - alert: UnauthorizedPrivilegeEscalation
    expr: increase(privilege_escalation_attempts_total[1h]) > 0
    for: 0m
    labels:
      severity: critical
      category: security
    annotations:
      summary: "Unauthorized privilege escalation detected"
      description: "{{ $value }} privilege escalation attempts in the last hour"
  
  # Conformité
  - alert: ComplianceCheckFailure
    expr: compliance_check_status == 0
    for: 0m
    labels:
      severity: high
      category: compliance
    annotations:
      summary: "Compliance check failure"
      description: "Compliance check {{ $labels.check_name }} has failed"
```

Ce cours sur DevSecOps et CI/CD complète votre formation en cybersécurité en couvrant l'intégration de la sécurité dans tout le cycle de développement et déploiement.
