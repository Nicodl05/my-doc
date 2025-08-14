# **🎯 QUIZ INTERACTIF C# - TOUS NIVEAUX**

## 🟢 **NIVEAU DÉBUTANT** (1-10)

### **Question 1:** Syntaxe de base
```csharp
int x = 5;
int y = ++x;
Console.WriteLine($"x = {x}, y = {y}");
```
**Que va afficher ce code ?**
- A) x = 5, y = 5
- B) x = 6, y = 5  
- C) x = 6, y = 6
- D) x = 5, y = 6

<details>
<summary>🔍 Réponse</summary>
**C) x = 6, y = 6**
`++x` incrémente x AVANT de l'assigner à y.
</details>

---

### **Question 2:** Types de référence vs valeur
```csharp
int a = 10;
int b = a;
b = 20;
Console.WriteLine(a);
```
**Que va afficher ce code ?**
- A) 10
- B) 20
- C) Erreur de compilation
- D) Exception à l'exécution

<details>
<summary>🔍 Réponse</summary>
**A) 10**
`int` est un type valeur. Modifier `b` n'affecte pas `a`.
</details>

---

### **Question 3:** Méthodes et paramètres
**Quelle est la différence entre `ref` et `out` ?**

<details>
<summary>🔍 Réponse</summary>
- **`ref`** : La variable doit être initialisée avant l'appel
- **`out`** : La variable n'a pas besoin d'être initialisée, mais doit être assignée dans la méthode
```csharp
// ref
int x = 5;
ModifyRef(ref x);

// out  
int y; // Pas besoin d'initialiser
GetValue(out y);
```
</details>

---

### **Question 4:** Boucles
**Écrivez une boucle qui affiche les nombres pairs de 0 à 10.**

<details>
<summary>🔍 Réponse</summary>
```csharp
for (int i = 0; i <= 10; i += 2)
{
    Console.WriteLine(i);
}
// OU
for (int i = 0; i <= 10; i++)
{
    if (i % 2 == 0)
        Console.WriteLine(i);
}
```
</details>

---

### **Question 5:** Collections
**Quelle collection choisir pour stocker des éléments uniques ?**
- A) List<T>
- B) Dictionary<T,U>
- C) HashSet<T>
- D) Queue<T>

<details>
<summary>🔍 Réponse</summary>
**C) HashSet<T>**
HashSet garantit l'unicité des éléments automatiquement.
</details>

---

## 🟡 **NIVEAU INTERMÉDIAIRE** (6-15)

### **Question 6:** LINQ
```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
var result = numbers.Where(x => x > 2).Select(x => x * 2);
Console.WriteLine(result.Count());
```
**Combien d'éléments contient `result` ?**

<details>
<summary>🔍 Réponse</summary>
**3 éléments**
- Filtre : 3, 4, 5 (> 2)
- Transform : 6, 8, 10 (* 2)
- Count : 3
</details>

---

### **Question 7:** Exceptions
**Corrigez ce code :**
```csharp
try
{
    int result = 10 / 0;
}
catch (Exception ex)
{
    throw ex; // ❌ PROBLÈME ICI
}
```

<details>
<summary>🔍 Réponse</summary>
```csharp
try
{
    int result = 10 / 0;
}
catch (DivideByZeroException ex) // Spécifique
{
    LogError(ex);
    throw; // ✅ Préserve la stack trace
}
```
**Problème :** `throw ex` réinitialise la stack trace.
**Solution :** Utiliser `throw` seul.
</details>

---

### **Question 8:** Async/Await
**Quelle est la différence entre ces deux codes ?**
```csharp
// Code A
await Task1();
await Task2();

// Code B  
await Task.WhenAll(Task1(), Task2());
```

<details>
<summary>🔍 Réponse</summary>
- **Code A :** Exécution **séquentielle** (Task2 attend que Task1 soit finie)
- **Code B :** Exécution **parallèle** (les deux tâches s'exécutent en même temps)

**Performance :** Code B est plus rapide si les tâches sont indépendantes.
</details>

---

### **Question 9:** Interfaces vs Classes abstraites
**Quand utiliser une interface plutôt qu'une classe abstraite ?**

<details>
<summary>🔍 Réponse</summary>
**Utilisez une Interface quand :**
- Contrat pur (pas d'implémentation partagée)
- Héritage multiple nécessaire
- Découplage maximum souhaité

**Utilisez une Classe Abstraite quand :**
- Code commun à partager
- Contrôle plus strict de l'héritage
- Constructeurs nécessaires

```csharp
// Interface - Contrat pur
interface IFlyable
{
    void Fly();
}

// Classe abstraite - Code partagé
abstract class Vehicle
{
    public string Brand { get; set; }
    public abstract void Start(); // Doit être implémentée
    public virtual void Stop() => Console.WriteLine("Stopping"); // Peut être redéfinie
}
```
</details>

---

### **Question 10:** Délégués et Événements
**Expliquez la différence et donnez un exemple.**

<details>
<summary>🔍 Réponse</summary>
**Délégué :** Pointeur de fonction, peut être appelé directement.
**Événement :** Délégué encapsulé, ne peut être déclenché que par la classe qui le déclare.

```csharp
public class EventPublisher
{
    // Délégué - DANGEREUX depuis l'extérieur
    public Action<string> OnMessageDelegate;
    
    // Événement - SÉCURISÉ
    public event Action<string> OnMessageEvent;
    
    public void SendMessage(string msg)
    {
        OnMessageDelegate?.Invoke(msg); // OK
        OnMessageEvent?.Invoke(msg);    // OK
    }
}

// Utilisation
var pub = new EventPublisher();

// Délégué - Peut être manipulé dangereusement
pub.OnMessageDelegate = Console.WriteLine;
pub.OnMessageDelegate("Test"); // ❌ Appel direct possible
pub.OnMessageDelegate = null;  // ❌ Peut supprimer tous les abonnés

// Événement - Utilisation sécurisée
pub.OnMessageEvent += Console.WriteLine; // ✅ Seulement += et -=
// pub.OnMessageEvent("Test"); // ❌ Erreur de compilation
```
</details>

---

## 🔴 **NIVEAU AVANCÉ** (16-25)

### **Question 11:** Génériques avancés
**Que fait cette contrainte générique ?**
```csharp
public class Repository<T> where T : class, IEntity, new()
{
    // Implémentation
}
```

<details>
<summary>🔍 Réponse</summary>
**Contraintes :**
- `class` : T doit être un type référence
- `IEntity` : T doit implémenter l'interface IEntity  
- `new()` : T doit avoir un constructeur sans paramètres

**Utilisation :**
```csharp
public T CreateNew()
{
    var entity = new T(); // Possible grâce à new()
    // entity.Id disponible grâce à IEntity
    return entity;
}
```
</details>

---

### **Question 12:** Expressions et Réflexion
**À quoi sert ce code ?**
```csharp
Expression<Func<Person, string>> expr = p => p.Name;
string propertyName = ((MemberExpression)expr.Body).Member.Name;
```

<details>
<summary>🔍 Réponse</summary>
**Extraction du nom de propriété à partir d'une expression lambda.**

**Utilité :** 
- ORM (Entity Framework)
- Validation
- Mapping automatique
- INotifyPropertyChanged

**Exemple d'utilisation :**
```csharp
public void RaisePropertyChanged<T>(Expression<Func<T>> expr)
{
    var name = ((MemberExpression)expr.Body).Member.Name;
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// Usage
RaisePropertyChanged(() => Name); // Au lieu de RaisePropertyChanged("Name")
```
</details>

---

### **Question 13:** Design Patterns
**Implémentez le pattern Strategy pour un système de calcul de réduction.**

<details>
<summary>🔍 Réponse</summary>
```csharp
// Interface Strategy
public interface IDiscountStrategy
{
    decimal ApplyDiscount(decimal originalPrice);
    string Description { get; }
}

// Stratégies concrètes
public class NoDiscount : IDiscountStrategy
{
    public string Description => "Aucune réduction";
    public decimal ApplyDiscount(decimal price) => price;
}

public class PercentageDiscount : IDiscountStrategy
{
    private readonly decimal _percentage;
    
    public PercentageDiscount(decimal percentage)
    {
        _percentage = percentage;
    }
    
    public string Description => $"Réduction de {_percentage}%";
    
    public decimal ApplyDiscount(decimal price)
    {
        return price * (1 - _percentage / 100);
    }
}

public class FixedAmountDiscount : IDiscountStrategy
{
    private readonly decimal _amount;
    
    public FixedAmountDiscount(decimal amount)
    {
        _amount = amount;
    }
    
    public string Description => $"Réduction de {_amount:C}";
    
    public decimal ApplyDiscount(decimal price)
    {
        return Math.Max(0, price - _amount);
    }
}

// Context
public class ShoppingCart
{
    private IDiscountStrategy _discountStrategy = new NoDiscount();
    private readonly List<decimal> _items = new();
    
    public void SetDiscountStrategy(IDiscountStrategy strategy)
    {
        _discountStrategy = strategy;
    }
    
    public void AddItem(decimal price) => _items.Add(price);
    
    public decimal GetTotal()
    {
        var subtotal = _items.Sum();
        var total = _discountStrategy.ApplyDiscount(subtotal);
        return total;
    }
    
    public string GetDiscountInfo() => _discountStrategy.Description;
}

// Utilisation
var cart = new ShoppingCart();
cart.AddItem(100);
cart.AddItem(50);

cart.SetDiscountStrategy(new PercentageDiscount(20));
Console.WriteLine($"Total avec 20% : {cart.GetTotal():C}"); // 120€

cart.SetDiscountStrategy(new FixedAmountDiscount(25));
Console.WriteLine($"Total avec 25€ de réduction : {cart.GetTotal():C}"); // 125€
```
</details>

---

### **Question 14:** Performance et Mémoire
**Optimisez ce code qui crée beaucoup d'allocations :**
```csharp
public string ProcessItems(IEnumerable<string> items)
{
    string result = "";
    foreach (var item in items)
    {
        result += item.ToUpper() + ", ";
    }
    return result.TrimEnd(',', ' ');
}
```

<details>
<summary>🔍 Réponse</summary>
**Problèmes :**
- Concaténation de string = nouvelles allocations à chaque itération
- ToUpper() sur chaque item

**Solution optimisée :**
```csharp
public string ProcessItems(IEnumerable<string> items)
{
    return string.Join(", ", items.Select(item => item.ToUpper()));
}

// OU avec StringBuilder pour plus de contrôle
public string ProcessItemsWithStringBuilder(IEnumerable<string> items)
{
    var sb = new StringBuilder();
    foreach (var item in items)
    {
        if (sb.Length > 0) sb.Append(", ");
        sb.Append(item.ToUpper());
    }
    return sb.ToString();
}

// OU avec Span<T> pour performance maximale (C# 7.2+)
public string ProcessItemsWithSpan(ReadOnlySpan<string> items)
{
    if (items.IsEmpty) return string.Empty;
    
    var totalLength = items.Length * 2; // Estimation
    foreach (var item in items)
        totalLength += item.Length;
    
    return string.Create(totalLength, items, (span, items) =>
    {
        int pos = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (i > 0)
            {
                span[pos++] = ',';
                span[pos++] = ' ';
            }
            
            var upperItem = items[i].ToUpperInvariant();
            upperItem.AsSpan().CopyTo(span.Slice(pos));
            pos += upperItem.Length;
        }
    });
}
```
</details>

---

### **Question 15:** Patterns Avancés
**Implémentez le pattern Repository avec Unit of Work.**

<details>
<summary>🔍 Réponse</summary>
```csharp
// Entité de base
public interface IEntity
{
    int Id { get; set; }
}

// Repository générique
public interface IRepository<T> where T : class, IEntity
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
}

// Unit of Work
public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Order> Orders { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// Implémentation avec Entity Framework
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private IDbContextTransaction _transaction;
    
    private IRepository<User> _users;
    private IRepository<Order> _orders;
    
    public UnitOfWork(DbContext context)
    {
        _context = context;
    }
    
    public IRepository<User> Users => 
        _users ??= new Repository<User>(_context);
    
    public IRepository<Order> Orders => 
        _orders ??= new Repository<Order>(_context);
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context?.Dispose();
    }
}

// Repository concret
public class Repository<T> : IRepository<T> where T : class, IEntity
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;
    
    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }
    
    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }
    
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }
    
    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }
}

// Service utilisant l'Unit of Work
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Order> CreateOrderAsync(int userId, List<OrderItem> items)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            // Vérifier que l'utilisateur existe
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Utilisateur non trouvé");
            
            // Créer la commande
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Items = items
            };
            
            _unitOfWork.Orders.Add(order);
            await _unitOfWork.SaveChangesAsync();
            
            await _unitOfWork.CommitTransactionAsync();
            return order;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```
</details>

---

## 🏆 **DÉFI ULTIME** - Question Bonus

### **Question Bonus:** Architecture Clean
**Concevez une architecture Clean pour un système de gestion de bibliothèque avec les fonctionnalités : emprunter/retourner des livres, gérer les utilisateurs, calculer les amendes.**

<details>
<summary>🔍 Solution Complète</summary>

**1. Domain Layer (Entités et Règles Métier)**
```csharp
// Entités
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }
    public bool IsAvailable { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime MembershipDate { get; set; }
}

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int UserId { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    
    public decimal CalculateFine()
    {
        if (ReturnDate.HasValue || DateTime.Now <= DueDate)
            return 0;
            
        var daysLate = (DateTime.Now - DueDate).Days;
        return daysLate * 0.50m; // 0.50€ par jour de retard
    }
}
```

**2. Application Layer (Use Cases)**
```csharp
// DTOs
public class LoanBookRequest
{
    public int BookId { get; set; }
    public int UserId { get; set; }
}

public class LoanBookResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public DateTime? DueDate { get; set; }
}

// Interfaces (Ports)
public interface IBookRepository
{
    Task<Book> GetByIdAsync(int id);
    Task<bool> IsAvailableAsync(int bookId);
    Task UpdateAsync(Book book);
}

public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
}

public interface ILoanRepository
{
    Task<IEnumerable<Loan>> GetActiveLoansByUserAsync(int userId);
    Task AddAsync(Loan loan);
    Task UpdateAsync(Loan loan);
}

// Use Cases
public class LoanBookUseCase
{
    private readonly IBookRepository _bookRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILoanRepository _loanRepository;
    
    public LoanBookUseCase(
        IBookRepository bookRepository,
        IUserRepository userRepository,
        ILoanRepository loanRepository)
    {
        _bookRepository = bookRepository;
        _userRepository = userRepository;
        _loanRepository = loanRepository;
    }
    
    public async Task<LoanBookResponse> ExecuteAsync(LoanBookRequest request)
    {
        // Validation
        var book = await _bookRepository.GetByIdAsync(request.BookId);
        if (book == null)
            return new LoanBookResponse { Success = false, Message = "Livre non trouvé" };
            
        if (!book.IsAvailable)
            return new LoanBookResponse { Success = false, Message = "Livre non disponible" };
            
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return new LoanBookResponse { Success = false, Message = "Utilisateur non trouvé" };
        
        // Vérifier les limites d'emprunt
        var activeLoans = await _loanRepository.GetActiveLoansByUserAsync(request.UserId);
        if (activeLoans.Count() >= 3)
            return new LoanBookResponse { Success = false, Message = "Limite d'emprunts atteinte" };
        
        // Créer l'emprunt
        var loan = new Loan
        {
            BookId = request.BookId,
            UserId = request.UserId,
            LoanDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(14)
        };
        
        // Mettre à jour les données
        book.IsAvailable = false;
        await _bookRepository.UpdateAsync(book);
        await _loanRepository.AddAsync(loan);
        
        return new LoanBookResponse 
        { 
            Success = true, 
            Message = "Emprunt effectué avec succès",
            DueDate = loan.DueDate
        };
    }
}
```

**3. Infrastructure Layer (Adapteurs)**
```csharp
// Repository avec Entity Framework
public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;
    
    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }
    
    public async Task<Book> GetByIdAsync(int id)
    {
        return await _context.Books.FindAsync(id);
    }
    
    public async Task<bool> IsAvailableAsync(int bookId)
    {
        var book = await GetByIdAsync(bookId);
        return book?.IsAvailable ?? false;
    }
    
    public async Task UpdateAsync(Book book)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync();
    }
}
```

**4. API Layer (Contrôleurs)**
```csharp
[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly LoanBookUseCase _loanBookUseCase;
    
    public LoansController(LoanBookUseCase loanBookUseCase)
    {
        _loanBookUseCase = loanBookUseCase;
    }
    
    [HttpPost]
    public async Task<ActionResult<LoanBookResponse>> LoanBook(LoanBookRequest request)
    {
        var response = await _loanBookUseCase.ExecuteAsync(request);
        
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }
}
```

**5. Dependency Injection (Program.cs)**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Infrastructure
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();

// Use Cases
builder.Services.AddScoped<LoanBookUseCase>();

var app = builder.Build();
```

**Avantages de cette architecture :**
- ✅ Séparation claire des responsabilités
- ✅ Testabilité maximale (mocking facile)
- ✅ Indépendance de la base de données
- ✅ Règles métier centralisées
- ✅ Facilité de maintenance et d'évolution
</details>

---

## 📊 **SYSTÈME DE SCORING**

- **Questions 1-5 (Débutant) :** 1 point chacune
- **Questions 6-10 (Intermédiaire) :** 2 points chacune  
- **Questions 11-15 (Avancé) :** 3 points chacune
- **Question Bonus :** 5 points

**Total possible : 35 points**

### 🏅 **Niveaux :**
- **🥉 Bronze (0-15)** : Débutant - Continuez à apprendre !
- **🥈 Argent (16-25)** : Intermédiaire - Bon niveau !
- **🥇 Or (26-35)** : Expert - Excellent travail !

---

*Bon apprentissage ! 🚀*
