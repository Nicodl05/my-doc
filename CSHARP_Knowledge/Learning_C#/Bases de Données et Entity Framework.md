# **Bases de Données et Entity Framework en C# - Guide Technique**

> **🔗 Références :**
> - [LINQ](./LINQ%20et%20Expressions%20Lambda.md) - Requêtes LINQ avec EF
> - [Async](./Délégués%20et%20Async.md) - Opérations asynchrones avec EF
> - [Design Patterns](./Design%20Patterns.md) - Repository, Unit of Work
> - [Performance](./Complexité%20Algorithmique%20et%20Performance.md) - Optimisation EF

---

# **BASES DE DONNÉES ET ENTITY FRAMEWORK**

## **1. Entity Framework Core - Configuration de Base**

### **Q: Comment configurer Entity Framework Core ?**

```csharp
// Modèles
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation properties
    public List<Order> Orders { get; set; } = new();
    public UserProfile Profile { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    
    // Foreign Key
    public int UserId { get; set; }
    public User User { get; set; }
    
    public List<OrderItem> OrderItems { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    public int OrderId { get; set; }
    public Order Order { get; set; }
    
    public int ProductId { get; set; }
    public Product Product { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    
    public List<OrderItem> OrderItems { get; set; } = new();
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public List<Product> Products { get; set; } = new();
}

public class UserProfile
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Address { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

// DbContext
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuration des entités
        ConfigureUser(modelBuilder);
        ConfigureOrder(modelBuilder);
        ConfigureProduct(modelBuilder);
        ConfigureUserProfile(modelBuilder);
        
        // Seed data
        SeedData(modelBuilder);
    }
    
    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.HasIndex(e => e.Email)
                .IsUnique();
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Relations
            entity.HasMany(e => e.Orders)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Profile)
                .WithOne(e => e.User)
                .HasForeignKey<UserProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18,2)");
            
            entity.Property(e => e.Status)
                .HasConversion<string>();
            
            entity.HasIndex(e => new { e.UserId, e.OrderDate });
        });
        
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18,2)");
            
            // Composite index
            entity.HasIndex(e => new { e.OrderId, e.ProductId });
            
            // Relations
            entity.HasOne(e => e.Order)
                .WithMany(e => e.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Product)
                .WithMany(e => e.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    private void ConfigureProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18,2)");
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CategoryId);
        });
        
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.HasIndex(e => e.Name)
                .IsUnique();
            
            entity.HasMany(e => e.Products)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    private void ConfigureUserProfile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);
            
            entity.Property(e => e.Address)
                .HasMaxLength(500);
        });
    }
    
    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Electronic devices" },
            new Category { Id = 2, Name = "Books", Description = "Books and literature" },
            new Category { Id = 3, Name = "Clothing", Description = "Clothes and accessories" }
        );
        
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Description = "Gaming laptop", Price = 1299.99m, StockQuantity = 50, CategoryId = 1 },
            new Product { Id = 2, Name = "C# Programming", Description = "Learn C#", Price = 49.99m, StockQuantity = 100, CategoryId = 2 },
            new Product { Id = 3, Name = "T-Shirt", Description = "Cotton t-shirt", Price = 19.99m, StockQuantity = 200, CategoryId = 3 }
        );
    }
}

// Configuration dans Program.cs / Startup.cs
public class DatabaseConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    
                    sqlOptions.CommandTimeout(30);
                }));
        
        // PostgreSQL alternative
        // services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseNpgsql(configuration.GetConnectionString("PostgreSqlConnection")));
        
        // Configuration avancée
        services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            
            // Logging et diagnostics
            options.EnableSensitiveDataLogging(true); // Développement seulement
            options.EnableDetailedErrors(true);
            
            // Performance
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
        });
    }
}
```

---

## **2. Requêtes LINQ avec Entity Framework**

### **Q: Comment optimiser les requêtes Entity Framework ?**

```csharp
public class EfQueryService
{
    private readonly ApplicationDbContext _context;
    
    public EfQueryService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Requêtes de base avec optimisations
    public async Task<User> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.Orders)
            .ThenInclude(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
    
    // Projection pour éviter de charger toutes les données
    public async Task<List<UserSummaryDto>> GetUserSummariesAsync()
    {
        return await _context.Users
            .Select(u => new UserSummaryDto
            {
                Id = u.Id,
                FullName = u.FirstName + " " + u.LastName,
                Email = u.Email,
                OrderCount = u.Orders.Count(),
                TotalSpent = u.Orders.Sum(o => o.TotalAmount),
                LastOrderDate = u.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => o.OrderDate)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }
    
    // Requêtes complexes avec groupage
    public async Task<List<CategorySalesDto>> GetCategorySalesAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .Where(oi => oi.Order.OrderDate >= fromDate && oi.Order.OrderDate <= toDate)
            .GroupBy(oi => new { oi.Product.Category.Id, oi.Product.Category.Name })
            .Select(g => new CategorySalesDto
            {
                CategoryId = g.Key.Id,
                CategoryName = g.Key.Name,
                TotalQuantity = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice),
                OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
            })
            .OrderByDescending(c => c.TotalRevenue)
            .ToListAsync();
    }
    
    // Requête avec pagination efficace
    public async Task<PagedResult<ProductDto>> GetProductsPagedAsync(int page, int pageSize, string searchTerm = null)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || 
                                   p.Description.Contains(searchTerm));
        }
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name
            })
            .ToListAsync();
        
        return new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
    
    // Split Query pour éviter les cartesian products
    public async Task<List<Order>> GetOrdersWithDetailsAsync()
    {
        return await _context.Orders
            .AsSplitQuery() // Évite les jointures cartésiennes
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .ToListAsync();
    }
    
    // Raw SQL pour requêtes complexes
    public async Task<List<TopCustomerDto>> GetTopCustomersAsync(int topCount)
    {
        return await _context.Database
            .SqlQueryRaw<TopCustomerDto>(@"
                SELECT TOP (@topCount)
                    u.Id,
                    u.FirstName + ' ' + u.LastName AS FullName,
                    u.Email,
                    COUNT(o.Id) AS OrderCount,
                    SUM(o.TotalAmount) AS TotalSpent
                FROM Users u
                INNER JOIN Orders o ON u.Id = o.UserId
                GROUP BY u.Id, u.FirstName, u.LastName, u.Email
                ORDER BY SUM(o.TotalAmount) DESC",
                new SqlParameter("@topCount", topCount))
            .ToListAsync();
    }
    
    // Bulk operations avec EF Core Extensions
    public async Task BulkUpdateProductPricesAsync(List<ProductPriceUpdate> updates)
    {
        foreach (var update in updates)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Products SET Price = {0} WHERE Id = {1}",
                update.NewPrice, update.ProductId);
        }
    }
    
    // Optimisation avec NoTracking
    public async Task<List<ProductDto>> GetProductsReadOnlyAsync()
    {
        return await _context.Products
            .AsNoTracking() // Pas de change tracking
            .Include(p => p.Category)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name
            })
            .ToListAsync();
    }
    
    // Compiled Query pour performance
    private static readonly Func<ApplicationDbContext, int, Task<User>> GetUserByIdCompiled =
        EF.CompileAsyncQuery((ApplicationDbContext context, int userId) =>
            context.Users
                .Include(u => u.Profile)
                .FirstOrDefault(u => u.Id == userId));
    
    public async Task<User> GetUserByIdCompiledAsync(int userId)
    {
        return await GetUserByIdCompiled(_context, userId);
    }
}

// DTOs
public class UserSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastOrderDate { get; set; }
}

public class CategorySalesDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; }
}

public class TopCustomerDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ProductPriceUpdate
{
    public int ProductId { get; set; }
    public decimal NewPrice { get; set; }
}
```

---

## **3. Repository Pattern et Unit of Work**

### **Q: Implémentez le Repository Pattern avec Entity Framework ?**

```csharp
// Interface générique du Repository
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}

// Implémentation générique
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }
    
    public virtual async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.SingleOrDefaultAsync(predicate);
    }
    
    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }
    
    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }
    
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }
    
    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }
    
    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }
    
    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }
    
    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
    
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }
    
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }
}

// Repositories spécialisés
public interface IUserRepository : IRepository<User>
{
    Task<User> GetUserWithProfileAsync(int userId);
    Task<User> GetUserWithOrdersAsync(int userId);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<User> GetUserByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersWithRecentOrdersAsync(DateTime fromDate);
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<User> GetUserWithProfileAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
    
    public async Task<User> GetUserWithOrdersAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.Orders)
            .ThenInclude(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
    
    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        return await _dbSet
            .Where(u => u.LastLoginAt >= thirtyDaysAgo)
            .ToListAsync();
    }
    
    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<IEnumerable<User>> GetUsersWithRecentOrdersAsync(DateTime fromDate)
    {
        return await _dbSet
            .Where(u => u.Orders.Any(o => o.OrderDate >= fromDate))
            .Include(u => u.Orders.Where(o => o.OrderDate >= fromDate))
            .ToListAsync();
    }
}

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
    Task<Order> GetOrderWithDetailsAsync(int orderId);
    Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<decimal> GetTotalRevenueAsync();
}

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId)
    {
        return await _dbSet
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _dbSet
            .Where(o => o.Status == status)
            .Include(o => o.User)
            .ToListAsync();
    }
    
    public async Task<Order> GetOrderWithDetailsAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }
    
    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
            .OrderBy(o => o.OrderDate)
            .ToListAsync();
    }
    
    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _dbSet
            .Where(o => o.Status == OrderStatus.Delivered)
            .SumAsync(o => o.TotalAmount);
    }
}

// Unit of Work Pattern
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IOrderRepository Orders { get; }
    IRepository<Product> Products { get; }
    IRepository<Category> Categories { get; }
    IRepository<OrderItem> OrderItems { get; }
    
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction _transaction;
    
    private IUserRepository _users;
    private IOrderRepository _orders;
    private IRepository<Product> _products;
    private IRepository<Category> _categories;
    private IRepository<OrderItem> _orderItems;
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);
    public IRepository<OrderItem> OrderItems => _orderItems ??= new Repository<OrderItem>(_context);
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            await _transaction?.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        try
        {
            await _transaction?.RollbackAsync();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }
    
    public void Dispose()
    {
        _transaction?.Dispose();
        _context?.Dispose();
    }
}

// Service utilisant Unit of Work
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;
    
    public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<Order> CreateOrderAsync(int userId, List<CreateOrderItemDto> orderItems)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Vérifier l'utilisateur
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");
            
            // Créer la commande
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItem>()
            };
            
            decimal totalAmount = 0;
            
            // Ajouter les articles et vérifier le stock
            foreach (var item in orderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new ArgumentException($"Product {item.ProductId} not found");
                
                if (product.StockQuantity < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
                
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };
                
                order.OrderItems.Add(orderItem);
                totalAmount += orderItem.Quantity * orderItem.UnitPrice;
                
                // Mettre à jour le stock
                product.StockQuantity -= item.Quantity;
                _unitOfWork.Products.Update(product);
            }
            
            order.TotalAmount = totalAmount;
            await _unitOfWork.Orders.AddAsync(order);
            
            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation("Order {OrderId} created for user {UserId}", order.Id, userId);
            
            return order;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating order for user {UserId}", userId);
            throw;
        }
    }
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
```

---

## **4. Gestion des Performances et Optimisations**

### **Q: Comment optimiser les performances d'Entity Framework ?**

```csharp
public class EfPerformanceOptimizations
{
    private readonly ApplicationDbContext _context;
    
    public EfPerformanceOptimizations(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // 1. Batch operations
    public async Task BulkInsertUsersAsync(List<User> users)
    {
        // EF Core permet les batch inserts automatiquement
        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();
    }
    
    // 2. Éviter N+1 queries avec Include
    public async Task<List<Order>> GetOrdersWithDetails_Bad()
    {
        // MAUVAIS - N+1 queries
        var orders = await _context.Orders.ToListAsync();
        
        foreach (var order in orders)
        {
            // Chaque accès fait une requête séparée !
            var user = order.User; // Lazy loading
            var items = order.OrderItems; // Lazy loading
        }
        
        return orders;
    }
    
    public async Task<List<Order>> GetOrdersWithDetails_Good()
    {
        // BON - Une seule requête avec joins
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToListAsync();
    }
    
    // 3. Utiliser des projections pour limiter les données
    public async Task<List<OrderSummaryDto>> GetOrderSummaries()
    {
        return await _context.Orders
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CustomerName = o.User.FirstName + " " + o.User.LastName,
                ItemCount = o.OrderItems.Count()
            })
            .ToListAsync();
    }
    
    // 4. Pagination efficace
    public async Task<PagedResult<Order>> GetOrdersPaged(int page, int pageSize)
    {
        var query = _context.Orders.AsQueryable();
        
        var totalCount = await query.CountAsync();
        
        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new PagedResult<Order>
        {
            Items = orders,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
    
    // 5. Utiliser AsNoTracking pour les lectures seules
    public async Task<List<Product>> GetProductsReadOnly()
    {
        return await _context.Products
            .AsNoTracking() // Pas de change tracking
            .Include(p => p.Category)
            .ToListAsync();
    }
    
    // 6. Connection pooling et DbContext pooling
    public static void ConfigureDbContextPool(IServiceCollection services, string connectionString)
    {
        services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        }, poolSize: 128); // Taille du pool
    }
    
    // 7. Query caching avec interceptors
    public class QueryCacheInterceptor : DbCommandInterceptor
    {
        private static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 1000
        });
        
        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            // Implémenter la logique de cache ici
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }
    }
    
    // 8. Optimisation des index
    public static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Index composites pour requêtes fréquentes
        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.UserId, o.OrderDate, o.Status })
            .HasDatabaseName("IX_Order_UserId_OrderDate_Status");
        
        // Index avec colonnes incluses (SQL Server)
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.CategoryId)
            .IncludeProperties(p => new { p.Name, p.Price })
            .HasDatabaseName("IX_Product_CategoryId_Includes");
        
        // Index filtrés
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .HasFilter("[Email] IS NOT NULL")
            .IsUnique();
    }
    
    // 9. Raw SQL pour requêtes complexes
    public async Task<List<SalesReportDto>> GetSalesReport(DateTime fromDate, DateTime toDate)
    {
        return await _context.Database
            .SqlQueryRaw<SalesReportDto>(@"
                WITH MonthlySales AS (
                    SELECT 
                        YEAR(o.OrderDate) as Year,
                        MONTH(o.OrderDate) as Month,
                        SUM(o.TotalAmount) as Revenue,
                        COUNT(*) as OrderCount
                    FROM Orders o
                    WHERE o.OrderDate >= {0} AND o.OrderDate <= {1}
                    AND o.Status = 'Delivered'
                    GROUP BY YEAR(o.OrderDate), MONTH(o.OrderDate)
                )
                SELECT 
                    Year,
                    Month,
                    Revenue,
                    OrderCount,
                    LAG(Revenue) OVER (ORDER BY Year, Month) as PreviousMonthRevenue
                FROM MonthlySales
                ORDER BY Year, Month",
                fromDate, toDate)
            .ToListAsync();
    }
    
    // 10. Monitoring et diagnostics
    public class EfDiagnosticsService
    {
        private readonly ILogger<EfDiagnosticsService> _logger;
        
        public EfDiagnosticsService(ILogger<EfDiagnosticsService> logger)
        {
            _logger = logger;
        }
        
        public void LogSlowQueries(DbContext context)
        {
            // Configurer dans Program.cs
            context.Database.SetCommandTimeout(30);
            
            // Log des requêtes lentes via ILogger
            // Configuration dans appsettings.json:
            // "Microsoft.EntityFrameworkCore.Database.Command": "Information"
        }
    }
}

// DTOs pour optimisations
public class OrderSummaryDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string CustomerName { get; set; }
    public int ItemCount { get; set; }
}

public class SalesReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal? PreviousMonthRevenue { get; set; }
}
```

Cette documentation complète vous donne tous les outils pour maîtriser Entity Framework et les bases de données en C# ! 🚀
