## **Multithreading et Concurrence**

### 1. Qu'est-ce que le multithreading ?
Le **multithreading** est un mécanisme qui permet à un processus d'exécuter plusieurs threads (fil d'exécution) simultanément. Chaque thread partage la mémoire du processus, ce qui permet de réaliser des tâches en parallèle et d'améliorer les performances, surtout pour les applications CPU-bound ou I/O-bound.

- **Avantages** :
  - Meilleure utilisation des ressources CPU.
  - Réactivité accrue pour les applications interactives.
- **Risques** :
  - Problèmes de synchronisation (race conditions, deadlocks).
  - Complexité accrue du code.

---

### 2. Quelle différence entre une tâche et un processus ?
| **Tâche (Task)** | **Processus** |
|------------------|---------------|
| Unité légère d'exécution dans un processus. | Instance d'un programme en cours d'exécution. |
| Partage la mémoire avec d'autres tâches du même processus. | A son propre espace mémoire isolé. |
| Gérée par le runtime (.NET). | Gérée par le système d'exploitation. |
| Moins coûteuse en ressources. | Plus coûteuse en ressources. |

---

### 3. Comment gérez-vous les accès concurrents avec `Task` en C# ?
Pour gérer les accès concurrents, utilisez :
- **`lock`** : Bloque un objet pour éviter les accès simultanés.
  ```csharp
  private readonly object _lock = new object();
  lock (_lock) { /* Code critique */ }
  ```
- **`Monitor`** : Plus flexible que `lock`.
- **`Mutex`** ou **`Semaphore`** : Pour synchroniser des threads entre processus.
- **`ConcurrentCollections`** : Collections thread-safe comme `ConcurrentDictionary` ou `ConcurrentQueue`.
- **`async`/`await`** : Pour éviter les blocages inutiles.

---

### 4. Comment synchroniser deux tâches en C# ?
- **`Task.Wait()`** : Bloque jusqu'à la fin de la tâche.
- **`Task.WhenAll()`** : Attend la fin de plusieurs tâches.
- **`Task.WhenAny()`** : Attend la fin de la première tâche.
- **`CancellationToken`** : Pour annuler des tâches.
- **`ManualResetEventSlim`** ou **`AutoResetEvent`** : Pour signaler entre threads.

---

## **Gestion des Ressources**

### 5. Le mot-clé `using` peut-il être utilisé avec n’importe quelle classe ?
Non, `using` ne peut être utilisé qu'avec des classes qui implémentent **`IDisposable`**. Il garantit que la méthode `Dispose()` est appelée pour libérer les ressources (fichiers, connexions, etc.).

---

### 6. Comment garantir la libération des ressources non gérées en C# ?
- Implémenter **`IDisposable`** et utiliser `using`.
- Utiliser un **finaliseur** (`~MyClass`) pour les ressources non gérées, mais privilégier `SafeHandle` pour éviter les fuites.
- Appeler explicitement `Dispose()` si `using` n'est pas applicable.

---

## **Paramètres de Méthodes et Types de Données**

### 7. Quelle est la différence entre `ref` et `out` en C# ?
| **`ref`** | **`out`** |
|-----------|-----------|
| La variable doit être initialisée avant l'appel. | La variable n'a pas besoin d'être initialisée. |
| Utilisé pour passer une variable par référence. | Utilisé pour retourner plusieurs valeurs depuis une méthode. |

---

### 8. Quelle est la valeur par défaut d’un `DateTime`, et comment vérifier s’il a été initialisé ?
- **Valeur par défaut** : `01/01/0001 00:00:00` (`DateTime.MinValue`).
- **Vérification** :
  ```csharp
  DateTime date = default;
  if (date == default(DateTime)) { /* Non initialisé */ }
  ```

---

### 9. Qu'est-ce qu'une variable primitive ?
En C#, les **types primitifs** sont des types de base intégrés au langage (ex : `int`, `bool`, `double`). Ils sont stockés sur la pile (sauf dans certains cas comme les tableaux) et ne nécessitent pas d'instanciation.

---

## **Boucles et Collections**

### 10. Quels sont les prérequis pour utiliser `foreach` sur une collection ?
La collection doit implémenter :
- **`IEnumerable`** (ou `IEnumerable<T>`).
- Une méthode `GetEnumerator()` qui retourne un **`IEnumerator`**.

---

### 11. Quelle est la différence entre une `List` et un `Dictionary` en C# ?
| **`List<T>`** | **`Dictionary<K,V>`** |
|---------------|----------------------|
| Collection ordonnée d'éléments. | Collection de paires clé/valeur. |
| Accès par index. | Accès par clé (rapide, O(1)). |
| Duplicatas autorisés. | Clés uniques. |

---

### 12. Quels types peuvent être utilisés comme clés de dictionnaire ? Comment sont gérées les collisions ?
- **Types valides** : Tout type qui implémente `GetHashCode()` et `Equals()` correctement (ex : `string`, `int`).
- **Collisions** : Gérées par des **listes chaînées** (chaînage) ou des **tableaux ouverts**.

---

### 13. Comment assurer l’unicité des clés dans un dictionnaire quand celles-ci sont des objets complexes ?
- **Redéfinir `GetHashCode()` et `Equals()`** dans la classe de la clé.
- Utiliser un **`IEqualityComparer<T>`** personnalisé.

---

### 14. Qu’est-ce qu’une `LinkedList` et dans quels cas l’utiliser ?
- **`LinkedList<T>`** : Collection de nœuds liés dynamiquement.
- **Cas d'usage** :
  - Insertions/suppressions fréquentes au milieu de la liste.
  - Pas d'accès aléatoire (O(n)).

---

### 15. Quelle est la différence entre une **Stack** et une **Heap** ?
| **Stack** | **Heap** |
|-----------|----------|
| Mémoire allouée pour les variables locales et les appels de méthodes. | Mémoire allouée dynamiquement pour les objets. |
| Gérée automatiquement (LIFO). | Gérée manuellement (via le garbage collector). |
| Accès très rapide. | Accès plus lent. |

---

### 16. Quelles sont les principales collections utilisées en .NET et dans quels cas les privilégier ?
| **Collection** | **Cas d'usage** |
|----------------|----------------|
| `List<T>` | Liste dynamique, accès par index. |
| `Dictionary<K,V>` | Recherche rapide par clé. |
| `HashSet<T>` | Ensembles uniques. |
| `Queue<T>` | FIFO (files d'attente). |
| `Stack<T>` | LIFO (piles). |
| `ConcurrentDictionary` | Dictionnaire thread-safe. |

---

## **Programmation Orientée Objet**

### 17. Qu’est-ce qu’une interface et à quoi sert-elle ?
Une **interface** est un contrat qui définit des méthodes, propriétés ou événements **sans implémentation**. Elle permet :
- L'abstraction.
- Le polymorphisme.
- La définition de comportements communs entre classes non liées.

---

### 18. Quelle est la différence entre une méthode virtuelle et non virtuelle ?
| **Méthode virtuelle** | **Méthode non virtuelle** |
|-----------------------|---------------------------|
| Peut être redéfinie (`override`) dans une classe dérivée. | Ne peut pas être redéfinie. |
| Utilise le **liage dynamique**. | Utilise le **liage statique**. |

---

### 19. L’héritage multiple est-il possible en C# ? Si non, quelles sont les alternatives ?
Non, C# ne supporte pas l'héritage multiple de classes. **Alternatives** :
- Implémenter plusieurs **interfaces**.
- Utiliser la **composition** (injection de dépendances).

---

### 20. Quelle est la différence entre une interface et une classe abstraite ?
| **Interface** | **Classe abstraite** |
|---------------|----------------------|
| Ne contient que des signatures. | Peut contenir des implémentations. |
| Pas de champs ou constructeurs. | Peut avoir des champs et constructeurs. |
| Une classe peut implémenter plusieurs interfaces. | Une classe ne peut hériter que d'une seule classe abstraite. |

---

### 21. Quel est l'intérêt du langage orienté objet ?
- **Encapsulation** : Protection des données.
- **Héritage** : Réutilisation du code.
- **Polymorphisme** : Flexibilité.
- **Abstraction** : Simplification de la complexité.

---

### 22. Qu'est-ce qu'une classe `sealed` et dans quel cas l'utiliser ?
- Une classe **`sealed`** ne peut pas être héritée.
- **Cas d'usage** :
  - Sécurité (ex : `string` en C#).
  - Optimisation (le compilateur peut inliner les méthodes).

---

### 23. Qu’est-ce que l’inversion de dépendance et pourquoi l’utiliser ?
Principe selon lequel les **modules de haut niveau** ne doivent pas dépendre des modules de bas niveau, mais des **abstractions**. Utilisé pour :
- Découpler le code.
- Faciliter les tests unitaires.
- Améliorer la maintenabilité.

---

## **Gestion des Exceptions**

### 24. Quel mécanisme recommandez-vous pour le logging et la propagation des exceptions ?
- **Logging** : Utiliser **Serilog**, **NLog** ou **ILogger** (.NET Core).
- **Propagation** :
  - Capturer les exceptions spécifiques.
  - Utiliser des **filtres d'exceptions** (`try-catch` granulaire).
  - Éviter de catcher `Exception` de manière générique.

---

## **Design Patterns & Architecture**

### 25. Citez un modèle de conception adapté à une application lourde (type client lourd).
**MVVM (Model-View-ViewModel)** :
- Séparation claire entre logique métier et interface utilisateur.
- Idéal pour WPF, UWP, ou Xamarin.

---

### 26. Citez trois autres modèles de conception et expliquez leur rôle.
1. **Singleton** : Garantit qu'une classe n'a qu'une seule instance.
2. **Factory Method** : Délègue l'instanciation à des sous-classes.
3. **Observer** : Notifie des objets en cas de changement d'état.

---

## **Questions clés pour les besoins .NET**

### 27. Quelles sont les différences entre .NET Framework et .NET Core/.NET 8 ?
| **.NET Framework** | **.NET Core / .NET 8** |
|---------------------|-----------------------|
| Windows-only. | Multiplateforme (Linux, macOS). |
| Monolithique. | Modulaire (nugets). |
| Prise en charge des technologies héritées (ex : WPF, WinForms). | Optimisé pour le cloud et les microservices. |

---

### 28. Quels outils et stratégies utilisez-vous pour optimiser les performances et garantir la scalabilité d'une application C# sur le cloud ?
- **Outils** : **Application Insights**, **Profiler (dotTrace, dotMemory)**.
- **Stratégies** :
  - Utiliser **async/await** pour les E/S.
  - Mettre en cache avec **Redis** ou **MemoryCache**.
  - Scaler horizontalement avec **Kubernetes** ou **Azure App Service**.

---

### 29. Comment mettez-vous en place une intégration CI/CD efficace pour une application .NET Core déployée sur Kubernetes ?
- **CI** : GitHub Actions ou Azure DevOps pour build/test.
- **CD** : Helm ou Kustomize pour le déploiement.
- **Stratégie** : Blue-Green ou Canary Deployment.

---

### 30. Comment sécurisez-vous une API .NET avec OAuth2 et OpenID Connect ?
- Utiliser **IdentityServer4** ou **Azure AD**.
- Configurer **JWT Bearer Authentication**.
- Valider les **scopes** et **claims**.

---

### 31. Quels sont les avantages et cas d’usage de Kafka vs RabbitMQ ?
| **Kafka** | **RabbitMQ** |
|-----------|-------------|
| **Streaming** de données en temps réel. | **File d'attente** traditionnelle. |
| Haute disponibilité et scalabilité. | Facile à configurer pour des workflows simples. |
| Idéal pour l'analyse de logs ou l'event sourcing. | Idéal pour les tâches asynchrones (ex : envoi d'emails). |

---

## **WPF (Windows Presentation Foundation)**

### 32. Quel est le premier fichier ou la première méthode exécutée par une application WPF ?
- **Fichier** : `App.xaml` (définit la classe `Application`).
- **Méthode** : `Main()` dans `App.xaml.cs` (ou `StartupUri` pour la fenêtre principale).

---