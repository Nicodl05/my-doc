# **Sécurité Microservices - Fiche de Révision Entretien**

## **Questions Fondamentales Architecture**

### **Q: Quels sont les défis sécuritaires spécifiques aux microservices ?**

**Défis vs Monolithe :**
1. **Surface d'attaque élargie** : N services = N points d'entrée
2. **Complexité réseau** : Communications inter-services
3. **Gestion des identités** distribuée
4. **Secrets management** à l'échelle
5. **Observabilité** des flux de sécurité
6. **Consistency** des politiques de sécurité

### **Q: Qu'est-ce que Zero Trust et comment l'implémenter ?**

**Principe Zero Trust :**
> "Never trust, always verify"

**Implémentation technique :**
```
┌─────────────────┐    mTLS    ┌─────────────────┐
│   Service A     │◄──────────►│   Service B     │
│ + Identity      │   + AuthZ   │ + Identity      │
│ + Policy Engine │            │ + Policy Engine │
└─────────────────┘            └─────────────────┘
```

**Composants essentiels :**
- **Identity** pour chaque service (certificats)
- **Encryption** bout-en-bout (mTLS)
- **Authorization** à chaque appel
- **Monitoring** de tous les flux

---

## **Authentification Inter-Services**

### **Q: Comment authentifier les services entre eux ?**

**Solutions techniques :**

1. **Service Tokens (JWT)**
   ```csharp
   // Génération token service-to-service
   var claims = new[]
   {
       new Claim("service_id", "user-service"),
       new Claim("target", "order-service"),
       new Claim("scope", "read:orders")
   };
   
   // Signature avec clé privée du service
   var token = new JwtSecurityToken(
       issuer: "user-service",
       audience: "order-service", 
       claims: claims,
       expires: DateTime.UtcNow.AddMinutes(5), // Court!
       signingCredentials: serviceCredentials
   );
   ```

2. **Mutual TLS (mTLS)**
   - Certificats clients pour chaque service
   - Validation bidirectionnelle
   - Rotation automatique des certificats

3. **Service Mesh (Istio)**
   ```yaml
   # mTLS automatique entre tous les services
   apiVersion: security.istio.io/v1beta1
   kind: PeerAuthentication
   metadata:
     name: default
   spec:
     mtls:
       mode: STRICT
   ```

### **Q: Comment propager le contexte utilisateur ?**

**Context Propagation Pattern :**
```csharp
// Headers standardisés
X-User-ID: user123
X-User-Roles: admin,user
X-Tenant-ID: tenant456
X-Correlation-ID: trace789

// Middleware automatique
public class UserContextMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userContext = ExtractUserContext(context.Request);
        context.Items["UserContext"] = userContext;
        
        // Propager aux appels sortants
        _httpClient.DefaultRequestHeaders.Add("X-User-ID", userContext.UserId);
    }
}
```

---

## **Autorisation Distribuée**

### **Q: Comment gérer les permissions dans une architecture distribuée ?**

**Patterns d'autorisation :**

1. **Centralized Authorization (Policy Engine)**
   ```
   Service A → [Policy Decision Point] → Allow/Deny
                      ↓
              [Policy Information Point]
   ```

2. **Distributed with Token Claims**
   ```csharp
   // Token avec permissions explicites
   {
     "sub": "user123",
     "permissions": [
       "orders:read",
       "orders:create", 
       "users:read"
     ],
     "tenant": "tenant456"
   }
   ```

3. **Hierarchical Scopes**
   ```
   admin           → Tous les droits
   admin:orders    → Admin sur les commandes
   orders:read     → Lecture commandes seulement
   ```

### **Q: Qu'est-ce que RBAC vs ABAC ?**

**RBAC (Role-Based)** :
- Permissions basées sur les rôles
- Simple mais rigide
- Exemple : Admin, User, Guest

**ABAC (Attribute-Based)** :
- Permissions basées sur attributs contextuels
- Flexible mais complexe
- Exemple : "Admin peut supprimer si ressource.owner == user.id ET time < 18h"

---

## **Communication Sécurisée**

### **Q: Comment sécuriser les communications inter-services ?**

**Niveaux de sécurité :**

1. **Transport Level (mTLS)**
   ```csharp
   // Configuration mTLS client
   var handler = new HttpClientHandler();
   handler.ClientCertificates.Add(clientCertificate);
   handler.ServerCertificateCustomValidationCallback = ValidateCert;
   
   var client = new HttpClient(handler);
   ```

2. **Message Level (JWE)**
   ```csharp
   // Chiffrement du payload
   var encryptedPayload = JWE.Encrypt(
       payload: jsonData,
       recipients: new[] { recipientPublicKey },
       algorithm: JweAlgorithm.RsaOaep256,
       encryption: JweEncryption.Aes256CbcHmacSha512
   );
   ```

3. **Service Mesh (Transparent)**
   - Envoy proxy gère automatiquement mTLS
   - Policies déclaratives
   - Observabilité intégrée

### **Q: Comment gérer la rotation des certificats ?**

**Stratégie de rotation :**
1. **Automated Certificate Management** (cert-manager)
2. **Graceful rollover** (support multi-certificats)
3. **Health checks** post-rotation
4. **Alerting** en cas d'échec

---

## **Secrets Management**

### **Q: Comment gérer les secrets dans une architecture microservices ?**

**Patterns de gestion :**

1. **External Secret Store**
   ```csharp
   // HashiCorp Vault
   var secret = await vaultClient.V1.Secrets.KeyValue.V2
       .ReadSecretAsync("database/credentials");
   
   // Azure Key Vault
   var connectionString = await keyVaultClient
       .GetSecretAsync("database-connection");
   ```

2. **Kubernetes Secrets with CSI**
   ```yaml
   # Secret injecté comme volume
   volumeMounts:
   - name: secrets-store
     mountPath: "/mnt/secrets"
     readOnly: true
   ```

3. **Init Container Pattern**
   - Container d'initialisation récupère les secrets
   - Secrets stockés dans un volume partagé
   - Application principale lit depuis le volume

### **Q: Comment implémenter la rotation de secrets ?**

**Automated Rotation Process :**
```csharp
public class SecretRotationService
{
    public async Task RotateSecretAsync(string secretName)
    {
        // 1. Générer nouveau secret
        var newSecret = GenerateNewSecret();
        
        // 2. Stocker avec nouvelle version
        await _secretStore.SetSecretAsync($"{secretName}-v2", newSecret);
        
        // 3. Notifier les services (webhook/event)
        await NotifyServicesAsync(secretName, "v2");
        
        // 4. Attendre confirmation migration
        await WaitForMigrationConfirmation();
        
        // 5. Supprimer ancienne version
        await _secretStore.DeleteSecretAsync($"{secretName}-v1");
    }
}
```

---

## **Service Mesh & Network Security**

### **Q: Qu'est-ce qu'un Service Mesh et ses avantages sécurité ?**

**Service Mesh Benefits :**
- **mTLS automatique** entre tous les services
- **Traffic policies** déclaratives
- **Zero-trust network** par défaut
- **Observability** du trafic chiffré
- **Circuit breaking** automatique

**Istio Security Features :**
```yaml
# Authorization Policy
apiVersion: security.istio.io/v1beta1
kind: AuthorizationPolicy
metadata:
  name: user-service-authz
spec:
  selector:
    matchLabels:
      app: user-service
  rules:
  - from:
    - source:
        principals: ["cluster.local/ns/default/sa/api-gateway"]
    to:
    - operation:
        methods: ["GET", "POST"]
```

### **Q: Comment implémenter la segmentation réseau ?**

**Network Policies Kubernetes :**
```yaml
# Deny all by default
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: default-deny-all
spec:
  podSelector: {}
  policyTypes:
  - Ingress
  - Egress

---
# Allow specific communication
apiVersion: networking.k8s.io/v1  
kind: NetworkPolicy
metadata:
  name: api-to-user-service
spec:
  podSelector:
    matchLabels:
      app: user-service
  policyTypes:
  - Ingress
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: api-gateway
    ports:
    - protocol: TCP
      port: 8080
```

---

## **Monitoring & Observability**

### **Q: Comment monitorer la sécurité dans une architecture microservices ?**

**Security Observability Stack :**

1. **Distributed Tracing avec contexte sécurité**
   ```csharp
   // Ajouter attributs sécurité aux traces
   Activity.Current?.SetTag("security.user_id", userId);
   Activity.Current?.SetTag("security.operation", "sensitive_data_access");
   Activity.Current?.SetTag("security.result", "success");
   ```

2. **Security Metrics**
   ```csharp
   // Métriques Prometheus
   private readonly Counter _authFailures = Metrics
       .CreateCounter("auth_failures_total", "Authentication failures",
                     new[] { "service", "reason" });
   
   private readonly Histogram _authDuration = Metrics
       .CreateHistogram("auth_duration_seconds", "Authentication duration");
   ```

3. **Audit Logging**
   ```csharp
   // Log structuré pour SIEM
   logger.LogWarning("Security event: {EventType} User: {UserId} Service: {Service} Result: {Result}",
       "authentication", userId, serviceName, "failed");
   ```

### **Q: Comment détecter les anomalies de sécurité ?**

**Anomaly Detection Patterns :**
- **Behavioral baselines** par service
- **Geographic anomalies** (IP inhabituelles)
- **Temporal patterns** (accès hors heures)
- **Volume spikes** (attaques DDoS)
- **Error rate increases** (scanning/brute force)

---

## **Container & Kubernetes Security**

### **Q: Comment sécuriser les containers en production ?**

**Container Security Checklist :**
1. **Base images** minimales (distroless, alpine)
2. **Non-root user** obligatoire
3. **Read-only filesystem** quand possible
4. **Security contexts** restrictifs
5. **Resource limits** pour éviter DoS
6. **Image scanning** (Trivy, Clair)

**Pod Security Standards :**
```yaml
apiVersion: v1
kind: Pod
spec:
  securityContext:
    runAsNonRoot: true
    runAsUser: 1000
    fsGroup: 2000
  containers:
  - name: app
    securityContext:
      allowPrivilegeEscalation: false
      readOnlyRootFilesystem: true
      capabilities:
        drop:
        - ALL
```

### **Q: Comment implémenter RBAC dans Kubernetes ?**

**Principle of Least Privilege :**
```yaml
# ServiceAccount pour le service
apiVersion: v1
kind: ServiceAccount
metadata:
  name: user-service-sa

---
# Role avec permissions minimales
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: user-service-role
rules:
- apiGroups: [""]
  resources: ["configmaps"]
  verbs: ["get", "list"]
- apiGroups: [""]
  resources: ["secrets"]
  verbs: ["get"]
  resourceNames: ["user-service-secret"]

---
# RoleBinding
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: user-service-binding
subjects:
- kind: ServiceAccount
  name: user-service-sa
roleRef:
  kind: Role
  name: user-service-role
  apiGroup: rbac.authorization.k8s.io
```

---

## **Questions Pièges Entretien**

### **Q: "Les microservices sont-ils plus sécurisés que les monolithes ?"**
**Réponse nuancée :**
- ✅ **Isolation** : Compromise limitée à un service
- ✅ **Least privilege** : Permissions granulaires
- ❌ **Complexité** : Plus de surface d'attaque
- ❌ **Network security** : Plus de communications à sécuriser

**Verdict** : Microservices peuvent être plus sécurisés SI bien implémentés.

### **Q: "Comment gérer les sessions dans une architecture stateless ?"**
**Solutions :**
1. **JWT avec refresh tokens** (expiration courte)
2. **Distributed cache** (Redis) pour état session
3. **Database sessions** avec cache
4. **Sticky sessions** (moins scalable)

### **Q: "Que faire si un service est compromis ?"**
**Incident Response Plan :**
1. **Isolation** immédiate du service
2. **Révocation** des credentials/certificats
3. **Analysis** des logs et traces
4. **Lateral movement** assessment
5. **Recovery** avec image clean
6. **Post-mortem** et amélioration

---

## **Introduction aux défis de sécurité**

### Défis spécifiques aux microservices
Les architectures de microservices introduisent de nouveaux défis de sécurité :

#### 1. Surface d'attaque élargie
```
Monolithe          vs          Microservices
┌─────────────┐                ┌───┐ ┌───┐ ┌───┐
│             │                │ A │ │ B │ │ C │
│   App       │                └───┘ └───┘ └───┘
│             │                  │     │     │
└─────────────┘                ┌───┐ ┌───┐ ┌───┐
      │                        │ D │ │ E │ │ F │
┌─────────────┐                └───┘ └───┘ └───┘
│    DB       │
└─────────────┘

1 endpoint                    N endpoints
1 base de données            N bases de données
```

#### 2. Complexité des communications
- Communications réseau entre services
- Authentification et autorisation distribuées
- Gestion des secrets répartie
- Traçabilité des requêtes cross-services

#### 3. Nouvelles vulnérabilités
- **Service-to-Service attacks**
- **Token hijacking**
- **Network sniffing**
- **Configuration drift**

---

## **Architecture Zero Trust**

### Principes fondamentaux
> "Never trust, always verify"

#### 1. Vérification continue
```csharp
public class ZeroTrustMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITokenValidator _tokenValidator;
    private readonly IServiceRegistry _serviceRegistry;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Vérifier l'identité du service appelant
        var serviceToken = ExtractServiceToken(context);
        var callingService = await _tokenValidator.ValidateServiceTokenAsync(serviceToken);
        
        // 2. Vérifier les permissions pour cette opération
        var operation = GetOperation(context);
        if (!await AuthorizeOperationAsync(callingService, operation))
        {
            context.Response.StatusCode = 403;
            return;
        }
        
        // 3. Vérifier la santé du service appelant
        if (!await _serviceRegistry.IsServiceHealthyAsync(callingService.Id))
        {
            context.Response.StatusCode = 503;
            return;
        }
        
        // 4. Continuer avec des headers de contexte sécurisé
        context.Items["CallingService"] = callingService;
        await _next(context);
    }
}
```

#### 2. Moindre privilège
```csharp
public class ServicePermissionMatrix
{
    private readonly Dictionary<string, HashSet<string>> _permissions = new()
    {
        ["user-service"] = new() { "read:users", "write:users" },
        ["order-service"] = new() { "read:users", "read:products", "write:orders" },
        ["payment-service"] = new() { "read:orders", "write:payments" },
        ["notification-service"] = new() { "read:users", "send:notifications" }
    };
    
    public bool HasPermission(string serviceId, string permission)
    {
        return _permissions.TryGetValue(serviceId, out var perms) && 
               perms.Contains(permission);
    }
}
```

#### 3. Segmentation réseau
```yaml
# Kubernetes Network Policies
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: user-service-policy
spec:
  podSelector:
    matchLabels:
      app: user-service
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: api-gateway
    - podSelector:
        matchLabels:
          app: order-service
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
```

---

## **Authentification et autorisation distribuées**

### Token-based Authentication

#### 1. JWT pour l'authentification inter-services
```csharp
public class ServiceTokenProvider
{
    private readonly string _privateKey;
    private readonly string _serviceId;
    
    public string GenerateServiceToken(string targetService, string[] permissions)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new ECDsaSecurityKey(LoadPrivateKey(_privateKey));
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("service_id", _serviceId),
                new Claim("target_service", targetService),
                new Claim("permissions", string.Join(",", permissions)),
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(5), // Court délai
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
```

#### 2. Propagation de contexte utilisateur
```csharp
public class UserContextPropagationService
{
    public class UserContext
    {
        public string UserId { get; set; }
        public string[] Roles { get; set; }
        public string TenantId { get; set; }
        public Dictionary<string, string> Claims { get; set; }
    }
    
    public string SerializeUserContext(UserContext context)
    {
        var payload = JsonSerializer.Serialize(context);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
    }
    
    public UserContext DeserializeUserContext(string encodedContext)
    {
        var payload = Encoding.UTF8.GetString(Convert.FromBase64String(encodedContext));
        return JsonSerializer.Deserialize<UserContext>(payload);
    }
}

// Middleware pour propager le contexte
public class UserContextMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Extraire le contexte utilisateur du header
        if (context.Request.Headers.TryGetValue("X-User-Context", out var contextHeader))
        {
            var userContext = _contextService.DeserializeUserContext(contextHeader);
            context.Items["UserContext"] = userContext;
        }
        
        await _next(context);
    }
}
```

### OAuth 2.0 et OpenID Connect pour microservices

#### 1. Client Credentials Flow
```csharp
public class OAuthClientService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _tokenCache;
    
    public async Task<string> GetAccessTokenAsync(string scope)
    {
        var cacheKey = $"oauth_token_{scope}";
        
        if (_tokenCache.TryGetValue(cacheKey, out string cachedToken))
        {
            return cachedToken;
        }
        
        var tokenRequest = new
        {
            grant_type = "client_credentials",
            client_id = Configuration["OAuth:ClientId"],
            client_secret = Configuration["OAuth:ClientSecret"],
            scope = scope
        };
        
        var response = await _httpClient.PostAsJsonAsync("/oauth/token", tokenRequest);
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        
        // Cache avec expiration
        _tokenCache.Set(cacheKey, tokenResponse.AccessToken, 
            TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 30));
        
        return tokenResponse.AccessToken;
    }
}
```

#### 2. Token introspection
```csharp
public class TokenIntrospectionService
{
    public async Task<TokenInfo> IntrospectTokenAsync(string token)
    {
        var request = new
        {
            token = token,
            client_id = Configuration["OAuth:ClientId"],
            client_secret = Configuration["OAuth:ClientSecret"]
        };
        
        var response = await _httpClient.PostAsJsonAsync("/oauth/introspect", request);
        var introspectionResult = await response.Content.ReadFromJsonAsync<IntrospectionResponse>();
        
        if (!introspectionResult.Active)
        {
            throw new UnauthorizedAccessException("Token is not active");
        }
        
        return new TokenInfo
        {
            Subject = introspectionResult.Sub,
            Scopes = introspectionResult.Scope?.Split(' '),
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(introspectionResult.Exp),
            ClientId = introspectionResult.ClientId
        };
    }
}
```

---

## **Sécurité des communications inter-services**

### mTLS (Mutual TLS)

#### 1. Configuration automatique avec Istio
```yaml
# Politique mTLS stricte
apiVersion: security.istio.io/v1beta1
kind: PeerAuthentication
metadata:
  name: default
  namespace: production
spec:
  mtls:
    mode: STRICT

---
# Règles d'autorisation
apiVersion: security.istio.io/v1beta1
kind: AuthorizationPolicy
metadata:
  name: user-service-authz
  namespace: production
spec:
  selector:
    matchLabels:
      app: user-service
  rules:
  - from:
    - source:
        principals: ["cluster.local/ns/production/sa/api-gateway"]
    - source:
        principals: ["cluster.local/ns/production/sa/order-service"]
    to:
    - operation:
        methods: ["GET", "POST"]
```

#### 2. Implémentation manuelle mTLS
```csharp
public class MutualTLSHttpClient
{
    private readonly HttpClient _httpClient;
    
    public MutualTLSHttpClient(X509Certificate2 clientCertificate)
    {
        var handler = new HttpClientHandler();
        
        // Certificat client pour l'authentification
        handler.ClientCertificates.Add(clientCertificate);
        
        // Validation personnalisée du certificat serveur
        handler.ServerCertificateCustomValidationCallback = ValidateServerCertificate;
        
        _httpClient = new HttpClient(handler);
    }
    
    private bool ValidateServerCertificate(HttpRequestMessage message, 
        X509Certificate2 certificate, X509Chain chain, SslPolicyErrors errors)
    {
        // Vérifier que le certificat est dans notre liste de confiance
        var trustedThumbprints = Configuration.GetSection("TrustedCertificates")
            .Get<string[]>();
            
        return trustedThumbprints.Contains(certificate.Thumbprint);
    }
}
```

### Message-level Security

#### 1. Chiffrement des payloads
```csharp
public class SecureMessageService
{
    private readonly IEncryptionService _encryption;
    
    public async Task<SecureMessage> CreateSecureMessageAsync<T>(T data, string recipientServiceId)
    {
        // 1. Sérialiser les données
        var jsonData = JsonSerializer.Serialize(data);
        
        // 2. Générer une clé symétrique
        var symmetricKey = GenerateSymmetricKey();
        
        // 3. Chiffrer les données avec la clé symétrique
        var encryptedData = await _encryption.EncryptAsync(jsonData, symmetricKey);
        
        // 4. Chiffrer la clé symétrique avec la clé publique du destinataire
        var recipientPublicKey = await GetServicePublicKeyAsync(recipientServiceId);
        var encryptedKey = await _encryption.EncryptAsymmetricAsync(symmetricKey, recipientPublicKey);
        
        // 5. Créer la signature
        var signature = await _encryption.SignAsync(encryptedData);
        
        return new SecureMessage
        {
            EncryptedData = encryptedData,
            EncryptedKey = encryptedKey,
            Signature = signature,
            SenderId = Configuration["ServiceId"],
            RecipientId = recipientServiceId,
            Timestamp = DateTimeOffset.UtcNow
        };
    }
    
    public async Task<T> DecryptMessageAsync<T>(SecureMessage message)
    {
        // 1. Vérifier la signature
        var senderPublicKey = await GetServicePublicKeyAsync(message.SenderId);
        if (!await _encryption.VerifySignatureAsync(message.EncryptedData, message.Signature, senderPublicKey))
        {
            throw new SecurityException("Invalid message signature");
        }
        
        // 2. Déchiffrer la clé symétrique
        var symmetricKey = await _encryption.DecryptAsymmetricAsync(message.EncryptedKey);
        
        // 3. Déchiffrer les données
        var decryptedJson = await _encryption.DecryptAsync(message.EncryptedData, symmetricKey);
        
        // 4. Désérialiser
        return JsonSerializer.Deserialize<T>(decryptedJson);
    }
}
```

#### 2. Message Authentication Codes (MAC)
```csharp
public class MessageAuthenticationService
{
    private readonly byte[] _sharedSecret;
    
    public string GenerateMAC(string message, string timestamp, string nonce)
    {
        var data = $"{message}|{timestamp}|{nonce}";
        using (var hmac = new HMACSHA256(_sharedSecret))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }
    }
    
    public bool VerifyMAC(string message, string timestamp, string nonce, string providedMAC)
    {
        var expectedMAC = GenerateMAC(message, timestamp, nonce);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(expectedMAC),
            Convert.FromBase64String(providedMAC)
        );
    }
}
```

---

## **Service Mesh et sécurité**

### Istio Security Architecture

#### 1. Citadel - Certificate Management
```yaml
# Configuration automatique des certificats
apiVersion: install.istio.io/v1alpha1
kind: IstioOperator
metadata:
  name: control-plane
spec:
  values:
    pilot:
      env:
        EXTERNAL_ISTIOD: false
    global:
      meshID: mesh1
      multiCluster:
        clusterName: cluster1
      network: network1
      # Certificats personnalisés
      caAddress: "your-ca-server:443"
```

#### 2. Envoy Proxy Security
```yaml
# Configuration Envoy pour RBAC
apiVersion: networking.istio.io/v1alpha3
kind: EnvoyFilter
metadata:
  name: rbac-filter
spec:
  configPatches:
  - applyTo: HTTP_FILTER
    match:
      context: SIDECAR_INBOUND
      listener:
        filterChain:
          filter:
            name: "envoy.filters.network.http_connection_manager"
    patch:
      operation: INSERT_BEFORE
      value:
        name: envoy.filters.http.rbac
        typed_config:
          "@type": type.googleapis.com/envoy.extensions.filters.http.rbac.v3.RBAC
          rules:
            action: ALLOW
            policies:
              "allow-authenticated":
                permissions:
                - any: true
                principals:
                - authenticated:
                    principal_name:
                      exact: "cluster.local/ns/default/sa/api-gateway"
```

### Consul Connect

#### 1. Configuration de service
```hcl
# Configuration Consul Connect
service {
  name = "user-service"
  id   = "user-service-1"
  port = 8080
  
  connect {
    sidecar_service {
      proxy {
        upstreams = [
          {
            destination_name = "database"
            local_bind_port  = 5432
          }
        ]
      }
    }
  }
  
  check {
    name     = "User Service Health"
    http     = "http://localhost:8080/health"
    interval = "10s"
  }
}
```

#### 2. Intentions (Politique d'autorisation)
```hcl
# Autoriser API Gateway -> User Service
resource "consul_config_entry" "user_service_intention" {
  kind = "service-intentions"
  name = "user-service"
  
  config_json = jsonencode({
    Sources = [
      {
        Name   = "api-gateway"
        Action = "allow"
      },
      {
        Name   = "order-service"  
        Action = "allow"
      },
      {
        Name   = "*"
        Action = "deny"
      }
    ]
  })
}
```

---

## **Gestion des secrets et configuration**

### HashiCorp Vault Integration

#### 1. Configuration Vault pour microservices
```csharp
public class VaultSecretManager
{
    private readonly VaultClient _vaultClient;
    private readonly string _mountPoint;
    
    public VaultSecretManager(string vaultUrl, string token, string mountPoint)
    {
        var settings = new VaultClientSettings(vaultUrl, new TokenAuthMethodInfo(token));
        _vaultClient = new VaultClient(settings);
        _mountPoint = mountPoint;
    }
    
    public async Task<T> GetSecretAsync<T>(string path)
    {
        var secret = await _vaultClient.V1.Secrets.KeyValue.V2
            .ReadSecretAsync(path, mountPoint: _mountPoint);
            
        var data = secret.Data.Data;
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(data));
    }
    
    public async Task SetSecretAsync<T>(string path, T secretData)
    {
        var data = new Dictionary<string, object>();
        
        foreach (var prop in typeof(T).GetProperties())
        {
            data[prop.Name] = prop.GetValue(secretData);
        }
        
        await _vaultClient.V1.Secrets.KeyValue.V2
            .WriteSecretAsync(path, data, mountPoint: _mountPoint);
    }
}
```

#### 2. Dynamic secrets avec rotation
```csharp
public class DatabaseCredentialManager
{
    private readonly VaultClient _vault;
    private readonly ILogger<DatabaseCredentialManager> _logger;
    private Timer _rotationTimer;
    
    public async Task<DatabaseCredentials> GetCredentialsAsync()
    {
        // Demander de nouvelles credentials à Vault
        var response = await _vault.V1.Secrets.Database
            .GenerateCredentialsAsync("my-database-role");
            
        var credentials = new DatabaseCredentials
        {
            Username = response.Data["username"].ToString(),
            Password = response.Data["password"].ToString(),
            LeaseId = response.LeaseId,
            LeaseDuration = response.LeaseDurationSeconds
        };
        
        // Programmer le renouvellement
        ScheduleRenewal(credentials);
        
        return credentials;
    }
    
    private void ScheduleRenewal(DatabaseCredentials credentials)
    {
        var renewalTime = TimeSpan.FromSeconds(credentials.LeaseDuration * 0.8); // 80% de la durée
        
        _rotationTimer = new Timer(async _ =>
        {
            try
            {
                await _vault.V1.System.RenewLeaseAsync(credentials.LeaseId);
                _logger.LogInformation("Renewed database credentials lease");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to renew database credentials");
                // Générer de nouvelles credentials
                await GetCredentialsAsync();
            }
        }, null, renewalTime, renewalTime);
    }
}
```

### Kubernetes Secrets avec CSI Driver

#### 1. Configuration Vault CSI
```yaml
# SecretProviderClass pour Vault
apiVersion: secrets-store.csi.x-k8s.io/v1
kind: SecretProviderClass
metadata:
  name: app-secrets
spec:
  provider: vault
  parameters:
    vaultAddress: "https://vault.example.com:8200"
    roleName: "user-service"
    objects: |
      - objectName: "database-password"
        secretPath: "secret/data/user-service/database"
        secretKey: "password"
      - objectName: "api-key"
        secretPath: "secret/data/user-service/external-api"
        secretKey: "key"

---
# Déploiement utilisant les secrets
apiVersion: apps/v1
kind: Deployment
metadata:
  name: user-service
spec:
  template:
    spec:
      serviceAccountName: user-service
      containers:
      - name: user-service
        image: user-service:latest
        volumeMounts:
        - name: secrets-store
          mountPath: "/mnt/secrets"
          readOnly: true
        env:
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: app-secrets
              key: database-password
      volumes:
      - name: secrets-store
        csi:
          driver: secrets-store.csi.k8s.io
          readOnly: true
          volumeAttributes:
            secretProviderClass: "app-secrets"
```

---

## **Monitoring et observabilité**

### Distributed Tracing pour la sécurité

#### 1. OpenTelemetry avec security context
```csharp
public class SecurityTracingService
{
    private readonly ActivitySource _activitySource;
    
    public SecurityTracingService()
    {
        _activitySource = new ActivitySource("SecurityService");
    }
    
    public async Task<T> TraceSecurityOperationAsync<T>(
        string operationName, 
        Func<Task<T>> operation,
        string userId = null,
        string resourceId = null)
    {
        using var activity = _activitySource.StartActivity(operationName);
        
        // Ajouter les tags de sécurité
        activity?.SetTag("security.operation", operationName);
        activity?.SetTag("security.user_id", userId);
        activity?.SetTag("security.resource_id", resourceId);
        activity?.SetTag("security.service_id", Environment.GetEnvironmentVariable("SERVICE_ID"));
        
        try
        {
            var result = await operation();
            activity?.SetTag("security.result", "success");
            return result;
        }
        catch (SecurityException ex)
        {
            activity?.SetTag("security.result", "denied");
            activity?.SetTag("security.error", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetTag("security.result", "error");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
```

#### 2. Security metrics avec Prometheus
```csharp
public class SecurityMetrics
{
    private readonly Counter _authenticationAttempts;
    private readonly Counter _authorizationFailures;
    private readonly Histogram _authenticationDuration;
    private readonly Gauge _activeSessions;
    
    public SecurityMetrics()
    {
        _authenticationAttempts = Metrics
            .CreateCounter("authentication_attempts_total", 
                          "Total number of authentication attempts",
                          new[] { "service", "result" });
                          
        _authorizationFailures = Metrics
            .CreateCounter("authorization_failures_total",
                          "Total number of authorization failures", 
                          new[] { "service", "resource", "action" });
                          
        _authenticationDuration = Metrics
            .CreateHistogram("authentication_duration_seconds",
                           "Duration of authentication operations");
                           
        _activeSessions = Metrics
            .CreateGauge("active_sessions_count",
                        "Number of active user sessions");
    }
    
    public void RecordAuthenticationAttempt(string result)
    {
        _authenticationAttempts
            .WithLabels(Environment.GetEnvironmentVariable("SERVICE_NAME"), result)
            .Inc();
    }
    
    public void RecordAuthorizationFailure(string resource, string action)
    {
        _authorizationFailures
            .WithLabels(Environment.GetEnvironmentVariable("SERVICE_NAME"), resource, action)
            .Inc();
    }
}
```

### Anomaly Detection

#### 1. Détection d'anomalies comportementales
```csharp
public class SecurityAnomalyDetector
{
    private readonly ILogger<SecurityAnomalyDetector> _logger;
    private readonly IMemoryCache _userBehaviorCache;
    
    public async Task<bool> DetectAnomalousActivityAsync(SecurityEvent securityEvent)
    {
        var cacheKey = $"user_behavior_{securityEvent.UserId}";
        var userBehavior = _userBehaviorCache.Get<UserBehaviorProfile>(cacheKey) 
                          ?? new UserBehaviorProfile();
        
        var isAnomalous = false;
        
        // 1. Vérifier la géolocalisation
        if (IsGeographicalAnomaly(userBehavior.TypicalLocations, securityEvent.IPAddress))
        {
            isAnomalous = true;
            _logger.LogWarning("Geographical anomaly detected for user {UserId} from {IPAddress}",
                securityEvent.UserId, securityEvent.IPAddress);
        }
        
        // 2. Vérifier les patterns temporels
        if (IsTemporalAnomaly(userBehavior.TypicalAccessHours, securityEvent.Timestamp))
        {
            isAnomalous = true;
            _logger.LogWarning("Temporal anomaly detected for user {UserId} at {Timestamp}",
                securityEvent.UserId, securityEvent.Timestamp);
        }
        
        // 3. Vérifier la fréquence d'accès
        if (IsFrequencyAnomaly(userBehavior.TypicalRequestRate, securityEvent))
        {
            isAnomalous = true;
            _logger.LogWarning("Frequency anomaly detected for user {UserId}",
                securityEvent.UserId);
        }
        
        // Mettre à jour le profil comportemental
        UpdateUserBehaviorProfile(userBehavior, securityEvent);
        _userBehaviorCache.Set(cacheKey, userBehavior, TimeSpan.FromDays(30));
        
        return isAnomalous;
    }
}
```

---

## **Sécurité des containers et Kubernetes**

### Container Security

#### 1. Multi-stage builds sécurisés
```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["UserService.csproj", "."]
RUN dotnet restore "UserService.csproj"
COPY . .
RUN dotnet build "UserService.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "UserService.csproj" -c Release -o /app/publish

# Stage 3: Runtime (image minimale)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# Créer un utilisateur non-root
RUN adduser --disabled-password --home /app --gecos '' appuser && chown -R appuser /app
USER appuser

WORKDIR /app
COPY --from=publish /app/publish .

# Scanner de vulnérabilités pendant le build
# RUN apt-get update && apt-get install -y --no-install-recommends \
#     curl && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
ENTRYPOINT ["dotnet", "UserService.dll"]
```

#### 2. Security contexts dans Kubernetes
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: user-service
spec:
  template:
    spec:
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
        runAsGroup: 3000
        fsGroup: 2000
        seccompProfile:
          type: RuntimeDefault
      containers:
      - name: user-service
        image: user-service:latest
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
            memory: "512Mi"
            cpu: "500m"
          requests:
            memory: "256Mi"
            cpu: "250m"
        volumeMounts:
        - name: tmp-volume
          mountPath: /tmp
        - name: var-log
          mountPath: /var/log
      volumes:
      - name: tmp-volume
        emptyDir: {}
      - name: var-log
        emptyDir: {}
```

### RBAC (Role-Based Access Control)

#### 1. Principe du moindre privilège
```yaml
# ServiceAccount pour le service
apiVersion: v1
kind: ServiceAccount
metadata:
  name: user-service
  namespace: production

---
# Role avec permissions minimales
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  namespace: production
  name: user-service-role
rules:
- apiGroups: [""]
  resources: ["configmaps"]
  verbs: ["get", "list"]
- apiGroups: [""]
  resources: ["secrets"]
  verbs: ["get"]
  resourceNames: ["user-service-secrets"]

---
# RoleBinding
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: user-service-binding
  namespace: production
subjects:
- kind: ServiceAccount
  name: user-service
  namespace: production
roleRef:
  kind: Role
  name: user-service-role
  apiGroup: rbac.authorization.k8s.io
```

### Pod Security Standards

#### 1. Pod Security Policy (deprecated) / Pod Security Standards
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: production
  labels:
    pod-security.kubernetes.io/enforce: restricted
    pod-security.kubernetes.io/audit: restricted
    pod-security.kubernetes.io/warn: restricted
```

---

## **Patterns de sécurité pour microservices**

### 1. API Gateway Pattern
```csharp
public class SecurityApiGateway
{
    private readonly IAuthenticationService _auth;
    private readonly IAuthorizationService _authz;
    private readonly IRateLimitingService _rateLimit;
    private readonly IServiceDiscovery _serviceDiscovery;
    
    public async Task<IActionResult> ProcessRequestAsync(HttpContext context)
    {
        // 1. Rate limiting
        if (!await _rateLimit.IsAllowedAsync(context.Connection.RemoteIpAddress))
        {
            return new StatusCodeResult(429);
        }
        
        // 2. Authentication
        var authResult = await _auth.AuthenticateAsync(context);
        if (!authResult.Succeeded)
        {
            return new UnauthorizedResult();
        }
        
        // 3. Authorization
        var authzResult = await _authz.AuthorizeAsync(authResult.Principal, context);
        if (!authzResult.Succeeded)
        {
            return new ForbidResult();
        }
        
        // 4. Service discovery et routing
        var targetService = await _serviceDiscovery.FindServiceAsync(context.Request.Path);
        
        // 5. Request transformation
        var transformedRequest = TransformRequest(context.Request, authResult.Principal);
        
        // 6. Proxy vers le service
        return await ProxyToServiceAsync(targetService, transformedRequest);
    }
}
```

### 2. Circuit Breaker Pattern
```csharp
public class SecureCircuitBreaker
{
    private readonly CircuitBreakerState _state = CircuitBreakerState.Closed;
    private readonly int _failureThreshold = 5;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);
    private int _failureCount = 0;
    private DateTime _lastFailureTime;
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (_state == CircuitBreakerState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _timeout)
            {
                _state = CircuitBreakerState.HalfOpen;
            }
            else
            {
                throw new CircuitBreakerOpenException("Service is currently unavailable");
            }
        }
        
        try
        {
            var result = await operation();
            
            if (_state == CircuitBreakerState.HalfOpen)
            {
                _state = CircuitBreakerState.Closed;
                _failureCount = 0;
            }
            
            return result;
        }
        catch (SecurityException)
        {
            // Les exceptions de sécurité ne doivent pas ouvrir le circuit
            throw;
        }
        catch (Exception)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            
            if (_failureCount >= _failureThreshold)
            {
                _state = CircuitBreakerState.Open;
            }
            
            throw;
        }
    }
}
```

### 3. Bulkhead Pattern
```csharp
public class SecurityBulkheadService
{
    private readonly SemaphoreSlim _authenticationSemaphore;
    private readonly SemaphoreSlim _authorizationSemaphore;
    private readonly SemaphoreSlim _auditingSemaphore;
    
    public SecurityBulkheadService()
    {
        // Isolation des ressources critiques
        _authenticationSemaphore = new SemaphoreSlim(10, 10); // 10 auth concurrentes max
        _authorizationSemaphore = new SemaphoreSlim(20, 20);  // 20 authz concurrentes max
        _auditingSemaphore = new SemaphoreSlim(5, 5);         // 5 audits concurrents max
    }
    
    public async Task<AuthResult> AuthenticateAsync(string token)
    {
        await _authenticationSemaphore.WaitAsync();
        try
        {
            return await PerformAuthenticationAsync(token);
        }
        finally
        {
            _authenticationSemaphore.Release();
        }
    }
    
    public async Task AuditSecurityEventAsync(SecurityEvent evt)
    {
        await _auditingSemaphore.WaitAsync();
        try
        {
            await WriteAuditLogAsync(evt);
        }
        finally
        {
            _auditingSemaphore.Release();
        }
    }
}
```

---

## **Checklist de sécurité pour microservices**

### Développement
- [ ] Validation stricte des entrées
- [ ] Gestion sécurisée des erreurs (pas de fuite d'information)
- [ ] Principe du moindre privilège
- [ ] Chiffrement des données sensibles
- [ ] Audit et logging complets

### Communication
- [ ] mTLS ou chiffrement au niveau message
- [ ] Authentification des services
- [ ] Autorisation granulaire
- [ ] Rate limiting et protection DDoS
- [ ] Validation des certificats

### Déploiement
- [ ] Images containers scannées
- [ ] Utilisateur non-root
- [ ] Secrets externalisés
- [ ] Network policies configurées
- [ ] Security contexts appliqués

### Monitoring
- [ ] Tracing distribué avec contexte sécurité
- [ ] Métriques de sécurité
- [ ] Détection d'anomalies
- [ ] Alertes en temps réel
- [ ] Audit trail complet

Cette formation couvre les aspects critiques de la sécurité dans les architectures de microservices, complétant ainsi votre arsenal de connaissances en cybersécurité.
