# **Structures de Données Avancées en C# - Guide Technique**

> **🔗 Références :**
> - [Collections de Base](./boucles%20et%20collections.md) - List, Dictionary, HashSet
> - [Complexité Algorithmique](./Complexité%20Algorithmique%20et%20Performance.md) - Analyse de performance
> - [Algorithmes de Tri](./Algorithmes%20de%20Tri.md) - Heap et tri basé sur structures
> - [Théorie des Graphes](./Théorie%20des%20Graphes.md) - Union-Find et structures pour graphes

---

# **STRUCTURES DE DONNÉES AVANCÉES**

## **1. Heap (Tas) et Priority Queue**

### **Q: Implémentez un Heap Min et Max en C# ?**

```csharp
public class MinHeap<T> where T : IComparable<T>
{
    private readonly List<T> _heap;
    
    public MinHeap()
    {
        _heap = new List<T>();
    }
    
    public MinHeap(IEnumerable<T> collection)
    {
        _heap = new List<T>(collection);
        
        // Heapify bottom-up
        for (int i = Parent(_heap.Count - 1); i >= 0; i--)
        {
            HeapifyDown(i);
        }
    }
    
    public int Count => _heap.Count;
    public bool IsEmpty => _heap.Count == 0;
    
    // O(log n) - Insérer un élément
    public void Insert(T item)
    {
        _heap.Add(item);
        HeapifyUp(_heap.Count - 1);
    }
    
    // O(log n) - Extraire le minimum
    public T ExtractMin()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Heap is empty");
        
        T min = _heap[0];
        
        // Déplacer le dernier élément à la racine
        _heap[0] = _heap[_heap.Count - 1];
        _heap.RemoveAt(_heap.Count - 1);
        
        if (!IsEmpty)
            HeapifyDown(0);
        
        return min;
    }
    
    // O(1) - Voir le minimum sans l'extraire
    public T Peek()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Heap is empty");
        
        return _heap[0];
    }
    
    // O(log n) - Supprimer un élément à un index donné
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _heap.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        
        _heap[index] = _heap[_heap.Count - 1];
        _heap.RemoveAt(_heap.Count - 1);
        
        if (index < _heap.Count)
        {
            HeapifyUp(index);
            HeapifyDown(index);
        }
    }
    
    // O(n) - Trouver et supprimer un élément
    public bool Remove(T item)
    {
        int index = _heap.IndexOf(item);
        if (index == -1) return false;
        
        RemoveAt(index);
        return true;
    }
    
    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = Parent(index);
            
            if (_heap[index].CompareTo(_heap[parentIndex]) >= 0)
                break;
            
            Swap(index, parentIndex);
            index = parentIndex;
        }
    }
    
    private void HeapifyDown(int index)
    {
        while (true)
        {
            int smallest = index;
            int leftChild = LeftChild(index);
            int rightChild = RightChild(index);
            
            if (leftChild < _heap.Count && 
                _heap[leftChild].CompareTo(_heap[smallest]) < 0)
                smallest = leftChild;
            
            if (rightChild < _heap.Count && 
                _heap[rightChild].CompareTo(_heap[smallest]) < 0)
                smallest = rightChild;
            
            if (smallest == index)
                break;
            
            Swap(index, smallest);
            index = smallest;
        }
    }
    
    private void Swap(int i, int j)
    {
        (_heap[i], _heap[j]) = (_heap[j], _heap[i]);
    }
    
    private static int Parent(int index) => (index - 1) / 2;
    private static int LeftChild(int index) => 2 * index + 1;
    private static int RightChild(int index) => 2 * index + 2;
    
    public IEnumerable<T> GetSortedElements()
    {
        var copy = new MinHeap<T>(_heap);
        while (!copy.IsEmpty)
        {
            yield return copy.ExtractMin();
        }
    }
}

// Max Heap - inverse la logique de comparaison
public class MaxHeap<T> where T : IComparable<T>
{
    private readonly MinHeap<T> _minHeap;
    
    public MaxHeap()
    {
        _minHeap = new MinHeap<T>();
    }
    
    public MaxHeap(IEnumerable<T> collection, IComparer<T> comparer = null)
    {
        // Utiliser un comparateur inversé
        var reversed = collection.Select(x => new ReverseComparer<T>(x, comparer));
        // Implementation avec comparateur personnalisé...
    }
    
    public int Count => _minHeap.Count;
    public bool IsEmpty => _minHeap.IsEmpty;
    
    public void Insert(T item) => _minHeap.Insert(item);
    public T ExtractMax() => _minHeap.ExtractMin();
    public T Peek() => _minHeap.Peek();
}

// Wrapper pour inverser la comparaison
public class ReverseComparer<T> : IComparable<ReverseComparer<T>> where T : IComparable<T>
{
    public T Value { get; }
    private readonly IComparer<T> _comparer;
    
    public ReverseComparer(T value, IComparer<T> comparer = null)
    {
        Value = value;
        _comparer = comparer ?? Comparer<T>.Default;
    }
    
    public int CompareTo(ReverseComparer<T> other)
    {
        return _comparer.Compare(other.Value, Value); // Ordre inversé
    }
}
```

---

## **2. Trie (Arbre de Préfixes)**

### **Q: Implémentez un Trie pour la recherche de mots ?**

```csharp
public class TrieNode
{
    public Dictionary<char, TrieNode> Children { get; }
    public bool IsEndOfWord { get; set; }
    public string Word { get; set; } // Optionnel : stocker le mot complet
    
    public TrieNode()
    {
        Children = new Dictionary<char, TrieNode>();
        IsEndOfWord = false;
    }
}

public class Trie
{
    private readonly TrieNode _root;
    
    public Trie()
    {
        _root = new TrieNode();
    }
    
    // O(m) où m = longueur du mot
    public void Insert(string word)
    {
        if (string.IsNullOrEmpty(word))
            return;
        
        TrieNode current = _root;
        
        foreach (char c in word.ToLower())
        {
            if (!current.Children.ContainsKey(c))
            {
                current.Children[c] = new TrieNode();
            }
            current = current.Children[c];
        }
        
        current.IsEndOfWord = true;
        current.Word = word; // Stockage optionnel
    }
    
    // O(m) - Rechercher un mot exact
    public bool Search(string word)
    {
        TrieNode node = SearchNode(word);
        return node != null && node.IsEndOfWord;
    }
    
    // O(m) - Vérifier si un préfixe existe
    public bool StartsWith(string prefix)
    {
        return SearchNode(prefix) != null;
    }
    
    private TrieNode SearchNode(string word)
    {
        if (string.IsNullOrEmpty(word))
            return null;
        
        TrieNode current = _root;
        
        foreach (char c in word.ToLower())
        {
            if (!current.Children.ContainsKey(c))
                return null;
            
            current = current.Children[c];
        }
        
        return current;
    }
    
    // Supprimer un mot
    public bool Delete(string word)
    {
        return DeleteHelper(_root, word.ToLower(), 0);
    }
    
    private bool DeleteHelper(TrieNode node, string word, int index)
    {
        if (index == word.Length)
        {
            if (!node.IsEndOfWord)
                return false; // Mot n'existe pas
            
            node.IsEndOfWord = false;
            node.Word = null;
            
            // Retourner true si le nœud n'a pas d'enfants
            return node.Children.Count == 0;
        }
        
        char c = word[index];
        if (!node.Children.ContainsKey(c))
            return false; // Mot n'existe pas
        
        TrieNode childNode = node.Children[c];
        bool shouldDeleteChild = DeleteHelper(childNode, word, index + 1);
        
        if (shouldDeleteChild)
        {
            node.Children.Remove(c);
            
            // Retourner true si le nœud actuel n'a pas d'enfants et n'est pas une fin de mot
            return !node.IsEndOfWord && node.Children.Count == 0;
        }
        
        return false;
    }
    
    // Autocomplete - trouver tous les mots avec un préfixe
    public List<string> GetWordsWithPrefix(string prefix)
    {
        var results = new List<string>();
        TrieNode prefixNode = SearchNode(prefix);
        
        if (prefixNode != null)
        {
            CollectWords(prefixNode, prefix, results);
        }
        
        return results;
    }
    
    private void CollectWords(TrieNode node, string currentWord, List<string> results)
    {
        if (node.IsEndOfWord)
        {
            results.Add(currentWord);
        }
        
        foreach (var (c, childNode) in node.Children)
        {
            CollectWords(childNode, currentWord + c, results);
        }
    }
    
    // Recherche avec wildcards (? = n'importe quel caractère)
    public List<string> SearchWithWildcard(string pattern)
    {
        var results = new List<string>();
        SearchWildcardHelper(_root, pattern, 0, "", results);
        return results;
    }
    
    private void SearchWildcardHelper(TrieNode node, string pattern, int index, 
                                     string currentWord, List<string> results)
    {
        if (index == pattern.Length)
        {
            if (node.IsEndOfWord)
                results.Add(currentWord);
            return;
        }
        
        char c = pattern[index];
        
        if (c == '?')
        {
            // Wildcard - essayer tous les caractères possibles
            foreach (var (ch, childNode) in node.Children)
            {
                SearchWildcardHelper(childNode, pattern, index + 1, 
                                   currentWord + ch, results);
            }
        }
        else
        {
            // Caractère normal
            if (node.Children.ContainsKey(c))
            {
                SearchWildcardHelper(node.Children[c], pattern, index + 1, 
                                   currentWord + c, results);
            }
        }
    }
    
    // Compter le nombre de mots dans le Trie
    public int CountWords()
    {
        return CountWordsHelper(_root);
    }
    
    private int CountWordsHelper(TrieNode node)
    {
        int count = node.IsEndOfWord ? 1 : 0;
        
        foreach (var childNode in node.Children.Values)
        {
            count += CountWordsHelper(childNode);
        }
        
        return count;
    }
}

// Trie compressé (Patricia Tree / Radix Tree)
public class CompressedTrie
{
    private class Node
    {
        public Dictionary<string, Node> Children { get; }
        public bool IsEndOfWord { get; set; }
        public string Word { get; set; }
        
        public Node()
        {
            Children = new Dictionary<string, Node>();
        }
    }
    
    private readonly Node _root;
    
    public CompressedTrie()
    {
        _root = new Node();
    }
    
    public void Insert(string word)
    {
        if (string.IsNullOrEmpty(word)) return;
        
        InsertHelper(_root, word.ToLower(), word);
    }
    
    private void InsertHelper(Node node, string remaining, string originalWord)
    {
        if (string.IsNullOrEmpty(remaining))
        {
            node.IsEndOfWord = true;
            node.Word = originalWord;
            return;
        }
        
        // Chercher une arête qui commence par le même caractère
        foreach (var (edge, childNode) in node.Children)
        {
            int commonPrefixLength = GetCommonPrefixLength(edge, remaining);
            
            if (commonPrefixLength > 0)
            {
                if (commonPrefixLength == edge.Length)
                {
                    // L'arête entière est un préfixe du mot restant
                    InsertHelper(childNode, remaining.Substring(commonPrefixLength), originalWord);
                    return;
                }
                else
                {
                    // Diviser l'arête
                    SplitEdge(node, edge, childNode, commonPrefixLength);
                    InsertHelper(node, remaining, originalWord);
                    return;
                }
            }
        }
        
        // Aucune arête correspondante trouvée, créer une nouvelle
        var newNode = new Node();
        node.Children[remaining] = newNode;
        newNode.IsEndOfWord = true;
        newNode.Word = originalWord;
    }
    
    private void SplitEdge(Node parent, string edge, Node child, int splitPoint)
    {
        string commonPrefix = edge.Substring(0, splitPoint);
        string remainder = edge.Substring(splitPoint);
        
        // Retirer l'ancienne arête
        parent.Children.Remove(edge);
        
        // Créer un nouveau nœud intermédiaire
        var middleNode = new Node();
        parent.Children[commonPrefix] = middleNode;
        middleNode.Children[remainder] = child;
    }
    
    private int GetCommonPrefixLength(string s1, string s2)
    {
        int i = 0;
        int minLength = Math.Min(s1.Length, s2.Length);
        
        while (i < minLength && s1[i] == s2[i])
            i++;
        
        return i;
    }
    
    public bool Search(string word)
    {
        var node = SearchNode(word.ToLower());
        return node != null && node.IsEndOfWord;
    }
    
    private Node SearchNode(string word)
    {
        Node current = _root;
        string remaining = word;
        
        while (!string.IsNullOrEmpty(remaining))
        {
            bool found = false;
            
            foreach (var (edge, childNode) in current.Children)
            {
                if (remaining.StartsWith(edge))
                {
                    remaining = remaining.Substring(edge.Length);
                    current = childNode;
                    found = true;
                    break;
                }
            }
            
            if (!found)
                return null;
        }
        
        return current;
    }
}
```

---

## **3. Segment Tree (Arbre de Segments)**

### **Q: Implémentez un Segment Tree pour les requêtes de somme et mise à jour ?**

```csharp
public class SegmentTree
{
    private readonly int[] _tree;
    private readonly int _n;
    
    public SegmentTree(int[] array)
    {
        _n = array.Length;
        _tree = new int[4 * _n]; // Taille suffisante pour tous les cas
        
        Build(array, 0, 0, _n - 1);
    }
    
    // O(n) - Construire l'arbre
    private void Build(int[] array, int node, int start, int end)
    {
        if (start == end)
        {
            _tree[node] = array[start];
        }
        else
        {
            int mid = (start + end) / 2;
            int leftChild = 2 * node + 1;
            int rightChild = 2 * node + 2;
            
            Build(array, leftChild, start, mid);
            Build(array, rightChild, mid + 1, end);
            
            _tree[node] = _tree[leftChild] + _tree[rightChild];
        }
    }
    
    // O(log n) - Mettre à jour une valeur
    public void Update(int index, int value)
    {
        Update(0, 0, _n - 1, index, value);
    }
    
    private void Update(int node, int start, int end, int index, int value)
    {
        if (start == end)
        {
            _tree[node] = value;
        }
        else
        {
            int mid = (start + end) / 2;
            int leftChild = 2 * node + 1;
            int rightChild = 2 * node + 2;
            
            if (index <= mid)
            {
                Update(leftChild, start, mid, index, value);
            }
            else
            {
                Update(rightChild, mid + 1, end, index, value);
            }
            
            _tree[node] = _tree[leftChild] + _tree[rightChild];
        }
    }
    
    // O(log n) - Requête de somme sur un intervalle
    public int Query(int left, int right)
    {
        return Query(0, 0, _n - 1, left, right);
    }
    
    private int Query(int node, int start, int end, int left, int right)
    {
        if (right < start || left > end)
        {
            return 0; // Pas d'intersection
        }
        
        if (left <= start && end <= right)
        {
            return _tree[node]; // Complètement inclus
        }
        
        // Intersection partielle
        int mid = (start + end) / 2;
        int leftChild = 2 * node + 1;
        int rightChild = 2 * node + 2;
        
        int leftSum = Query(leftChild, start, mid, left, right);
        int rightSum = Query(rightChild, mid + 1, end, left, right);
        
        return leftSum + rightSum;
    }
    
    // Mise à jour par intervalle avec lazy propagation
    public void UpdateRange(int left, int right, int delta)
    {
        UpdateRange(0, 0, _n - 1, left, right, delta);
    }
    
    private void UpdateRange(int node, int start, int end, int left, int right, int delta)
    {
        if (right < start || left > end)
            return;
        
        if (left <= start && end <= right)
        {
            _tree[node] += delta * (end - start + 1);
            // Marquer pour lazy propagation si nécessaire
            return;
        }
        
        int mid = (start + end) / 2;
        int leftChild = 2 * node + 1;
        int rightChild = 2 * node + 2;
        
        UpdateRange(leftChild, start, mid, left, right, delta);
        UpdateRange(rightChild, mid + 1, end, left, right, delta);
        
        _tree[node] = _tree[leftChild] + _tree[rightChild];
    }
}

// Segment Tree générique avec opérations personnalisées
public class GenericSegmentTree<T>
{
    private readonly T[] _tree;
    private readonly int _n;
    private readonly Func<T, T, T> _combine;
    private readonly T _identity;
    
    public GenericSegmentTree(T[] array, Func<T, T, T> combineFunction, T identityElement)
    {
        _n = array.Length;
        _tree = new T[4 * _n];
        _combine = combineFunction;
        _identity = identityElement;
        
        Build(array, 0, 0, _n - 1);
    }
    
    private void Build(T[] array, int node, int start, int end)
    {
        if (start == end)
        {
            _tree[node] = array[start];
        }
        else
        {
            int mid = (start + end) / 2;
            int leftChild = 2 * node + 1;
            int rightChild = 2 * node + 2;
            
            Build(array, leftChild, start, mid);
            Build(array, rightChild, mid + 1, end);
            
            _tree[node] = _combine(_tree[leftChild], _tree[rightChild]);
        }
    }
    
    public void Update(int index, T value)
    {
        Update(0, 0, _n - 1, index, value);
    }
    
    private void Update(int node, int start, int end, int index, T value)
    {
        if (start == end)
        {
            _tree[node] = value;
        }
        else
        {
            int mid = (start + end) / 2;
            int leftChild = 2 * node + 1;
            int rightChild = 2 * node + 2;
            
            if (index <= mid)
            {
                Update(leftChild, start, mid, index, value);
            }
            else
            {
                Update(rightChild, mid + 1, end, index, value);
            }
            
            _tree[node] = _combine(_tree[leftChild], _tree[rightChild]);
        }
    }
    
    public T Query(int left, int right)
    {
        return Query(0, 0, _n - 1, left, right);
    }
    
    private T Query(int node, int start, int end, int left, int right)
    {
        if (right < start || left > end)
        {
            return _identity;
        }
        
        if (left <= start && end <= right)
        {
            return _tree[node];
        }
        
        int mid = (start + end) / 2;
        int leftChild = 2 * node + 1;
        int rightChild = 2 * node + 2;
        
        T leftResult = Query(leftChild, start, mid, left, right);
        T rightResult = Query(rightChild, mid + 1, end, left, right);
        
        return _combine(leftResult, rightResult);
    }
}

// Exemples d'utilisation pour différentes opérations
public static class SegmentTreeExamples
{
    // Segment Tree pour minimum
    public static GenericSegmentTree<int> CreateMinTree(int[] array)
    {
        return new GenericSegmentTree<int>(array, Math.Min, int.MaxValue);
    }
    
    // Segment Tree pour maximum
    public static GenericSegmentTree<int> CreateMaxTree(int[] array)
    {
        return new GenericSegmentTree<int>(array, Math.Max, int.MinValue);
    }
    
    // Segment Tree pour GCD
    public static GenericSegmentTree<int> CreateGcdTree(int[] array)
    {
        return new GenericSegmentTree<int>(array, GCD, 0);
    }
    
    private static int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}
```

---

## **4. Fenwick Tree (Binary Indexed Tree)**

### **Q: Implémentez un Fenwick Tree pour les sommes de préfixes ?**

```csharp
public class FenwickTree
{
    private readonly int[] _tree;
    private readonly int _n;
    
    public FenwickTree(int size)
    {
        _n = size;
        _tree = new int[_n + 1]; // Index 1-based
    }
    
    public FenwickTree(int[] array) : this(array.Length)
    {
        for (int i = 0; i < array.Length; i++)
        {
            Update(i, array[i]);
        }
    }
    
    // O(log n) - Mettre à jour un élément
    public void Update(int index, int delta)
    {
        index++; // Conversion vers 1-based
        
        while (index <= _n)
        {
            _tree[index] += delta;
            index += index & (-index); // Ajouter le bit le plus bas
        }
    }
    
    // O(log n) - Calculer la somme du préfixe [0, index]
    public int PrefixSum(int index)
    {
        index++; // Conversion vers 1-based
        int sum = 0;
        
        while (index > 0)
        {
            sum += _tree[index];
            index -= index & (-index); // Retirer le bit le plus bas
        }
        
        return sum;
    }
    
    // O(log n) - Calculer la somme d'un intervalle [left, right]
    public int RangeSum(int left, int right)
    {
        return PrefixSum(right) - (left > 0 ? PrefixSum(left - 1) : 0);
    }
    
    // O(log n) - Mise à jour d'un point (remplacer la valeur)
    public void Set(int index, int value)
    {
        int currentValue = RangeSum(index, index);
        Update(index, value - currentValue);
    }
    
    // O(log n) - Trouver l'index où la somme de préfixe >= target
    public int LowerBound(int target)
    {
        int index = 0;
        int sum = 0;
        
        // Commencer par la plus grande puissance de 2 <= n
        for (int bit = Integer.HighestOneBit(_n); bit > 0; bit >>= 1)
        {
            int newIndex = index + bit;
            if (newIndex <= _n && sum + _tree[newIndex] < target)
            {
                index = newIndex;
                sum += _tree[index];
            }
        }
        
        return index; // Retourner en 0-based
    }
}

// Extension pour Integer.HighestOneBit en C#
public static class IntegerExtensions
{
    public static int HighestOneBit(int n)
    {
        n |= (n >> 1);
        n |= (n >> 2);
        n |= (n >> 4);
        n |= (n >> 8);
        n |= (n >> 16);
        return n - (n >> 1);
    }
}

// Fenwick Tree 2D pour matrices
public class FenwickTree2D
{
    private readonly int[,] _tree;
    private readonly int _rows, _cols;
    
    public FenwickTree2D(int rows, int cols)
    {
        _rows = rows;
        _cols = cols;
        _tree = new int[rows + 1, cols + 1]; // 1-based
    }
    
    public FenwickTree2D(int[,] matrix) : this(matrix.GetLength(0), matrix.GetLength(1))
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                Update(i, j, matrix[i, j]);
            }
        }
    }
    
    // O(log m * log n) - Mettre à jour un élément
    public void Update(int row, int col, int delta)
    {
        for (int i = row + 1; i <= _rows; i += i & (-i))
        {
            for (int j = col + 1; j <= _cols; j += j & (-j))
            {
                _tree[i, j] += delta;
            }
        }
    }
    
    // O(log m * log n) - Somme du rectangle [0,0] à [row, col]
    public int PrefixSum(int row, int col)
    {
        int sum = 0;
        
        for (int i = row + 1; i > 0; i -= i & (-i))
        {
            for (int j = col + 1; j > 0; j -= j & (-j))
            {
                sum += _tree[i, j];
            }
        }
        
        return sum;
    }
    
    // O(log m * log n) - Somme d'un rectangle
    public int RangeSum(int row1, int col1, int row2, int col2)
    {
        return PrefixSum(row2, col2) - 
               (row1 > 0 ? PrefixSum(row1 - 1, col2) : 0) -
               (col1 > 0 ? PrefixSum(row2, col1 - 1) : 0) +
               (row1 > 0 && col1 > 0 ? PrefixSum(row1 - 1, col1 - 1) : 0);
    }
}

// Fenwick Tree pour mise à jour d'intervalle et requête de point
public class RangeUpdatePointQueryFenwick
{
    private readonly FenwickTree _tree;
    
    public RangeUpdatePointQueryFenwick(int size)
    {
        _tree = new FenwickTree(size);
    }
    
    // O(log n) - Mettre à jour un intervalle [left, right] avec delta
    public void UpdateRange(int left, int right, int delta)
    {
        _tree.Update(left, delta);
        if (right + 1 < _tree._n)
            _tree.Update(right + 1, -delta);
    }
    
    // O(log n) - Obtenir la valeur à un point
    public int Query(int index)
    {
        return _tree.PrefixSum(index);
    }
}
```

---

## **5. Disjoint Set Union (Union-Find)**

### **Q: Implémentez Union-Find avec optimisations ?**

```csharp
public class DisjointSetUnion
{
    private readonly int[] _parent;
    private readonly int[] _rank;
    private readonly int[] _size;
    private int _numSets;
    
    public DisjointSetUnion(int n)
    {
        _parent = new int[n];
        _rank = new int[n];
        _size = new int[n];
        _numSets = n;
        
        for (int i = 0; i < n; i++)
        {
            _parent[i] = i;
            _rank[i] = 0;
            _size[i] = 1;
        }
    }
    
    // O(α(n)) amortized - Find avec compression de chemin
    public int Find(int x)
    {
        if (_parent[x] != x)
        {
            _parent[x] = Find(_parent[x]); // Compression de chemin
        }
        return _parent[x];
    }
    
    // O(α(n)) amortized - Union par rang
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
            _size[rootY] += _size[rootX];
        }
        else if (_rank[rootX] > _rank[rootY])
        {
            _parent[rootY] = rootX;
            _size[rootX] += _size[rootY];
        }
        else
        {
            _parent[rootY] = rootX;
            _size[rootX] += _size[rootY];
            _rank[rootX]++;
        }
        
        _numSets--;
        return true;
    }
    
    // O(α(n)) - Vérifier si deux éléments sont connectés
    public bool IsConnected(int x, int y)
    {
        return Find(x) == Find(y);
    }
    
    // O(α(n)) - Obtenir la taille d'un ensemble
    public int GetSize(int x)
    {
        return _size[Find(x)];
    }
    
    // O(1) - Nombre d'ensembles disjoints
    public int GetNumSets()
    {
        return _numSets;
    }
    
    // O(n) - Obtenir tous les représentants
    public List<int> GetRepresentatives()
    {
        var representatives = new List<int>();
        
        for (int i = 0; i < _parent.Length; i++)
        {
            if (Find(i) == i)
                representatives.Add(i);
        }
        
        return representatives;
    }
    
    // O(n) - Obtenir tous les ensembles
    public Dictionary<int, List<int>> GetAllSets()
    {
        var sets = new Dictionary<int, List<int>>();
        
        for (int i = 0; i < _parent.Length; i++)
        {
            int root = Find(i);
            
            if (!sets.ContainsKey(root))
                sets[root] = new List<int>();
            
            sets[root].Add(i);
        }
        
        return sets;
    }
}

// Union-Find avec rollback (pour algorithmes offline)
public class RollbackUnionFind
{
    private readonly int[] _parent;
    private readonly int[] _rank;
    private readonly Stack<(int index, int oldParent, int oldRank, int numSets)> _history;
    private int _numSets;
    
    public RollbackUnionFind(int n)
    {
        _parent = new int[n];
        _rank = new int[n];
        _history = new Stack<(int, int, int, int)>();
        _numSets = n;
        
        for (int i = 0; i < n; i++)
        {
            _parent[i] = i;
            _rank[i] = 0;
        }
    }
    
    public int Find(int x)
    {
        // Pas de compression de chemin pour permettre le rollback
        while (_parent[x] != x)
            x = _parent[x];
        return x;
    }
    
    public bool Union(int x, int y)
    {
        int rootX = Find(x);
        int rootY = Find(y);
        
        if (rootX == rootY)
        {
            _history.Push((-1, -1, -1, _numSets));
            return false;
        }
        
        // Union par rang
        if (_rank[rootX] < _rank[rootY])
        {
            (rootX, rootY) = (rootY, rootX);
        }
        
        _history.Push((rootY, _parent[rootY], _rank[rootX], _numSets));
        
        _parent[rootY] = rootX;
        if (_rank[rootX] == _rank[rootY])
            _rank[rootX]++;
        
        _numSets--;
        return true;
    }
    
    public void Rollback()
    {
        if (_history.Count == 0)
            throw new InvalidOperationException("Nothing to rollback");
        
        var (index, oldParent, oldRank, oldNumSets) = _history.Pop();
        
        if (index != -1)
        {
            _parent[index] = oldParent;
            _rank[index] = oldRank;
        }
        
        _numSets = oldNumSets;
    }
    
    public bool IsConnected(int x, int y)
    {
        return Find(x) == Find(y);
    }
    
    public int GetNumSets()
    {
        return _numSets;
    }
}
```

Cette documentation complète des structures de données avancées vous donne tous les outils pour les entretiens techniques ! 🚀
