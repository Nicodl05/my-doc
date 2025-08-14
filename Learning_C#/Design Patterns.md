# Patterns de Conception (Design Patterns) en C#

## 1. Pattern Singleton

**But :** Garantir qu'une classe n'a qu'une seule instance et fournir un point d'accès global.

**Implémentation thread-safe :**
```csharp
public sealed class Singleton
{
    private static readonly Lazy<Singleton> _instance = 
        new Lazy<Singleton>(() => new Singleton());
    
    private Singleton() { }
    
    public static Singleton Instance => _instance.Value;
    
    public void DoSomething()
    {
        Console.WriteLine("Singleton en action");
    }
}

// Utilisation
var singleton = Singleton.Instance;
singleton.DoSomething();
```

**Version avec initialisation contrôlée :**
```csharp
public sealed class DatabaseConnection
{
    private static DatabaseConnection _instance;
    private static readonly object _lock = new object();
    
    private DatabaseConnection() 
    {
        // Initialisation coûteuse
        ConnectionString = ConfigurationManager.ConnectionStrings["Main"].ConnectionString;
    }
    
    public static DatabaseConnection Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new DatabaseConnection();
                }
            }
            return _instance;
        }
    }
    
    public string ConnectionString { get; private set; }
}
```

## 2. Pattern Factory et Abstract Factory

**Factory Method :**
```csharp
public abstract class LoggerFactory
{
    public abstract ILogger CreateLogger();
    
    public void LogMessage(string message)
    {
        var logger = CreateLogger();
        logger.Log(message);
    }
}

public class FileLoggerFactory : LoggerFactory
{
    public override ILogger CreateLogger()
    {
        return new FileLogger();
    }
}

public class ConsoleLoggerFactory : LoggerFactory
{
    public override ILogger CreateLogger()
    {
        return new ConsoleLogger();
    }
}

public interface ILogger
{
    void Log(string message);
}

public class FileLogger : ILogger
{
    public void Log(string message)
    {
        File.AppendAllText("log.txt", $"{DateTime.Now}: {message}\n");
    }
}

public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"{DateTime.Now}: {message}");
    }
}
```

**Abstract Factory :**
```csharp
public interface IUIFactory
{
    IButton CreateButton();
    ITextBox CreateTextBox();
}

public class WindowsUIFactory : IUIFactory
{
    public IButton CreateButton() => new WindowsButton();
    public ITextBox CreateTextBox() => new WindowsTextBox();
}

public class MacUIFactory : IUIFactory
{
    public IButton CreateButton() => new MacButton();
    public ITextBox CreateTextBox() => new MacTextBox();
}

// Utilisation
IUIFactory factory = Environment.OSVersion.Platform == PlatformID.Win32NT 
    ? new WindowsUIFactory() 
    : new MacUIFactory();

var button = factory.CreateButton();
var textBox = factory.CreateTextBox();
```

## 3. Pattern Observer

**Implémentation classique :**
```csharp
public interface IObserver<T>
{
    void Update(T data);
}

public interface ISubject<T>
{
    void Subscribe(IObserver<T> observer);
    void Unsubscribe(IObserver<T> observer);
    void Notify(T data);
}

public class StockPrice : ISubject<decimal>
{
    private readonly List<IObserver<decimal>> _observers = new List<IObserver<decimal>>();
    private decimal _price;
    
    public decimal Price
    {
        get => _price;
        set
        {
            _price = value;
            Notify(_price);
        }
    }
    
    public void Subscribe(IObserver<decimal> observer)
    {
        _observers.Add(observer);
    }
    
    public void Unsubscribe(IObserver<decimal> observer)
    {
        _observers.Remove(observer);
    }
    
    public void Notify(decimal data)
    {
        foreach (var observer in _observers)
        {
            observer.Update(data);
        }
    }
}

public class StockDisplay : IObserver<decimal>
{
    private readonly string _stockName;
    
    public StockDisplay(string stockName)
    {
        _stockName = stockName;
    }
    
    public void Update(decimal price)
    {
        Console.WriteLine($"{_stockName}: {price:C}");
    }
}
```

**Implémentation avec événements C# :**
```csharp
public class StockPriceModern
{
    private decimal _price;
    
    public event Action<decimal> PriceChanged;
    
    public decimal Price
    {
        get => _price;
        set
        {
            _price = value;
            PriceChanged?.Invoke(_price);
        }
    }
}

// Utilisation
var stock = new StockPriceModern();
stock.PriceChanged += price => Console.WriteLine($"Apple: {price:C}");
stock.PriceChanged += price => { if (price > 100) Console.WriteLine("Price Alert!"); };

stock.Price = 150.50m; // Déclenche les notifications
```

## 4. Pattern Strategy

**Définition :** Définir une famille d'algorithmes, les encapsuler et les rendre interchangeables.

```csharp
public interface IPaymentStrategy
{
    bool ProcessPayment(decimal amount);
    string GetPaymentDetails();
}

public class CreditCardPayment : IPaymentStrategy
{
    private readonly string _cardNumber;
    
    public CreditCardPayment(string cardNumber)
    {
        _cardNumber = cardNumber;
    }
    
    public bool ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing {amount:C} via Credit Card ending in {_cardNumber.Substring(_cardNumber.Length - 4)}");
        // Logique de paiement par carte
        return true;
    }
    
    public string GetPaymentDetails() => $"Credit Card: ****{_cardNumber.Substring(_cardNumber.Length - 4)}";
}

public class PayPalPayment : IPaymentStrategy
{
    private readonly string _email;
    
    public PayPalPayment(string email)
    {
        _email = email;
    }
    
    public bool ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing {amount:C} via PayPal for {_email}");
        // Logique de paiement PayPal
        return true;
    }
    
    public string GetPaymentDetails() => $"PayPal: {_email}";
}

public class PaymentProcessor
{
    private IPaymentStrategy _paymentStrategy;
    
    public void SetPaymentStrategy(IPaymentStrategy strategy)
    {
        _paymentStrategy = strategy;
    }
    
    public bool ProcessOrder(decimal amount)
    {
        if (_paymentStrategy == null)
            throw new InvalidOperationException("Payment strategy not set");
            
        Console.WriteLine($"Payment method: {_paymentStrategy.GetPaymentDetails()}");
        return _paymentStrategy.ProcessPayment(amount);
    }
}

// Utilisation
var processor = new PaymentProcessor();
processor.SetPaymentStrategy(new CreditCardPayment("1234567890123456"));
processor.ProcessOrder(99.99m);

processor.SetPaymentStrategy(new PayPalPayment("user@example.com"));
processor.ProcessOrder(149.99m);
```

## 5. Pattern Command

**But :** Encapsuler une requête en tant qu'objet, permettant de paramétrer, mettre en file d'attente et annuler les opérations.

```csharp
public interface ICommand
{
    void Execute();
    void Undo();
}

public class Document
{
    private StringBuilder _content = new StringBuilder();
    
    public void Write(string text)
    {
        _content.Append(text);
    }
    
    public void DeleteLast(int count)
    {
        if (_content.Length >= count)
            _content.Remove(_content.Length - count, count);
    }
    
    public string GetContent() => _content.ToString();
}

public class WriteCommand : ICommand
{
    private readonly Document _document;
    private readonly string _text;
    
    public WriteCommand(Document document, string text)
    {
        _document = document;
        _text = text;
    }
    
    public void Execute()
    {
        _document.Write(_text);
    }
    
    public void Undo()
    {
        _document.DeleteLast(_text.Length);
    }
}

public class TextEditor
{
    private readonly Document _document = new Document();
    private readonly Stack<ICommand> _history = new Stack<ICommand>();
    
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _history.Push(command);
    }
    
    public void Undo()
    {
        if (_history.Count > 0)
        {
            var command = _history.Pop();
            command.Undo();
        }
    }
    
    public string GetContent() => _document.GetContent();
}

// Utilisation
var editor = new TextEditor();
editor.ExecuteCommand(new WriteCommand(editor._document, "Hello "));
editor.ExecuteCommand(new WriteCommand(editor._document, "World!"));
Console.WriteLine(editor.GetContent()); // "Hello World!"

editor.Undo();
Console.WriteLine(editor.GetContent()); // "Hello "
```

---

# Jeu de Questions-Réponses : Design Patterns

## Questions de Base

**Q1:** Quel pattern garantit qu'une classe n'a qu'une seule instance ?
<details>
<summary>Réponse</summary>
Le pattern Singleton
</details>

**Q2:** Dans le pattern Observer, qui notifie qui ?
<details>
<summary>Réponse</summary>
Le Subject (observé) notifie tous ses Observers (observateurs) quand son état change.
</details>

**Q3:** Quel est l'avantage principal du pattern Strategy ?
<details>
<summary>Réponse</summary>
Il permet de changer l'algorithme utilisé à l'exécution, favorisant la flexibilité et l'extensibilité.
</details>

## Questions Intermédiaires

**Q4:** Pourquoi utiliser `Lazy<T>` pour implémenter Singleton ?
<details>
<summary>Réponse</summary>
- Thread-safe par défaut
- Initialisation paresseuse (lazy loading)
- Performance optimale
- Code plus simple et lisible
</details>

**Q5:** Quelle est la différence entre Factory Method et Abstract Factory ?
<details>
<summary>Réponse</summary>
- **Factory Method** : Crée un type d'objet, méthode virtuelle redéfinie dans les sous-classes
- **Abstract Factory** : Crée une famille d'objets liés, interface avec plusieurs méthodes de création
</details>

**Q6:** Dans quels cas éviter le pattern Singleton ?
<details>
<summary>Réponse</summary>
- Tests unitaires difficiles (dépendance globale)
- Applications multi-tenant
- Quand l'injection de dépendance est préférable
- Violation du principe de responsabilité unique
</details>

## Questions Avancées

**Q7:** Implémentez un pattern Decorator pour ajouter des fonctionnalités à une boisson.
<details>
<summary>Réponse</summary>
```csharp
public interface IBeverage
{
    string Description { get; }
    decimal Cost { get; }
}

public class Coffee : IBeverage
{
    public string Description => "Coffee";
    public decimal Cost => 2.00m;
}

public abstract class BeverageDecorator : IBeverage
{
    protected IBeverage _beverage;
    
    public BeverageDecorator(IBeverage beverage)
    {
        _beverage = beverage;
    }
    
    public virtual string Description => _beverage.Description;
    public virtual decimal Cost => _beverage.Cost;
}

public class MilkDecorator : BeverageDecorator
{
    public MilkDecorator(IBeverage beverage) : base(beverage) { }
    
    public override string Description => _beverage.Description + ", Milk";
    public override decimal Cost => _beverage.Cost + 0.50m;
}
```
</details>

**Q8:** Comment implémenter le pattern Repository avec Entity Framework ?
<details>
<summary>Réponse</summary>
```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public class Repository<T> : IRepository<T> where T : class
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
    
    // Autres méthodes...
}
```
</details>
