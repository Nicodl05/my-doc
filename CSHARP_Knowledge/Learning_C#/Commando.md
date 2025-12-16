## 1. Multithreading

**Q1. Qu’est-ce que le multithreading ?**
Le **multithreading** est la capacité d’une application à exécuter plusieurs **threads** (fils d’exécution) en parallèle *au sein d’un même processus*.

* **Pourquoi ?**

  * Exploiter au maximum un **Central Processing Unit** (CPU, unité centrale de traitement) multicœur.
  * Distinguer travail CPU-bound (calcul intensif) et travail I/O-bound (**Input/Output**, opérations bloquantes comme accès disque ou réseau).
* **Modèle mémoire .NET**

  * Tous les threads partagent la **heap** (tas mémoire) et les données statiques.
  * Chaque thread possède sa propre pile d’appels (stack) et son contexte (registre instruction pointer, registre base pointer…).
* **Gestion des threads**

  * **ThreadPool** (.NET Thread Pool) : pool de threads géré par le runtime pour réutiliser les threads et éviter l’explosion de threads.
  * **Priorités** : on peut définir `Thread.Priority`, mais attention à la starvation (threads à faible priorité jamais servis).
* **Pièges**

  * **Overhead de contexte** : chaque thread coûte \~1 Mo par défaut pour la stack.
  * **Starvation** : un thread de basse priorité peut ne jamais s’exécuter.
  * **Deadlocks** (interblocage), **livelocks** (verrouillage actif sans progrès), **race conditions** (conditions de concurrence).

```csharp
// Exemple de création manuelle de threads (pas recommandé en production)
var t1 = new Thread(() => Console.WriteLine("Travail 1"));
var t2 = new Thread(() => Console.WriteLine("Travail 2"));
t1.Start(); t2.Start();
t1.Join(); t2.Join();
```

---

## 2. Tâches vs Processus

**Q2. Quelle différence entre une tâche (`Task`) et un processus ?**

| Aspect           | Processus                                          | Task (.NET Task Parallel Library)                         |
| ---------------- | -------------------------------------------------- | --------------------------------------------------------- |
| Isolation        | Espace mémoire privé (adressage virtuel, handles)  | Partage la heap et le pool de threads du processus parent |
| Coût de création | Élevé (chargement DLL, table handle, etc.)         | Léger – planifié par le **ThreadPool**                    |
| Communication    | IPC (Inter-Process Communication) : pipes, sockets | Mémoire partagée + `async`/`await`                        |
| Orchestration    | Docker, Kubernetes, processus système              | Fine-grained parallelisme à l’intérieur d’un service      |

```csharp
// Task pour lancer un calcul asynchrone
async Task<int> ComputeAsync()
{
    await Task.Delay(1000); // non bloquant
    return 42;
}
```

---

## 3. Concurrence avec `Task`

**Q3. Comment gérer les accès concurrents avec `Task` en C# ?**

* **Verrous**

  * `lock(obj)` → traduit en `Monitor.Enter(obj)` / `Monitor.Exit(obj)`.
  * `SemaphoreSlim` : limite le nombre de threads simultanés.
  * `ReaderWriterLockSlim` : optimisé pour plusieurs lecteurs / un seul écrivain.
* **Opérations atomiques** via la classe `Interlocked` (CAS, *Compare-And-Swap*) :

  ```csharp
  Interlocked.Increment(ref counter);
  Interlocked.CompareExchange(ref location, newValue, comparand);
  ```
* **Collections thread-safe**

  * `ConcurrentDictionary<TKey,TValue>`
  * `BlockingCollection<T>` (utile pour modèles producteur-consommateur)
  * `ConcurrentBag<T>`
* **Structures immuables**

  * `ImmutableList<T>`, `ImmutableDictionary<TKey,TValue>` (lock-free, safe for multi-thread)
* **Annulation**

  * Toujours accepter un `CancellationToken` et vérifier `ct.ThrowIfCancellationRequested()`.

**Q4. Comment synchroniser deux tâches en C# ?**

* `await Task.WhenAll(t1, t2);` : attendre la fin des deux.
* `await Task.WhenAny(t1, t2);` : dès qu’une des deux se termine.
* Signaux manuels :

  * `ManualResetEventSlim` / `AutoResetEvent`
  * `CountdownEvent` (compteur décrémental)
  * `Barrier` (synchronisation multi-phase)
* **TaskCompletionSource<TResult>** pour exposer explicitement un signal :

```csharp
var tcs = new TaskCompletionSource<bool>();
// Thread A
Task.Run(() => { /* travail */ tcs.SetResult(true); });
// Thread B
await tcs.Task; // attend le SetResult()
```

---

## 4. Gestion des Ressources

**Q5. Le mot-clé `using` peut-il être utilisé avec n’importe quelle classe ?**
Non. Il ne fonctionne qu’avec les classes implémentant `IDisposable`. Depuis C# 8, on peut écrire :

```csharp
using var reader = new StreamReader(...);
// Dispose() automatique à la fin de la portée
```

**Q6. Comment garantir la libération des ressources non-mangées ?**
Implémenter le *pattern* `IDisposable` et un finaliseur (*destructeur*) :

```csharp
public class MyResource : IDisposable
{
    private IntPtr _handle;     // ressource non-magée
    private bool _disposed;
    public MyResource() { /* OpenHandle */ }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // GC = Garbage Collector
    }
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            // libère ressources managées (IDisposable)
        }
        // libère ressources non-mangées
        NativeMethods.CloseHandle(_handle);
        _disposed = true;
    }
    ~MyResource() => Dispose(false);
}
```

---

## 5. Paramètres de Méthode & Types de Données

**Q7. Différence `ref` vs `out`**

* `ref` (référence) : la variable doit être initialisée avant l’appel et peut être lue/écrite.
* `out` (sortie) : la variable n’a pas besoin d’être initialisée avant, mais la méthode **doit** l’affecter avant de retourner.

```csharp
void FooRef(ref int x)  { x += 1; }
void FooOut(out int x) { x = 42; }
```

**Q8. Valeur par défaut d’un `DateTime` & vérification**

* `default(DateTime)` = `DateTime.MinValue` (01/01/0001 00:00:00).
* **Bonnes pratiques** : utiliser `DateTime?` (nullable) pour représenter l’absence de date.

```csharp
if (myDateTime == default(DateTime)) { /* non initialisé */ }
```

**Q9. Qu’est-ce qu’une variable primitive ?**

* **Types valeur** simples du Common Language Runtime (CLR) :

  * `Int32` (`int`), `Double`, `Boolean`, `Char`
* Stockés **directement** sur la stack (pile d’appels) lorsque locaux, unboxing quand transformés en `object`.

---

## 6. Boucles & Collections

**Q10. Prérequis pour `foreach`**
Le type doit implémenter `IEnumerable` ou `IEnumerable<T>` (fournit `GetEnumerator()`, `MoveNext()`, `Current`).

**Q11. `List<T>` vs `Dictionary<TKey,TValue>`**

* `List<T>` : tableau dynamique contigu en mémoire, accès par index O(1), parcours séquentiel O(n).
* `Dictionary` : table de hachage, accès clé→valeur O(1) en moyenne, dépend du **load factor** (\~0,72).

**Q12. Types de clés & gestion des collisions**

* Clés : tout type surchargé `GetHashCode()` + `Equals()`.
* Collisions :

  * Bucket chain (chaîne liée) ou
  * Depuis .NET 6, *treeify* (transformation en arbre rouge-noir) pour bucket > 8 éléments.

**Q13. Assurer l’unicité des clés objets complexes**

* Implémenter `IEquatable<T>` pour éviter la réflexion dans `Equals()`.
* Fournir un `IEqualityComparer<T>` personnalisé :

```csharp
var dict = new Dictionary<MyObj,int>(new MyObjComparer());
```

**Q14. `LinkedList<T>` & cas d’usage**

* Liste doublement chaînée ; `LinkedListNode<T>` permet insertion/suppression O(1) si le nœud est connu.
* Exemple : implémenter un cache LRU (*Least Recently Used*).

```csharp
var lru = new LinkedList<int>();
// MoveToFront, RemoveLast pour eviction
```

**Q15. Stack vs Heap**

* **Stack** (pile d’appels) : LIFO (Last-In First-Out), allocation/désallocation automatique, rapide.
* **Heap** (tas mémoire) : allocation dynamique pour objets références, gérée par le **Garbage Collector** (GC), allocation plus lente, désallocation non déterministe, fragmentation potentielle, générations GC (0/1/2) et **Large Object Heap** (LOH) pour objets > 85 000 octets.

**Q16. Collections .NET courantes & scénarios**

| Collection                                              | Cas d’usage typique                                                   |
| ------------------------------------------------------- | --------------------------------------------------------------------- |
| `List<T>`                                               | Tableau dynamique, accès indexé                                       |
| `Dictionary<TKey,TValue>`                               | Répertoires clé→valeur, recherches fréquentes                         |
| `HashSet<T>`                                            | Ensemble unique, membership O(1)                                      |
| `Queue<T>`                                              | FIFO (First-In First-Out) pour producteur-consommateur                |
| `Stack<T>`                                              | LIFO (Last-In First-Out) pour undo/redo, parcours en profondeur (DFS) |
| `LinkedList<T>`                                         | Modifications fréquentes au milieu de la liste                        |
| `ConcurrentDictionary<TKey,TValue>`                     | Scénarios multi-threads sans locks externes                           |
| `ConcurrentBag<T>`                                      | Couture bag non ordonné, usage intensif multi-threads                 |
| `BlockingCollection<T>`                                 | Gestion de back-pressure dans pipelines TPL Dataflow                  |
| `ObservableCollection<T>`                               | Binding UI (WPF, **Windows Presentation Foundation**)                 |
| `SortedList<TKey,TValue>`                               | Clés triées, accès O(log n) / insertion O(n)                          |
| `ImmutableList<T>` / `ImmutableDictionary<TKey,TValue>` | Collections immuables, lock-free                                      |

---

## 7. Programmation Orientée Objet (Object-Oriented Programming)

**Q17. Qu’est-ce qu’une interface et à quoi sert-elle ?**

* **Interface** : contrat de signatures (méthodes/propriétés/événements) sans état.
* Avantages : découplage, inversion de dépendances, multi-héritage d’abstractions, substitution facile (polymorphisme).

**Q18. Méthode `virtual` vs non-`virtual`**

* `virtual` : crée une entrée dans la **vtable** (table des méthodes virtuelles), liaison dynamique.
* `sealed override` : supprime la lookup vtable pour cette méthode, peut améliorer le *inlining* du JIT (Just-In-Time compiler).

**Q19. Héritage multiple en C# ?**

* **Classes** : non, hériter d’une seule classe de base.
* **Interfaces** : oui, implémenter plusieurs interfaces.
* **Alternatives** : composition d’objets, design par délégation.

**Q20. Interface vs classe abstraite**

|                 | Interface                                      | Classe abstraite                          |
| --------------- | ---------------------------------------------- | ----------------------------------------- |
| Implémentations | Signatures (+ méthodes par défaut depuis C# 8) | Méthodes concrètes, champs, constructeurs |
| Héritage        | Multiple                                       | Unique                                    |
| Usage           | Contrats, injection de dépendances             | Réutilisation de code, partial template   |

**Q21. Intérêt de l’OOP**

* **Encapsulation** : protéger l’état interne
* **Abstraction** : exposer une interface simple
* **Héritage** : réutiliser et spécialiser du code
* **Polymorphisme** : même appel, comportements variés

**Q22. Classe `sealed` et cas d’usage**

* `sealed` : interdire l’héritage.
* Utilité : verrouiller une API, garantir la non-extension de comportement, optimiser le JIT.

**Q23. Inversion de dépendances** (Dependency Inversion Principle + Inversion of Control)

* Dépendre d’**abstractions** (interfaces), pas de détails concrets.
* Conteneur **IoC** (Inversion of Control) : Microsoft.Extensions.DependencyInjection, Autofac, Unity…
* **Cycles de vie** :

  * **Transient** : nouvelle instance à chaque injection
  * **Scoped** : une instance par **scope** (par ex. requête HTTP)
  * **Singleton** : une instance pour toute l’application

```csharp
services.AddScoped<IRepository, SqlRepository>();
services.AddSingleton<ILogger, SerilogLogger>();
```

---

## 8. Gestion des Exceptions

**Q24. Quel mécanisme recommandez-vous pour le logging et la propagation des exceptions ?**

* Utiliser des frameworks de logging structurés : **Serilog**, **NLog**, **log4net**.
* **Exception filters** (`catch (Exception ex) when (Log(ex))`) pour logger sans casser la pile.
* **Global handlers** :

  * `AppDomain.CurrentDomain.UnhandledException`
  * `TaskScheduler.UnobservedTaskException`
* Toujours *rethrow* avec `throw;` pour ne pas perdre la trace de la pile d’appels (`StackTrace`).

---

## 9. Design Patterns & Architecture

**Q25. Citez un modèle de conception adapté à une application lourde (type client lourd).**

* **MVVM** (Model-View-ViewModel) en **Windows Presentation Foundation** (WPF) :

  * **Model** : logique métier / accès données
  * **ViewModel** : logique de présentation, expose `INotifyPropertyChanged` et `ICommand`
  * **View** : XAML + binding

**Q26. Trois autres modèles de conception**

1. **Singleton** : garantir une unique instance partagée (ex. loggeur).

   ```csharp
   public sealed class Logger
   {
     private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
     public static Logger Instance => _instance.Value;
     private Logger() {}
   }
   ```
2. **Factory Method** : déléguer la création d’objets à des sous-classes.
3. **Observer** : pattern de publication/abonnement (ex. `INotifyPropertyChanged`, `IObservable<T>`).

---

## 10. Questions clés pour .NET

### Q27. .NET Framework vs .NET Core / .NET 8 (depuis .NET 5)

| Critère            | .NET Framework                | .NET Core / .NET 5+                                               |
| ------------------ | ----------------------------- | ----------------------------------------------------------------- |
| Plateforme         | Windows-only                  | Cross-platform (Windows, Linux, macOS)                            |
| Distribution       | Livré avec l’OS, monolithique | Paquets NuGet, self-contained ou framework-dependent              |
| Performances       | GC/JIT plus anciens           | GC low-latency, tiered JIT, AOT possible (*Ahead-Of-Time*)        |
| Conteneurs & Cloud | Peu optimisé                  | Images Docker légères (< 100 Mo), ARM64                           |
| Cycle de release   | Lent (Service Packs)          | Release semestriel, LTS (*Long Term Support*) 3 ans               |
| Interop Windows    | WCF, WebForms intégrés        | Migration vers gRPC (Google Remote Procedure Call) / ASP.NET Core |
| .NET Standard      | Version unique                | Supersédé par .NET 5+ unifié                                      |

<details>
<summary>Exemple de publication “self-contained” (.NET 8 SDK)</summary>

```bash
dotnet publish -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true \
  -o ./publish/linux
```

Génère un exécutable unique portable sans runtime préinstallé.

</details>

---

### Q28. Outils & stratégies pour optimiser les performances et la scalabilité dans le cloud

1. **Profiling & diagnostics**

   * `dotnet-counters` (compteurs de perf), `dotnet-trace` (traces temps réel)
   * PerfView, **Visual Studio Profiler**, JetBrains **dotTrace**
   * **OpenTelemetry** → exporter vers **Prometheus** / **Grafana**

2. **Gestion mémoire avancée**

   * `Span<T>` / `Memory<T>` pour buffers sans allocation GC
   * `stackalloc` pour allouer sur la stack
   * `ArrayPool<T>.Shared` pour recycler tableaux
   * Configurer `<ServerGarbageCollection>true</ServerGarbageCollection>`

3. **Asynchronisme & parallélisme**

   * `async`/`await` + `ValueTask<T>` pour réduction des allocations
   * `Parallel.ForEach`, **Parallel LINQ** (PLINQ) pour code CPU-bound, attention à la surcharge thread

4. **Caching**

   * **In-memory** : `MemoryCache`
   * **Distribué** : **Redis** pour partager entre instances

5. **Résilience**

   * **Polly** (Retry, Circuit Breaker, Timeout, Bulkhead)

   ```csharp
   var policy = Policy.Handle<HttpRequestException>()
                      .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));
   ```

6. **Autoscaling**

   * Kubernetes **Horizontal Pod Autoscaler** (HPA) basé sur CPU/mémoire ou métriques personnalisées
   * **Azure Functions Premium Plan** ou **AWS Lambda** pour scalabilité serverless

---

### Q29. Mettre en place une intégration CI/CD pour .NET Core sur Kubernetes

1. **Pipeline CI** (YAML)

   ```yaml
   steps:
   - script: dotnet restore
   - script: dotnet build -c Release
   - script: dotnet test  -c Release --logger trx
   - task: PublishTestResults@2
     inputs: { testResultsFormat: VSTest, testResultsFiles: '**/*.trx' }
   ```

2. **Docker multistage**

   ```dockerfile
   FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
   WORKDIR /src
   COPY *.sln ./
   COPY MyApp/*.csproj MyApp/
   RUN dotnet restore
   COPY . .
   RUN dotnet publish -c Release -o /app

   FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
   WORKDIR /app
   COPY --from=build /app .
   ENTRYPOINT ["./MyApp"]
   ```

3. **Registry & sécurité**

   * Push vers **Azure Container Registry** ou **Docker Hub**
   * Scan d’images avec **Trivy** ou **Clair**

4. **Déploiement GitOps**

   * **Helm** (templates) + `values.yaml`
   * **Argo CD** ou **Flux** pour appliquer les manifests déployés

5. **Validation post-déploiement**

   * Probes de readiness/liveness dans le **Deployment**
   * Tests de fumée, dashboards Grafana/Prometheus sur erreurs et latences

---

### Q30. Sécuriser une API .NET avec OAuth 2.0 et OpenID Connect

1. **Fournisseurs d’identité**

   * **IdentityServer4/5**, **Azure Active Directory B2C**, **Keycloak**

2. **Flows OAuth 2.0**

   * **Authorization Code** + **Proof Key for Code Exchange** (PKCE) pour applications web JS & mobiles
   * **Client Credentials** pour communications inter-services

3. **Middleware ASP.NET Core**

   ```csharp
   services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(options =>
     {
       options.Authority = "https://auth.example.com";
       options.Audience  = "api1";
       options.TokenValidationParameters = new TokenValidationParameters
       {
         ValidateIssuer   = true,
         ValidateAudience = true,
         ValidateLifetime = true
       };
     });
   services.AddAuthorization();
   ```

4. **Exemple d’usage**

   ```csharp
   [Authorize(Policy = "AdminOnly")]
   [HttpGet("secure-data")]
   public IActionResult GetSecureData() => Ok("Contenu sécurisé");
   ```

5. **Bonnes pratiques**

   * HTTPS obligatoire
   * Rotation de clés (**JSON Web Key Set**, JWKS)
   * CORS restreint (Cross-Origin Resource Sharing)
   * Stockage sécurisé des refresh tokens (HttpOnly cookie)

---

### Q31. Kafka vs RabbitMQ

| Critère                          | Kafka                                           | RabbitMQ                                                      |
| -------------------------------- | ----------------------------------------------- | ------------------------------------------------------------- |
| **Modèle**                       | Log distribué partitionné, consommateur “pull”  | Broker central, consommateur “push”                           |
| **Performance**                  | Très haut débit (> 100 000 messages/s)          | Débit moyen, latence plus faible en local                     |
| **Persistance**                  | Rétention configurable par topic, compaction    | Messages persistants ou éphémères, DLX (Dead Letter eXchange) |
| **Garanties d’ordre**            | Garantie d’ordre au sein d’une partition        | Pas de garantie globale, dépend de l’exchange / queue         |
| **Cas d’usage**                  | Event sourcing, analytics temps réel, streaming | Work queues, RPC, workflows transactionnels                   |
| **Exactly-once Semantics (EOS)** | Transactions & producteurs idempotents          | Moins natif, nécessite ack/nack et idempotence custom         |

> **Exemples**
>
> * **Kafka** pour ingérer et traiter en streaming les données de transactions Money Market, partitions par région.
> * **RabbitMQ** pour orchestrer les confirmations de traitement back-office via des files de tâches fiables.

---

## 11. Windows Presentation Foundation (WPF)

**Q32. Quel est le premier fichier ou la première méthode exécutée par une application WPF ?**

1. **App.xaml** → génère `App.g.cs` (fichier partiel)
2. Méthode `static void Main()` marquée `[STAThread]` (Single-Threaded Apartment pour COM) :

   ```csharp
   public partial class App : Application
   {
     [System.STAThread]
     public static void Main()
     {
       var app = new App();
       app.InitializeComponent();   // charge ResourceDictionaries, StartupUri
       app.Run();                   // lance la boucle de dispatch
     }
   }
   ```
3. **Dispatcher** : gère la file d’exécution de l’UI thread,
   `SynchronizationContext` permet aux `await` de revenir sur le thread UI.

---

## 12. Principes SOLID

1. **S** – **Single Responsibility Principle** (Principe de responsabilité unique)
   Une classe ne doit avoir qu’une seule raison de changer.

   ```csharp
   class OrderService { /* logique métier */ }
   class OrderRepository { /* persistance */ }
   ```
2. **O** – **Open/Closed Principle** (Principe ouvert/fermé)
   Le code est ouvert à l’extension, fermé à la modification.
3. **L** – **Liskov Substitution Principle** (Principe de substitution de Liskov)
   Les objets d’une classe dérivée doivent pouvoir remplacer les objets de la classe de base sans altérer la correction du programme.
4. **I** – **Interface Segregation Principle** (Principe de ségrégation des interfaces)
   Les clients ne doivent pas dépendre d’interfaces qu’ils n’utilisent pas.
5. **D** – **Dependency Inversion Principle** (Principe d’inversion de dépendances)
   Les modules hauts niveaux ne doivent pas dépendre de modules bas niveaux, mais tous deux doivent dépendre d’abstractions.

```csharp
public class BillingService
{
  public BillingService(IInvoiceRepository repo, IPaymentProcessor processor) { … }
}
```

---

## 13. Propriétés ACID & Niveaux d’isolation

* **A** – **Atomicity** (Atomicité) : « tout ou rien »
* **C** – **Consistency** (Cohérence) : respecter les règles d’intégrité
* **I** – **Isolation** : transactions concurrentes ne se perturbent pas
* **D** – **Durability** (Durabilité) : persistance après commit
* **Niveaux d’isolation** en **Structured Query Language** (SQL) :

  * Read Uncommitted → Read Committed → Repeatable Read → Serializable
  * **Snapshot Isolation** : versioning pour éviter non-repeatable et phantom reads

---

## 14. Questions “commando” approfondies

1. **Verrous avancés**

   * `lock(obj)` vs `Monitor.Enter/Exit` vs `SemaphoreSlim` vs `ReaderWriterLockSlim` vs `SpinLock` (pour sections très courtes, CPU-bound).

2. **Garbage Collector interne**

   * Générations 0/1/2, Large Object Heap (LOH), fragmentation, montée en charge.
   * Diagnostics : `dotnet dump analyze`, SOS (`!gcroot`), WinDBG.

3. **Internals de `async`/`await`**

   * Machine d’état générée par le compilateur (`IAsyncStateMachine.MoveNext()`).
   * Overhead d’allocation d’objets de state machine, `ConfigureAwait(false)` pour ne pas capturer le contexte synchronisation.

4. **Patterns distribués & d’architecture**

   * **CQRS** (Command Query Responsibility Segregation) vs **Event Sourcing** vs **Mediator** (via Mediator pattern / library MediatR).
   * **Saga Pattern** : orchestrée vs chorégraphiée pour transactions distribuées.

5. **Testing & Qualité de code**

   * Mock (`Moq`), Stub, Fake vs **Fakes Framework** (Microsoft)
   * **Mutation Testing** (Stryker.NET) pour mesurer la solidité de la suite de tests.

---

Avec cette version **exhaustive**, **chacun** des points est traité de façon précise, technique, avec exemples et nuances destinées à un expert de très haut niveau.
