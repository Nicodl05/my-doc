# **Théorie des Graphes en C# - Guide Technique**

> **🔗 Références :**
> - [Algorithmes de Tri](./Algorithmes%20de%20Tri.md) - Tri topologique et algorithmes
> - [Complexité Algorithmique](./Complexité%20Algorithmique%20et%20Performance.md) - Analyse de performance
> - [Collections](./boucles%20et%20collections.md) - Structures de données pour graphes
> - [Design Patterns](./Design%20Patterns.md) - Patterns pour graphes

---

# **THÉORIE DES GRAPHES ET ALGORITHMES**

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
    
    public override string ToString() => $"{Source} -> {Destination} (w: {Weight})";
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
    
    public HashSet<int> GetAllVertices()
    {
        return _adjacencyList.Keys.ToHashSet();
    }
    
    public List<WeightedEdge> GetAllEdges()
    {
        var edges = new List<WeightedEdge>();
        var processed = new HashSet<(int, int)>();
        
        foreach (var (vertex, edgeList) in _adjacencyList)
        {
            foreach (var edge in edgeList)
            {
                var edgePair = (Math.Min(edge.Source, edge.Destination), 
                               Math.Max(edge.Source, edge.Destination));
                
                if (!processed.Contains(edgePair))
                {
                    edges.Add(edge);
                    processed.Add(edgePair);
                }
            }
        }
        
        return edges;
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
    
    // Détection de cycle dans un graphe orienté
    public static bool HasCycleDirected(AdjacencyListGraph graph)
    {
        var visited = new HashSet<int>();
        var recursionStack = new HashSet<int>();
        
        foreach (int vertex in graph.GetAllVertices())
        {
            if (!visited.Contains(vertex))
            {
                if (HasCycleDirectedDFS(graph, vertex, visited, recursionStack))
                    return true;
            }
        }
        
        return false;
    }
    
    private static bool HasCycleDirectedDFS(AdjacencyListGraph graph, int vertex,
                                           HashSet<int> visited, HashSet<int> recursionStack)
    {
        visited.Add(vertex);
        recursionStack.Add(vertex);
        
        foreach (int neighbor in graph.GetNeighbors(vertex))
        {
            if (!visited.Contains(neighbor))
            {
                if (HasCycleDirectedDFS(graph, neighbor, visited, recursionStack))
                    return true;
            }
            else if (recursionStack.Contains(neighbor))
            {
                return true; // Back edge trouvé = cycle
            }
        }
        
        recursionStack.Remove(vertex);
        return false;
    }
}

// Extension pour obtenir tous les vertices d'un graphe
public static class GraphExtensions
{
    public static HashSet<int> GetAllVertices(this AdjacencyListGraph graph)
    {
        // Cette méthode devrait être ajoutée à la classe AdjacencyListGraph
        var vertices = new HashSet<int>();
        // Implementation dépend de l'accès aux données internes
        return vertices;
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
        var vertices = graph.GetAllVertices();
        
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
    
    // Version avec PriorityQueue de .NET 6+
    public static Dictionary<int, int> DijkstraPriorityQueue(WeightedGraph graph, int source)
    {
        var distances = new Dictionary<int, int>();
        var priorityQueue = new PriorityQueue<int, int>();
        var vertices = graph.GetAllVertices();
        
        // Initialisation
        foreach (int vertex in vertices)
        {
            distances[vertex] = int.MaxValue;
        }
        
        distances[source] = 0;
        priorityQueue.Enqueue(source, 0);
        
        while (priorityQueue.Count > 0)
        {
            int currentVertex = priorityQueue.Dequeue();
            
            foreach (var edge in graph.GetEdges(currentVertex))
            {
                int neighbor = edge.Destination;
                int newDistance = distances[currentVertex] + edge.Weight;
                
                if (newDistance < distances[neighbor])
                {
                    distances[neighbor] = newDistance;
                    priorityQueue.Enqueue(neighbor, newDistance);
                }
            }
        }
        
        return distances;
    }
    
    // Algorithme de Bellman-Ford - O(VE) - gère les poids négatifs
    public static (Dictionary<int, int> distances, bool hasNegativeCycle) 
        BellmanFord(WeightedGraph graph, int source)
    {
        var vertices = graph.GetAllVertices();
        var edges = graph.GetAllEdges();
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
        var vertices = graph.GetAllVertices().ToList();
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
    
    // A* Algorithm pour pathfinding avec heuristique
    public static List<int> AStar(WeightedGraph graph, int start, int goal, 
                                 Func<int, int, int> heuristic)
    {
        var openSet = new PriorityQueue<int, int>();
        var cameFrom = new Dictionary<int, int>();
        var gScore = new Dictionary<int, int>();
        var fScore = new Dictionary<int, int>();
        
        var vertices = graph.GetAllVertices();
        
        foreach (int vertex in vertices)
        {
            gScore[vertex] = int.MaxValue;
            fScore[vertex] = int.MaxValue;
        }
        
        gScore[start] = 0;
        fScore[start] = heuristic(start, goal);
        openSet.Enqueue(start, fScore[start]);
        
        while (openSet.Count > 0)
        {
            int current = openSet.Dequeue();
            
            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }
            
            foreach (var edge in graph.GetEdges(current))
            {
                int neighbor = edge.Destination;
                int tentativeGScore = gScore[current] + edge.Weight;
                
                if (tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + heuristic(neighbor, goal);
                    
                    openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }
        
        return new List<int>(); // Pas de chemin trouvé
    }
    
    private static List<int> ReconstructPath(Dictionary<int, int> cameFrom, int current)
    {
        var path = new List<int> { current };
        
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        
        return path;
    }
}
```

---

## **4. Arbres Couvrants Minimaux (MST)**

### **Q: Implémentez l'algorithme de Kruskal et Prim ?**

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
    
    public bool IsConnected(int x, int y)
    {
        return Find(x) == Find(y);
    }
}

public class MinimumSpanningTree
{
    // Algorithme de Kruskal - O(E log E)
    public static List<WeightedEdge> Kruskal(WeightedGraph graph)
    {
        var mst = new List<WeightedEdge>();
        var edges = graph.GetAllEdges();
        var vertices = graph.GetAllVertices();
        var unionFind = new UnionFind(vertices.Max() + 1);
        
        // Trier les arêtes par poids
        edges.Sort((e1, e2) => e1.Weight.CompareTo(e2.Weight));
        
        foreach (var edge in edges)
        {
            // Si les sommets ne sont pas dans le même ensemble
            if (unionFind.Union(edge.Source, edge.Destination))
            {
                mst.Add(edge);
                
                // Arrêter quand on a V-1 arêtes
                if (mst.Count == vertices.Count - 1)
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
        var priorityQueue = new PriorityQueue<WeightedEdge, int>();
        
        visited.Add(startVertex);
        
        // Ajouter toutes les arêtes du sommet de départ
        foreach (var edge in graph.GetEdges(startVertex))
        {
            priorityQueue.Enqueue(edge, edge.Weight);
        }
        
        while (priorityQueue.Count > 0 && mst.Count < visited.Count)
        {
            var edge = priorityQueue.Dequeue();
            
            if (visited.Contains(edge.Destination))
                continue;
            
            // Ajouter le sommet et l'arête au MST
            visited.Add(edge.Destination);
            mst.Add(edge);
            
            // Ajouter les nouvelles arêtes
            foreach (var newEdge in graph.GetEdges(edge.Destination))
            {
                if (!visited.Contains(newEdge.Destination))
                {
                    priorityQueue.Enqueue(newEdge, newEdge.Weight);
                }
            }
        }
        
        return mst;
    }
    
    // Version alternative de Prim avec matrice d'adjacence - O(V²)
    public static List<WeightedEdge> PrimMatrix(int[,] adjacencyMatrix)
    {
        int vertices = adjacencyMatrix.GetLength(0);
        var mst = new List<WeightedEdge>();
        var inMST = new bool[vertices];
        var key = new int[vertices];
        var parent = new int[vertices];
        
        // Initialisation
        for (int i = 0; i < vertices; i++)
        {
            key[i] = int.MaxValue;
            parent[i] = -1;
        }
        
        key[0] = 0; // Commencer par le vertex 0
        
        for (int count = 0; count < vertices - 1; count++)
        {
            // Trouver le vertex avec la clé minimum
            int u = MinKey(key, inMST);
            inMST[u] = true;
            
            // Mettre à jour les clés des vertices adjacents
            for (int v = 0; v < vertices; v++)
            {
                if (adjacencyMatrix[u, v] != 0 && !inMST[v] && 
                    adjacencyMatrix[u, v] < key[v])
                {
                    parent[v] = u;
                    key[v] = adjacencyMatrix[u, v];
                }
            }
        }
        
        // Construire le MST
        for (int i = 1; i < vertices; i++)
        {
            if (parent[i] != -1)
            {
                mst.Add(new WeightedEdge(parent[i], i, adjacencyMatrix[parent[i], i]));
            }
        }
        
        return mst;
    }
    
    private static int MinKey(int[] key, bool[] inMST)
    {
        int min = int.MaxValue;
        int minIndex = -1;
        
        for (int v = 0; v < key.Length; v++)
        {
            if (!inMST[v] && key[v] < min)
            {
                min = key[v];
                minIndex = v;
            }
        }
        
        return minIndex;
    }
    
    // Calculer le poids total du MST
    public static int GetMSTWeight(List<WeightedEdge> mst)
    {
        return mst.Sum(edge => edge.Weight);
    }
    
    // Vérifier si le graphe est connexe (prérequis pour MST)
    public static bool IsConnected(WeightedGraph graph)
    {
        var vertices = graph.GetAllVertices();
        if (vertices.Count == 0) return true;
        
        var visited = new HashSet<int>();
        var stack = new Stack<int>();
        
        int startVertex = vertices.First();
        stack.Push(startVertex);
        
        while (stack.Count > 0)
        {
            int current = stack.Pop();
            if (visited.Contains(current)) continue;
            
            visited.Add(current);
            
            foreach (var edge in graph.GetEdges(current))
            {
                if (!visited.Contains(edge.Destination))
                {
                    stack.Push(edge.Destination);
                }
            }
        }
        
        return visited.Count == vertices.Count;
    }
}
```

---

## **5. Tri Topologique**

### **Q: Implémentez le tri topologique et expliquez son utilisation ?**

**Utilisation :** DAG (Directed Acyclic Graph) - ordonnancement de tâches, dépendances, etc.

```csharp
public class TopologicalSort
{
    // Tri topologique avec DFS - O(V + E)
    public static List<int> TopologicalSortDFS(AdjacencyListGraph graph)
    {
        var visited = new HashSet<int>();
        var stack = new Stack<int>();
        var vertices = graph.GetAllVertices();
        
        foreach (int vertex in vertices)
        {
            if (!visited.Contains(vertex))
            {
                TopologicalSortUtil(graph, vertex, visited, stack);
            }
        }
        
        return stack.ToList(); // L'ordre topologique
    }
    
    private static void TopologicalSortUtil(AdjacencyListGraph graph, int vertex,
                                           HashSet<int> visited, Stack<int> stack)
    {
        visited.Add(vertex);
        
        foreach (int neighbor in graph.GetNeighbors(vertex))
        {
            if (!visited.Contains(neighbor))
            {
                TopologicalSortUtil(graph, neighbor, visited, stack);
            }
        }
        
        stack.Push(vertex); // Ajouter après avoir visité tous les descendants
    }
    
    // Tri topologique avec Kahn's Algorithm (BFS) - O(V + E)
    public static List<int> TopologicalSortKahn(AdjacencyListGraph graph)
    {
        var vertices = graph.GetAllVertices();
        var inDegree = new Dictionary<int, int>();
        var result = new List<int>();
        var queue = new Queue<int>();
        
        // Calculer les degrés entrants
        foreach (int vertex in vertices)
        {
            inDegree[vertex] = 0;
        }
        
        foreach (int vertex in vertices)
        {
            foreach (int neighbor in graph.GetNeighbors(vertex))
            {
                inDegree[neighbor]++;
            }
        }
        
        // Ajouter tous les vertices avec degré entrant 0
        foreach (var (vertex, degree) in inDegree)
        {
            if (degree == 0)
            {
                queue.Enqueue(vertex);
            }
        }
        
        // Traitement BFS
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            result.Add(current);
            
            foreach (int neighbor in graph.GetNeighbors(current))
            {
                inDegree[neighbor]--;
                
                if (inDegree[neighbor] == 0)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        // Vérifier s'il y a un cycle
        if (result.Count != vertices.Count)
        {
            throw new InvalidOperationException("Le graphe contient un cycle - tri topologique impossible");
        }
        
        return result;
    }
    
    // Détection de cycle et tri topologique combinés
    public static (bool hasCycle, List<int> topologicalOrder) SafeTopologicalSort(AdjacencyListGraph graph)
    {
        var visited = new HashSet<int>();
        var recursionStack = new HashSet<int>();
        var stack = new Stack<int>();
        var vertices = graph.GetAllVertices();
        
        foreach (int vertex in vertices)
        {
            if (!visited.Contains(vertex))
            {
                if (HasCycleAndSort(graph, vertex, visited, recursionStack, stack))
                {
                    return (true, new List<int>()); // Cycle détecté
                }
            }
        }
        
        return (false, stack.ToList());
    }
    
    private static bool HasCycleAndSort(AdjacencyListGraph graph, int vertex,
                                       HashSet<int> visited, HashSet<int> recursionStack,
                                       Stack<int> stack)
    {
        visited.Add(vertex);
        recursionStack.Add(vertex);
        
        foreach (int neighbor in graph.GetNeighbors(vertex))
        {
            if (!visited.Contains(neighbor))
            {
                if (HasCycleAndSort(graph, neighbor, visited, recursionStack, stack))
                    return true;
            }
            else if (recursionStack.Contains(neighbor))
            {
                return true; // Cycle détecté
            }
        }
        
        recursionStack.Remove(vertex);
        stack.Push(vertex);
        return false;
    }
}

// Application pratique : Ordonnancement de tâches
public class TaskScheduler
{
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> Dependencies { get; set; } = new();
        public TimeSpan Duration { get; set; }
        
        public override string ToString() => $"Task {Id}: {Name}";
    }
    
    public static List<Task> ScheduleTasks(List<Task> tasks)
    {
        var graph = new AdjacencyListGraph();
        var taskMap = tasks.ToDictionary(t => t.Id, t => t);
        
        // Construire le graphe de dépendances
        foreach (var task in tasks)
        {
            graph.AddVertex(task.Id);
            
            foreach (int dependency in task.Dependencies)
            {
                graph.AddEdge(dependency, task.Id); // dependency -> task
            }
        }
        
        // Tri topologique
        try
        {
            var order = TopologicalSort.TopologicalSortKahn(graph);
            return order.Select(id => taskMap[id]).ToList();
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("Dépendances circulaires détectées dans les tâches");
        }
    }
    
    public static TimeSpan CalculateCriticalPath(List<Task> tasks)
    {
        var scheduledTasks = ScheduleTasks(tasks);
        var taskMap = tasks.ToDictionary(t => t.Id, t => t);
        var earliestStart = new Dictionary<int, TimeSpan>();
        
        foreach (var task in scheduledTasks)
        {
            TimeSpan maxDependencyEnd = TimeSpan.Zero;
            
            foreach (int depId in task.Dependencies)
            {
                TimeSpan depEnd = earliestStart[depId] + taskMap[depId].Duration;
                if (depEnd > maxDependencyEnd)
                    maxDependencyEnd = depEnd;
            }
            
            earliestStart[task.Id] = maxDependencyEnd;
        }
        
        // Trouver la fin la plus tardive
        TimeSpan criticalPathLength = TimeSpan.Zero;
        foreach (var task in tasks)
        {
            TimeSpan taskEnd = earliestStart[task.Id] + task.Duration;
            if (taskEnd > criticalPathLength)
                criticalPathLength = taskEnd;
        }
        
        return criticalPathLength;
    }
}
```

---

## **6. Applications Avancées**

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
    
    // Optimisation 2-opt
    public static (List<int> tour, int cost) TSP2Opt(int[,] costMatrix, List<int> initialTour = null)
    {
        int n = costMatrix.GetLength(0);
        
        // Tour initial
        var tour = initialTour ?? TSPNearestNeighbor(costMatrix).tour;
        bool improved = true;
        
        while (improved)
        {
            improved = false;
            
            for (int i = 1; i < tour.Count - 2; i++)
            {
                for (int j = i + 1; j < tour.Count - 1; j++)
                {
                    if (j - i == 1) continue; // Arêtes adjacentes
                    
                    var newTour = TwoOptSwap(tour, i, j);
                    int currentCost = CalculateTourCost(tour, costMatrix);
                    int newCost = CalculateTourCost(newTour, costMatrix);
                    
                    if (newCost < currentCost)
                    {
                        tour = newTour;
                        improved = true;
                    }
                }
            }
        }
        
        return (tour, CalculateTourCost(tour, costMatrix));
    }
    
    private static List<int> TwoOptSwap(List<int> tour, int i, int j)
    {
        var newTour = new List<int>();
        
        // Ajouter la première partie
        for (int k = 0; k <= i; k++)
            newTour.Add(tour[k]);
        
        // Inverser la partie du milieu
        for (int k = j; k >= i + 1; k--)
            newTour.Add(tour[k]);
        
        // Ajouter la dernière partie
        for (int k = j + 1; k < tour.Count; k++)
            newTour.Add(tour[k]);
        
        return newTour;
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
```

---

## **📊 Résumé des Complexités**

| Algorithme | Complexité Temporelle | Complexité Spatiale | Cas d'Usage |
|------------|----------------------|-------------------|-------------|
| **Parcours** |
| BFS | O(V + E) | O(V) | Plus court chemin non pondéré |
| DFS | O(V + E) | O(V) | Détection de cycle, topologique |
| **Plus Courts Chemins** |
| Dijkstra | O((V + E) log V) | O(V) | Poids positifs, une source |
| Bellman-Ford | O(VE) | O(V) | Poids négatifs, une source |
| Floyd-Warshall | O(V³) | O(V²) | Tous les chemins |
| A* | O(b^d) | O(b^d) | Pathfinding avec heuristique |
| **MST** |
| Kruskal | O(E log E) | O(V) | Graphes épars |
| Prim | O(E log V) | O(V) | Graphes denses |
| **Topologique** |
| DFS | O(V + E) | O(V) | Tri topologique |
| Kahn | O(V + E) | O(V) | Détection de cycle + tri |

Cette documentation complète vous donne toutes les clés pour maîtriser la théorie des graphes en C# ! 🚀
