# **Architecture et Microservices en C# - Guide Expert**

> **🔗 Références :**
> - [Design Patterns](./Design%20Patterns.md) - Patterns architecturaux
> - [Async](./Délégués%20et%20Async.md) - Communication asynchrone
> - [Tests](./Tests%20Unitaires%20et%20TDD.md) - Tests de microservices
> - [Performance](./Complexité%20Algorithmique%20et%20Performance.md) - Optimisation distribuée

---

# **ARCHITECTURE ET MICROSERVICES**

## **1. Principes d'Architecture Logicielle**

### **Q: Comment concevoir une architecture Clean Architecture ?**

```csharp
// ===== COUCHE DOMAIN (Core) =====
// Entités métier - Pas de dépendances externes
namespace Domain.Entities
{
    public class Order
    {
        private readonly List<OrderItem> _orderItems = new();
        
        public int Id { get; private set; }
        public string OrderNumber { get; private set; }
        public int CustomerId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public OrderStatus Status { get; private set; }
        public decimal TotalAmount { get; private set; }
        
        public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
        
        public Order(int customerId, string orderNumber)
        {
            CustomerId = customerId;
            OrderNumber = orderNumber;
            OrderDate = DateTime.UtcNow;
            Status = OrderStatus.Created;
            TotalAmount = 0;
        }
        
        // Logique métier pure
        public void AddItem(int productId, decimal unitPrice, int quantity)
        {
            if (Status != OrderStatus.Created)
                throw new InvalidOperationException("Cannot modify confirmed order");
            
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");
            
            if (unitPrice <= 0)
                throw new ArgumentException("Unit price must be positive");
            
            var existingItem = _orderItems.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                _orderItems.Add(new OrderItem(productId, unitPrice, quantity));
            }
            
            RecalculateTotal();
        }
        
        public void Confirm()
        {
            if (Status != OrderStatus.Created)
                throw new InvalidOperationException("Order already confirmed");
            
            if (!_orderItems.Any())
                throw new InvalidOperationException("Cannot confirm empty order");
            
            Status = OrderStatus.Confirmed;
            
            // Domaine event
            AddDomainEvent(new OrderConfirmedEvent(Id, CustomerId, TotalAmount));
        }
        
        private void RecalculateTotal()
        {
            TotalAmount = _orderItems.Sum(item => item.LineTotal);
        }
        
        // Domain Events
        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        
        public void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents.Add(eventItem);
        }
        
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
    
    public class OrderItem
    {
        public int ProductId { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }
        public decimal LineTotal => UnitPrice * Quantity;
        
        public OrderItem(int productId, decimal unitPrice, int quantity)
        {
            ProductId = productId;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }
        
        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be positive");
            
            Quantity = newQuantity;
        }
    }
    
    public enum OrderStatus
    {
        Created,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }
}

// Value Objects
namespace Domain.ValueObjects
{
    public class Money : IEquatable<Money>
    {
        public decimal Amount { get; }
        public string Currency { get; }
        
        public Money(decimal amount, string currency)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative");
            
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required");
            
            Amount = amount;
            Currency = currency.ToUpper();
        }
        
        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot add different currencies");
            
            return new Money(Amount + other.Amount, Currency);
        }
        
        public bool Equals(Money other)
        {
            return other != null && Amount == other.Amount && Currency == other.Currency;
        }
        
        public override bool Equals(object obj) => Equals(obj as Money);
        public override int GetHashCode() => HashCode.Combine(Amount, Currency);
        
        public static bool operator ==(Money left, Money right) =>
            EqualityComparer<Money>.Default.Equals(left, right);
        
        public static bool operator !=(Money left, Money right) => !(left == right);
    }
}

// Domain Events
namespace Domain.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
    
    public class OrderConfirmedEvent : IDomainEvent
    {
        public int OrderId { get; }
        public int CustomerId { get; }
        public decimal TotalAmount { get; }
        public DateTime OccurredOn { get; }
        
        public OrderConfirmedEvent(int orderId, int customerId, decimal totalAmount)
        {
            OrderId = orderId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
            OccurredOn = DateTime.UtcNow;
        }
    }
}

// ===== COUCHE APPLICATION (Use Cases) =====
namespace Application.UseCases
{
    // Ports (Interfaces)
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(int id);
        Task<Order> GetByOrderNumberAsync(string orderNumber);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
    }
    
    public interface IProductService
    {
        Task<Product> GetProductAsync(int productId);
        Task<bool> IsAvailableAsync(int productId, int quantity);
    }
    
    public interface INotificationService
    {
        Task SendOrderConfirmationAsync(int orderId);
    }
    
    // Use Cases / Command Handlers
    public class CreateOrderCommand
    {
        public int CustomerId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
    
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    
    public class CreateOrderHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductService _productService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CreateOrderHandler> _logger;
        
        public CreateOrderHandler(
            IOrderRepository orderRepository,
            IProductService productService,
            INotificationService notificationService,
            ILogger<CreateOrderHandler> logger)
        {
            _orderRepository = orderRepository;
            _productService = productService;
            _notificationService = notificationService;
            _logger = logger;
        }
        
        public async Task<int> HandleAsync(CreateOrderCommand command)
        {
            try
            {
                // Générer numéro de commande
                var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
                
                // Créer la commande
                var order = new Order(command.CustomerId, orderNumber);
                
                // Ajouter les articles
                foreach (var item in command.Items)
                {
                    var product = await _productService.GetProductAsync(item.ProductId);
                    if (product == null)
                        throw new BusinessException($"Product {item.ProductId} not found");
                    
                    var isAvailable = await _productService.IsAvailableAsync(item.ProductId, item.Quantity);
                    if (!isAvailable)
                        throw new BusinessException($"Product {product.Name} not available in requested quantity");
                    
                    order.AddItem(item.ProductId, product.Price, item.Quantity);
                }
                
                // Confirmer la commande
                order.Confirm();
                
                // Sauvegarder
                await _orderRepository.AddAsync(order);
                
                // Traiter les événements de domaine
                await ProcessDomainEventsAsync(order);
                
                _logger.LogInformation("Order {OrderId} created successfully", order.Id);
                
                return order.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for customer {CustomerId}", command.CustomerId);
                throw;
            }
        }
        
        private async Task ProcessDomainEventsAsync(Order order)
        {
            var events = order.DomainEvents.ToList();
            order.ClearDomainEvents();
            
            foreach (var domainEvent in events)
            {
                if (domainEvent is OrderConfirmedEvent orderConfirmed)
                {
                    await _notificationService.SendOrderConfirmationAsync(orderConfirmed.OrderId);
                }
            }
        }
    }
}

// ===== COUCHE INFRASTRUCTURE =====
namespace Infrastructure.Persistence
{
    // Adaptateur pour le repository
    public class EfOrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        
        public EfOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<Order> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
        
        public async Task<Order> GetByOrderNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }
        
        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }
        
        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}

// ===== COUCHE PRESENTATION (API) =====
namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly CreateOrderHandler _createOrderHandler;
        private readonly ILogger<OrdersController> _logger;
        
        public OrdersController(CreateOrderHandler createOrderHandler, ILogger<OrdersController> logger)
        {
            _createOrderHandler = createOrderHandler;
            _logger = logger;
        }
        
        [HttpPost]
        public async Task<ActionResult<int>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var command = new CreateOrderCommand
                {
                    CustomerId = request.CustomerId,
                    Items = request.Items.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                };
                
                var orderId = await _createOrderHandler.HandleAsync(command);
                
                return Ok(orderId);
            }
            catch (BusinessException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating order");
                return StatusCode(500, "Internal server error");
            }
        }
    }
    
    public class CreateOrderRequest
    {
        public int CustomerId { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }
    
    public class CreateOrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

// Configuration DI
public static class ServiceConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Use Cases
        services.AddScoped<CreateOrderHandler>();
        
        // Domain Services
        services.AddScoped<IOrderRepository, EfOrderRepository>();
        
        return services;
    }
}
```

---

## **2. Architecture Microservices**

### **Q: Comment concevoir et implémenter des microservices ?**

```csharp
// ===== SERVICE GATEWAY (API Gateway) =====
namespace ApiGateway
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Configuration Ocelot
            services.AddOcelot()
                .AddPolly(); // Pour la résilience
            
            // Configuration CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
            
            // JWT Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "your-issuer",
                        ValidAudience = "your-audience",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key"))
                    };
                });
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseOcelot().Wait();
        }
    }
}

// Configuration Ocelot (ocelot.json)
/*
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/orders/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "order-service",
          "Port": 80
        }
      ],
      "UpstreamPathTemplate": "/api/orders/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      },
      "RateLimitOptions": {
        "ClientWhitelist": [],
        "EnableRateLimiting": true,
        "Period": "1m",
        "PeriodTimespan": 60,
        "Limit": 100
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://api.mycompany.com"
  }
}
*/

// ===== ORDER MICROSERVICE =====
namespace OrderService
{
    // Message Bus avec MediatR
    public interface IEventBus
    {
        Task PublishAsync<T>(T @event) where T : class;
        Task SubscribeAsync<T>(Func<T, Task> handler) where T : class;
    }
    
    public class RabbitMQEventBus : IEventBus
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQEventBus> _logger;
        
        public RabbitMQEventBus(IConnectionFactory connectionFactory, ILogger<RabbitMQEventBus> logger)
        {
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _logger = logger;
        }
        
        public async Task PublishAsync<T>(T @event) where T : class
        {
            var eventName = typeof(T).Name;
            var routingKey = eventName.ToLowerInvariant();
            
            _channel.ExchangeDeclare(exchange: "events", type: ExchangeType.Topic);
            
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);
            
            _channel.BasicPublish(
                exchange: "events",
                routingKey: routingKey,
                basicProperties: null,
                body: body);
            
            _logger.LogInformation("Published event {EventName}: {Event}", eventName, message);
        }
        
        public async Task SubscribeAsync<T>(Func<T, Task> handler) where T : class
        {
            var eventName = typeof(T).Name;
            var queueName = $"{eventName}_queue";
            var routingKey = eventName.ToLowerInvariant();
            
            _channel.ExchangeDeclare(exchange: "events", type: ExchangeType.Topic);
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: queueName, exchange: "events", routingKey: routingKey);
            
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var @event = JsonSerializer.Deserialize<T>(message);
                    
                    await handler(@event);
                    
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event {EventName}", eventName);
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };
            
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }
    }
    
    // Events pour communication inter-services
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemEvent> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
    
    public class OrderItemEvent
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
    
    // Service Order
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IEventBus _eventBus;
        
        public OrderController(IOrderService orderService, IEventBus eventBus)
        {
            _orderService = orderService;
            _eventBus = eventBus;
        }
        
        [HttpPost]
        public async Task<ActionResult<int>> CreateOrder([FromBody] CreateOrderDto request)
        {
            var order = await _orderService.CreateOrderAsync(request);
            
            // Publier l'événement
            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new OrderItemEvent
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
            
            await _eventBus.PublishAsync(orderCreatedEvent);
            
            return Ok(order.Id);
        }
    }
}

// ===== INVENTORY MICROSERVICE =====
namespace InventoryService
{
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        
        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }
        
        [HttpPost("reserve")]
        public async Task<ActionResult<bool>> ReserveStock([FromBody] ReserveStockRequest request)
        {
            var success = await _inventoryService.ReserveStockAsync(request.ProductId, request.Quantity);
            return Ok(success);
        }
        
        [HttpPost("release")]
        public async Task<ActionResult> ReleaseStock([FromBody] ReleaseStockRequest request)
        {
            await _inventoryService.ReleaseStockAsync(request.ProductId, request.Quantity);
            return Ok();
        }
    }
    
    // Event Handler pour les commandes
    public class OrderCreatedEventHandler : BackgroundService
    {
        private readonly IEventBus _eventBus;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<OrderCreatedEventHandler> _logger;
        
        public OrderCreatedEventHandler(
            IEventBus eventBus,
            IInventoryService inventoryService,
            ILogger<OrderCreatedEventHandler> logger)
        {
            _eventBus = eventBus;
            _inventoryService = inventoryService;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _eventBus.SubscribeAsync<OrderCreatedEvent>(HandleOrderCreated);
        }
        
        private async Task HandleOrderCreated(OrderCreatedEvent orderCreated)
        {
            _logger.LogInformation("Processing order created event for order {OrderId}", orderCreated.OrderId);
            
            try
            {
                foreach (var item in orderCreated.Items)
                {
                    var reserved = await _inventoryService.ReserveStockAsync(item.ProductId, item.Quantity);
                    if (!reserved)
                    {
                        _logger.LogWarning("Could not reserve stock for product {ProductId}", item.ProductId);
                        
                        // Publier événement d'échec
                        await _eventBus.PublishAsync(new StockReservationFailedEvent
                        {
                            OrderId = orderCreated.OrderId,
                            ProductId = item.ProductId,
                            RequestedQuantity = item.Quantity
                        });
                        
                        return;
                    }
                }
                
                // Publier événement de succès
                await _eventBus.PublishAsync(new StockReservedEvent
                {
                    OrderId = orderCreated.OrderId,
                    Items = orderCreated.Items.Select(i => new ReservedStockItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order created event for order {OrderId}", orderCreated.OrderId);
            }
        }
    }
    
    public class StockReservationFailedEvent
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int RequestedQuantity { get; set; }
    }
    
    public class StockReservedEvent
    {
        public int OrderId { get; set; }
        public List<ReservedStockItem> Items { get; set; } = new();
    }
    
    public class ReservedStockItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

// ===== PAYMENT MICROSERVICE =====
namespace PaymentService
{
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IEventBus _eventBus;
        
        public PaymentController(IPaymentProcessor paymentProcessor, IEventBus eventBus)
        {
            _paymentProcessor = paymentProcessor;
            _eventBus = eventBus;
        }
        
        [HttpPost("process")]
        public async Task<ActionResult<PaymentResult>> ProcessPayment([FromBody] PaymentRequest request)
        {
            var result = await _paymentProcessor.ProcessPaymentAsync(request);
            
            if (result.Success)
            {
                await _eventBus.PublishAsync(new PaymentProcessedEvent
                {
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    TransactionId = result.TransactionId,
                    ProcessedAt = DateTime.UtcNow
                });
            }
            else
            {
                await _eventBus.PublishAsync(new PaymentFailedEvent
                {
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    Reason = result.ErrorMessage,
                    FailedAt = DateTime.UtcNow
                });
            }
            
            return Ok(result);
        }
    }
    
    public class PaymentProcessedEvent
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
    
    public class PaymentFailedEvent
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public DateTime FailedAt { get; set; }
    }
}

// ===== SAGA PATTERN pour orchestration =====
namespace OrderOrchestration
{
    public class OrderSaga
    {
        private readonly IEventBus _eventBus;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderSaga> _logger;
        
        public OrderSaga(IEventBus eventBus, IOrderRepository orderRepository, ILogger<OrderSaga> logger)
        {
            _eventBus = eventBus;
            _orderRepository = orderRepository;
            _logger = logger;
        }
        
        // État de la saga
        public class OrderSagaState
        {
            public int OrderId { get; set; }
            public bool StockReserved { get; set; }
            public bool PaymentProcessed { get; set; }
            public bool EmailSent { get; set; }
            public SagaStatus Status { get; set; }
            public List<string> CompensationActions { get; set; } = new();
        }
        
        public enum SagaStatus
        {
            Started,
            StockReservationPending,
            PaymentPending,
            NotificationPending,
            Completed,
            Failed,
            Compensating
        }
        
        // Gestionnaire d'événements de la saga
        public async Task HandleOrderCreated(OrderCreatedEvent orderCreated)
        {
            var sagaState = new OrderSagaState
            {
                OrderId = orderCreated.OrderId,
                Status = SagaStatus.Started
            };
            
            // Sauvegarder l'état de la saga
            await SaveSagaStateAsync(sagaState);
            
            _logger.LogInformation("Order saga started for order {OrderId}", orderCreated.OrderId);
        }
        
        public async Task HandleStockReserved(StockReservedEvent stockReserved)
        {
            var sagaState = await GetSagaStateAsync(stockReserved.OrderId);
            if (sagaState == null) return;
            
            sagaState.StockReserved = true;
            sagaState.Status = SagaStatus.PaymentPending;
            sagaState.CompensationActions.Add("ReleaseStock");
            
            await SaveSagaStateAsync(sagaState);
            
            // Déclencher le paiement
            await _eventBus.PublishAsync(new ProcessPaymentCommand
            {
                OrderId = stockReserved.OrderId,
                // ... autres propriétés
            });
            
            _logger.LogInformation("Stock reserved for order {OrderId}, proceeding to payment", stockReserved.OrderId);
        }
        
        public async Task HandlePaymentProcessed(PaymentProcessedEvent paymentProcessed)
        {
            var sagaState = await GetSagaStateAsync(paymentProcessed.OrderId);
            if (sagaState == null) return;
            
            sagaState.PaymentProcessed = true;
            sagaState.Status = SagaStatus.NotificationPending;
            sagaState.CompensationActions.Add("RefundPayment");
            
            await SaveSagaStateAsync(sagaState);
            
            // Déclencher la notification
            await _eventBus.PublishAsync(new SendOrderConfirmationCommand
            {
                OrderId = paymentProcessed.OrderId
            });
            
            _logger.LogInformation("Payment processed for order {OrderId}, sending notification", paymentProcessed.OrderId);
        }
        
        public async Task HandleStockReservationFailed(StockReservationFailedEvent stockFailed)
        {
            var sagaState = await GetSagaStateAsync(stockFailed.OrderId);
            if (sagaState == null) return;
            
            sagaState.Status = SagaStatus.Failed;
            await SaveSagaStateAsync(sagaState);
            
            // Pas de compensation nécessaire car aucune action n'a réussi
            await _eventBus.PublishAsync(new OrderFailedEvent
            {
                OrderId = stockFailed.OrderId,
                Reason = "Stock not available"
            });
            
            _logger.LogWarning("Order {OrderId} failed due to stock unavailability", stockFailed.OrderId);
        }
        
        public async Task HandlePaymentFailed(PaymentFailedEvent paymentFailed)
        {
            var sagaState = await GetSagaStateAsync(paymentFailed.OrderId);
            if (sagaState == null) return;
            
            sagaState.Status = SagaStatus.Compensating;
            await SaveSagaStateAsync(sagaState);
            
            // Compenser les actions précédentes
            await CompensateAsync(sagaState);
            
            _logger.LogWarning("Order {OrderId} failed due to payment failure, compensating", paymentFailed.OrderId);
        }
        
        private async Task CompensateAsync(OrderSagaState sagaState)
        {
            foreach (var action in sagaState.CompensationActions.AsEnumerable().Reverse())
            {
                switch (action)
                {
                    case "ReleaseStock":
                        await _eventBus.PublishAsync(new ReleaseStockCommand
                        {
                            OrderId = sagaState.OrderId
                        });
                        break;
                    
                    case "RefundPayment":
                        await _eventBus.PublishAsync(new RefundPaymentCommand
                        {
                            OrderId = sagaState.OrderId
                        });
                        break;
                }
            }
            
            sagaState.Status = SagaStatus.Failed;
            await SaveSagaStateAsync(sagaState);
        }
        
        private async Task<OrderSagaState> GetSagaStateAsync(int orderId)
        {
            // Implémentation de récupération d'état (Redis, DB, etc.)
            return new OrderSagaState();
        }
        
        private async Task SaveSagaStateAsync(OrderSagaState sagaState)
        {
            // Implémentation de sauvegarde d'état
        }
    }
    
    // Commandes pour la saga
    public class ProcessPaymentCommand
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
    }
    
    public class SendOrderConfirmationCommand
    {
        public int OrderId { get; set; }
    }
    
    public class ReleaseStockCommand
    {
        public int OrderId { get; set; }
    }
    
    public class RefundPaymentCommand
    {
        public int OrderId { get; set; }
    }
    
    public class OrderFailedEvent
    {
        public int OrderId { get; set; }
        public string Reason { get; set; }
    }
}
```

---

## **3. Resilience et Circuit Breaker**

### **Q: Comment implémenter la résilience dans les microservices ?**

```csharp
// Configuration Polly pour résilience
namespace Infrastructure.Resilience
{
    public static class ResiliencePolicies
    {
        // Circuit Breaker
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (exception, duration) =>
                    {
                        Console.WriteLine($"Circuit breaker opened for {duration}");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("Circuit breaker reset");
                    });
        }
        
        // Retry avec backoff exponentiel
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, delay, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount} after {delay} seconds");
                    });
        }
        
        // Timeout
        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
        }
        
        // Politique combinée
        public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
        {
            var retryPolicy = GetRetryPolicy();
            var circuitBreakerPolicy = GetCircuitBreakerPolicy();
            var timeoutPolicy = GetTimeoutPolicy();
            
            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        }
    }
    
    // Service client avec résilience
    public interface IOrderServiceClient
    {
        Task<Order> GetOrderAsync(int orderId);
        Task<int> CreateOrderAsync(CreateOrderDto order);
    }
    
    public class ResilientOrderServiceClient : IOrderServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy<HttpResponseMessage> _resilencePolicy;
        private readonly ILogger<ResilientOrderServiceClient> _logger;
        
        public ResilientOrderServiceClient(
            HttpClient httpClient,
            ILogger<ResilientOrderServiceClient> logger)
        {
            _httpClient = httpClient;
            _resilencePolicy = ResiliencePolicies.GetCombinedPolicy();
            _logger = logger;
        }
        
        public async Task<Order> GetOrderAsync(int orderId)
        {
            try
            {
                var response = await _resilencePolicy.ExecuteAsync(async () =>
                {
                    return await _httpClient.GetAsync($"/api/orders/{orderId}");
                });
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Order>(content);
                }
                
                return null;
            }
            catch (CircuitBreakerOpenException)
            {
                _logger.LogWarning("Circuit breaker is open for Order Service");
                return GetOrderFromCache(orderId); // Fallback
            }
            catch (TimeoutRejectedException)
            {
                _logger.LogWarning("Timeout calling Order Service for order {OrderId}", orderId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Order Service for order {OrderId}", orderId);
                throw;
            }
        }
        
        private Order GetOrderFromCache(int orderId)
        {
            // Implémentation du cache de fallback
            return new Order { Id = orderId, Status = "Unknown" };
        }
        
        public async Task<int> CreateOrderAsync(CreateOrderDto order)
        {
            var json = JsonSerializer.Serialize(order);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _resilencePolicy.ExecuteAsync(async () =>
            {
                return await _httpClient.PostAsync("/api/orders", content);
            });
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<int>(responseContent);
            }
            
            throw new HttpRequestException($"Failed to create order: {response.StatusCode}");
        }
    }
    
    // Health Checks
    public class OrderServiceHealthCheck : IHealthCheck
    {
        private readonly IOrderServiceClient _orderServiceClient;
        
        public OrderServiceHealthCheck(IOrderServiceClient orderServiceClient)
        {
            _orderServiceClient = orderServiceClient;
        }
        
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Test simple : récupérer un ordre de test
                var testOrder = await _orderServiceClient.GetOrderAsync(-1); // ID qui n'existe pas
                
                return HealthCheckResult.Healthy("Order Service is responsive");
            }
            catch (CircuitBreakerOpenException)
            {
                return HealthCheckResult.Degraded("Order Service circuit breaker is open");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Order Service is not responsive", ex);
            }
        }
    }
    
    // Configuration dans Startup
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // HTTP Clients avec Polly
            services.AddHttpClient<IOrderServiceClient, ResilientOrderServiceClient>(client =>
            {
                client.BaseAddress = new Uri("https://order-service-url");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(ResiliencePolicies.GetCombinedPolicy());
            
            // Health Checks
            services.AddHealthChecks()
                .AddCheck<OrderServiceHealthCheck>("order-service")
                .AddUrlGroup(new Uri("https://inventory-service/health"), "inventory-service")
                .AddSqlServer(connectionString: "your-connection-string", name: "database");
            
            // Logging et métriques
            services.AddApplicationInsightsTelemetry();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Health Check endpoint
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }
    }
}

// Monitoring et observabilité
namespace Infrastructure.Observability
{
    public class DistributedTracingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DistributedTracingMiddleware> _logger;
        
        public DistributedTracingMiddleware(RequestDelegate next, ILogger<DistributedTracingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            using var activity = Activity.Current?.Source.StartActivity("HTTP Request");
            
            // Ajouter des tags pour le tracing
            activity?.SetTag("http.method", context.Request.Method);
            activity?.SetTag("http.url", context.Request.Path);
            activity?.SetTag("user.id", context.User.Identity?.Name);
            
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers.Add("X-Correlation-ID", correlationId);
            }
            
            context.Response.Headers.Add("X-Correlation-ID", correlationId);
            activity?.SetTag("correlation.id", correlationId);
            
            try
            {
                await _next(context);
                
                activity?.SetTag("http.status_code", context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(ex, "Request failed with correlation ID {CorrelationId}", correlationId);
                throw;
            }
        }
    }
}
```

Cette documentation complète couvre l'architecture Clean Architecture, les microservices, la communication inter-services, la résilience et l'observabilité ! 🏗️🚀
