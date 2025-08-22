# **Sécurité Web - Fiche de Révision Entretien**

## **OWASP Top 10 - Questions & Solutions Techniques**

### **A01: Broken Access Control**

**Q: Comment prévenir les failles d'autorisation ?**

**Solutions immédiates :**
1. **Contrôles côté serveur obligatoires**
   ```csharp
   // ❌ Mauvais : UI cache les boutons
   if (user.Role == "User") { /* hide admin buttons */ }
   
   // ✅ Bon : Validation serveur
   [Authorize(Policy = "AdminOnly")]
   public IActionResult DeleteUser(int id) { }
   ```

2. **Principle of Least Privilege**
   - Accès refusé par défaut
   - Permissions explicites uniquement
   - Vérification à chaque requête

3. **Object-Level Authorization**
   ```csharp
   // Vérifier que l'utilisateur peut accéder à CETTE ressource
   if (document.OwnerId != currentUser.Id && !currentUser.IsAdmin)
       return Forbid();
   ```

### **A02: Cryptographic Failures**

**Q: Comment éviter les échecs cryptographiques ?**

**Checklist technique :**
- ✅ **TLS 1.3** minimum, certificats valides
- ✅ **Données sensibles chiffrées** au repos (AES-256)
- ✅ **Clés séparées** du code (Key Vault)
- ✅ **Salt unique** par mot de passe
- ✅ **Perfect Forward Secrecy** (ECDHE)

### **A03: Injection**

**Q: Comment se protéger des injections SQL ?**

**Solutions prioritaires :**
1. **Requêtes paramétrées** (jamais de concaténation)
   ```csharp
   // ❌ Vulnérable
   var sql = $"SELECT * FROM Users WHERE Id = {userId}";
   
   // ✅ Sécurisé
   var sql = "SELECT * FROM Users WHERE Id = @UserId";
   command.Parameters.AddWithValue("@UserId", userId);
   ```

2. **ORM avec protection** (Entity Framework, Dapper)
3. **Validation stricte** des entrées
4. **Principe du moindre privilège** DB

**Q: Et pour NoSQL injection ?**
```javascript
// ❌ Vulnérable
db.users.find({ username: req.body.username });

// ✅ Sécurisé
db.users.find({ username: { $eq: req.body.username } });
```

### **A04: Insecure Design**

**Q: Qu'est-ce qu'un design sécurisé ?**

**Principes fondamentaux :**
- **Threat Modeling** dès la conception
- **Security by Default** (config sécurisée)
- **Fail Securely** (échec = mode sécurisé)
- **Defense in Depth** (plusieurs couches)

**Exemple concret :**
```
Reset password = Email + SMS + Questions sécurité
(pas juste email)
```

### **A05: Security Misconfiguration**

**Q: Comment éviter les mauvaises configurations ?**

**Checklist déploiement :**
- ❌ **Erreurs détaillées** en production
- ❌ **Comptes par défaut** activés
- ❌ **Fonctionnalités inutiles** actives
- ✅ **Headers de sécurité** configurés
- ✅ **Mises à jour** régulières
- ✅ **Principe du moindre privilège**

**Headers sécurité essentiels :**
```http
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Content-Security-Policy: default-src 'self'
```

### **A06: Vulnerable Components**

**Q: Comment gérer les dépendances vulnérables ?**

**Process continu :**
1. **Inventaire** des composants (SBOM)
2. **Scan automatique** (Dependabot, Snyk)
3. **Mise à jour** prioritaire (CVE critiques)
4. **Tests de régression** après updates
5. **Monitoring** des nouvelles CVE

### **A07: Authentication Failures**

**Q: Comment implémenter une authentification robuste ?**

**Éléments essentiels :**
1. **MFA obligatoire** (TOTP, FIDO2)
2. **Politique mots de passe** forte
   ```
   Min 12 caractères + 4 types de caractères
   Pas de mots du dictionnaire
   ```
3. **Rate limiting** anti-brute force
4. **Session management** sécurisé
   ```csharp
   // Cookies sécurisés
   options.Cookie.Secure = true;
   options.Cookie.HttpOnly = true;
   options.Cookie.SameSite = SameSiteMode.Strict;
   ```

### **A08: Software Integrity Failures**

**Q: Comment garantir l'intégrité logicielle ?**

**Mesures techniques :**
- **Signature de code** (Authenticode, GPG)
- **Checksum SHA-256** des téléchargements
- **Pipeline CI/CD** sécurisé
- **Supply chain security** (vérification des sources)

### **A09: Logging Failures**

**Q: Que logger pour la sécurité ?**

**Événements critiques :**
```csharp
// Login attempts
logger.LogWarning("Failed login: {Username} from {IP}", username, ipAddress);

// Privilege changes
logger.LogInformation("User {UserId} granted role {Role} by {AdminId}", 
    userId, role, adminId);

// Data access
logger.LogInformation("Sensitive data accessed: {Resource} by {UserId}", 
    resourceId, userId);
```

**Ne PAS logger :**
- Mots de passe, tokens
- Données personnelles (RGPD)
- Numéros de carte bancaire

### **A10: Server-Side Request Forgery (SSRF)**

**Q: Comment prévenir les attaques SSRF ?**

**Protection technique :**
```csharp
// Whitelist d'URLs autorisées
private readonly string[] _allowedHosts = { "api.trusted.com" };

public async Task<string> FetchData(string url)
{
    var uri = new Uri(url);
    
    // Vérifier le host
    if (!_allowedHosts.Contains(uri.Host))
        throw new SecurityException("Host not allowed");
    
    // Bloquer les IPs privées
    var ip = await Dns.GetHostAddressesAsync(uri.Host);
    if (IsPrivateIP(ip.First()))
        throw new SecurityException("Private IP not allowed");
    
    return await _httpClient.GetStringAsync(url);
}
```

---

## **Authentification & Autorisation - Questions Entretien**

### **Q: OAuth 2.0 vs OpenID Connect ?**

**OAuth 2.0** : Authorization (déléguer l'accès)
**OpenID Connect** : Authentication (vérifier l'identité) = OAuth 2.0 + JWT ID Token

**Flows OAuth :**
- **Authorization Code** : Applications web (avec PKCE)
- **Client Credentials** : Service-to-service
- **Device Flow** : TV, IoT
- ❌ **Implicit** : Déprécié (tokens dans URL)

### **Q: JWT vs Sessions ?**

**JWT** :
- ✅ Stateless, scalable
- ❌ Révocation difficile, taille limitée
- Usage : API, microservices

**Sessions** :
- ✅ Révocation immédiate
- ❌ Stockage serveur requis
- Usage : Applications web traditionnelles

### **Q: Comment sécuriser une API REST ?**

**Checklist sécurité API :**
1. **HTTPS obligatoire** (TLS 1.3)
2. **Authentication** (Bearer token, API key)
3. **Authorization** (scopes, permissions)
4. **Rate limiting** (par utilisateur/IP)
5. **Input validation** stricte
6. **CORS** configuré correctement
7. **Versioning** sécurisé

---

## **Attaques Courantes - Détection & Prévention**

### **Cross-Site Scripting (XSS)**

**Types & Solutions :**
1. **Stored XSS** : Données malveillantes stockées
   - Solution : Encoding HTML à l'affichage
2. **Reflected XSS** : Payload dans l'URL
   - Solution : Validation input + CSP
3. **DOM XSS** : Manipulation côté client
   - Solution : Sanitisation JS + CSP

**Content Security Policy :**
```http
Content-Security-Policy: default-src 'self'; 
script-src 'self' 'nonce-abc123'; 
style-src 'self' 'unsafe-inline'
```

### **Cross-Site Request Forgery (CSRF)**

**Protection technique :**
```csharp
// Token anti-CSRF
services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SameSite = SameSiteMode.Strict;
});

[ValidateAntiForgeryToken]
public IActionResult Transfer(TransferRequest request) { }
```

### **Clickjacking**

**Headers de protection :**
```http
X-Frame-Options: DENY
Content-Security-Policy: frame-ancestors 'none'
```

---

## **Questions Pièges & Réponses Types**

### **Q: "Que faire si on découvre une faille 0-day ?"**
1. **Isolation** immédiate du système
2. **Assessment** de l'impact
3. **Patch temporaire** ou contournement
4. **Communication** transparente aux parties prenantes
5. **Post-mortem** et amélioration du processus

### **Q: "Comment tester la sécurité d'une application ?"**
**Approche multicouche :**
- **SAST** : Analyse statique du code
- **DAST** : Tests dynamiques (ZAP, Burp)
- **IAST** : Tests interactifs
- **Pentest** : Tests manuels d'experts
- **Bug bounty** : Crowdsourcing

### **Q: "Quelle est la différence entre chiffrement et hachage ?"**
- **Chiffrement** : Réversible avec la clé (confidentialité)
- **Hachage** : Irréversible (intégrité, authentification)

---

## **Introduction**

La sécurité des applications web est cruciale car elles sont exposées à Internet et constituent souvent la porte d'entrée vers les systèmes d'information d'une organisation.

### Principes fondamentaux
- **Defense in Depth** : Plusieurs couches de sécurité
- **Fail Secure** : En cas d'erreur, échouer de manière sécurisée
- **Least Privilege** : Accorder le minimum de privilèges nécessaires
- **Separation of Duties** : Séparer les responsabilités critiques

---

## **Top 10 OWASP**

### 1. A01: Broken Access Control
**Problème** : Contrôles d'accès défaillants permettant aux utilisateurs d'agir en dehors de leurs permissions.

#### Exemples d'attaques
```csharp
// ❌ Vulnérable : Pas de vérification d'autorisation
[HttpGet("users/{id}")]
public async Task<User> GetUser(int id)
{
    return await _userService.GetByIdAsync(id);
}

// ✅ Sécurisé : Vérification d'autorisation
[HttpGet("users/{id}")]
[Authorize]
public async Task<User> GetUser(int id)
{
    var currentUserId = User.GetUserId();
    if (id != currentUserId && !User.IsInRole("Admin"))
    {
        throw new UnauthorizedAccessException();
    }
    return await _userService.GetByIdAsync(id);
}
```

#### Prévention
- Implémenter des contrôles d'accès centralisés
- Dénier par défaut
- Valider les permissions côté serveur
- Limiter l'accès aux ressources par leur propriétaire

### 2. A02: Cryptographic Failures
**Problème** : Échecs liés à la cryptographie (voir cours précédent).

### 3. A03: Injection
**Problème** : Injection de code malveillant (SQL, NoSQL, OS, LDAP).

#### SQL Injection
```csharp
// ❌ Vulnérable
public User GetUser(string username)
{
    var query = $"SELECT * FROM Users WHERE Username = '{username}'";
    return _db.Query<User>(query).FirstOrDefault();
}

// ✅ Sécurisé - Paramètres
public User GetUser(string username)
{
    var query = "SELECT * FROM Users WHERE Username = @username";
    return _db.Query<User>(query, new { username }).FirstOrDefault();
}

// ✅ Sécurisé - ORM
public User GetUser(string username)
{
    return _context.Users.FirstOrDefault(u => u.Username == username);
}
```

#### NoSQL Injection
```javascript
// ❌ Vulnérable
app.post('/login', (req, res) => {
    const { username, password } = req.body;
    
    db.collection('users').findOne({
        username: username,
        password: password
    });
});

// ✅ Sécurisé
app.post('/login', (req, res) => {
    const { username, password } = req.body;
    
    // Validation et sanitisation
    if (typeof username !== 'string' || typeof password !== 'string') {
        return res.status(400).json({ error: 'Invalid input' });
    }
    
    db.collection('users').findOne({
        username: { $eq: username },
        password: { $eq: password }
    });
});
```

### 4. A04: Insecure Design
**Problème** : Défauts de conception de sécurité.

#### Exemples et solutions
```csharp
// ❌ Design non sécurisé : Reset password par email seulement
public async Task ResetPassword(string email)
{
    var user = await _userService.GetByEmailAsync(email);
    var token = GenerateToken();
    await _emailService.SendResetLinkAsync(email, token);
}

// ✅ Design sécurisé : Multi-facteur
public async Task ResetPassword(ResetPasswordRequest request)
{
    var user = await _userService.GetByEmailAsync(request.Email);
    
    // Vérifications multiples
    await ValidateSecurityQuestionsAsync(user.Id, request.SecurityAnswers);
    await ValidateSMSCodeAsync(user.PhoneNumber, request.SMSCode);
    
    var token = GenerateSecureToken();
    await _emailService.SendResetLinkAsync(request.Email, token);
}
```

### 5. A05: Security Misconfiguration
**Problème** : Configurations de sécurité incorrectes.

#### Exemples courants
```csharp
// ❌ Erreurs détaillées en production
public void ConfigureServices(IServiceCollection services)
{
    services.AddDeveloperExceptionPage(); // Dangereux en production
}

// ✅ Configuration appropriée
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }
}
```

### 6. A06: Vulnerable and Outdated Components
**Problème** : Utilisation de composants vulnérables.

#### Prévention
```xml
<!-- Monitoring des vulnérabilités -->
<PackageReference Include="Microsoft.AspNetCore.App" Version="6.0.10" />
<!-- Utiliser la dernière version stable -->

<!-- Audit des dépendances -->
<PropertyGroup>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <NuGetAudit>true</NuGetAudit>
</PropertyGroup>
```

### 7. A07: Identification and Authentication Failures
**Problème** : Défaillances d'authentification.

#### Bonnes pratiques
```csharp
public class SecureAuthenticationService
{
    // Politique de mot de passe forte
    public bool ValidatePassword(string password)
    {
        return password.Length >= 12 &&
               password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit) &&
               password.Any(c => "!@#$%^&*".Contains(c));
    }
    
    // Protection contre la force brute
    public async Task<bool> AttemptLoginAsync(string username, string password)
    {
        var attempts = await _rateLimiter.GetAttemptsAsync(username);
        if (attempts > 5)
        {
            await Task.Delay(TimeSpan.FromMinutes(Math.Pow(2, attempts - 5)));
        }
        
        var isValid = await ValidateCredentialsAsync(username, password);
        if (!isValid)
        {
            await _rateLimiter.IncrementAttemptsAsync(username);
        }
        
        return isValid;
    }
}
```

### 8. A08: Software and Data Integrity Failures
**Problème** : Manque de vérification d'intégrité.

#### Solutions
```csharp
// Vérification d'intégrité des mises à jour
public class IntegrityChecker
{
    public async Task<bool> VerifyUpdateIntegrityAsync(byte[] updateData, string expectedHash)
    {
        using (var sha256 = SHA256.Create())
        {
            var computedHash = sha256.ComputeHash(updateData);
            var computedHashString = Convert.ToHexString(computedHash);
            return computedHashString.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
```

### 9. A09: Security Logging and Monitoring Failures
**Problème** : Insuffisance des logs et du monitoring.

#### Implémentation
```csharp
public class SecurityAuditLogger
{
    private readonly ILogger<SecurityAuditLogger> _logger;
    
    public void LogSuccessfulLogin(string userId, string ipAddress)
    {
        _logger.LogInformation("Successful login: User {UserId} from {IPAddress}", 
            userId, ipAddress);
    }
    
    public void LogFailedLogin(string username, string ipAddress, string reason)
    {
        _logger.LogWarning("Failed login attempt: Username {Username} from {IPAddress}. Reason: {Reason}", 
            username, ipAddress, reason);
    }
    
    public void LogSuspiciousActivity(string activity, string userId, object details)
    {
        _logger.LogError("SECURITY ALERT: {Activity} by User {UserId}. Details: {@Details}", 
            activity, userId, details);
    }
}
```

### 10. A10: Server-Side Request Forgery (SSRF)
**Problème** : Le serveur effectue des requêtes vers des ressources non vérifiées.

#### Prévention
```csharp
public class SecureHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string[] _allowedHosts = { "api.example.com", "secure-service.com" };
    
    public async Task<string> FetchDataAsync(string url)
    {
        var uri = new Uri(url);
        
        // Validation de l'hôte
        if (!_allowedHosts.Contains(uri.Host))
        {
            throw new SecurityException($"Host {uri.Host} is not allowed");
        }
        
        // Bloquer les IPs privées
        var ipAddress = await Dns.GetHostAddressesAsync(uri.Host);
        if (IsPrivateIPAddress(ipAddress.First()))
        {
            throw new SecurityException("Private IP addresses are not allowed");
        }
        
        return await _httpClient.GetStringAsync(url);
    }
}
```

---

## **Authentification et autorisation**

### Types d'authentification

#### 1. Basic Authentication
```csharp
// Simple mais non sécurisé sans HTTPS
public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (authHeader?.StartsWith("Basic ") == true)
        {
            var encodedCredentials = authHeader.Substring("Basic ".Length);
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var parts = credentials.Split(':', 2);
            
            if (await ValidateCredentialsAsync(parts[0], parts[1]))
            {
                var claims = new[] { new Claim(ClaimTypes.Name, parts[0]) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
            }
        }
        
        return AuthenticateResult.Fail("Invalid credentials");
    }
}
```

#### 2. JWT (JSON Web Tokens)
```csharp
public class JWTService
{
    private readonly string _secretKey;
    
    public string GenerateToken(string userId, string[] roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, string.Join(",", roles))
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
        
        var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
        return principal;
    }
}
```

#### 3. OAuth 2.0 et OpenID Connect
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-identity-server.com";
        options.Audience = "your-api";
        options.RequireHttpsMetadata = true;
    });
}
```

### Autorisation

#### 1. Role-based Access Control (RBAC)
```csharp
[Authorize(Roles = "Admin,Manager")]
public class AdminController : ControllerBase
{
    [HttpDelete("users/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}
```

#### 2. Policy-based Authorization
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthorization(options =>
    {
        options.AddPolicy("MinimumAge", policy =>
            policy.Requirements.Add(new MinimumAgeRequirement(18)));
            
        options.AddPolicy("CanDeleteUser", policy =>
            policy.Requirements.Add(new CanDeleteUserRequirement()));
    });
}

public class CanDeleteUserHandler : AuthorizationHandler<CanDeleteUserRequirement, User>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CanDeleteUserRequirement requirement,
        User user)
    {
        var currentUserId = context.User.GetUserId();
        
        if (user.Id == currentUserId || context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
```

### Multi-Factor Authentication (MFA)
```csharp
public class MFAService
{
    public async Task<string> GenerateTOTPAsync(string userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        var secretKey = user.MFASecretKey ?? GenerateSecretKey();
        
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        return GenerateHOTP(secretKey, timestamp);
    }
    
    public async Task<bool> ValidateTOTPAsync(string userId, string code)
    {
        var user = await _userService.GetByIdAsync(userId);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        
        // Vérifier le code actuel et les 2 précédents (pour compenser le décalage)
        for (int i = 0; i <= 2; i++)
        {
            var expectedCode = GenerateHOTP(user.MFASecretKey, timestamp - i);
            if (code == expectedCode)
            {
                return true;
            }
        }
        
        return false;
    }
}
```

---

## **Sécurisation des APIs**

### Rate Limiting
```csharp
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var key = $"rate_limit_{clientId}";
        
        var requestCount = _cache.Get<int>(key);
        if (requestCount >= 100) // 100 requêtes par heure
        {
            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }
        
        _cache.Set(key, requestCount + 1, TimeSpan.FromHours(1));
        await _next(context);
    }
}
```

### API Versioning Security
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class UsersController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    [Obsolete("This version is deprecated. Use v2.0")]
    public async Task<IActionResult> GetUsersV1()
    {
        // Version obsolète avec restrictions
        if (!User.IsInRole("Admin"))
        {
            return Forbid("This API version requires admin privileges");
        }
        
        return Ok(await _userService.GetUsersV1Async());
    }
    
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetUsersV2()
    {
        return Ok(await _userService.GetUsersV2Async());
    }
}
```

### Input Validation
```csharp
public class CreateUserRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only letters and spaces allowed")]
    public string FirstName { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]",
        ErrorMessage = "Password must contain uppercase, lowercase, digit and special character")]
    public string Password { get; set; }
}

public class InputValidationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var param in context.ActionArguments.Values)
        {
            if (param is string str)
            {
                // Détection XSS basique
                if (ContainsPotentialXSS(str))
                {
                    context.Result = new BadRequestObjectResult("Potential XSS detected");
                    return;
                }
            }
        }
        
        base.OnActionExecuting(context);
    }
    
    private bool ContainsPotentialXSS(string input)
    {
        var dangerousPatterns = new[]
        {
            "<script", "javascript:", "onload=", "onerror=", 
            "onclick=", "onmouseover=", "<iframe", "<object"
        };
        
        return dangerousPatterns.Any(pattern => 
            input.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
```

---

## **Protection contre les attaques courantes**

### Cross-Site Scripting (XSS)
```csharp
// Protection côté serveur
public class XSSProtectionService
{
    public string SanitizeHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        // Utiliser une bibliothèque comme HtmlSanitizer
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.Add("p");
        sanitizer.AllowedTags.Add("b");
        sanitizer.AllowedTags.Add("i");
        
        return sanitizer.Sanitize(input);
    }
    
    public string EncodeForHTML(string input)
    {
        return HttpUtility.HtmlEncode(input);
    }
}

// Configuration des headers de sécurité
public void Configure(IApplicationBuilder app)
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Content-Security-Policy", 
            "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");
        
        await next();
    });
}
```

### Cross-Site Request Forgery (CSRF)
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-CSRF-TOKEN";
        options.SuppressXFrameOptionsHeader = false;
    });
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    // Action protégée contre CSRF
    var user = await _userService.CreateAsync(request);
    return Ok(user);
}
```

### Clickjacking
```csharp
// Middleware de protection contre le clickjacking
public class ClickjackingProtectionMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
        // Ou plus strict : "DENY"
        // Ou avec CSP : frame-ancestors 'self'
        
        await _next(context);
    }
}
```

---

## **Sécurité côté client**

### Content Security Policy (CSP)
```html
<!-- Meta tag CSP -->
<meta http-equiv="Content-Security-Policy" 
      content="default-src 'self'; 
               script-src 'self' 'unsafe-inline' https://trusted-cdn.com; 
               style-src 'self' 'unsafe-inline'; 
               img-src 'self' data: https:;">
```

### Subresource Integrity (SRI)
```html
<!-- Vérification d'intégrité des ressources externes -->
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.6.0/jquery.min.js"
        integrity="sha384-vtXRMe3mGCbOeY7l30aIg8H9p3GdeSe4IFlP6G8JMa7o7lXvnz3GFKzPuSJqe7Zb"
        crossorigin="anonymous"></script>
```

### Secure Cookie Configuration
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.Secure = true; // HTTPS uniquement
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });
}
```

---

## **Configuration et déploiement sécurisés**

### HTTPS et TLS
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Redirection HTTPS obligatoire
    services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
        options.HttpsPort = 443;
    });
    
    // HSTS (HTTP Strict Transport Security)
    services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
    });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (!env.IsDevelopment())
    {
        app.UseHsts();
    }
    
    app.UseHttpsRedirection();
}
```

### Secrets Management
```csharp
// Configuration des secrets
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Azure Key Vault
        services.AddSingleton<ISecretManager>(provider =>
        {
            var keyVaultUrl = Configuration["KeyVault:VaultUrl"];
            return new AzureKeyVaultSecretManager(keyVaultUrl);
        });
    }
}

public class AzureKeyVaultSecretManager : ISecretManager
{
    private readonly SecretClient _secretClient;
    
    public AzureKeyVaultSecretManager(string vaultUrl)
    {
        _secretClient = new SecretClient(new Uri(vaultUrl), new DefaultAzureCredential());
    }
    
    public async Task<string> GetSecretAsync(string secretName)
    {
        var secret = await _secretClient.GetSecretAsync(secretName);
        return secret.Value.Value;
    }
}
```

### Environment Security
```csharp
// Configuration par environnement
public class SecurityConfiguration
{
    public static void ConfigureForEnvironment(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            // Environnement de développement
            app.UseDeveloperExceptionPage();
        }
        else if (env.IsStaging())
        {
            // Environnement de test
            app.UseExceptionHandler("/Error");
            // Logs détaillés pour le debug
        }
        else // Production
        {
            // Environnement de production
            app.UseExceptionHandler("/Error");
            app.UseHsts();
            
            // Désactiver les informations de serveur
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Remove("Server");
                await next();
            });
        }
    }
}
```

---

## **Monitoring et détection**

### Logging sécurisé
```csharp
public class SecurityLogger
{
    private readonly ILogger<SecurityLogger> _logger;
    
    public void LogSecurityEvent(SecurityEventType eventType, string userId, 
        string details, string ipAddress)
    {
        var logEntry = new
        {
            EventType = eventType.ToString(),
            UserId = userId,
            Details = details,
            IPAddress = ipAddress,
            Timestamp = DateTime.UtcNow,
            Severity = GetSeverity(eventType)
        };
        
        switch (GetSeverity(eventType))
        {
            case LogLevel.Warning:
                _logger.LogWarning("Security Event: {@SecurityEvent}", logEntry);
                break;
            case LogLevel.Error:
                _logger.LogError("Security Alert: {@SecurityEvent}", logEntry);
                break;
            case LogLevel.Critical:
                _logger.LogCritical("Critical Security Alert: {@SecurityEvent}", logEntry);
                // Déclencher une alerte immédiate
                TriggerSecurityAlert(logEntry);
                break;
        }
    }
}

public enum SecurityEventType
{
    SuccessfulLogin,
    FailedLogin,
    PasswordReset,
    PrivilegeEscalation,
    SuspiciousActivity,
    DataAccess,
    ConfigurationChange
}
```

---

## **Tests de sécurité**

### Tests unitaires de sécurité
```csharp
[Test]
public async Task Login_WithInvalidCredentials_ShouldNotRevealUserExistence()
{
    // Arrange
    var invalidUser = "nonexistent@test.com";
    var invalidPassword = "wrongpassword";
    
    // Act
    var result = await _authService.LoginAsync(invalidUser, invalidPassword);
    
    // Assert
    Assert.IsFalse(result.Success);
    Assert.AreEqual("Invalid credentials", result.Message);
    // Ne doit pas révéler si l'utilisateur existe ou non
}

[Test]
public void PasswordValidator_WithWeakPassword_ShouldReject()
{
    // Arrange
    var weakPasswords = new[] { "123456", "password", "qwerty" };
    
    // Act & Assert
    foreach (var password in weakPasswords)
    {
        var result = _passwordValidator.Validate(password);
        Assert.IsFalse(result.IsValid);
    }
}
```

Cette formation sur la sécurité des applications web couvre les aspects essentiels pour développer et déployer des applications sécurisées. Elle complète parfaitement le cours sur le chiffrement.
