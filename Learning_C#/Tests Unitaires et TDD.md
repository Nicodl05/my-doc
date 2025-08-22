# **Tests Unitaires et TDD en C# - Guide Complet**

> **🔗 Références :**
> - [Design Patterns](./Design%20Patterns.md) - Patterns pour la testabilité
> - [Async](./Délégués%20et%20Async.md) - Tests de code asynchrone
> - [Bases de Données](./Bases%20de%20Données%20et%20Entity%20Framework.md) - Tests avec EF
> - [Performance](./Complexité%20Algorithmique%20et%20Performance.md) - Tests de performance

---

# **TESTS UNITAIRES ET TDD (TEST-DRIVEN DEVELOPMENT)**

## **1. Fondamentaux des Tests Unitaires avec xUnit**

### **Q: Comment structurer et écrire des tests unitaires efficaces ?**

```csharp
// Classe à tester - Service métier
public interface ICalculatorService
{
    decimal Add(decimal a, decimal b);
    decimal Subtract(decimal a, decimal b);
    decimal Multiply(decimal a, decimal b);
    decimal Divide(decimal a, decimal b);
    decimal CalculateCompoundInterest(decimal principal, decimal rate, int periods);
    bool IsPrime(int number);
}

public class CalculatorService : ICalculatorService
{
    private readonly ILogger<CalculatorService> _logger;
    
    public CalculatorService(ILogger<CalculatorService> logger)
    {
        _logger = logger;
    }
    
    public decimal Add(decimal a, decimal b)
    {
        _logger.LogDebug("Adding {A} + {B}", a, b);
        return a + b;
    }
    
    public decimal Subtract(decimal a, decimal b)
    {
        _logger.LogDebug("Subtracting {A} - {B}", a, b);
        return a - b;
    }
    
    public decimal Multiply(decimal a, decimal b)
    {
        _logger.LogDebug("Multiplying {A} * {B}", a, b);
        
        if (a == 0 || b == 0)
            return 0;
        
        return a * b;
    }
    
    public decimal Divide(decimal a, decimal b)
    {
        _logger.LogDebug("Dividing {A} / {B}", a, b);
        
        if (b == 0)
            throw new DivideByZeroException("Cannot divide by zero");
        
        return a / b;
    }
    
    public decimal CalculateCompoundInterest(decimal principal, decimal rate, int periods)
    {
        if (principal < 0)
            throw new ArgumentException("Principal must be positive", nameof(principal));
        
        if (rate < 0)
            throw new ArgumentException("Rate must be positive", nameof(rate));
        
        if (periods <= 0)
            throw new ArgumentException("Periods must be greater than zero", nameof(periods));
        
        return principal * (decimal)Math.Pow((double)(1 + rate), periods);
    }
    
    public bool IsPrime(int number)
    {
        if (number <= 1) return false;
        if (number <= 3) return true;
        if (number % 2 == 0 || number % 3 == 0) return false;
        
        for (int i = 5; i * i <= number; i += 6)
        {
            if (number % i == 0 || number % (i + 2) == 0)
                return false;
        }
        
        return true;
    }
}

// Tests unitaires avec xUnit
public class CalculatorServiceTests
{
    private readonly Mock<ILogger<CalculatorService>> _mockLogger;
    private readonly CalculatorService _calculatorService;
    
    public CalculatorServiceTests()
    {
        _mockLogger = new Mock<ILogger<CalculatorService>>();
        _calculatorService = new CalculatorService(_mockLogger.Object);
    }
    
    // Tests de base - AAA Pattern (Arrange, Act, Assert)
    [Fact]
    public void Add_WithTwoPositiveNumbers_ReturnsCorrectSum()
    {
        // Arrange
        decimal a = 5.5m;
        decimal b = 3.2m;
        decimal expected = 8.7m;
        
        // Act
        decimal result = _calculatorService.Add(a, b);
        
        // Assert
        Assert.Equal(expected, result, 1); // Précision à 1 décimale
    }
    
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 1, 2)]
    [InlineData(-1, 1, 0)]
    [InlineData(10.5, 5.3, 15.8)]
    [InlineData(-5.2, -3.1, -8.3)]
    public void Add_WithVariousInputs_ReturnsCorrectSum(decimal a, decimal b, decimal expected)
    {
        // Act
        decimal result = _calculatorService.Add(a, b);
        
        // Assert
        Assert.Equal(expected, result, 1);
    }
    
    // Tests avec données complexes
    [Theory]
    [MemberData(nameof(GetAdditionTestData))]
    public void Add_WithMemberData_ReturnsCorrectSum(decimal a, decimal b, decimal expected)
    {
        // Act
        decimal result = _calculatorService.Add(a, b);
        
        // Assert
        Assert.Equal(expected, result, 2);
    }
    
    public static IEnumerable<object[]> GetAdditionTestData()
    {
        yield return new object[] { 1.11m, 2.22m, 3.33m };
        yield return new object[] { 100m, 200m, 300m };
        yield return new object[] { -50.5m, 25.25m, -25.25m };
        yield return new object[] { decimal.MaxValue, 0m, decimal.MaxValue };
    }
    
    // Tests d'exceptions
    [Fact]
    public void Divide_ByZero_ThrowsDivideByZeroException()
    {
        // Arrange
        decimal a = 10;
        decimal b = 0;
        
        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => _calculatorService.Divide(a, b));
    }
    
    [Theory]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public void CalculateCompoundInterest_WithNegativePrincipal_ThrowsArgumentException(decimal principal)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            _calculatorService.CalculateCompoundInterest(principal, 0.05m, 10));
        
        Assert.Equal("Principal must be positive (Parameter 'principal')", exception.Message);
    }
    
    // Tests avec ClassData pour données plus complexes
    [Theory]
    [ClassData(typeof(PrimeNumberTestData))]
    public void IsPrime_WithVariousNumbers_ReturnsCorrectResult(int number, bool expected)
    {
        // Act
        bool result = _calculatorService.IsPrime(number);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    // Tests de performance
    [Fact]
    public void IsPrime_WithLargeNumber_CompletesWithinTimeLimit()
    {
        // Arrange
        int largeNumber = 982451653; // Grand nombre premier
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        bool result = _calculatorService.IsPrime(largeNumber);
        stopwatch.Stop();
        
        // Assert
        Assert.True(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Performance test failed. Took {stopwatch.ElapsedMilliseconds}ms");
    }
    
    // Tests de logging avec Moq
    [Fact]
    public void Add_LogsDebugMessage()
    {
        // Arrange
        decimal a = 5;
        decimal b = 3;
        
        // Act
        _calculatorService.Add(a, b);
        
        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding 5 + 3")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

// Classe pour données de test complexes
public class PrimeNumberTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 1, false };
        yield return new object[] { 2, true };
        yield return new object[] { 3, true };
        yield return new object[] { 4, false };
        yield return new object[] { 5, true };
        yield return new object[] { 97, true };
        yield return new object[] { 100, false };
        yield return new object[] { 997, true };
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// Fixtures pour partage de ressources
public class DatabaseFixture : IDisposable
{
    public ApplicationDbContext Context { get; private set; }
    
    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
        
        // Seed data
        SeedTestData();
    }
    
    private void SeedTestData()
    {
        Context.Users.AddRange(
            new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new User { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
        );
        
        Context.SaveChanges();
    }
    
    public void Dispose()
    {
        Context?.Dispose();
    }
}

// Collection pour partage de fixtures
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // Cette classe n'a pas de code, elle sert juste pour la déclaration
}
```

---

## **2. Test-Driven Development (TDD) - Cycle Red-Green-Refactor**

### **Q: Comment pratiquer le TDD efficacement ?**

```csharp
// Exemple TDD : Développement d'une classe BankAccount

// ÉTAPE 1: RED - Écrire le test qui échoue
public class BankAccountTests
{
    [Fact]
    public void NewAccount_ShouldHaveZeroBalance()
    {
        // Cette classe n'existe pas encore !
        var account = new BankAccount();
        
        Assert.Equal(0, account.Balance);
    }
}

// ÉTAPE 2: GREEN - Écrire le minimum de code pour passer le test
public class BankAccount
{
    public decimal Balance { get; } = 0;
}

// ÉTAPE 3: RED - Ajouter un nouveau test
[Fact]
public void Deposit_WithPositiveAmount_IncreasesBalance()
{
    var account = new BankAccount();
    
    account.Deposit(100);
    
    Assert.Equal(100, account.Balance);
}

// ÉTAPE 4: GREEN - Faire passer le test
public class BankAccount
{
    private decimal _balance = 0;
    
    public decimal Balance => _balance;
    
    public void Deposit(decimal amount)
    {
        _balance += amount;
    }
}

// ÉTAPE 5: RED - Tests plus complexes
[Theory]
[InlineData(-1)]
[InlineData(-100)]
[InlineData(0)]
public void Deposit_WithNonPositiveAmount_ThrowsArgumentException(decimal amount)
{
    var account = new BankAccount();
    
    Assert.Throws<ArgumentException>(() => account.Deposit(amount));
}

[Fact]
public void Withdraw_WithSufficientFunds_DecreasesBalance()
{
    var account = new BankAccount();
    account.Deposit(100);
    
    account.Withdraw(30);
    
    Assert.Equal(70, account.Balance);
}

[Fact]
public void Withdraw_WithInsufficientFunds_ThrowsInvalidOperationException()
{
    var account = new BankAccount();
    account.Deposit(50);
    
    Assert.Throws<InvalidOperationException>(() => account.Withdraw(100));
}

// ÉTAPE 6: GREEN - Implémentation complète
public class BankAccount
{
    private decimal _balance = 0;
    private readonly List<Transaction> _transactions = new();
    
    public decimal Balance => _balance;
    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();
    
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
        
        _balance += amount;
        _transactions.Add(new Transaction(TransactionType.Deposit, amount, DateTime.UtcNow));
    }
    
    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
        
        if (amount > _balance)
            throw new InvalidOperationException("Insufficient funds");
        
        _balance -= amount;
        _transactions.Add(new Transaction(TransactionType.Withdrawal, amount, DateTime.UtcNow));
    }
}

public class Transaction
{
    public TransactionType Type { get; }
    public decimal Amount { get; }
    public DateTime Date { get; }
    
    public Transaction(TransactionType type, decimal amount, DateTime date)
    {
        Type = type;
        Amount = amount;
        Date = date;
    }
}

public enum TransactionType
{
    Deposit,
    Withdrawal
}

// ÉTAPE 7: REFACTOR - Tests pour les nouvelles fonctionnalités
[Fact]
public void Deposit_RecordsTransaction()
{
    var account = new BankAccount();
    
    account.Deposit(100);
    
    Assert.Single(account.Transactions);
    Assert.Equal(TransactionType.Deposit, account.Transactions[0].Type);
    Assert.Equal(100, account.Transactions[0].Amount);
}

[Fact]
public void MultipleOperations_MaintainCorrectBalance()
{
    var account = new BankAccount();
    
    account.Deposit(100);
    account.Withdraw(30);
    account.Deposit(50);
    account.Withdraw(20);
    
    Assert.Equal(100, account.Balance);
    Assert.Equal(4, account.Transactions.Count);
}

// Test d'intégration plus complexe
[Fact]
public void TransferBetweenAccounts_UpdatesBothBalances()
{
    var accountA = new BankAccount();
    var accountB = new BankAccount();
    accountA.Deposit(200);
    
    var transferService = new TransferService();
    
    transferService.Transfer(accountA, accountB, 75);
    
    Assert.Equal(125, accountA.Balance);
    Assert.Equal(75, accountB.Balance);
}

// Service pour les transferts
public class TransferService
{
    public void Transfer(BankAccount fromAccount, BankAccount toAccount, decimal amount)
    {
        if (fromAccount == null)
            throw new ArgumentNullException(nameof(fromAccount));
        
        if (toAccount == null)
            throw new ArgumentNullException(nameof(toAccount));
        
        if (amount <= 0)
            throw new ArgumentException("Transfer amount must be positive", nameof(amount));
        
        fromAccount.Withdraw(amount);
        toAccount.Deposit(amount);
    }
}
```

---

## **3. Tests d'Intégration et Tests avec Bases de Données**

### **Q: Comment tester les couches d'accès aux données ?**

```csharp
// Tests d'intégration avec base de données en mémoire
[Collection("Database collection")]
public class UserRepositoryIntegrationTests
{
    private readonly DatabaseFixture _fixture;
    private readonly UserRepository _repository;
    
    public UserRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new UserRepository(_fixture.Context);
    }
    
    [Fact]
    public async Task CreateUser_SavesCorrectly()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test.user@example.com",
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        await _repository.AddAsync(user);
        await _fixture.Context.SaveChangesAsync();
        
        // Assert
        var savedUser = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(savedUser);
        Assert.Equal("Test", savedUser.FirstName);
        Assert.Equal("test.user@example.com", savedUser.Email);
    }
    
    [Fact]
    public async Task GetUserByEmail_ExistingUser_ReturnsUser()
    {
        // Arrange
        var email = "john@test.com"; // De la seed data
        
        // Act
        var user = await _repository.GetUserByEmailAsync(email);
        
        // Assert
        Assert.NotNull(user);
        Assert.Equal(email, user.Email);
        Assert.Equal("John", user.FirstName);
    }
    
    [Fact]
    public async Task GetActiveUsers_ReturnsOnlyActiveUsers()
    {
        // Arrange
        var activeUser = new User
        {
            FirstName = "Active",
            LastName = "User",
            Email = "active@test.com",
            LastLoginAt = DateTime.UtcNow.AddDays(-5) // Actif
        };
        
        var inactiveUser = new User
        {
            FirstName = "Inactive",
            LastName = "User",
            Email = "inactive@test.com",
            LastLoginAt = DateTime.UtcNow.AddDays(-60) // Inactif
        };
        
        _fixture.Context.Users.AddRange(activeUser, inactiveUser);
        await _fixture.Context.SaveChangesAsync();
        
        // Act
        var activeUsers = await _repository.GetActiveUsersAsync();
        
        // Assert
        Assert.Contains(activeUsers, u => u.Email == "active@test.com");
        Assert.DoesNotContain(activeUsers, u => u.Email == "inactive@test.com");
    }
}

// Tests avec TestContainers pour vraie base de données
public class OrderServiceIntegrationTests : IClassFixture<DatabaseTestContainer>
{
    private readonly DatabaseTestContainer _container;
    private readonly ApplicationDbContext _context;
    private readonly OrderService _orderService;
    
    public OrderServiceIntegrationTests(DatabaseTestContainer container)
    {
        _container = container;
        _context = _container.CreateContext();
        
        var unitOfWork = new UnitOfWork(_context);
        var logger = Mock.Of<ILogger<OrderService>>();
        _orderService = new OrderService(unitOfWork, logger);
    }
    
    [Fact]
    public async Task CreateOrder_WithValidData_CreatesOrderAndUpdatesStock()
    {
        // Arrange
        var user = new User 
        { 
            FirstName = "Test", 
            LastName = "User", 
            Email = "test@example.com" 
        };
        _context.Users.Add(user);
        
        var product = new Product 
        { 
            Name = "Test Product", 
            Price = 50.00m, 
            StockQuantity = 10,
            CategoryId = 1
        };
        _context.Products.Add(product);
        
        await _context.SaveChangesAsync();
        
        var orderItems = new List<CreateOrderItemDto>
        {
            new CreateOrderItemDto { ProductId = product.Id, Quantity = 2 }
        };
        
        // Act
        var order = await _orderService.CreateOrderAsync(user.Id, orderItems);
        
        // Assert
        Assert.NotNull(order);
        Assert.Equal(100.00m, order.TotalAmount);
        Assert.Equal(OrderStatus.Pending, order.Status);
        
        // Vérifier la mise à jour du stock
        var updatedProduct = await _context.Products.FindAsync(product.Id);
        Assert.Equal(8, updatedProduct.StockQuantity);
    }
    
    [Fact]
    public async Task CreateOrder_InsufficientStock_ThrowsException()
    {
        // Arrange
        var user = new User 
        { 
            FirstName = "Test", 
            LastName = "User", 
            Email = "test2@example.com" 
        };
        _context.Users.Add(user);
        
        var product = new Product 
        { 
            Name = "Limited Product", 
            Price = 25.00m, 
            StockQuantity = 1,
            CategoryId = 1
        };
        _context.Products.Add(product);
        
        await _context.SaveChangesAsync();
        
        var orderItems = new List<CreateOrderItemDto>
        {
            new CreateOrderItemDto { ProductId = product.Id, Quantity = 5 } // Plus que le stock
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orderService.CreateOrderAsync(user.Id, orderItems));
        
        // Vérifier que le stock n'a pas été modifié
        var unchangedProduct = await _context.Products.FindAsync(product.Id);
        Assert.Equal(1, unchangedProduct.StockQuantity);
    }
}

// Container de test avec TestContainers
public class DatabaseTestContainer : IDisposable
{
    private readonly SqlServerContainer _container;
    private readonly string _connectionString;
    
    public DatabaseTestContainer()
    {
        _container = new SqlServerBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Strong_Password123!")
            .Build();
        
        _container.StartAsync().Wait();
        _connectionString = _container.GetConnectionString();
        
        // Initialiser le schéma
        InitializeDatabase();
    }
    
    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;
        
        return new ApplicationDbContext(options);
    }
    
    private void InitializeDatabase()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();
        
        // Seed data de base
        context.Categories.AddRange(
            new Category { Id = 1, Name = "Test Category", Description = "For testing" }
        );
        
        context.SaveChanges();
    }
    
    public void Dispose()
    {
        _container?.DisposeAsync().AsTask().Wait();
    }
}
```

---

## **4. Tests de Performance et Mocking Avancé**

### **Q: Comment tester les performances et créer des mocks sophistiqués ?**

```csharp
// Tests de performance avec BenchmarkDotNet
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class AlgorithmPerformanceTests
{
    private List<int> _smallData;
    private List<int> _largeData;
    
    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _smallData = Enumerable.Range(0, 1000)
            .Select(_ => random.Next(1, 1000))
            .ToList();
        
        _largeData = Enumerable.Range(0, 100000)
            .Select(_ => random.Next(1, 10000))
            .ToList();
    }
    
    [Benchmark]
    public List<int> QuickSort_SmallData()
    {
        var data = _smallData.ToList();
        var sorter = new QuickSortAlgorithm();
        return sorter.Sort(data);
    }
    
    [Benchmark]
    public List<int> MergeSort_SmallData()
    {
        var data = _smallData.ToList();
        var sorter = new MergeSortAlgorithm();
        return sorter.Sort(data);
    }
    
    [Benchmark]
    public List<int> QuickSort_LargeData()
    {
        var data = _largeData.ToList();
        var sorter = new QuickSortAlgorithm();
        return sorter.Sort(data);
    }
}

// Tests de charge et de stress
public class LoadTests
{
    [Fact]
    public async Task ProcessMultipleOrdersConcurrently_HandlesLoad()
    {
        // Arrange
        var orderService = CreateOrderService();
        var tasks = new List<Task<Order>>();
        
        // Act - Traiter 100 commandes simultanément
        for (int i = 0; i < 100; i++)
        {
            var orderItems = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = 1, Quantity = 1 }
            };
            
            tasks.Add(orderService.CreateOrderAsync(1, orderItems));
        }
        
        var results = await Task.WhenAll(tasks);
        
        // Assert
        Assert.Equal(100, results.Length);
        Assert.All(results, order => Assert.NotNull(order));
    }
    
    [Fact]
    public async Task DatabaseQuery_UnderLoad_MaintainsPerformance()
    {
        // Arrange
        var repository = CreateUserRepository();
        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task>();
        
        // Act - 50 requêtes simultanées
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(repository.GetUserWithOrdersAsync(1));
        }
        
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 5000,
            $"Queries took too long: {stopwatch.ElapsedMilliseconds}ms");
    }
}

// Mocking avancé avec Moq
public class AdvancedMockingTests
{
    [Fact]
    public async Task EmailService_SendOrderConfirmation_CallsCorrectMethods()
    {
        // Arrange
        var mockEmailProvider = new Mock<IEmailProvider>();
        var mockTemplateService = new Mock<ITemplateService>();
        var mockLogger = new Mock<ILogger<EmailService>>();
        
        // Setup des mocks avec callbacks
        var sentEmails = new List<Email>();
        mockEmailProvider
            .Setup(x => x.SendAsync(It.IsAny<Email>()))
            .Callback<Email>(email => sentEmails.Add(email))
            .Returns(Task.CompletedTask);
        
        mockTemplateService
            .Setup(x => x.GenerateOrderConfirmationHtml(It.IsAny<Order>()))
            .Returns("<html>Order confirmation</html>");
        
        var emailService = new EmailService(
            mockEmailProvider.Object,
            mockTemplateService.Object,
            mockLogger.Object);
        
        var order = new Order 
        { 
            Id = 123, 
            User = new User { Email = "test@example.com" } 
        };
        
        // Act
        await emailService.SendOrderConfirmationAsync(order);
        
        // Assert
        mockTemplateService.Verify(
            x => x.GenerateOrderConfirmationHtml(It.Is<Order>(o => o.Id == 123)),
            Times.Once);
        
        mockEmailProvider.Verify(
            x => x.SendAsync(It.Is<Email>(e => 
                e.To == "test@example.com" && 
                e.Subject.Contains("Order Confirmation"))),
            Times.Once);
        
        Assert.Single(sentEmails);
        Assert.Equal("test@example.com", sentEmails[0].To);
    }
    
    [Fact]
    public async Task PaymentService_ProcessPayment_HandlesRetries()
    {
        // Arrange
        var mockPaymentGateway = new Mock<IPaymentGateway>();
        var callCount = 0;
        
        // Premier appel échoue, deuxième réussit
        mockPaymentGateway
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                    throw new HttpRequestException("Network error");
                
                return Task.FromResult(new PaymentResult { Success = true });
            });
        
        var paymentService = new PaymentService(mockPaymentGateway.Object);
        
        var request = new PaymentRequest 
        { 
            Amount = 100, 
            CardNumber = "1234567890123456" 
        };
        
        // Act
        var result = await paymentService.ProcessWithRetryAsync(request);
        
        // Assert
        Assert.True(result.Success);
        mockPaymentGateway.Verify(
            x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>()),
            Times.Exactly(2));
    }
    
    // Mock avec propriétés complexes
    [Fact]
    public void ConfigurationService_LoadsCorrectSettings()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        
        // Setup de sections imbriquées
        var mockEmailSection = new Mock<IConfigurationSection>();
        mockEmailSection.Setup(x => x["SmtpHost"]).Returns("smtp.test.com");
        mockEmailSection.Setup(x => x["Port"]).Returns("587");
        
        var mockDatabaseSection = new Mock<IConfigurationSection>();
        mockDatabaseSection.Setup(x => x["ConnectionString"])
            .Returns("Server=test;Database=test");
        
        mockConfiguration.Setup(x => x.GetSection("Email")).Returns(mockEmailSection.Object);
        mockConfiguration.Setup(x => x.GetSection("Database")).Returns(mockDatabaseSection.Object);
        
        var configService = new ConfigurationService(mockConfiguration.Object);
        
        // Act
        var emailSettings = configService.GetEmailSettings();
        var dbSettings = configService.GetDatabaseSettings();
        
        // Assert
        Assert.Equal("smtp.test.com", emailSettings.SmtpHost);
        Assert.Equal(587, emailSettings.Port);
        Assert.Equal("Server=test;Database=test", dbSettings.ConnectionString);
    }
    
    // Tests avec AutoFixture pour génération de données
    [Theory, AutoData]
    public void UserValidator_ValidUser_ReturnsTrue(User user)
    {
        // AutoFixture génère automatiquement des données valides
        // Arrange
        user.Email = "valid@example.com"; // Override pour garantir un email valide
        user.FirstName = "John";
        user.LastName = "Doe";
        
        var validator = new UserValidator();
        
        // Act
        var result = validator.Validate(user);
        
        // Assert
        Assert.True(result.IsValid);
    }
}

// Services pour les tests
public interface IEmailProvider
{
    Task SendAsync(Email email);
}

public interface ITemplateService
{
    string GenerateOrderConfirmationHtml(Order order);
}

public class EmailService
{
    private readonly IEmailProvider _emailProvider;
    private readonly ITemplateService _templateService;
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(IEmailProvider emailProvider, ITemplateService templateService, ILogger<EmailService> logger)
    {
        _emailProvider = emailProvider;
        _templateService = templateService;
        _logger = logger;
    }
    
    public async Task SendOrderConfirmationAsync(Order order)
    {
        var html = _templateService.GenerateOrderConfirmationHtml(order);
        
        var email = new Email
        {
            To = order.User.Email,
            Subject = $"Order Confirmation #{order.Id}",
            Body = html
        };
        
        await _emailProvider.SendAsync(email);
        _logger.LogInformation("Order confirmation sent for order {OrderId}", order.Id);
    }
}

public class Email
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
}

public class PaymentService
{
    private readonly IPaymentGateway _paymentGateway;
    
    public PaymentService(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway;
    }
    
    public async Task<PaymentResult> ProcessWithRetryAsync(PaymentRequest request, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await _paymentGateway.ProcessPaymentAsync(request);
            }
            catch (HttpRequestException) when (attempt < maxRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt)); // Backoff exponentiel
            }
        }
        
        throw new InvalidOperationException("Payment failed after all retries");
    }
}

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string CardNumber { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; }
}
```

Cette documentation complète couvre tous les aspects des tests en C# : tests unitaires, TDD, tests d'intégration, mocking et tests de performance ! 🧪✅
