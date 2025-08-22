# **Cryptographie - Fiche de Révision Entretien**

## **Questions Fréquentes et Réponses Types**

### **Q: Quelle est la différence entre chiffrement symétrique et asymétrique ?**

**Réponse structurée :**
- **Symétrique** : Une seule clé pour chiffrer/déchiffrer
  - Avantages : Rapide, efficace pour gros volumes
  - Inconvénients : Partage de clé complexe
  - Exemples : AES-256, ChaCha20
  - Usage : Chiffrement de données, communications établies

- **Asymétrique** : Paire clé publique/privée
  - Avantages : Pas de partage secret, non-répudiation
  - Inconvénients : Lent, limité en taille
  - Exemples : RSA-2048+, ECC-256
  - Usage : Échange de clés, signatures, authentification

### **Q: Comment sécuriser les communications entre microservices ?**

**Solutions techniques :**

1. **mTLS (Mutual TLS)**
   ```
   Service A ←→ [Certificat Client/Serveur] ←→ Service B
   ```
   - Authentification bidirectionnelle
   - Chiffrement bout-en-bout
   - Rotation automatique des certificats

2. **JWT avec RS256**
   ```
   API Gateway → [JWT signé] → Service → Vérification signature
   ```
   - Signature asymétrique (RS256 > HS256)
   - Claims courts, expiration courte (<15min)
   - Validation stricte côté service

3. **Service Mesh (Istio/Linkerd)**
   - mTLS automatique entre tous les services
   - Policies d'autorisation déclaratives
   - Observabilité intégrée

### **Q: Que faire contre une attaque Man-in-the-Middle ?**

**Mesures de prévention :**
- **Certificate Pinning** : Épingler les certificats attendus
- **HSTS** : Forcer HTTPS avec preload
- **mTLS** : Authentification mutuelle
- **Certificate Transparency** : Surveiller les certificats émis

### **Q: Comment choisir un algorithme de hachage ?**

**Recommandations par usage :**
- **Mots de passe** : bcrypt, scrypt, Argon2 (avec salt + iterations)
- **Intégrité fichiers** : SHA-256, SHA-3
- **HMAC** : SHA-256 ou supérieur
- **⚠️ Éviter** : MD5, SHA-1 (vulnérables aux collisions)

### **Q: Qu'est-ce que le Perfect Forward Secrecy (PFS) ?**

**Définition** : Si une clé privée est compromise, les communications passées restent sécurisées.

**Implémentation** :
- Utiliser des clés éphémères (ECDHE, DHE)
- Renouveler les clés de session régulièrement
- Ne pas réutiliser les clés

---

## **Concepts Clés pour Entretiens**

### **Salt et Poivre (Salt & Pepper)**
- **Salt** : Valeur aléatoire unique par mot de passe
- **Pepper** : Secret global partagé (optionnel)
- **Objectif** : Empêcher les attaques par rainbow tables

### **Key Derivation Functions (KDF)**
- **PBKDF2** : Standard, configurable
- **scrypt** : Résistant aux attaques matérielles
- **Argon2** : Gagnant concours 2015, recommandé

### **Modes de chiffrement (AES)**
- **ECB** : ❌ Patterns visibles, jamais utiliser
- **CBC** : ✅ Avec IV aléatoire
- **GCM** : ✅ Authentification intégrée (recommandé)
- **CTR** : ✅ Parallélisable

### **Rotation des clés**
- **Fréquence** : Selon criticité (90 jours typique)
- **Processus** : Génération → Migration → Révocation
- **Zero-downtime** : Support multi-versions temporaire

---

## **Erreurs Courantes à Éviter**

### **❌ Erreurs Cryptographiques**
1. **Clés hardcodées** dans le code
2. **IV/Nonce réutilisés** (surtout en GCM)
3. **Générateurs aléatoires faibles** (Math.random())
4. **Comparaisons temporelles** vulnérables aux timing attacks
5. **Validation de certificats désactivée** en dev/test

### **✅ Bonnes Pratiques**
1. **Utiliser des bibliothèques éprouvées** (pas de crypto maison)
2. **Stockage sécurisé** des clés (HSM, Key Vault)
3. **Rotation automatique** des clés
4. **Audit et monitoring** des opérations crypto
5. **Tests de non-régression** sécurité

---

## **Questions Pièges Courantes**

### **Q: "Le HTTPS suffit-il à sécuriser une API ?"**
**Réponse** : Non, HTTPS sécurise le transport mais il faut aussi :
- Authentification (OAuth 2.0, JWT)
- Autorisation (RBAC, ABAC)
- Validation des entrées
- Rate limiting
- Audit des accès

### **Q: "Peut-on utiliser MD5 pour vérifier l'intégrité ?"**
**Réponse** : Non, MD5 est vulnérable aux collisions. Utiliser SHA-256+ ou SHA-3.

### **Q: "Comment stocker des mots de passe de façon sécurisée ?"**
**Réponse** : 
```
Mot de passe → Salt unique → bcrypt/Argon2 → Hash stocké
```
- Jamais en clair ou avec hash simple
- Salt différent par utilisateur
- Iterations suffisantes (bcrypt: 12+, Argon2: selon config)

---

## **Implémentations Techniques Rapides**

### **Chiffrement AES-GCM (.NET)**
```csharp
// Sécurisé : AES-GCM avec clé et nonce uniques
using var aes = new AesGcm(key);
aes.Encrypt(nonce, plaintext, ciphertext, tag);
```

### **Validation JWT**
```csharp
// Validation stricte avec RS256
var validationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = GetPublicKey(),
    ValidateIssuer = true,
    ValidIssuer = "trusted-issuer",
    ValidateAudience = true,
    ValidAudience = "api-audience",
    ClockSkew = TimeSpan.Zero // Pas de tolérance d'horloge
};
```

### **Hash de mot de passe**
```csharp
// bcrypt avec salt automatique
string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
```

---

## **Introduction à la cryptographie**

### Qu'est-ce que la cryptographie ?
La **cryptographie** est la science qui étudie les techniques permettant de sécuriser les communications et les données en les rendant incompréhensibles pour les personnes non autorisées.

### Objectifs de la cryptographie
- **Confidentialité** : Seules les personnes autorisées peuvent lire les données
- **Intégrité** : Assurer que les données n'ont pas été modifiées
- **Authentification** : Vérifier l'identité des parties
- **Non-répudiation** : Empêcher le déni d'une action

### Types de cryptographie
1. **Cryptographie symétrique** (clé secrète)
2. **Cryptographie asymétrique** (clé publique/privée)
3. **Fonctions de hachage** (empreintes)

---

## **Chiffrement symétrique**

### Principe
Le chiffrement symétrique utilise **la même clé** pour chiffrer et déchiffrer les données. Cette clé doit être partagée entre l'expéditeur et le destinataire.

```
Données + Clé secrète → Chiffrement → Données chiffrées
Données chiffrées + Clé secrète → Déchiffrement → Données originales
```

### Algorithmes populaires

#### AES (Advanced Encryption Standard)
- **Standard actuel** pour le chiffrement symétrique
- Tailles de clés : 128, 192, 256 bits
- Très rapide et sécurisé
- Utilisé par les gouvernements et entreprises

```csharp
// Exemple AES en C#
using System.Security.Cryptography;

public class AESExample
{
    public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            
            using (var encryptor = aes.CreateEncryptor())
            {
                return encryptor.TransformFinalBlock(data, 0, data.Length);
            }
        }
    }
}
```

#### Autres algorithmes
- **DES** : Obsolète (clé trop courte : 56 bits)
- **3DES** : Amélioration de DES, mais lent
- **ChaCha20** : Alternative moderne à AES

### Modes de chiffrement
- **ECB** (Electronic Codebook) : ⚠️ Non sécurisé
- **CBC** (Cipher Block Chaining) : Sécurisé avec IV
- **GCM** (Galois/Counter Mode) : ✅ Recommandé (authentification intégrée)
- **CTR** (Counter Mode) : Parallélisable

### Avantages
- ✅ **Très rapide**
- ✅ **Efficace** pour de gros volumes de données
- ✅ **Moins de calculs** que l'asymétrique

### Inconvénients
- ❌ **Partage de clé** complexe
- ❌ **Problème de distribution** des clés
- ❌ **Pas de non-répudiation**

---

## **Chiffrement asymétrique**

### Principe
Le chiffrement asymétrique utilise **une paire de clés** :
- **Clé publique** : Partagée librement, utilisée pour chiffrer
- **Clé privée** : Gardée secrète, utilisée pour déchiffrer

```
Données + Clé publique → Chiffrement → Données chiffrées
Données chiffrées + Clé privée → Déchiffrement → Données originales
```

### Algorithmes populaires

#### RSA (Rivest-Shamir-Adleman)
- **Le plus utilisé** historiquement
- Basé sur la factorisation de grands nombres premiers
- Tailles de clés : 2048, 3072, 4096 bits minimum

```csharp
// Exemple RSA en C#
using System.Security.Cryptography;

public class RSAExample
{
    public static (string publicKey, string privateKey) GenerateKeys()
    {
        using (RSA rsa = RSA.Create(2048))
        {
            return (
                rsa.ToXmlString(false), // Clé publique
                rsa.ToXmlString(true)   // Clé privée
            );
        }
    }
    
    public static byte[] Encrypt(byte[] data, string publicKey)
    {
        using (RSA rsa = RSA.Create())
        {
            rsa.FromXmlString(publicKey);
            return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
        }
    }
}
```

#### ECC (Elliptic Curve Cryptography)
- **Plus moderne** et efficace que RSA
- **Clés plus courtes** pour la même sécurité (256 bits ECC ≈ 3072 bits RSA)
- Algorithmes : ECDSA, ECDH

#### Autres algorithmes
- **Diffie-Hellman** : Échange de clés
- **Ed25519** : Signatures (basé sur courbes elliptiques)

### Utilisations
1. **Chiffrement hybride** : Chiffrer une clé symétrique
2. **Signatures numériques** : Authentification et non-répudiation
3. **Échange de clés** : Établir une communication sécurisée

### Avantages
- ✅ **Pas de partage de secret** préalable
- ✅ **Non-répudiation** possible
- ✅ **Distribution de clés** simplifiée

### Inconvénients
- ❌ **Très lent** comparé au symétrique
- ❌ **Limité en taille** de données
- ❌ **Plus complexe** à implémenter

---

## **Fonctions de hachage**

### Principe
Une fonction de hachage transforme des données de taille arbitraire en une **empreinte fixe** (hash). C'est une fonction à sens unique.

```
Données → Fonction de hachage → Empreinte (Hash)
```

### Propriétés
- **Déterministe** : Même entrée = même hash
- **Avalanche** : Petite modification = hash complètement différent
- **Irréversible** : Impossible de retrouver l'original
- **Résistant aux collisions** : Difficile de trouver deux entrées avec le même hash

### Algorithmes

#### SHA-2 famille
- **SHA-256** : ✅ Standard actuel (256 bits)
- **SHA-384**, **SHA-512** : Versions plus longues

#### SHA-3
- **Keccak** : Alternative à SHA-2
- Plus résistant à certaines attaques

#### Obsolètes
- **MD5** : ⚠️ Vulnérable aux collisions
- **SHA-1** : ⚠️ Déprécié

```csharp
// Exemple SHA-256 en C#
using System.Security.Cryptography;
using System.Text;

public static string ComputeSHA256(string input)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
```

### Utilisations
- **Intégrité des données** : Vérifier qu'un fichier n'a pas été modifié
- **Stockage de mots de passe** : Avec salt et iterations
- **Proof of Work** : Bitcoin, blockchain
- **Signatures numériques** : Hacher avant de signer

---

## **Signatures numériques**

### Principe
Une signature numérique garantit :
- **Authentification** : Qui a signé
- **Intégrité** : Le document n'a pas été modifié
- **Non-répudiation** : Le signataire ne peut pas nier

### Processus
1. **Hacher** le document
2. **Chiffrer** le hash avec la clé privée = Signature
3. **Vérifier** en déchiffrant avec la clé publique

```
Document → Hash → Chiffrement (clé privée) → Signature
Signature + Document → Vérification (clé publique) → Valide/Invalide
```

### Implémentation
```csharp
// Signature avec RSA
public static byte[] SignData(byte[] data, RSA privateKey)
{
    return privateKey.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
}

public static bool VerifySignature(byte[] data, byte[] signature, RSA publicKey)
{
    return publicKey.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
}
```

---

## **Certificats et PKI**

### Certificats X.509
Un **certificat** lie une clé publique à une identité, signé par une autorité de certification (CA).

### Contenu d'un certificat
- **Clé publique** du propriétaire
- **Identité** (nom, organisation)
- **Période de validité**
- **Signature** de l'autorité de certification

### PKI (Public Key Infrastructure)
- **CA Root** : Autorité racine
- **CA Intermédiaires** : Délèguent la signature
- **Certificats finaux** : Pour les utilisateurs/serveurs

### Chaîne de confiance
```
CA Root → CA Intermédiaire → Certificat Serveur
```

---

## **Chiffrement dans le développement logiciel**

### Cas d'usage

#### 1. Stockage de données sensibles
```csharp
// Chiffrement des données en base
public class EncryptedUserRepository
{
    private readonly IAESEncryption _encryption;
    
    public void SaveUser(User user)
    {
        user.Email = _encryption.Encrypt(user.Email);
        user.Phone = _encryption.Encrypt(user.Phone);
        // Sauvegarde en base
    }
}
```

#### 2. Configuration et secrets
```csharp
// Chiffrement des configurations
public class SecureConfiguration
{
    public string GetConnectionString()
    {
        var encrypted = ConfigurationManager.AppSettings["DbConnection"];
        return _encryption.Decrypt(encrypted);
    }
}
```

#### 3. Communications API
```csharp
// Chiffrement des payloads API
[HttpPost]
public async Task<IActionResult> ProcessPayment([FromBody] EncryptedPayload payload)
{
    var decryptedData = _encryption.Decrypt(payload.Data);
    var payment = JsonSerializer.Deserialize<PaymentRequest>(decryptedData);
    // Traitement...
}
```

### Gestion des clés
```csharp
// Utilisation d'Azure Key Vault
public class KeyVaultEncryption
{
    private readonly KeyClient _keyClient;
    
    public async Task<string> EncryptAsync(string data, string keyName)
    {
        var key = await _keyClient.GetKeyAsync(keyName);
        // Chiffrement avec la clé récupérée
    }
}
```

---

## **Chiffrement et microservices**

### Défis spécifiques
- **Communication inter-services**
- **Gestion distribuée des clés**
- **Performance** sur de nombreux appels
- **Rotation des clés**

### Patterns de sécurité

#### 1. Service Mesh avec mTLS
```yaml
# Istio - mTLS automatique
apiVersion: security.istio.io/v1beta1
kind: PeerAuthentication
metadata:
  name: default
spec:
  mtls:
    mode: STRICT
```

#### 2. API Gateway avec JWT
```csharp
// Validation JWT dans l'API Gateway
public class JWTMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"];
        var principal = ValidateJWT(token);
        context.User = principal;
        // Redirection vers le microservice
    }
}
```

#### 3. Chiffrement au niveau application
```csharp
// Service de chiffrement centralisé
public interface IEncryptionService
{
    Task<string> EncryptAsync(string data, string serviceId);
    Task<string> DecryptAsync(string encryptedData, string serviceId);
}

// Utilisation dans un microservice
public class OrderService
{
    private readonly IEncryptionService _encryption;
    
    public async Task CreateOrderAsync(Order order)
    {
        order.CreditCard = await _encryption.EncryptAsync(order.CreditCard, "order-service");
        await _repository.SaveAsync(order);
    }
}
```

### Architecture recommandée

#### Centralized Key Management
```
┌─────────────────┐    ┌──────────────────┐
│   Key Vault     │◄───┤  Microservice A  │
│   (Azure/AWS)   │    └──────────────────┘
│                 │    ┌──────────────────┐
│                 │◄───┤  Microservice B  │
└─────────────────┘    └──────────────────┘
```

#### Service Mesh Pattern
```
┌──────────────┐    mTLS    ┌──────────────┐
│ Service A    │◄──────────►│  Service B   │
│ (Envoy Proxy)│            │(Envoy Proxy) │
└──────────────┘            └──────────────┘
        ▲                           ▲
        │                           │
        └─────── Control Plane ─────┘
             (Istio/Consul)
```

### Avantages
- ✅ **Isolation** : Chaque service gère ses propres clés
- ✅ **Scalabilité** : Distribution des opérations
- ✅ **Résilience** : Pas de point unique de défaillance

### Inconvénients
- ❌ **Complexité** : Gestion distribuée des clés
- ❌ **Performance** : Latence supplémentaire
- ❌ **Monitoring** : Traçabilité plus difficile

---

## **Attaques communes**

### 1. Man-in-the-Middle (MITM)
**Principe** : L'attaquant s'interpose entre deux parties pour intercepter ou modifier les communications.

#### Comment ça marche ?
```
Client ──────► Attaquant ──────► Serveur
       ◄──────         ◄──────
```

#### Scénarios
- **WiFi public non sécurisé**
- **Certificats falsifiés**
- **DNS Spoofing**
- **ARP Poisoning**

#### Protection
```csharp
// Validation stricte des certificats
public class SecureHttpClient
{
    private readonly HttpClient _client;
    
    public SecureHttpClient()
    {
        var handler = new HttpClientHandler();
        
        // Validation personnalisée du certificat
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            // Vérifier le certificat contre une liste blanche
            return ValidateCertificate(cert);
        };
        
        _client = new HttpClient(handler);
    }
}
```

### 2. Attaques par force brute
**Principe** : Essayer toutes les combinaisons possibles pour trouver la clé.

#### Protection
```csharp
// Ralentissement des tentatives
public class RateLimitedEncryption
{
    private readonly Dictionary<string, DateTime> _lastAttempts = new();
    
    public bool TryDecrypt(string encryptedData, string password)
    {
        var clientId = GetClientId();
        
        if (_lastAttempts.ContainsKey(clientId))
        {
            var timeSinceLastAttempt = DateTime.Now - _lastAttempts[clientId];
            if (timeSinceLastAttempt < TimeSpan.FromSeconds(1))
            {
                throw new TooManyAttemptsException();
            }
        }
        
        _lastAttempts[clientId] = DateTime.Now;
        return PerformDecryption(encryptedData, password);
    }
}
```

### 3. Attaques par collision (Hash)
**Principe** : Trouver deux entrées qui produisent le même hash.

#### Protection
- ✅ Utiliser **SHA-256** ou supérieur
- ✅ Ajouter un **salt** pour les mots de passe
- ❌ Éviter MD5 et SHA-1

### 4. Attaques temporelles (Timing attacks)
**Principe** : Analyser le temps de réponse pour deviner des informations.

#### Protection
```csharp
// Comparaison en temps constant
public static bool SecureEquals(byte[] a, byte[] b)
{
    if (a.Length != b.Length) return false;
    
    int result = 0;
    for (int i = 0; i < a.Length; i++)
    {
        result |= a[i] ^ b[i];
    }
    return result == 0;
}
```

### 5. Attaques par canal auxiliaire
**Principe** : Exploiter les informations qui "fuient" (consommation électrique, émissions électromagnétiques).

#### Protection
- Utiliser des **implémentations sécurisées**
- **Bibliothèques cryptographiques** éprouvées
- **Hardware Security Modules (HSM)**

---

## **Bonnes pratiques**

### 1. Choix des algorithmes
```
✅ Recommandés :
- Symétrique : AES-256-GCM
- Asymétrique : RSA-3072, ECC-256
- Hash : SHA-256, SHA-3
- Échange de clés : ECDH, X25519

❌ À éviter :
- DES, 3DES
- RSA < 2048 bits
- MD5, SHA-1
```

### 2. Gestion des clés
```csharp
// Ne jamais hardcoder les clés
❌ var key = "mySecretKey123"; 

// Utiliser des services dédiés
✅ var key = await _keyVault.GetSecretAsync("encryption-key");

// Rotation régulière
✅ public class KeyRotationService
{
    public async Task RotateKeysAsync()
    {
        var newKey = GenerateNewKey();
        await _keyVault.SetSecretAsync("encryption-key-new", newKey);
        
        // Migration graduelle
        await MigrateDataToNewKeyAsync();
        
        await _keyVault.DeleteSecretAsync("encryption-key-old");
    }
}
```

### 3. Stockage sécurisé
```csharp
// Hash des mots de passe avec salt
public class PasswordService
{
    public string HashPassword(string password)
    {
        var salt = GenerateRandomSalt();
        var hash = BCrypt.Net.BCrypt.HashPassword(password, salt, 12); // 12 rounds
        return hash;
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
```

### 4. Configuration sécurisée
```json
// appsettings.json - Ne jamais stocker de secrets
{
  "KeyVault": {
    "VaultUri": "https://myvault.vault.azure.net/"
  },
  "Encryption": {
    "Algorithm": "AES-256-GCM",
    "KeyRotationDays": 90
  }
}
```

### 5. Monitoring et audit
```csharp
public class CryptoAuditService
{
    public async Task LogEncryptionEventAsync(string operation, string keyId, string userId)
    {
        var auditEvent = new CryptoAuditEvent
        {
            Operation = operation,
            KeyId = keyId,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            SourceIP = GetClientIP()
        };
        
        await _auditLogger.LogAsync(auditEvent);
    }
}
```

### 6. Tests de sécurité
```csharp
[Test]
public void Encryption_ShouldProduceDifferentOutputs_ForSameInput()
{
    // Vérifier que l'IV/nonce est aléatoire
    var plaintext = "sensitive data";
    var encrypted1 = _encryption.Encrypt(plaintext);
    var encrypted2 = _encryption.Encrypt(plaintext);
    
    Assert.AreNotEqual(encrypted1, encrypted2);
    Assert.AreEqual(plaintext, _encryption.Decrypt(encrypted1));
    Assert.AreEqual(plaintext, _encryption.Decrypt(encrypted2));
}
```

---

## **Ressources supplémentaires**

### Bibliothèques recommandées
- **.NET** : System.Security.Cryptography, Bouncy Castle
- **Node.js** : crypto (built-in), node-forge
- **Python** : cryptography, PyCrypto
- **Java** : JCE, Bouncy Castle

### Standards et spécifications
- **NIST** : Guidelines for cryptographic algorithms
- **RFC 7539** : ChaCha20 and Poly1305
- **RFC 8446** : TLS 1.3
- **FIPS 140-2** : Security requirements for cryptographic modules

### Outils d'audit
- **OpenSSL** : Tests et génération de certificats
- **Nmap** : Scan des protocoles de chiffrement
- **SSLLabs** : Test de configuration TLS
- **Burp Suite** : Tests de sécurité des applications web
