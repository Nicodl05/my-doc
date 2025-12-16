# **Multithreading et Concurrence en C# - Guide Technique**

> **🔗 Références :**
> - [Délégués et Async](./Délégués%20et%20Async.md) - async/await et Task
> - [Collections](./boucles%20et%20collections.md) - Collections thread-safe
> - [Complexité](./Complexité%20Algorithmique%20et%20Performance.md) - Performance et optimisation
> - [Questions Threading](./Questions%20Entretiens%20-%20Threading%20et%20Concurrence.md) - Questions d'entretien

---

# **MULTITHREADING ET CONCURRENCE**

## **1. Fondamentaux des Threads**

### **Q: Différence entre Thread et Task ?**

```csharp
public class ThreadVsTaskDemo
{
    // Thread - contrôle de bas niveau
    public static void ThreadExample()
    {
        var thread = new Thread(() =>
        {
            Console.WriteLine($"Thread ID: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(1000);
            Console.WriteLine("Thread terminé");
        })
        {
            IsBackground = true,
            Name = "MonThread"
        };
        
        thread.Start();
        thread.Join(); // Attendre la fin
    }
    
    // Task - abstraction de haut niveau
    public static async Task TaskExample()
    {
        var task = Task.Run(() =>
        {
            Console.WriteLine($"Task Thread ID: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(1000);
            return "Task terminée";
        });
        
        string result = await task;
        Console.WriteLine(result);
    }
    
    // Thread Pool
    public static void ThreadPoolExample()
    {
        ThreadPool.QueueUserWorkItem(state =>
        {
            Console.WriteLine($"ThreadPool Thread ID: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"IsThreadPoolThread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(1000);
        });
        
        // Configurer le ThreadPool
        ThreadPool.SetMinThreads(4, 4);
        ThreadPool.SetMaxThreads(100, 100);
        
        ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
        Console.WriteLine($"Available: {workerThreads} worker, {completionPortThreads} completion");
    }
}
```

---

## **2. Synchronisation - Locks et Monitors**

### **Q: Implémentez différents mécanismes de synchronisation ?**

```csharp
public class SynchronizationDemo
{
    private readonly object _lockObject = new object();
    private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
    private readonly Mutex _mutex = new Mutex();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(3, 3); // Max 3 threads
    private readonly ManualResetEventSlim _manualResetEvent = new ManualResetEventSlim(false);
    private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
    
    private int _counter = 0;
    private readonly List<string> _data = new List<string>();
    
    // Lock basique - exclusion mutuelle
    public void LockExample()
    {
        lock (_lockObject)
        {
            _counter++;
            Console.WriteLine($"Counter: {_counter}, Thread: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(100);
        }
    }
    
    // Monitor avec timeout
    public bool MonitorExample(int timeoutMs = 1000)
    {
        bool lockTaken = false;
        
        try
        {
            Monitor.TryEnter(_lockObject, timeoutMs, ref lockTaken);
            
            if (lockTaken)
            {
                _counter++;
                Console.WriteLine($"Monitor acquired: {_counter}");
                
                // Wait/Pulse exemple
                if (_counter % 5 == 0)
                {
                    Console.WriteLine("Waiting for signal...");
                    Monitor.Wait(_lockObject, 2000);
                }
                
                return true;
            }
            else
            {
                Console.WriteLine("Failed to acquire lock within timeout");
                return false;
            }
        }
        finally
        {
            if (lockTaken)
                Monitor.Exit(_lockObject);
        }
    }
    
    // Signal pour Monitor.Wait
    public void SignalMonitor()
    {
        lock (_lockObject)
        {
            Console.WriteLine("Sending signal...");
            Monitor.PulseAll(_lockObject);
        }
    }
    
    // ReaderWriterLock pour lectures/écritures
    public string ReadData(int index)
    {
        _rwLock.EnterReadLock();
        try
        {
            Thread.Sleep(100); // Simulation lecture
            return index < _data.Count ? _data[index] : null;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }
    
    public void WriteData(string value)
    {
        _rwLock.EnterWriteLock();
        try
        {
            _data.Add(value);
            Console.WriteLine($"Added: {value}, Total: {_data.Count}");
            Thread.Sleep(200); // Simulation écriture
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }
    
    // Semaphore pour limiter l'accès concurrent
    public async Task SemaphoreExample(string taskName)
    {
        Console.WriteLine($"{taskName} waiting for semaphore...");
        
        await _semaphore.WaitAsync();
        try
        {
            Console.WriteLine($"{taskName} acquired semaphore");
            await Task.Delay(2000); // Simulation travail
            Console.WriteLine($"{taskName} releasing semaphore");
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    // ManualResetEvent - signal manuel
    public void ManualResetExample()
    {
        Task.Run(() =>
        {
            Console.WriteLine("Worker waiting for signal...");
            _manualResetEvent.Wait();
            Console.WriteLine("Worker received signal and continuing...");
        });
        
        Thread.Sleep(2000);
        Console.WriteLine("Signaling manual reset event...");
        _manualResetEvent.Set();
    }
    
    // AutoResetEvent - signal automatique
    public void AutoResetExample()
    {
        for (int i = 0; i < 3; i++)
        {
            int workerId = i;
            Task.Run(() =>
            {
                Console.WriteLine($"Worker {workerId} waiting for auto signal...");
                _autoResetEvent.WaitOne();
                Console.WriteLine($"Worker {workerId} received signal!");
            });
        }
        
        // Envoyer des signaux un par un
        for (int i = 0; i < 3; i++)
        {
            Thread.Sleep(1000);
            Console.WriteLine($"Sending auto signal {i + 1}...");
            _autoResetEvent.Set();
        }
    }
    
    public void Dispose()
    {
        _rwLock?.Dispose();
        _mutex?.Dispose();
        _semaphore?.Dispose();
        _manualResetEvent?.Dispose();
        _autoResetEvent?.Dispose();
    }
}
```

---

## **3. Collections Thread-Safe**

### **Q: Utilisez les collections concurrentes de .NET ?**

```csharp
public class ThreadSafeCollectionsDemo
{
    private readonly ConcurrentDictionary<string, int> _concurrentDict = new();
    private readonly ConcurrentQueue<string> _concurrentQueue = new();
    private readonly ConcurrentStack<string> _concurrentStack = new();
    private readonly ConcurrentBag<string> _concurrentBag = new();
    private readonly BlockingCollection<string> _blockingCollection = new();
    
    // ConcurrentDictionary - thread-safe dictionary
    public void ConcurrentDictionaryExample()
    {
        var tasks = new List<Task>();
        
        // Producteurs
        for (int i = 0; i < 5; i++)
        {
            int producerId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 10; j++)
                {
                    string key = $"key_{producerId}_{j}";
                    
                    // AddOrUpdate - atomique
                    _concurrentDict.AddOrUpdate(key, 1, (k, v) => v + 1);
                    
                    // TryAdd - tentative d'ajout
                    if (_concurrentDict.TryAdd($"try_{key}", j))
                    {
                        Console.WriteLine($"Added try_{key}");
                    }
                    
                    Thread.Sleep(10);
                }
            }));
        }
        
        // Consommateurs
        for (int i = 0; i < 3; i++)
        {
            int consumerId = i;
            tasks.Add(Task.Run(() =>
            {
                Thread.Sleep(100); // Laisser les producteurs commencer
                
                foreach (var kvp in _concurrentDict)
                {
                    if (_concurrentDict.TryRemove(kvp.Key, out int value))
                    {
                        Console.WriteLine($"Consumer {consumerId} removed {kvp.Key}={value}");
                    }
                    Thread.Sleep(50);
                }
            }));
        }
        
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"Remaining items: {_concurrentDict.Count}");
    }
    
    // ConcurrentQueue - FIFO thread-safe
    public void ConcurrentQueueExample()
    {
        var producerTask = Task.Run(() =>
        {
            for (int i = 0; i < 20; i++)
            {
                string item = $"item_{i}";
                _concurrentQueue.Enqueue(item);
                Console.WriteLine($"Enqueued: {item}");
                Thread.Sleep(100);
            }
        });
        
        var consumerTasks = new List<Task>();
        
        for (int c = 0; c < 3; c++)
        {
            int consumerId = c;
            consumerTasks.Add(Task.Run(() =>
            {
                while (!producerTask.IsCompleted || !_concurrentQueue.IsEmpty)
                {
                    if (_concurrentQueue.TryDequeue(out string item))
                    {
                        Console.WriteLine($"Consumer {consumerId} dequeued: {item}");
                        Thread.Sleep(150);
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
            }));
        }
        
        Task.WaitAll(new[] { producerTask }.Concat(consumerTasks).ToArray());
    }
    
    // BlockingCollection - producteur/consommateur avec blocage
    public void BlockingCollectionExample()
    {
        var producer = Task.Run(() =>
        {
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    string item = $"work_item_{i}";
                    _blockingCollection.Add(item);
                    Console.WriteLine($"Produced: {item}");
                    Thread.Sleep(200);
                }
            }
            finally
            {
                _blockingCollection.CompleteAdding();
                Console.WriteLine("Producer completed");
            }
        });
        
        var consumers = new List<Task>();
        
        for (int c = 0; c < 3; c++)
        {
            int consumerId = c;
            consumers.Add(Task.Run(() =>
            {
                try
                {
                    foreach (string item in _blockingCollection.GetConsumingEnumerable())
                    {
                        Console.WriteLine($"Consumer {consumerId} processing: {item}");
                        Thread.Sleep(300); // Simulation travail
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine($"Consumer {consumerId} finished");
                }
            }));
        }
        
        Task.WaitAll(new[] { producer }.Concat(consumers).ToArray());
    }
    
    // Partitioner pour parallélisation optimisée
    public void PartitionerExample()
    {
        var data = Enumerable.Range(0, 1000).ToArray();
        
        // Partitioning par défaut
        Parallel.ForEach(data, item =>
        {
            ProcessItem(item);
        });
        
        // Partitioning personnalisé par chunks
        var partitioner = Partitioner.Create(data, true);
        
        Parallel.ForEach(partitioner, item =>
        {
            ProcessItem(item);
        });
        
        // Range partitioning pour de gros tableaux
        var rangePartitioner = Partitioner.Create(0, data.Length);
        
        Parallel.ForEach(rangePartitioner, range =>
        {
            for (int i = range.Item1; i < range.Item2; i++)
            {
                ProcessItem(data[i]);
            }
        });
    }
    
    private void ProcessItem(int item)
    {
        // Simulation de traitement
        Thread.Sleep(1);
    }
    
    public void Dispose()
    {
        _blockingCollection?.Dispose();
    }
}
```

---

## **4. Patterns de Concurrence**

### **Q: Implémentez des patterns de concurrence avancés ?**

```csharp
// Producer-Consumer avec pipeline
public class ProducerConsumerPipeline<T>
{
    private readonly Channel<T> _channel;
    private readonly ChannelWriter<T> _writer;
    private readonly ChannelReader<T> _reader;
    
    public ProducerConsumerPipeline(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };
        
        _channel = Channel.CreateBounded<T>(options);
        _writer = _channel.Writer;
        _reader = _channel.Reader;
    }
    
    public async Task ProduceAsync(IEnumerable<T> items)
    {
        try
        {
            foreach (var item in items)
            {
                await _writer.WriteAsync(item);
                Console.WriteLine($"Produced: {item}");
            }
        }
        finally
        {
            _writer.Complete();
        }
    }
    
    public async Task ConsumeAsync(Func<T, Task> processor, CancellationToken cancellationToken = default)
    {
        await foreach (var item in _reader.ReadAllAsync(cancellationToken))
        {
            await processor(item);
        }
    }
    
    public async Task<List<TResult>> ProcessPipelineAsync<TResult>(
        IEnumerable<T> items,
        Func<T, Task<TResult>> processor,
        int maxConcurrency = Environment.ProcessorCount)
    {
        var results = new ConcurrentBag<TResult>();
        var semaphore = new SemaphoreSlim(maxConcurrency);
        
        var producerTask = ProduceAsync(items);
        
        var consumerTasks = Enumerable.Range(0, maxConcurrency)
            .Select(async _ =>
            {
                await foreach (var item in _reader.ReadAllAsync())
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var result = await processor(item);
                        results.Add(result);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }
            });
        
        await Task.WhenAll(new[] { producerTask }.Concat(consumerTasks));
        return results.ToList();
    }
}

// Async Lock
public class AsyncLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public async Task<IDisposable> LockAsync()
    {
        await _semaphore.WaitAsync();
        return new LockReleaser(_semaphore);
    }
    
    private class LockReleaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed = false;
        
        public LockReleaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _semaphore.Release();
                _disposed = true;
            }
        }
    }
}

// Cache thread-safe avec expiration
public class ThreadSafeCache<TKey, TValue>
{
    private readonly ConcurrentDictionary<TKey, CacheItem> _cache = new();
    private readonly Timer _cleanupTimer;
    private readonly TimeSpan _defaultExpiry;
    
    public ThreadSafeCache(TimeSpan defaultExpiry)
    {
        _defaultExpiry = defaultExpiry;
        _cleanupTimer = new Timer(Cleanup, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }
    
    public async Task<TValue> GetOrAddAsync<TState>(
        TKey key,
        Func<TKey, TState, Task<TValue>> valueFactory,
        TState state,
        TimeSpan? expiry = null)
    {
        var expiryTime = DateTime.UtcNow.Add(expiry ?? _defaultExpiry);
        
        var item = await _cache.AddOrUpdate(
            key,
            async k => new CacheItem(await valueFactory(k, state), expiryTime),
            async (k, existing) =>
            {
                if (existing.IsExpired)
                {
                    return new CacheItem(await valueFactory(k, state), expiryTime);
                }
                return existing;
            });
        
        return item.Value;
    }
    
    public bool TryGet(TKey key, out TValue value)
    {
        if (_cache.TryGetValue(key, out var item) && !item.IsExpired)
        {
            value = item.Value;
            return true;
        }
        
        value = default;
        return false;
    }
    
    public void Remove(TKey key)
    {
        _cache.TryRemove(key, out _);
    }
    
    private void Cleanup(object state)
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }
    }
    
    private class CacheItem
    {
        public TValue Value { get; }
        public DateTime ExpiryTime { get; }
        public bool IsExpired => DateTime.UtcNow > ExpiryTime;
        
        public CacheItem(TValue value, DateTime expiryTime)
        {
            Value = value;
            ExpiryTime = expiryTime;
        }
    }
    
    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

// Worker Pool avec priorités
public class PriorityWorkerPool : IDisposable
{
    private readonly PriorityQueue<WorkItem, int> _workQueue = new();
    private readonly SemaphoreSlim _workAvailable = new(0);
    private readonly List<Task> _workers;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly object _queueLock = new object();
    
    public PriorityWorkerPool(int workerCount = Environment.ProcessorCount)
    {
        _workers = new List<Task>();
        
        for (int i = 0; i < workerCount; i++)
        {
            _workers.Add(Task.Run(WorkerLoop));
        }
    }
    
    public Task EnqueueAsync(Func<CancellationToken, Task> work, int priority = 0)
    {
        var tcs = new TaskCompletionSource<bool>();
        var workItem = new WorkItem(work, tcs, priority);
        
        lock (_queueLock)
        {
            _workQueue.Enqueue(workItem, priority);
        }
        
        _workAvailable.Release();
        return tcs.Task;
    }
    
    public Task<T> EnqueueAsync<T>(Func<CancellationToken, Task<T>> work, int priority = 0)
    {
        var tcs = new TaskCompletionSource<T>();
        var workItem = new WorkItem(async ct =>
        {
            try
            {
                var result = await work(ct);
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null, priority);
        
        lock (_queueLock)
        {
            _workQueue.Enqueue(workItem, priority);
        }
        
        _workAvailable.Release();
        return tcs.Task;
    }
    
    private async Task WorkerLoop()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await _workAvailable.WaitAsync(_cancellationTokenSource.Token);
                
                WorkItem workItem;
                lock (_queueLock)
                {
                    if (!_workQueue.TryDequeue(out workItem, out _))
                        continue;
                }
                
                try
                {
                    await workItem.Work(_cancellationTokenSource.Token);
                    workItem.TaskCompletionSource?.SetResult(true);
                }
                catch (Exception ex)
                {
                    workItem.TaskCompletionSource?.SetException(ex);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
    
    private record WorkItem(
        Func<CancellationToken, Task> Work,
        TaskCompletionSource<bool> TaskCompletionSource,
        int Priority);
    
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        Task.WaitAll(_workers.ToArray(), TimeSpan.FromSeconds(5));
        _cancellationTokenSource.Dispose();
        _workAvailable.Dispose();
    }
}
```

---

## **5. Gestion des Deadlocks et Race Conditions**

### **Q: Comment détecter et éviter les deadlocks ?**

```csharp
public class DeadlockDemo
{
    private readonly object _lock1 = new object();
    private readonly object _lock2 = new object();
    
    // DANGEREUX - Peut causer un deadlock
    public void DeadlockRisk()
    {
        var task1 = Task.Run(() =>
        {
            lock (_lock1)
            {
                Console.WriteLine("Task1 acquired lock1");
                Thread.Sleep(100);
                
                lock (_lock2)
                {
                    Console.WriteLine("Task1 acquired lock2");
                }
            }
        });
        
        var task2 = Task.Run(() =>
        {
            lock (_lock2)
            {
                Console.WriteLine("Task2 acquired lock2");
                Thread.Sleep(100);
                
                lock (_lock1)
                {
                    Console.WriteLine("Task2 acquired lock1");
                }
            }
        });
        
        Task.WaitAll(task1, task2);
    }
    
    // SOLUTION 1: Ordre cohérent des locks
    public void DeadlockAvoidanceByOrdering()
    {
        var task1 = Task.Run(() =>
        {
            // Toujours lock1 puis lock2
            lock (_lock1)
            {
                Console.WriteLine("Task1 acquired lock1");
                Thread.Sleep(100);
                
                lock (_lock2)
                {
                    Console.WriteLine("Task1 acquired lock2");
                }
            }
        });
        
        var task2 = Task.Run(() =>
        {
            // Même ordre: lock1 puis lock2
            lock (_lock1)
            {
                Console.WriteLine("Task2 acquired lock1");
                Thread.Sleep(100);
                
                lock (_lock2)
                {
                    Console.WriteLine("Task2 acquired lock2");
                }
            }
        });
        
        Task.WaitAll(task1, task2);
    }
    
    // SOLUTION 2: Timeout avec Monitor
    public void DeadlockAvoidanceWithTimeout()
    {
        var task1 = Task.Run(() => TryAcquireBothLocks("Task1", _lock1, _lock2));
        var task2 = Task.Run(() => TryAcquireBothLocks("Task2", _lock2, _lock1));
        
        Task.WaitAll(task1, task2);
    }
    
    private void TryAcquireBothLocks(string taskName, object firstLock, object secondLock)
    {
        bool firstLockTaken = false;
        bool secondLockTaken = false;
        
        try
        {
            Monitor.TryEnter(firstLock, 1000, ref firstLockTaken);
            if (!firstLockTaken)
            {
                Console.WriteLine($"{taskName} failed to acquire first lock");
                return;
            }
            
            Console.WriteLine($"{taskName} acquired first lock");
            Thread.Sleep(100);
            
            Monitor.TryEnter(secondLock, 1000, ref secondLockTaken);
            if (!secondLockTaken)
            {
                Console.WriteLine($"{taskName} failed to acquire second lock");
                return;
            }
            
            Console.WriteLine($"{taskName} acquired both locks - work completed");
        }
        finally
        {
            if (secondLockTaken) Monitor.Exit(secondLock);
            if (firstLockTaken) Monitor.Exit(firstLock);
        }
    }
}

// Détecteur de deadlock simple
public class DeadlockDetector
{
    private static readonly ConcurrentDictionary<int, HashSet<object>> ThreadLocks = new();
    
    public static void RecordLockAcquisition(object lockObject)
    {
        int threadId = Thread.CurrentThread.ManagedThreadId;
        ThreadLocks.AddOrUpdate(threadId,
            new HashSet<object> { lockObject },
            (key, existing) =>
            {
                existing.Add(lockObject);
                return existing;
            });
    }
    
    public static void RecordLockRelease(object lockObject)
    {
        int threadId = Thread.CurrentThread.ManagedThreadId;
        if (ThreadLocks.TryGetValue(threadId, out var locks))
        {
            locks.Remove(lockObject);
            if (locks.Count == 0)
            {
                ThreadLocks.TryRemove(threadId, out _);
            }
        }
    }
    
    public static bool DetectPotentialDeadlock()
    {
        var allThreads = ThreadLocks.ToArray();
        
        // Simple heuristique: si plusieurs threads attendent des locks
        // et ont des intersections dans leurs ensembles de locks
        foreach (var thread1 in allThreads)
        {
            foreach (var thread2 in allThreads)
            {
                if (thread1.Key != thread2.Key &&
                    thread1.Value.Intersect(thread2.Value).Any())
                {
                    Console.WriteLine($"Potential deadlock between threads {thread1.Key} and {thread2.Key}");
                    return true;
                }
            }
        }
        
        return false;
    }
}

// Race condition et correction
public class RaceConditionDemo
{
    private int _counter = 0;
    private volatile bool _flag = false;
    private readonly object _lock = new object();
    
    // PROBLÈME: Race condition
    public void UnsafeIncrement()
    {
        for (int i = 0; i < 10000; i++)
        {
            _counter++; // Non atomique !
        }
    }
    
    // SOLUTION 1: Lock
    public void SafeIncrementWithLock()
    {
        for (int i = 0; i < 10000; i++)
        {
            lock (_lock)
            {
                _counter++;
            }
        }
    }
    
    // SOLUTION 2: Interlocked
    public void SafeIncrementWithInterlocked()
    {
        for (int i = 0; i < 10000; i++)
        {
            Interlocked.Increment(ref _counter);
        }
    }
    
    // SOLUTION 3: Atomic operations
    public void AtomicOperationsDemo()
    {
        int value = 100;
        
        // Compare and swap
        int original = Interlocked.CompareExchange(ref value, 200, 100);
        Console.WriteLine($"Original: {original}, New: {value}");
        
        // Add
        int result = Interlocked.Add(ref value, 50);
        Console.WriteLine($"After add: {result}");
        
        // Exchange
        int exchanged = Interlocked.Exchange(ref value, 999);
        Console.WriteLine($"Exchanged: {exchanged}, Current: {value}");
    }
    
    // Memory barriers avec volatile
    public void VolatileDemo()
    {
        Task.Run(() =>
        {
            while (!_flag)
            {
                Thread.SpinWait(100);
            }
            Console.WriteLine("Flag detected!");
        });
        
        Thread.Sleep(1000);
        _flag = true; // Volatile garantit la visibilité
    }
    
    public void TestRaceCondition()
    {
        _counter = 0;
        
        var tasks = new Task[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(UnsafeIncrement);
        }
        
        Task.WaitAll(tasks);
        Console.WriteLine($"Unsafe result: {_counter} (should be 100000)");
        
        _counter = 0;
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(SafeIncrementWithInterlocked);
        }
        
        Task.WaitAll(tasks);
        Console.WriteLine($"Safe result: {_counter}");
    }
}
```

Cette documentation complète sur le multithreading vous prépare à tous les aspects de la concurrence en C# ! 🚀
