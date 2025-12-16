 # Gestion Avancée des Exceptions et Patterns

## 1. Hiérarchie des exceptions en C#

**Structure de base :**
```
System.Object
    └── System.Exception
        ├── System.SystemException
        │   ├── ArgumentException
        │   ├── InvalidOperationException
        │   ├── NotSupportedException
        │   └── ...
        └── System.ApplicationException (obsolète)
            └── Exceptions personnalisées (déconseillé)
```

**Exceptions courantes :**
```csharp
// Exceptions d'arguments
ArgumentNullException     // Paramètre null non autorisé
ArgumentOutOfRangeException // Valeur hors limites
ArgumentException         // Argument invalide

// Exceptions d'état
InvalidOperationException // Opération invalide dans l'état actuel
NotSupportedException     // Opération non supportée
ObjectDisposedException   // Objet déjà disposé

// Exceptions système
OutOfMemoryException      // Mémoire insuffisante
StackOverflowException    // Débordement de pile
```

## 2. Création d'exceptions personnalisées

**Bonnes pratiques :**
```csharp
// Exception personnalisée bien conçue
[Serializable]
public class BusinessLogicException : Exception
{
    public string ErrorCode { get; }
    public DateTime Timestamp { get; }
    
    public BusinessLogicException() : base() 
    {
        Timestamp = DateTime.UtcNow;
    }
    
    public BusinessLogicException(string message) : base(message) 
    {
        Timestamp = DateTime.UtcNow;
    }
    
    public BusinessLogicException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
        Timestamp = DateTime.UtcNow;
    }
    
    public BusinessLogicException(string message, Exception innerException) 
        : base(message, innerException) 
    {
        Timestamp = DateTime.UtcNow;
    }
    
    // Constructeur pour la sérialisation
    protected BusinessLogicException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
        ErrorCode = info.GetString(nameof(ErrorCode));
        Timestamp = info.GetDateTime(nameof(Timestamp));
    }
    
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ErrorCode), ErrorCode);
        info.AddValue(nameof(Timestamp), Timestamp);
    }
}
```

## 3. Patterns de gestion d'exceptions

**Pattern Try-Parse :**
```csharp
public static bool TryParseAge(string input, out int age)
{
    age = 0;
    
    if (string.IsNullOrWhiteSpace(input))
        return false;
        
    if (!int.TryParse(input, out age))
        return false;
        
    if (age < 0 || age > 150)
        return false;
        
    return true;
}

// Utilisation
if (TryParseAge(userInput, out int age))
{
    Console.WriteLine($"Âge valide: {age}");
}
else
{
    Console.WriteLine("Âge invalide");
}
```

**Pattern Result<T> :**
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    
    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static Result<T> Success(T value) => new Result<T>(true, value, null);
    public static Result<T> Failure(string error) => new Result<T>(false, default(T), error);
}

public Result<User> CreateUser(string name, string email)
{
    if (string.IsNullOrWhiteSpace(name))
        return Result<User>.Failure("Le nom est requis");
        
    if (!IsValidEmail(email))
        return Result<User>.Failure("Email invalide");
        
    var user = new User { Name = name, Email = email };
    return Result<User>.Success(user);
}
```

## 4. Exception Handling avancé

**Filtres d'exceptions (C# 6+) :**
```csharp
try
{
    // Code risqué
    ProcessData();
}
catch (HttpRequestException ex) when (ex.Message.Contains("timeout"))
{
    // Gérer seulement les timeouts
    await RetryOperation();
}
catch (HttpRequestException ex) when (ex.Message.Contains("404"))
{
    // Gérer les erreurs 404
    HandleNotFound();
}
catch (HttpRequestException ex)
{
    // Autres erreurs HTTP
    LogError(ex);
    throw; // Relancer l'exception
}
```

**Finally vs using :**
```csharp
// Ancien style avec finally
FileStream file = null;
try
{
    file = new FileStream("data.txt", FileMode.Open);
    // Traitement du fichier
}
catch (Exception ex)
{
    LogError(ex);
    throw;
}
finally
{
    file?.Dispose(); // Nettoyage garanti
}

// Style moderne avec using
try
{
    using (var file = new FileStream("data.txt", FileMode.Open))
    {
        // Traitement du fichier
        // Dispose() appelé automatiquement
    }
}
catch (Exception ex)
{
    LogError(ex);
    throw;
}
```

## 5. Gestion globale des exceptions

**Pour les applications Console :**
```csharp
class Program
{
    static void Main(string[] args)
    {
        // Gestionnaire global pour les exceptions non gérées
        AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;
        
        try
        {
            RunApplication();
        }
        catch (Exception ex)
        {
            LogCriticalError(ex);
            Console.WriteLine("Une erreur critique s'est produite.");
        }
    }
    
    static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        LogCriticalError(exception);
        
        if (e.IsTerminating)
        {
            Console.WriteLine("L'application va se fermer...");
        }
    }
}
```

**Pour les applications ASP.NET Core :**
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur non gérée s'est produite");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            message = "Une erreur interne s'est produite",
            details = exception.Message // En développement seulement
        };
        
        context.Response.StatusCode = exception switch
        {
            ArgumentNullException => 400,
            InvalidOperationException => 400,
            UnauthorizedAccessException => 401,
            _ => 500
        };
        
        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}
```

---

# Jeu de Questions-Réponses : Exceptions

## Questions de Base

**Q1:** Quelle est la différence entre `throw` et `throw ex` ?
<details>
<summary>Réponse</summary>
- `throw` : Relance l'exception en préservant la stack trace originale
- `throw ex` : Relance l'exception mais réinitialise la stack trace (perte d'information)
</details>

**Q2:** Dans quel ordre sont exécutés les blocs try-catch-finally ?
<details>
<summary>Réponse</summary>
1. try (jusqu'à l'exception ou la fin)
2. catch (si exception correspondante)
3. finally (toujours exécuté, même en cas de return dans try/catch)
</details>

**Q3:** Que se passe-t-il si une exception est levée dans un bloc finally ?
<details>
<summary>Réponse</summary>
L'exception du bloc finally masque l'exception originale du try/catch. C'est pourquoi il faut éviter de lever des exceptions dans finally.
</details>

## Questions Intermédiaires

**Q4:** Corrigez ce code problématique :
```csharp
public void ProcessFile(string path)
{
    var file = File.Open(path, FileMode.Open);
    try
    {
        // Traitement
    }
    catch (Exception ex)
    {
        file.Close();
        throw ex;
    }
}
```
<details>
<summary>Réponse</summary>

```csharp
public void ProcessFile(string path)
{
    using (var file = File.Open(path, FileMode.Open))
    {
        try
        {
            // Traitement
        }
        catch (Exception ex)
        {
            // Log de l'erreur si nécessaire
            throw; // Pas "throw ex"
        }
        // file.Close() automatique via using
    }
}
```
</details>

**Q5:** Quand utiliser des exceptions vs des codes de retour ?
<details>
<summary>Réponse</summary>

- **Exceptions** : Conditions exceptionnelles, erreurs inattendues
- **Codes de retour** : Conditions normales prévisibles (validation, parsing)
- Pattern Try-Parse pour éviter les exceptions dans les cas fréquents
</details>

## Questions Avancées

**Q6:** Implémentez une méthode qui retry automatiquement en cas d'exception transitoire.
<details>
<summary>Réponse</summary>

```csharp
public async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
{
    for (int i = 0; i <= maxRetries; i++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (i < maxRetries && IsTransientException(ex))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(Math.Pow(2, i) * 1000));
        }
    }
    
    // Dernière tentative sans catch
    return await operation();
}

private bool IsTransientException(Exception ex)
{
    return ex is HttpRequestException || 
           ex is TimeoutException || 
           ex is SocketException;
}
```
</details>

**Q7:** Comment implémenter une exception qui contient plusieurs erreurs de validation ?
<details>
<summary>Réponse</summary>

```csharp
public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }
    
    public ValidationException(IEnumerable<string> errors) 
        : base($"Validation failed: {string.Join(", ", errors)}")
    {
        Errors = errors.ToList().AsReadOnly();
    }
}

// Utilisation
var errors = new List<string>();
if (string.IsNullOrEmpty(name)) errors.Add("Name is required");
if (age < 0) errors.Add("Age must be positive");

if (errors.Any())
    throw new ValidationException(errors);
```
</details>
