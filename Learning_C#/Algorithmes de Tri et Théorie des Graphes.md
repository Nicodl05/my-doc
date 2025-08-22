# **Algorithmes de Tri et Théorie des Graphes - Guide Technique**

> **🔗 Références :**
> - [Complexité Algorithmique](./Complexité%20Algorithmique%20et%20Performance.md) - Analyse de performance
> - [Collections](./boucles%20et%20collections.md) - Structures de données de base
> - [LINQ](./LINQ%20et%20Expressions%20Lambda.md) - Tri avec LINQ

---

# **PARTIE I : ALGORITHMES DE TRI**

## **1. Tri à Bulles (Bubble Sort)**

### **Q: Implémentez le tri à bulles et analysez sa complexité ?**

**Principe :** Compare les éléments adjacents et les échange s'ils sont dans le mauvais ordre, répète jusqu'à ce qu'aucun échange ne soit nécessaire.

**Complexité :**
- **Temporelle** : O(n²) dans tous les cas
- **Spatiale** : O(1) - tri en place
- **Stabilité** : Stable (préserve l'ordre des éléments égaux)

```csharp
public class BubbleSort
{
    // Version basique - O(n²)
    public static void Sort(int[] array)
    {
        int n = array.Length;
        
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (array[j] > array[j + 1])
                {
                    // Échange
                    (array[j], array[j + 1]) = (array[j + 1], array[j]);
                }
            }
        }
    }
    
    // Version optimisée avec détection d'arrêt anticipé
    public static void SortOptimized(int[] array)
    {
        int n = array.Length;
        bool swapped;
        
        for (int i = 0; i < n - 1; i++)
        {
            swapped = false;
            
            for (int j = 0; j < n - i - 1; j++)
            {
                if (array[j] > array[j + 1])
                {
                    (array[j], array[j + 1]) = (array[j + 1], array[j]);
                    swapped = true;
                }
            }
            
            // Si aucun échange, le tableau est trié
            if (!swapped) break;
        }
    }
    
    // Version générique avec comparateur custom
    public static void Sort<T>(T[] array, IComparer<T> comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        int n = array.Length;
        
        for (int i = 0; i < n - 1; i++)
        {
            bool swapped = false;
            
            for (int j = 0; j < n - i - 1; j++)
            {
                if (comparer.Compare(array[j], array[j + 1]) > 0)
                {
                    (array[j], array[j + 1]) = (array[j + 1], array[j]);
                    swapped = true;
                }
            }
            
            if (!swapped) break;
        }
    }
}

// Exemple d'utilisation avec objets complexes
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    public override string ToString() => $"{Name} ({Age})";
}

public class PersonAgeComparer : IComparer<Person>
{
    public int Compare(Person x, Person y) => x.Age.CompareTo(y.Age);
}

// Usage
var people = new Person[]
{
    new Person { Name = "Alice", Age = 30 },
    new Person { Name = "Bob", Age = 25 },
    new Person { Name = "Charlie", Age = 35 }
};

BubbleSort.Sort(people, new PersonAgeComparer());
```

---

## **2. Tri par Insertion (Insertion Sort)**

### **Q: Quand utiliser le tri par insertion et pourquoi ?**

**Principe :** Construit progressivement un tableau trié en insérant chaque élément à sa position correcte.

**Avantages :**
- **Efficace pour petits tableaux** (< 50 éléments)
- **Adaptatif** : O(n) si déjà presque trié
- **Stable** et **en place**
- **Online** : peut trier au fur et à mesure

```csharp
public class InsertionSort
{
    // Version classique
    public static void Sort(int[] array)
    {
        for (int i = 1; i < array.Length; i++)
        {
            int key = array[i];
            int j = i - 1;
            
            // Décaler les éléments plus grands vers la droite
            while (j >= 0 && array[j] > key)
            {
                array[j + 1] = array[j];
                j--;
            }
            
            array[j + 1] = key;
        }
    }
    
    // Version avec recherche binaire (Binary Insertion Sort)
    public static void BinaryInsertionSort(int[] array)
    {
        for (int i = 1; i < array.Length; i++)
        {
            int key = array[i];
            
            // Trouver la position d'insertion avec recherche binaire
            int insertPos = BinarySearch(array, 0, i - 1, key);
            
            // Décaler les éléments
            for (int j = i - 1; j >= insertPos; j--)
            {
                array[j + 1] = array[j];
            }
            
            array[insertPos] = key;
        }
    }
    
    private static int BinarySearch(int[] array, int left, int right, int key)
    {
        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            
            if (array[mid] <= key)
                left = mid + 1;
            else
                right = mid - 1;
        }
        
        return left;
    }
    
    // Version avec span pour éviter allocations
    public static void SortSpan(Span<int> span)
    {
        for (int i = 1; i < span.Length; i++)
        {
            int key = span[i];
            int j = i - 1;
            
            while (j >= 0 && span[j] > key)
            {
                span[j + 1] = span[j];
                j--;
            }
            
            span[j + 1] = key;
        }
    }
}

// Hybrid sort - utilise insertion sort pour petits sous-tableaux
public class HybridSort
{
    private const int INSERTION_SORT_THRESHOLD = 16;
    
    public static void Sort(int[] array)
    {
        QuickSortHybrid(array, 0, array.Length - 1);
    }
    
    private static void QuickSortHybrid(int[] array, int low, int high)
    {
        if (high - low < INSERTION_SORT_THRESHOLD)
        {
            // Utiliser insertion sort pour petits sous-tableaux
            InsertionSortRange(array, low, high);
        }
        else if (low < high)
        {
            int pi = Partition(array, low, high);
            QuickSortHybrid(array, low, pi - 1);
            QuickSortHybrid(array, pi + 1, high);
        }
    }
    
    private static void InsertionSortRange(int[] array, int start, int end)
    {
        for (int i = start + 1; i <= end; i++)
        {
            int key = array[i];
            int j = i - 1;
            
            while (j >= start && array[j] > key)
            {
                array[j + 1] = array[j];
                j--;
            }
            
            array[j + 1] = key;
        }
    }
    
    private static int Partition(int[] array, int low, int high)
    {
        int pivot = array[high];
        int i = low - 1;
        
        for (int j = low; j < high; j++)
        {
            if (array[j] < pivot)
            {
                i++;
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
        
        (array[i + 1], array[high]) = (array[high], array[i + 1]);
        return i + 1;
    }
}
```

---

## **3. Tri par Sélection (Selection Sort)**

### **Q: Pourquoi le tri par sélection est-il rarement utilisé en pratique ?**

**Principe :** Trouve le minimum/maximum et le place à sa position finale, répète pour le reste.

**Problèmes :**
- **Toujours O(n²)** même si déjà trié
- **Non stable** par défaut
- **Plus d'écritures** que nécessaire

```csharp
public class SelectionSort
{
    public static void Sort(int[] array)
    {
        int n = array.Length;
        
        for (int i = 0; i < n - 1; i++)
        {
            int minIndex = i;
            
            // Trouver le minimum dans la partie non triée
            for (int j = i + 1; j < n; j++)
            {
                if (array[j] < array[minIndex])
                {
                    minIndex = j;
                }
            }
            
            // Échanger seulement si nécessaire
            if (minIndex != i)
            {
                (array[i], array[minIndex]) = (array[minIndex], array[i]);
            }
        }
    }
    
    // Version stable (plus complexe)
    public static void StableSort(int[] array)
    {
        int n = array.Length;
        
        for (int i = 0; i < n - 1; i++)
        {
            int minIndex = i;
            
            for (int j = i + 1; j < n; j++)
            {
                if (array[j] < array[minIndex])
                {
                    minIndex = j;
                }
            }
            
            // Insertion stable du minimum
            int minValue = array[minIndex];
            
            // Décaler tous les éléments vers la droite
            for (int k = minIndex; k > i; k--)
            {
                array[k] = array[k - 1];
            }
            
            array[i] = minValue;
        }
    }
}
```

---

## **4. Tri Rapide (Quick Sort)**

### **Q: Pourquoi QuickSort est-il si populaire malgré son pire cas O(n²) ?**

**Réponse :** Excellent cas moyen O(n log n), tri en place, cache-friendly, facilement parallélisable.

```csharp
public class QuickSort
{
    public static void Sort(int[] array)
    {
        Sort(array, 0, array.Length - 1);
    }
    
    private static void Sort(int[] array, int low, int high)
    {
        if (low < high)
        {
            int pi = Partition(array, low, high);
            
            Sort(array, low, pi - 1);
            Sort(array, pi + 1, high);
        }
    }
    
    // Partition de Lomuto
    private static int Partition(int[] array, int low, int high)
    {
        int pivot = array[high]; // Dernier élément comme pivot
        int i = low - 1;
        
        for (int j = low; j < high; j++)
        {
            if (array[j] < pivot)
            {
                i++;
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
        
        (array[i + 1], array[high]) = (array[high], array[i + 1]);
        return i + 1;
    }
    
    // Version optimisée avec pivot médian
    public static void SortOptimized(int[] array)
    {
        SortOptimized(array, 0, array.Length - 1);
    }
    
    private static void SortOptimized(int[] array, int low, int high)
    {
        while (low < high)
        {
            // Utiliser médian de 3 pour le pivot
            int pivotIndex = MedianOfThree(array, low, high);
            (array[pivotIndex], array[high]) = (array[high], array[pivotIndex]);
            
            int pi = PartitionHoare(array, low, high);
            
            // Optimisation queue call - trier la plus petite partition récursivement
            if (pi - low < high - pi)
            {
                SortOptimized(array, low, pi - 1);
                low = pi + 1;
            }
            else
            {
                SortOptimized(array, pi + 1, high);
                high = pi - 1;
            }
        }
    }
    
    private static int MedianOfThree(int[] array, int low, int high)
    {
        int mid = low + (high - low) / 2;
        
        if (array[mid] < array[low])
            (array[low], array[mid]) = (array[mid], array[low]);
        
        if (array[high] < array[low])
            (array[low], array[high]) = (array[high], array[low]);
        
        if (array[high] < array[mid])
            (array[mid], array[high]) = (array[high], array[mid]);
        
        return mid;
    }
    
    // Partition de Hoare (plus efficace)
    private static int PartitionHoare(int[] array, int low, int high)
    {
        int pivot = array[low];
        int i = low - 1;
        int j = high + 1;
        
        while (true)
        {
            do { i++; } while (array[i] < pivot);
            do { j--; } while (array[j] > pivot);
            
            if (i >= j) return j;
            
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
```

---

## **5. Tri Fusion (Merge Sort)**

### **Q: Quand préférer MergeSort à QuickSort ?**

**Cas d'usage :**
- **Stabilité requise**
- **Performance garantie** O(n log n)
- **Données externes** (trop grandes pour la mémoire)
- **Listes chaînées**

```csharp
public class MergeSort
{
    public static void Sort(int[] array)
    {
        if (array.Length <= 1) return;
        
        int[] temp = new int[array.Length];
        Sort(array, temp, 0, array.Length - 1);
    }
    
    private static void Sort(int[] array, int[] temp, int left, int right)
    {
        if (left < right)
        {
            int mid = left + (right - left) / 2;
            
            Sort(array, temp, left, mid);
            Sort(array, temp, mid + 1, right);
            Merge(array, temp, left, mid, right);
        }
    }
    
    private static void Merge(int[] array, int[] temp, int left, int mid, int right)
    {
        // Copier dans le tableau temporaire
        for (int i = left; i <= right; i++)
        {
            temp[i] = array[i];
        }
        
        int i1 = left, i2 = mid + 1, k = left;
        
        // Fusionner les deux parties
        while (i1 <= mid && i2 <= right)
        {
            if (temp[i1] <= temp[i2])
                array[k++] = temp[i1++];
            else
                array[k++] = temp[i2++];
        }
        
        // Copier les éléments restants
        while (i1 <= mid)
            array[k++] = temp[i1++];
        
        while (i2 <= right)
            array[k++] = temp[i2++];
    }
    
    // Version bottom-up (itérative) - évite la récursion
    public static void SortIterative(int[] array)
    {
        int n = array.Length;
        int[] temp = new int[n];
        
        for (int size = 1; size < n; size *= 2)
        {
            for (int left = 0; left < n - size; left += 2 * size)
            {
                int mid = left + size - 1;
                int right = Math.Min(left + 2 * size - 1, n - 1);
                
                Merge(array, temp, left, mid, right);
            }
        }
    }
    
    // Merge sort pour listes chaînées
    public class ListNode
    {
        public int val;
        public ListNode next;
        public ListNode(int val = 0, ListNode next = null)
        {
            this.val = val;
            this.next = next;
        }
    }
    
    public static ListNode SortList(ListNode head)
    {
        if (head?.next == null) return head;
        
        // Diviser la liste en deux
        ListNode mid = GetMiddle(head);
        ListNode secondHalf = mid.next;
        mid.next = null;
        
        // Trier récursivement
        ListNode left = SortList(head);
        ListNode right = SortList(secondHalf);
        
        // Fusionner
        return MergeLists(left, right);
    }
    
    private static ListNode GetMiddle(ListNode head)
    {
        ListNode slow = head, fast = head, prev = null;
        
        while (fast?.next != null)
        {
            prev = slow;
            slow = slow.next;
            fast = fast.next.next;
        }
        
        return prev;
    }
    
    private static ListNode MergeLists(ListNode l1, ListNode l2)
    {
        var dummy = new ListNode(0);
        var current = dummy;
        
        while (l1 != null && l2 != null)
        {
            if (l1.val <= l2.val)
            {
                current.next = l1;
                l1 = l1.next;
            }
            else
            {
                current.next = l2;
                l2 = l2.next;
            }
            current = current.next;
        }
        
        current.next = l1 ?? l2;
        return dummy.next;
    }
}
```

---

# **PARTIE II : THÉORIE DES GRAPHES**

## **1. Représentation des Graphes**

### **Q: Quelles sont les différentes façons de représenter un graphe ?**

```csharp
// Représentation par matrice d'adjacence
public class AdjacencyMatrixGraph
{
    private readonly bool[,] _adjacencyMatrix;
    private readonly int _vertices;
    
    public AdjacencyMatrixGraph(int vertices)
    {
        _vertices = vertices;
        _adjacencyMatrix = new bool[vertices, vertices];
    }
    
    // O(1) - Ajouter arête
    public void AddEdge(int source, int destination)
    {
        _adjacencyMatrix[source, destination] = true;
        _adjacencyMatrix[destination, source] = true; // Graphe non-orienté
    }
    
    // O(1) - Vérifier existence arête
    public bool HasEdge(int source, int destination)
    {
        return _adjacencyMatrix[source, destination];
    }
    
    // O(V) - Obtenir voisins
    public List<int> GetNeighbors(int vertex)
    {
        var neighbors = new List<int>();
        for (int i = 0; i < _vertices; i++)
        {
            if (_adjacencyMatrix[vertex, i])
                neighbors.Add(i);
        }
        return neighbors;
    }
    
    // Espace: O(V²)
    public void PrintGraph()
    {
        Console.WriteLine("Matrice d'adjacence:");
        for (int i = 0; i < _vertices; i++)
        {
            for (int j = 0; j < _vertices; j++)
            {
                Console.Write(_adjacencyMatrix[i, j] ? "1 " : "0 ");
            }
            Console.WriteLine();
        }
    }
}

// Représentation par liste d'adjacence
public class AdjacencyListGraph
{
    private readonly Dictionary<int, List<int>> _adjacencyList;
    
    public AdjacencyListGraph()
    {
        _adjacencyList = new Dictionary<int, List<int>>();
    }
    
    public void AddVertex(int vertex)
    {
        if (!_adjacencyList.ContainsKey(vertex))
            _adjacencyList[vertex] = new List<int>();
    }
    
    // O(1) - Ajouter arête
    public void AddEdge(int source, int destination)
    {
        AddVertex(source);
        AddVertex(destination);
        
        _adjacencyList[source].Add(destination);
        _adjacencyList[destination].Add(source); // Graphe non-orienté
    }
    
    // O(degree) - Vérifier existence arête
    public bool HasEdge(int source, int destination)
    {
        return _adjacencyList.ContainsKey(source) && 
               _adjacencyList[source].Contains(destination);
    }
    
    // O(1) - Obtenir voisins
    public List<int> GetNeighbors(int vertex)
    {
        return _adjacencyList.GetValueOrDefault(vertex, new List<int>());
    }
    
    // Espace: O(V + E)
    public void PrintGraph()
    {
        Console.WriteLine("Liste d'adjacence:");
        foreach (var (vertex, neighbors) in _adjacencyList)
        {
            Console.WriteLine($"{vertex}: {string.Join(", ", neighbors)}");
        }
    }
}

// Graphe pondéré avec arêtes
public class WeightedEdge
{
    public int Source { get; set; }
    public int Destination { get; set; }
    public int Weight { get; set; }
    
    public WeightedEdge(int source, int destination, int weight)
    {
        Source = source;
        Destination = destination;
        Weight = weight;
    }
}

public class WeightedGraph
{
    private readonly Dictionary<int, List<WeightedEdge>> _adjacencyList;
    
    public WeightedGraph()
    {
        _adjacencyList = new Dictionary<int, List<WeightedEdge>>();
    }
    
    public void AddEdge(int source, int destination, int weight)
    {
        if (!_adjacencyList.ContainsKey(source))
            _adjacencyList[source] = new List<WeightedEdge>();
        
        if (!_adjacencyList.ContainsKey(destination))
            _adjacencyList[destination] = new List<WeightedEdge>();
        
        _adjacencyList[source].Add(new WeightedEdge(source, destination, weight));
        _adjacencyList[destination].Add(new WeightedEdge(destination, source, weight));
    }
    
    public List<WeightedEdge> GetEdges(int vertex)
    {
        return _adjacencyList.GetValueOrDefault(vertex, new List<WeightedEdge>());
    }
}
```

---

## **2. Parcours de Graphes**

### **Q: Implémentez BFS et DFS et expliquez leurs utilisations ?**

```csharp
public class GraphTraversal
{
    // Parcours en Largeur (BFS) - O(V + E)
    public static List<int> BreadthFirstSearch(AdjacencyListGraph graph, int startVertex)
    {
        var visited = new HashSet<int>();
        var result = new List<int>();
        var queue = new Queue<int>();
        
        queue.Enqueue(startVertex);
        visited.Add(startVertex);
        
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            result.Add(current);
            
            foreach (int neighbor in graph.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        return result;
    }
    
    // BFS pour trouver le chemin le plus court (graphe non pondéré)
    public static List<int> ShortestPath(AdjacencyListGraph graph, int start, int end)
    {
        var visited = new HashSet<int>();
        var parent = new Dictionary<int, int>();
        var queue = new Queue<int>();
        
        queue.Enqueue(start);
        visited.Add(start);
        parent[start] = -1;
        
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            
            if (current == end)
            {
                // Reconstruire le chemin
                return ReconstructPath(parent, start, end);
            }
            
            foreach (int neighbor in graph.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    parent[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        return new List<int>(); // Pas de chemin trouvé
    }
    
    private static List<int> ReconstructPath(Dictionary<int, int> parent, int start, int end)
    {
        var path = new List<int>();
        int current = end;
        
        while (current != -1)
        {
            path.Add(current);
            current = parent[current];
        }
        
        path.Reverse();
        return path;
    }
    
    // Parcours en Profondeur (DFS) - Version récursive
    public static List<int> DepthFirstSearchRecursive(AdjacencyListGraph graph, int startVertex)
    {
        var visited = new HashSet<int>();
        var result = new List<int>();
        
        DFSRecursiveHelper(graph, startVertex, visited, result);
        return result;
    }
    
    private static void DFSRecursiveHelper(AdjacencyListGraph graph, int vertex, 
                                          HashSet<int> visited, List<int> result)
    {
        visited.Add(vertex);
        result.Add(vertex);
        
        foreach (int neighbor in graph.GetNeighbors(vertex))
        {
            if (!visited.Contains(neighbor))
            {
                DFSRecursiveHelper(graph, neighbor, visited, result);
            }
        }
    }
    
    // DFS Version itérative avec pile
    public static List<int> DepthFirstSearchIterative(AdjacencyListGraph graph, int startVertex)
    {
        var visited = new HashSet<int>();
        var result = new List<int>();
        var stack = new Stack<int>();
        
        stack.Push(startVertex);
        
        while (stack.Count > 0)
        {
            int current = stack.Pop();
            
            if (!visited.Contains(current))
            {
                visited.Add(current);
                result.Add(current);
                
                // Ajouter les voisins dans l'ordre inverse pour maintenir l'ordre
                var neighbors = graph.GetNeighbors(current);
                for (int i = neighbors.Count - 1; i >= 0; i--)
                {
                    if (!visited.Contains(neighbors[i]))
                    {
                        stack.Push(neighbors[i]);
                    }
                }
            }
        }
        
        return result;
    }
    
    // Détection de cycle dans un graphe non orienté
    public static bool HasCycleUndirected(AdjacencyListGraph graph, int startVertex)
    {
        var visited = new HashSet<int>();
        return HasCycleUndirectedDFS(graph, startVertex, -1, visited);
    }
    
    private static bool HasCycleUndirectedDFS(AdjacencyListGraph graph, int vertex, 
                                             int parent, HashSet<int> visited)
    {
        visited.Add(vertex);
        
        foreach (int neighbor in graph.GetNeighbors(vertex))
        {
            if (!visited.Contains(neighbor))
            {
                if (HasCycleUndirectedDFS(graph, neighbor, vertex, visited))
                    return true;
            }
            else if (neighbor != parent)
            {
                return true; // Cycle détecté
            }
        }
        
        return false;
    }
}
```

---

## **3. Algorithmes de Plus Court Chemin**

### **Q: Implémentez Dijkstra et expliquez quand l'utiliser ?**

**Utilisation :** Graphes pondérés **sans arêtes négatives**, plus court chemin depuis une source.

```csharp
public class ShortestPathAlgorithms
{
    // Algorithme de Dijkstra - O((V + E) log V) avec priority queue
    public static Dictionary<int, int> Dijkstra(WeightedGraph graph, int source)
    {
        var distances = new Dictionary<int, int>();
        var previous = new Dictionary<int, int>();
        var priorityQueue = new SortedSet<(int distance, int vertex)>();
        var vertices = GetAllVertices(graph);
        
        // Initialisation
        foreach (int vertex in vertices)
        {
            distances[vertex] = int.MaxValue;
            previous[vertex] = -1;
        }
        
        distances[source] = 0;
        priorityQueue.Add((0, source));
        
        while (priorityQueue.Count > 0)
        {
            var (currentDistance, currentVertex) = priorityQueue.Min;
            priorityQueue.Remove(priorityQueue.Min);
            
            // Si on a déjà trouvé un meilleur chemin, ignorer
            if (currentDistance > distances[currentVertex])
                continue;
            
            foreach (var edge in graph.GetEdges(currentVertex))
            {
                int neighbor = edge.Destination;
                int newDistance = distances[currentVertex] + edge.Weight;
                
                if (newDistance < distances[neighbor])
                {
                    // Retirer l'ancienne distance si elle existe
                    priorityQueue.Remove((distances[neighbor], neighbor));
                    
                    distances[neighbor] = newDistance;
                    previous[neighbor] = currentVertex;
                    priorityQueue.Add((newDistance, neighbor));
                }
            }
        }
        
        return distances;
    }
    
    // Algorithme de Bellman-Ford - O(VE) - gère les poids négatifs
    public static (Dictionary<int, int> distances, bool hasNegativeCycle) 
        BellmanFord(WeightedGraph graph, int source)
    {
        var vertices = GetAllVertices(graph);
        var edges = GetAllEdges(graph);
        var distances = new Dictionary<int, int>();
        
        // Initialisation
        foreach (int vertex in vertices)
        {
            distances[vertex] = int.MaxValue;
        }
        distances[source] = 0;
        
        // Relaxation (V-1) fois
        for (int i = 0; i < vertices.Count - 1; i++)
        {
            foreach (var edge in edges)
            {
                if (distances[edge.Source] != int.MaxValue &&
                    distances[edge.Source] + edge.Weight < distances[edge.Destination])
                {
                    distances[edge.Destination] = distances[edge.Source] + edge.Weight;
                }
            }
        }
        
        // Vérification des cycles négatifs
        foreach (var edge in edges)
        {
            if (distances[edge.Source] != int.MaxValue &&
                distances[edge.Source] + edge.Weight < distances[edge.Destination])
            {
                return (distances, true); // Cycle négatif détecté
            }
        }
        
        return (distances, false);
    }
    
    // Algorithme de Floyd-Warshall - O(V³) - tous les chemins
    public static int[,] FloydWarshall(WeightedGraph graph)
    {
        var vertices = GetAllVertices(graph).ToList();
        int n = vertices.Count;
        var dist = new int[n, n];
        
        // Mapping vertex to index
        var vertexToIndex = vertices.Select((v, i) => new { vertex = v, index = i })
                                  .ToDictionary(x => x.vertex, x => x.index);
        
        // Initialisation
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                    dist[i, j] = 0;
                else
                    dist[i, j] = int.MaxValue;
            }
        }
        
        // Ajouter les arêtes
        foreach (int vertex in vertices)
        {
            foreach (var edge in graph.GetEdges(vertex))
            {
                int i = vertexToIndex[edge.Source];
                int j = vertexToIndex[edge.Destination];
                dist[i, j] = Math.Min(dist[i, j], edge.Weight);
            }
        }
        
        // Floyd-Warshall
        for (int k = 0; k < n; k++)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (dist[i, k] != int.MaxValue && 
                        dist[k, j] != int.MaxValue &&
                        dist[i, k] + dist[k, j] < dist[i, j])
                    {
                        dist[i, j] = dist[i, k] + dist[k, j];
                    }
                }
            }
        }
        
        return dist;
    }
    
    private static HashSet<int> GetAllVertices(WeightedGraph graph)
    {
        var vertices = new HashSet<int>();
        // Implementation dépend de la structure interne du graphe
        // Ici on suppose qu'on peut obtenir tous les vertices
        return vertices;
    }
    
    private static List<WeightedEdge> GetAllEdges(WeightedGraph graph)
    {
        var edges = new List<WeightedEdge>();
        // Implementation dépend de la structure interne du graphe
        return edges;
    }
}
```

---

## **4. Arbres Couvrants Minimaux**

### **Q: Implémentez l'algorithme de Kruskal ?**

**But :** Trouver l'arbre couvrant de poids minimum dans un graphe connexe pondéré.

```csharp
// Union-Find (Disjoint Set) pour Kruskal
public class UnionFind
{
    private readonly int[] _parent;
    private readonly int[] _rank;
    
    public UnionFind(int size)
    {
        _parent = new int[size];
        _rank = new int[size];
        
        for (int i = 0; i < size; i++)
        {
            _parent[i] = i;
            _rank[i] = 0;
        }
    }
    
    // Find avec compression de chemin - O(α(n)) amortized
    public int Find(int x)
    {
        if (_parent[x] != x)
        {
            _parent[x] = Find(_parent[x]); // Compression de chemin
        }
        return _parent[x];
    }
    
    // Union par rang - O(α(n)) amortized
    public bool Union(int x, int y)
    {
        int rootX = Find(x);
        int rootY = Find(y);
        
        if (rootX == rootY)
            return false; // Déjà dans le même ensemble
        
        // Union par rang
        if (_rank[rootX] < _rank[rootY])
        {
            _parent[rootX] = rootY;
        }
        else if (_rank[rootX] > _rank[rootY])
        {
            _parent[rootY] = rootX;
        }
        else
        {
            _parent[rootY] = rootX;
            _rank[rootX]++;
        }
        
        return true;
    }
}

public class MinimumSpanningTree
{
    // Algorithme de Kruskal - O(E log E)
    public static List<WeightedEdge> Kruskal(List<WeightedEdge> edges, int vertexCount)
    {
        var mst = new List<WeightedEdge>();
        var unionFind = new UnionFind(vertexCount);
        
        // Trier les arêtes par poids
        edges.Sort((e1, e2) => e1.Weight.CompareTo(e2.Weight));
        
        foreach (var edge in edges)
        {
            // Si les sommets ne sont pas dans le même ensemble
            if (unionFind.Union(edge.Source, edge.Destination))
            {
                mst.Add(edge);
                
                // Arrêter quand on a V-1 arêtes
                if (mst.Count == vertexCount - 1)
                    break;
            }
        }
        
        return mst;
    }
    
    // Algorithme de Prim - O(E log V) avec priority queue
    public static List<WeightedEdge> Prim(WeightedGraph graph, int startVertex)
    {
        var mst = new List<WeightedEdge>();
        var visited = new HashSet<int>();
        var priorityQueue = new SortedSet<(int weight, int vertex, int parent)>();
        
        visited.Add(startVertex);
        
        // Ajouter toutes les arêtes du sommet de départ
        foreach (var edge in graph.GetEdges(startVertex))
        {
            priorityQueue.Add((edge.Weight, edge.Destination, edge.Source));
        }
        
        while (priorityQueue.Count > 0 && mst.Count < visited.Count)
        {
            var (weight, vertex, parent) = priorityQueue.Min;
            priorityQueue.Remove(priorityQueue.Min);
            
            if (visited.Contains(vertex))
                continue;
            
            // Ajouter le sommet et l'arête au MST
            visited.Add(vertex);
            mst.Add(new WeightedEdge(parent, vertex, weight));
            
            // Ajouter les nouvelles arêtes
            foreach (var edge in graph.GetEdges(vertex))
            {
                if (!visited.Contains(edge.Destination))
                {
                    priorityQueue.Add((edge.Weight, edge.Destination, vertex));
                }
            }
        }
        
        return mst;
    }
    
    // Calculer le poids total du MST
    public static int GetMSTWeight(List<WeightedEdge> mst)
    {
        return mst.Sum(edge => edge.Weight);
    }
}
```

---

## **5. Applications Pratiques**

### **Q: Résolvez le problème du voyageur de commerce (TSP) ?**

**Note :** TSP est NP-difficile, on montre plusieurs approches.

```csharp
public class TravelingSalesmanProblem
{
    // Approche brute force - O(n!)
    public static (List<int> tour, int minCost) TSPBruteForce(int[,] costMatrix)
    {
        int n = costMatrix.GetLength(0);
        var cities = Enumerable.Range(1, n - 1).ToList(); // Exclure la ville 0
        
        int minCost = int.MaxValue;
        List<int> bestTour = null;
        
        foreach (var permutation in GetPermutations(cities))
        {
            var tour = new List<int> { 0 }; // Commencer par la ville 0
            tour.AddRange(permutation);
            tour.Add(0); // Retourner à la ville 0
            
            int cost = CalculateTourCost(tour, costMatrix);
            
            if (cost < minCost)
            {
                minCost = cost;
                bestTour = new List<int>(tour);
            }
        }
        
        return (bestTour, minCost);
    }
    
    // Programmation dynamique avec masque de bits - O(n² * 2^n)
    public static (List<int> tour, int minCost) TSPDynamicProgramming(int[,] costMatrix)
    {
        int n = costMatrix.GetLength(0);
        int[,] dp = new int[1 << n, n];
        int[,] parent = new int[1 << n, n];
        
        // Initialisation
        for (int i = 0; i < (1 << n); i++)
        {
            for (int j = 0; j < n; j++)
            {
                dp[i, j] = int.MaxValue;
                parent[i, j] = -1;
            }
        }
        
        dp[1, 0] = 0; // Commencer à la ville 0
        
        // Remplir la table DP
        for (int mask = 1; mask < (1 << n); mask++)
        {
            for (int u = 0; u < n; u++)
            {
                if ((mask & (1 << u)) == 0 || dp[mask, u] == int.MaxValue)
                    continue;
                
                for (int v = 0; v < n; v++)
                {
                    if (mask & (1 << v)) continue;
                    
                    int newMask = mask | (1 << v);
                    int newCost = dp[mask, u] + costMatrix[u, v];
                    
                    if (newCost < dp[newMask, v])
                    {
                        dp[newMask, v] = newCost;
                        parent[newMask, v] = u;
                    }
                }
            }
        }
        
        // Trouver le coût minimum pour retourner à 0
        int minCost = int.MaxValue;
        int lastCity = -1;
        int finalMask = (1 << n) - 1;
        
        for (int i = 1; i < n; i++)
        {
            if (dp[finalMask, i] != int.MaxValue)
            {
                int totalCost = dp[finalMask, i] + costMatrix[i, 0];
                if (totalCost < minCost)
                {
                    minCost = totalCost;
                    lastCity = i;
                }
            }
        }
        
        // Reconstruire le tour
        var tour = ReconstructTSPTour(parent, finalMask, lastCity, n);
        return (tour, minCost);
    }
    
    // Heuristique du plus proche voisin - O(n²)
    public static (List<int> tour, int cost) TSPNearestNeighbor(int[,] costMatrix)
    {
        int n = costMatrix.GetLength(0);
        var tour = new List<int>();
        var visited = new bool[n];
        
        int current = 0;
        tour.Add(current);
        visited[current] = true;
        int totalCost = 0;
        
        for (int i = 1; i < n; i++)
        {
            int nearest = -1;
            int nearestCost = int.MaxValue;
            
            for (int j = 0; j < n; j++)
            {
                if (!visited[j] && costMatrix[current, j] < nearestCost)
                {
                    nearest = j;
                    nearestCost = costMatrix[current, j];
                }
            }
            
            if (nearest != -1)
            {
                tour.Add(nearest);
                visited[nearest] = true;
                totalCost += nearestCost;
                current = nearest;
            }
        }
        
        // Retourner au début
        tour.Add(0);
        totalCost += costMatrix[current, 0];
        
        return (tour, totalCost);
    }
    
    private static int CalculateTourCost(List<int> tour, int[,] costMatrix)
    {
        int cost = 0;
        for (int i = 0; i < tour.Count - 1; i++)
        {
            cost += costMatrix[tour[i], tour[i + 1]];
        }
        return cost;
    }
    
    private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> items)
    {
        if (!items.Any())
        {
            yield return Enumerable.Empty<T>();
            yield break;
        }
        
        foreach (var item in items)
        {
            var remainingItems = items.Where(i => !i.Equals(item));
            foreach (var permutation in GetPermutations(remainingItems))
            {
                yield return new[] { item }.Concat(permutation);
            }
        }
    }
    
    private static List<int> ReconstructTSPTour(int[,] parent, int mask, int lastCity, int n)
    {
        var tour = new List<int>();
        int current = lastCity;
        
        while (parent[mask, current] != -1)
        {
            tour.Add(current);
            int prev = parent[mask, current];
            mask ^= (1 << current);
            current = prev;
        }
        
        tour.Add(0);
        tour.Reverse();
        tour.Add(0); // Retourner au début
        
        return tour;
    }
}

// Exemple d'utilisation et tests
public class GraphAlgorithmsDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("=== DÉMONSTRATION ALGORITHMES DE TRI ===");
        
        int[] data = { 64, 34, 25, 12, 22, 11, 90 };
        Console.WriteLine($"Données originales: [{string.Join(", ", data)}]");
        
        // Test des algorithmes de tri
        var bubbleData = (int[])data.Clone();
        BubbleSort.Sort(bubbleData);
        Console.WriteLine($"Tri à bulles: [{string.Join(", ", bubbleData)}]");
        
        var insertionData = (int[])data.Clone();
        InsertionSort.Sort(insertionData);
        Console.WriteLine($"Tri par insertion: [{string.Join(", ", insertionData)}]");
        
        Console.WriteLine("\n=== DÉMONSTRATION THÉORIE DES GRAPHES ===");
        
        // Créer un graphe simple
        var graph = new AdjacencyListGraph();
        graph.AddEdge(0, 1);
        graph.AddEdge(0, 2);
        graph.AddEdge(1, 3);
        graph.AddEdge(2, 3);
        graph.AddEdge(3, 4);
        
        Console.WriteLine("Graphe créé:");
        graph.PrintGraph();
        
        // Test BFS et DFS
        var bfsResult = GraphTraversal.BreadthFirstSearch(graph, 0);
        Console.WriteLine($"BFS depuis 0: [{string.Join(", ", bfsResult)}]");
        
        var dfsResult = GraphTraversal.DepthFirstSearchRecursive(graph, 0);
        Console.WriteLine($"DFS depuis 0: [{string.Join(", ", dfsResult)}]");
        
        // Test plus court chemin
        var shortestPath = GraphTraversal.ShortestPath(graph, 0, 4);
        Console.WriteLine($"Plus court chemin 0→4: [{string.Join(" → ", shortestPath)}]");
    }
}
```

Cette documentation complète vous donne tous les outils nécessaires pour maîtriser les algorithmes de tri et la théorie des graphes lors de vos entretiens techniques ! 🚀
