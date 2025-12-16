# **Complexité Algorithmique et Performance en C# - Guide d'Entretien**

> **🔗 Cours Connexes :**
> - [Collections et Boucles](./boucles%20et%20collections.md) - Bases des structures de données
> - [LINQ et Lambda](./LINQ%20et%20Expressions%20Lambda.md) - Optimisation des requêtes
> - [Algorithmes de Tri et Graphes](./Algorithmes%20de%20Tri%20et%20Théorie%20des%20Graphes.md) - Algorithmes avancés
> - [Async/Await](./Délégués%20et%20Async.md) - Performance asynchrone
> - [Design Patterns](./Design%20Patterns.md) - Patterns d'optimisation

---

## **Concepts Fondamentaux**

### **Q: Qu'est-ce que la notation Big O et pourquoi est-elle importante ?**

**Définition :** La notation Big O décrit la **complexité temporelle** ou **spatiale** d'un algorithme dans le **pire des cas** en fonction de la taille de l'entrée.

**Complexités courantes :**
- **O(1)** - Constant : Accès tableau par index
- **O(log n)** - Logarithmique : Recherche binaire
- **O(n)** - Linéaire : Parcours de liste
- **O(n log n)** - Quasi-linéaire : Tri efficace (QuickSort, MergeSort)
- **O(n²)** - Quadratique : Tri à bulles, deux boucles imbriquées
- **O(2^n)** - Exponentielle : Calcul récursif de Fibonacci naïf

### **Q: Différence entre complexité temporelle et spatiale ?**

**Temporelle** : Temps d'exécution en fonction de la taille des données
**Spatiale** : Mémoire utilisée en fonction de la taille des données

```csharp
// Exemple avec trade-off temps/espace
public class FibonacciComparison
{
    // ❌ O(2^n) temps, O(n) espace (récursion)
    public static long FibonacciRecursive(int n)
    {
        if (n <= 1) return n;
        return FibonacciRecursive(n - 1) + FibonacciRecursive(n - 2);
    }
    
    // ✅ O(n) temps, O(n) espace (memoization)
    public static long FibonacciMemoization(int n, Dictionary<int, long> memo = null)
    {
        memo ??= new Dictionary<int, long>();
        if (n <= 1) return n;
        if (memo.ContainsKey(n)) return memo[n];
        
        memo[n] = FibonacciMemoization(n - 1, memo) + FibonacciMemoization(n - 2, memo);
        return memo[n];
    }
    
    // ✅ O(n) temps, O(1) espace (itératif)
    public static long FibonacciIterative(int n)
    {
        if (n <= 1) return n;
        long a = 0, b = 1, temp;
        for (int i = 2; i <= n; i++)
        {
            temp = a + b;
            a = b;
            b = temp;
        }
        return b;
    }
}
```

---

## **Collections .NET et Complexité**

### **Q: Quelles sont les complexités des opérations sur List<T> ?**

```csharp
public class ListComplexityAnalysis
{
    public static void AnalyzeListOperations()
    {
        var list = new List<int>();
        
        // O(1) amortized - Ajout en fin
        list.Add(42);
        
        // O(1) - Accès par index
        int value = list[0];
        
        // O(n) - Recherche linéaire
        bool contains = list.Contains(42);
        
        // O(n) - Insertion au milieu (décalage des éléments)
        list.Insert(0, 1);
        
        // O(n) - Suppression (décalage des éléments)
        list.Remove(42);
        
        // O(1) - Suppression en fin
        list.RemoveAt(list.Count - 1);
    }
}
```

### **Q: Dictionary<K,V> vs List<T> pour les recherches ?**

> **📖 Bases :** Pour les détails d'utilisation des collections, voir [Collections Guide](./boucles%20et%20collections.md#list-vs-dictionary)

```csharp
public class SearchComparison
{
    public static void CompareSearchPerformance()
    {
        var list = new List<string>();
        var dictionary = new Dictionary<string, int>();
        
        // Population avec 1M d'éléments
        for (int i = 0; i < 1_000_000; i++)
        {
            string key = $"key_{i}";
            list.Add(key);
            dictionary[key] = i;
        }
        
        string searchKey = "key_999999";
        
        // ❌ O(n) - Recherche linéaire dans List
        bool foundInList = list.Contains(searchKey);
        
        // ✅ O(1) average - Recherche par hash dans Dictionary
        bool foundInDict = dictionary.ContainsKey(searchKey);
    }
}
```

### **Q: Complexités des structures de données .NET courantes ?**

| Structure | Accès | Recherche | Insertion | Suppression | Espace |
|-----------|-------|-----------|-----------|-------------|--------|
| **Array** | O(1) | O(n) | N/A | N/A | O(n) |
| **List<T>** | O(1) | O(n) | O(1)* / O(n) | O(n) | O(n) |
| **Dictionary<K,V>** | O(1)* | O(1)* | O(1)* | O(1)* | O(n) |
| **HashSet<T>** | N/A | O(1)* | O(1)* | O(1)* | O(n) |
| **LinkedList<T>** | O(n) | O(n) | O(1) | O(1) | O(n) |
| **Queue<T>** | O(1) | O(n) | O(1) | O(1) | O(n) |
| **Stack<T>** | O(1) | O(n) | O(1) | O(1) | O(n) |
| **SortedDictionary<K,V>** | O(log n) | O(log n) | O(log n) | O(log n) | O(n) |

*\*Average case - worst case peut être O(n) pour hash-based collections*

---

## **Algorithmes de Tri en C#**

### **Q: Implémentez et comparez différents algorithmes de tri ?**

```csharp
public class SortingAlgorithms
{
    // O(n²) - Bubble Sort
    public static void BubbleSort(int[] arr)
    {
        int n = arr.Length;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (arr[j] > arr[j + 1])
                {
                    (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                }
            }
        }
    }
    
    // O(n²) average, O(n) best case - Insertion Sort
    public static void InsertionSort(int[] arr)
    {
        for (int i = 1; i < arr.Length; i++)
        {
            int key = arr[i];
            int j = i - 1;
            
            while (j >= 0 && arr[j] > key)
            {
                arr[j + 1] = arr[j];
                j--;
            }
            arr[j + 1] = key;
        }
    }
    
    // O(n log n) average, O(n²) worst case - Quick Sort
    public static void QuickSort(int[] arr, int low = 0, int high = -1)
    {
        if (high == -1) high = arr.Length - 1;
        
        if (low < high)
        {
            int pi = Partition(arr, low, high);
            QuickSort(arr, low, pi - 1);
            QuickSort(arr, pi + 1, high);
        }
    }
    
    private static int Partition(int[] arr, int low, int high)
    {
        int pivot = arr[high];
        int i = low - 1;
        
        for (int j = low; j < high; j++)
        {
            if (arr[j] < pivot)
            {
                i++;
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }
        (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
        return i + 1;
    }
    
    // O(n log n) - Merge Sort (stable)
    public static void MergeSort(int[] arr, int left = 0, int right = -1)
    {
        if (right == -1) right = arr.Length - 1;
        
        if (left < right)
        {
            int mid = left + (right - left) / 2;
            MergeSort(arr, left, mid);
            MergeSort(arr, mid + 1, right);
            Merge(arr, left, mid, right);
        }
    }
    
    private static void Merge(int[] arr, int left, int mid, int right)
    {
        int n1 = mid - left + 1;
        int n2 = right - mid;
        
        int[] leftArr = new int[n1];
        int[] rightArr = new int[n2];
        
        Array.Copy(arr, left, leftArr, 0, n1);
        Array.Copy(arr, mid + 1, rightArr, 0, n2);
        
        int i = 0, j = 0, k = left;
        
        while (i < n1 && j < n2)
        {
            if (leftArr[i] <= rightArr[j])
                arr[k++] = leftArr[i++];
            else
                arr[k++] = rightArr[j++];
        }
        
        while (i < n1) arr[k++] = leftArr[i++];
        while (j < n2) arr[k++] = rightArr[j++];
    }
}
```

### **Q: Quel algorithme de tri utilise Array.Sort() en .NET ?**

**.NET utilise :**
- **IntroSort** (Introspective Sort) : Hybride QuickSort + HeapSort + InsertionSort
- **O(n log n)** garantie dans le pire cas
- **QuickSort** pour la majorité, **HeapSort** si récursion trop profonde, **InsertionSort** pour petits tableaux

```csharp
public class DotNetSortAnalysis
{
    public static void CompareSortingMethods()
    {
        int[] data = GenerateRandomArray(100_000);
        
        // ✅ Optimisé par .NET - O(n log n) garanti
        var array1 = (int[])data.Clone();
        Array.Sort(array1);
        
        // ✅ LINQ OrderBy - utilise aussi un tri stable O(n log n)
        var array2 = data.OrderBy(x => x).ToArray();
        // 📖 Voir [LINQ Performance](./LINQ%20et%20Expressions%20Lambda.md) pour optimisations
        
        // ✅ List.Sort() - même algo qu'Array.Sort()
        var list = data.ToList();
        list.Sort();
    }
    
    private static int[] GenerateRandomArray(int size)
    {
        var random = new Random();
        return Enumerable.Range(0, size)
                        .Select(_ => random.Next(1, 1000))
                        .ToArray();
    }
}
```

---

## **Algorithmes de Recherche**

### **Q: Implémentez la recherche binaire et expliquez sa complexité ?**

```csharp
public class SearchAlgorithms
{
    // O(log n) - Recherche binaire (tableau trié requis)
    public static int BinarySearch(int[] sortedArray, int target)
    {
        int left = 0, right = sortedArray.Length - 1;
        
        while (left <= right)
        {
            int mid = left + (right - left) / 2; // Évite l'overflow
            
            if (sortedArray[mid] == target)
                return mid;
            
            if (sortedArray[mid] < target)
                left = mid + 1;
            else
                right = mid - 1;
        }
        
        return -1; // Non trouvé
    }
    
    // O(log n) - Version récursive
    public static int BinarySearchRecursive(int[] sortedArray, int target, int left = 0, int right = -1)
    {
        if (right == -1) right = sortedArray.Length - 1;
        
        if (left > right) return -1;
        
        int mid = left + (right - left) / 2;
        
        if (sortedArray[mid] == target) return mid;
        
        if (sortedArray[mid] > target)
            return BinarySearchRecursive(sortedArray, target, left, mid - 1);
        else
            return BinarySearchRecursive(sortedArray, target, mid + 1, right);
    }
    
    // O(n) - Recherche linéaire
    public static int LinearSearch(int[] array, int target)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == target)
                return i;
        }
        return -1;
    }
}
```

### **Q: Quand utiliser recherche binaire vs HashMap ?**

```csharp
public class SearchStrategyComparison
{
    // Recherche binaire : O(log n) recherche, O(1) espace supplémentaire
    public static bool BinarySearchApproach(int[] sortedData, int target)
    {
        // Pré-requis : données triées
        return Array.BinarySearch(sortedData, target) >= 0;
    }
    
    // HashMap : O(1) recherche, O(n) espace supplémentaire
    public static bool HashSetApproach(int[] data, int target)
    {
        var hashSet = new HashSet<int>(data); // O(n) construction
        return hashSet.Contains(target); // O(1) recherche
    }
    
    // Choix de stratégie basé sur les contraintes
    public static string RecommendSearchStrategy(int dataSize, int searchCount, bool isSorted)
    {
        if (searchCount == 1)
        {
            return isSorted ? "Binary Search" : "Linear Search";
        }
        
        if (searchCount > dataSize / 10)
        {
            return "HashSet (multiple searches justify O(n) preprocessing)";
        }
        
        return isSorted ? "Binary Search" : "Sort then Binary Search";
    }
}
```

---

## **Structures de Données Avancées**

### **Q: Implémentez un Trie (arbre de préfixes) et analysez sa complexité ?**

```csharp
public class TrieNode
{
    public Dictionary<char, TrieNode> Children { get; set; } = new();
    public bool IsEndOfWord { get; set; } = false;
}

public class Trie
{
    private readonly TrieNode _root = new();
    
    // O(m) où m = longueur du mot
    public void Insert(string word)
    {
        var current = _root;
        foreach (char c in word.ToLower())
        {
            if (!current.Children.ContainsKey(c))
            {
                current.Children[c] = new TrieNode();
            }
            current = current.Children[c];
        }
        current.IsEndOfWord = true;
    }
    
    // O(m) où m = longueur du mot
    public bool Search(string word)
    {
        var node = SearchNode(word);
        return node != null && node.IsEndOfWord;
    }
    
    // O(m) où m = longueur du préfixe
    public bool StartsWith(string prefix)
    {
        return SearchNode(prefix) != null;
    }
    
    private TrieNode SearchNode(string word)
    {
        var current = _root;
        foreach (char c in word.ToLower())
        {
            if (!current.Children.ContainsKey(c))
                return null;
            current = current.Children[c];
        }
        return current;
    }
    
    // O(p) où p = longueur du préfixe + nombre de mots avec ce préfixe
    public List<string> GetAllWordsWithPrefix(string prefix)
    {
        var result = new List<string>();
        var prefixNode = SearchNode(prefix);
        
        if (prefixNode != null)
        {
            CollectWords(prefixNode, prefix, result);
        }
        
        return result;
    }
    
    private void CollectWords(TrieNode node, string currentWord, List<string> result)
    {
        if (node.IsEndOfWord)
        {
            result.Add(currentWord);
        }
        
        foreach (var (c, childNode) in node.Children)
        {
            CollectWords(childNode, currentWord + c, result);
        }
    }
}

// Exemple d'utilisation avec analyse de performance
public class TrieUsageExample
{
    public static void AnalyzeTriePerformance()
    {
        var trie = new Trie();
        var words = new[] { "cat", "car", "card", "care", "careful", "cars", "carry" };
        
        // O(n * m) pour n mots de longueur moyenne m
        foreach (string word in words)
        {
            trie.Insert(word);
        }
        
        // O(m) - recherche rapide
        bool found = trie.Search("car"); // true
        
        // O(p + k) où p = longueur préfixe, k = nombre résultats
        var wordsWithCar = trie.GetAllWordsWithPrefix("car");
        // Résultat: ["car", "card", "care", "careful", "cars", "carry"]
    }
}
```

### **Q: Implémentez une MinHeap et analysez les opérations ?**

```csharp
public class MinHeap<T> where T : IComparable<T>
{
    private readonly List<T> _heap = new();
    
    public int Count => _heap.Count;
    public bool IsEmpty => _heap.Count == 0;
    
    // O(log n) - Insertion
    public void Insert(T item)
    {
        _heap.Add(item);
        HeapifyUp(_heap.Count - 1);
    }
    
    // O(log n) - Extraction du minimum
    public T ExtractMin()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Heap is empty");
        
        T min = _heap[0];
        T last = _heap[_heap.Count - 1];
        _heap.RemoveAt(_heap.Count - 1);
        
        if (!IsEmpty)
        {
            _heap[0] = last;
            HeapifyDown(0);
        }
        
        return min;
    }
    
    // O(1) - Peek minimum
    public T Peek()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Heap is empty");
        return _heap[0];
    }
    
    // O(log n) - Remontée
    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (_heap[index].CompareTo(_heap[parentIndex]) >= 0)
                break;
            
            (_heap[index], _heap[parentIndex]) = (_heap[parentIndex], _heap[index]);
            index = parentIndex;
        }
    }
    
    // O(log n) - Descente
    private void HeapifyDown(int index)
    {
        while (true)
        {
            int leftChild = 2 * index + 1;
            int rightChild = 2 * index + 2;
            int smallest = index;
            
            if (leftChild < _heap.Count && 
                _heap[leftChild].CompareTo(_heap[smallest]) < 0)
                smallest = leftChild;
            
            if (rightChild < _heap.Count && 
                _heap[rightChild].CompareTo(_heap[smallest]) < 0)
                smallest = rightChild;
            
            if (smallest == index)
                break;
            
            (_heap[index], _heap[smallest]) = (_heap[smallest], _heap[index]);
            index = smallest;
        }
    }
}

// Utilisation pour algorithme de Dijkstra simplifié
public class DijkstraExample
{
    public record EdgeDistance(int Node, int Distance) : IComparable<EdgeDistance>
    {
        public int CompareTo(EdgeDistance other) => Distance.CompareTo(other.Distance);
    }
    
    // O((V + E) log V) avec MinHeap
    public static Dictionary<int, int> FindShortestPaths(
        Dictionary<int, List<(int neighbor, int weight)>> graph, 
        int start)
    {
        var distances = new Dictionary<int, int>();
        var visited = new HashSet<int>();
        var priorityQueue = new MinHeap<EdgeDistance>();
        
        // Initialisation
        foreach (int node in graph.Keys)
        {
            distances[node] = int.MaxValue;
        }
        distances[start] = 0;
        priorityQueue.Insert(new EdgeDistance(start, 0));
        
        while (!priorityQueue.IsEmpty)
        {
            var current = priorityQueue.ExtractMin();
            
            if (visited.Contains(current.Node))
                continue;
            
            visited.Add(current.Node);
            
            if (!graph.ContainsKey(current.Node))
                continue;
            
            foreach (var (neighbor, weight) in graph[current.Node])
            {
                if (visited.Contains(neighbor))
                    continue;
                
                int newDistance = distances[current.Node] + weight;
                if (newDistance < distances[neighbor])
                {
                    distances[neighbor] = newDistance;
                    priorityQueue.Insert(new EdgeDistance(neighbor, newDistance));
                }
            }
        }
        
        return distances;
    }
}
```

---

## **Optimisation et Profiling**

### **Q: Comment mesurer et optimiser les performances en C# ?**

```csharp
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

// Utilisation de BenchmarkDotNet pour des mesures précises
[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80)]
public class CollectionPerformanceBenchmark
{
    private readonly int[] _data;
    private readonly List<int> _list;
    private readonly HashSet<int> _hashSet;
    private readonly Dictionary<int, int> _dictionary;
    
    public CollectionPerformanceBenchmark()
    {
        _data = Enumerable.Range(0, 100_000).ToArray();
        _list = new List<int>(_data);
        _hashSet = new HashSet<int>(_data);
        _dictionary = _data.ToDictionary(x => x, x => x);
    }
    
    [Benchmark]
    public bool SearchInList() => _list.Contains(99_999);
    
    [Benchmark]
    public bool SearchInHashSet() => _hashSet.Contains(99_999);
    
    [Benchmark]
    public bool SearchInDictionary() => _dictionary.ContainsKey(99_999);
    
    [Benchmark]
    public bool SearchInArray() => Array.IndexOf(_data, 99_999) != -1;
    
    [Benchmark]
    public bool BinarySearchInArray() => Array.BinarySearch(_data, 99_999) >= 0;
}

// Profiling manuel avec Stopwatch
public class ManualPerformanceProfiling
{
    public static void CompareAlgorithmPerformance()
    {
        int[] data = Enumerable.Range(0, 100_000).Reverse().ToArray();
        
        // Test Bubble Sort
        var bubbleData = (int[])data.Clone();
        var sw = Stopwatch.StartNew();
        SortingAlgorithms.BubbleSort(bubbleData);
        sw.Stop();
        Console.WriteLine($"Bubble Sort: {sw.ElapsedMilliseconds}ms");
        
        // Test Array.Sort (IntroSort)
        var arrayData = (int[])data.Clone();
        sw.Restart();
        Array.Sort(arrayData);
        sw.Stop();
        Console.WriteLine($"Array.Sort: {sw.ElapsedMilliseconds}ms");
        
        // Test Quick Sort
        var quickData = (int[])data.Clone();
        sw.Restart();
        SortingAlgorithms.QuickSort(quickData);
        sw.Stop();
        Console.WriteLine($"Quick Sort: {sw.ElapsedMilliseconds}ms");
    }
}

// Optimisation mémoire et GC
public class MemoryOptimization
{
    // ❌ Allocation excessive
    public static string ConcatenateStringsBad(string[] strings)
    {
        string result = "";
        foreach (string s in strings) // O(n²) à cause des allocations string
        {
            result += s;
        }
        return result;
    }
    
    // ✅ StringBuilder - O(n)
    public static string ConcatenateStringsGood(string[] strings)
    {
        var sb = new StringBuilder();
        foreach (string s in strings)
        {
            sb.Append(s);
        }
        return sb.ToString();
    }
    
    // ✅ Span<T> pour éviter allocations
    public static int SumArrayWithSpan(ReadOnlySpan<int> numbers)
    {
        int sum = 0;
        foreach (int number in numbers) // Pas d'allocation
        {
            sum += number;
        }
        return sum;
    }
    
    // ✅ ArrayPool pour réutiliser les tableaux
    public static void ProcessLargeArrayEfficiently()
    {
        var pool = ArrayPool<int>.Shared;
        int[] buffer = pool.Rent(1_000_000); // Réutilise un tableau existant
        
        try
        {
            // Traitement des données
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i * i;
            }
        }
        finally
        {
            pool.Return(buffer); // Remet dans le pool
        }
    }
}
```

---

## **Patterns d'Optimisation Avancés**

### **Q: Implémentez un cache LRU (Least Recently Used) ?**

```csharp
// O(1) pour toutes les opérations
public class LRUCache<TKey, TValue>
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _cache;
    private readonly LinkedList<(TKey Key, TValue Value)> _lruList;
    
    public LRUCache(int capacity)
    {
        _capacity = capacity;
        _cache = new Dictionary<TKey, LinkedListNode<(TKey, TValue)>>(capacity);
        _lruList = new LinkedList<(TKey, TValue)>();
    }
    
    // O(1) - Get value
    public TValue Get(TKey key)
    {
        if (_cache.TryGetValue(key, out var node))
        {
            // Déplacer vers le début (most recently used)
            _lruList.Remove(node);
            _lruList.AddFirst(node);
            return node.Value.Value;
        }
        
        throw new KeyNotFoundException($"Key {key} not found");
    }
    
    // O(1) - Put value
    public void Put(TKey key, TValue value)
    {
        if (_cache.TryGetValue(key, out var existingNode))
        {
            // Update existing
            existingNode.Value = (key, value);
            _lruList.Remove(existingNode);
            _lruList.AddFirst(existingNode);
        }
        else
        {
            // Add new
            if (_cache.Count >= _capacity)
            {
                // Remove least recently used
                var lastNode = _lruList.Last;
                _lruList.RemoveLast();
                _cache.Remove(lastNode.Value.Key);
            }
            
            var newNode = _lruList.AddFirst((key, value));
            _cache[key] = newNode;
        }
    }
    
    public bool TryGet(TKey key, out TValue value)
    {
        if (_cache.TryGetValue(key, out var node))
        {
            _lruList.Remove(node);
            _lruList.AddFirst(node);
            value = node.Value.Value;
            return true;
        }
        
        value = default;
        return false;
    }
}

// Exemple d'utilisation pour cache de calculs coûteux
public class ExpensiveCalculationService
{
    private readonly LRUCache<string, double> _cache = new(1000);
    
    public double ComplexCalculation(double x, double y)
    {
        string key = $"{x}_{y}";
        
        if (_cache.TryGet(key, out double cachedResult))
        {
            return cachedResult; // O(1) - cache hit
        }
        
        // Calcul coûteux O(n) ou plus
        double result = PerformExpensiveComputation(x, y);
        _cache.Put(key, result);
        
        return result;
    }
    
    private double PerformExpensiveComputation(double x, double y)
    {
        // Simulation d'un calcul complexe
        Thread.Sleep(100); // Simule latence
        return Math.Pow(x, y) + Math.Sin(x * y);
    }
}
```

### **Q: Implémentez un algorithme de sliding window pour optimiser les calculs ?**

```csharp
public class SlidingWindowAlgorithms
{
    // O(n) au lieu de O(n*k) pour somme de fenêtre glissante
    public static double[] MovingAverage(int[] numbers, int windowSize)
    {
        if (numbers.Length < windowSize)
            return Array.Empty<double>();
        
        var result = new double[numbers.Length - windowSize + 1];
        
        // Calculer la première somme
        int currentSum = 0;
        for (int i = 0; i < windowSize; i++)
        {
            currentSum += numbers[i];
        }
        result[0] = (double)currentSum / windowSize;
        
        // Sliding window: retirer l'ancien, ajouter le nouveau
        for (int i = 1; i < result.Length; i++)
        {
            currentSum = currentSum - numbers[i - 1] + numbers[i + windowSize - 1];
            result[i] = (double)currentSum / windowSize;
        }
        
        return result;
    }
    
    // O(n) - Plus long sous-array avec somme <= target
    public static int MaxSubarrayLength(int[] numbers, int targetSum)
    {
        int left = 0, maxLength = 0, currentSum = 0;
        
        for (int right = 0; right < numbers.Length; right++)
        {
            currentSum += numbers[right];
            
            // Rétrécir la fenêtre si somme dépasse target
            while (currentSum > targetSum && left <= right)
            {
                currentSum -= numbers[left];
                left++;
            }
            
            maxLength = Math.Max(maxLength, right - left + 1);
        }
        
        return maxLength;
    }
    
    // O(n) - Tous les anagrammes d'un pattern dans un texte
    public static List<int> FindAllAnagrams(string text, string pattern)
    {
        var result = new List<int>();
        if (text.Length < pattern.Length) return result;
        
        var patternCount = new Dictionary<char, int>();
        var windowCount = new Dictionary<char, int>();
        
        // Compter les caractères du pattern
        foreach (char c in pattern)
        {
            patternCount[c] = patternCount.GetValueOrDefault(c) + 1;
        }
        
        int windowSize = pattern.Length;
        
        // Initialiser la première fenêtre
        for (int i = 0; i < windowSize; i++)
        {
            char c = text[i];
            windowCount[c] = windowCount.GetValueOrDefault(c) + 1;
        }
        
        if (AreEqual(patternCount, windowCount))
        {
            result.Add(0);
        }
        
        // Sliding window
        for (int i = windowSize; i < text.Length; i++)
        {
            // Ajouter nouveau caractère
            char newChar = text[i];
            windowCount[newChar] = windowCount.GetValueOrDefault(newChar) + 1;
            
            // Retirer ancien caractère
            char oldChar = text[i - windowSize];
            windowCount[oldChar]--;
            if (windowCount[oldChar] == 0)
            {
                windowCount.Remove(oldChar);
            }
            
            if (AreEqual(patternCount, windowCount))
            {
                result.Add(i - windowSize + 1);
            }
        }
        
        return result;
    }
    
    private static bool AreEqual(Dictionary<char, int> dict1, Dictionary<char, int> dict2)
    {
        if (dict1.Count != dict2.Count) return false;
        
        foreach (var (key, value) in dict1)
        {
            if (!dict2.TryGetValue(key, out int otherValue) || value != otherValue)
                return false;
        }
        
        return true;
    }
}
```

---

## **Questions Pièges d'Entretien**

### **Q: "Votre algorithme a une complexité O(n log n), comment l'optimiser ?"**

**Stratégies de réponse :**

1. **Analyser le problème** : Est-ce que O(n log n) est optimal théoriquement ?
2. **Trade-offs** : Espace vs temps, préprocessing vs recherche
3. **Cas spéciaux** : Données partiellement triées, petites tailles
4. **Structures alternatives** : Hash tables, tries, bloom filters

```csharp
// Exemple: Trouver les doublons
public class DuplicateDetectionOptimization
{
    // ❌ O(n²) - Naïf
    public static bool HasDuplicatesNaive(int[] numbers)
    {
        for (int i = 0; i < numbers.Length; i++)
        {
            for (int j = i + 1; j < numbers.Length; j++)
            {
                if (numbers[i] == numbers[j])
                    return true;
            }
        }
        return false;
    }
    
    // ✅ O(n log n) - Tri puis comparaison
    public static bool HasDuplicatesSort(int[] numbers)
    {
        Array.Sort(numbers); // O(n log n)
        for (int i = 1; i < numbers.Length; i++) // O(n)
        {
            if (numbers[i] == numbers[i - 1])
                return true;
        }
        return false;
    }
    
    // ✅ O(n) temps, O(n) espace - HashSet
    public static bool HasDuplicatesHash(int[] numbers)
    {
        var seen = new HashSet<int>();
        foreach (int number in numbers)
        {
            if (!seen.Add(number)) // Add retourne false si déjà présent
                return true;
        }
        return false;
    }
    
    // ✅ O(n) si range limité - Counting sort
    public static bool HasDuplicatesCounting(int[] numbers, int maxValue)
    {
        if (maxValue < numbers.Length) return true; // Principe des tiroirs
        
        var counts = new bool[maxValue + 1];
        foreach (int number in numbers)
        {
            if (counts[number])
                return true;
            counts[number] = true;
        }
        return false;
    }
}
```

### **Q: "Comment gérez-vous les performances sur de très gros datasets ?"**

```csharp
public class BigDataPerformanceStrategies
{
    // Stratégie 1: Streaming et traitement par chunks
    public static async Task<long> ProcessLargeFileAsync(string filePath)
    {
        long count = 0;
        const int bufferSize = 8192;
        
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(fileStream, bufferSize: bufferSize);
        
        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            // Traiter ligne par ligne pour éviter de charger tout en mémoire
            if (IsValidData(line))
                count++;
        }
        
        return count;
    }
    
    // Stratégie 2: Parallélisation avec Parallel.ForEach
    public static double ProcessArrayParallel(double[] largeArray)
    {
        var partitioner = Partitioner.Create(largeArray, true);
        double result = 0;
        var lockObj = new object();
        
        Parallel.ForEach(partitioner, chunk =>
        {
            double localSum = 0;
            foreach (double value in chunk)
            {
                localSum += Math.Sin(value) * Math.Cos(value); // Calcul coûteux
            }
            
            lock (lockObj)
            {
                result += localSum;
            }
        });
        
        return result;
    }
    
    // Stratégie 3: Memory mapping pour gros fichiers
    public static unsafe long ProcessMemoryMappedFile(string filePath)
    {
        using var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, "data");
        using var accessor = mmf.CreateViewAccessor();
        
        byte* ptr = null;
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        
        try
        {
            long fileSize = new FileInfo(filePath).Length;
            long count = 0;
            
            // Traitement ultra-rapide en mémoire
            for (long i = 0; i < fileSize; i++)
            {
                if (ptr[i] == '\n')
                    count++;
            }
            
            return count;
        }
        finally
        {
            accessor.SafeMemoryMappedViewHandle.ReleasePointer();
        }
    }
    
    // Stratégie 4: Approximation pour analytics
    public static double EstimateDistinctCount(IEnumerable<string> data)
    {
        // HyperLogLog algorithm approximation
        const int buckets = 1024;
        var maxZeros = new int[buckets];
        
        foreach (string item in data)
        {
            uint hash = (uint)item.GetHashCode();
            int bucket = (int)(hash % buckets);
            int leadingZeros = CountLeadingZeros(hash);
            maxZeros[bucket] = Math.Max(maxZeros[bucket], leadingZeros);
        }
        
        double sum = maxZeros.Sum(z => Math.Pow(2, -z));
        return 0.79402 * buckets * buckets / sum; // HyperLogLog formula
    }
    
    private static int CountLeadingZeros(uint value)
    {
        if (value == 0) return 32;
        int count = 0;
        if ((value & 0xFFFF0000) == 0) { count += 16; value <<= 16; }
        if ((value & 0xFF000000) == 0) { count += 8; value <<= 8; }
        if ((value & 0xF0000000) == 0) { count += 4; value <<= 4; }
        if ((value & 0xC0000000) == 0) { count += 2; value <<= 2; }
        if ((value & 0x80000000) == 0) { count += 1; }
        return count;
    }
    
    private static bool IsValidData(string line) => !string.IsNullOrWhiteSpace(line);
}
```

### **Q: "Expliquez la différence entre complexité amortie et pire cas ?"**

**Complexité amortie** : Performance moyenne sur une séquence d'opérations
**Pire cas** : Performance dans le scénario le plus défavorable

```csharp
public class AmortizedAnalysisExample
{
    // List<T>.Add() - Exemple parfait de complexité amortie
    public static void DemonstrateAmortizedComplexity()
    {
        var list = new List<int>();
        
        // Add() est O(1) amortized, O(n) worst case
        for (int i = 0; i < 1000; i++)
        {
            list.Add(i);
            // Redimensionnement coûteux seulement quand capacity dépassée
            // Fréquence des redimensionnements: 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024
            // Coût total: n + n/2 + n/4 + ... ≈ 2n = O(n) pour n insertions
            // Donc O(1) amortized par insertion
        }
    }
    
    // Implémentation custom pour montrer l'amortissement
    public class DynamicArray<T>
    {
        private T[] _array;
        private int _size;
        private int _capacity;
        
        public DynamicArray()
        {
            _capacity = 1;
            _array = new T[_capacity];
            _size = 0;
        }
        
        // O(1) amortized - doublage de capacité
        public void Add(T item)
        {
            if (_size == _capacity)
            {
                // O(n) worst case - mais rare
                _capacity *= 2;
                var newArray = new T[_capacity];
                Array.Copy(_array, newArray, _size);
                _array = newArray;
            }
            
            _array[_size++] = item; // O(1) typical case
        }
        
        // Analyse: Sur n insertions
        // - Redimensionnements: log₂(n) fois
        // - Coût total copies: n + n/2 + n/4 + ... = 2n
        // - Coût amortized par insertion: 2n/n = 2 = O(1)
    }
}
```

Cette documentation complète sur la complexité algorithmique vous donne tous les outils nécessaires pour briller lors d'entretiens techniques en C# !
