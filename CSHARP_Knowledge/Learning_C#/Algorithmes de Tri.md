# **Algorithmes de Tri en C# - Guide Technique**

> **🔗 Références :**
> - [Complexité Algorithmique](./Complexité%20Algorithmique%20et%20Performance.md) - Analyse de performance
> - [Collections](./boucles%20et%20collections.md) - Structures de données de base
> - [LINQ](./LINQ%20et%20Expressions%20Lambda.md) - Tri avec LINQ
> - [Théorie des Graphes](./Théorie%20des%20Graphes.md) - Algorithmes sur graphes

---

# **ALGORITHMES DE TRI FONDAMENTAUX**

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

## **6. Tri par Tas (Heap Sort)**

### **Q: Implémentez HeapSort et expliquez ses avantages ?**

**Avantages :**
- **O(n log n) garanti** dans tous les cas
- **Tri en place** (O(1) espace)
- **Pas de récursion** (évite stack overflow)

```csharp
public class HeapSort
{
    public static void Sort(int[] array)
    {
        int n = array.Length;
        
        // Construire le tas (heapify)
        for (int i = n / 2 - 1; i >= 0; i--)
        {
            Heapify(array, n, i);
        }
        
        // Extraire les éléments un par un du tas
        for (int i = n - 1; i > 0; i--)
        {
            // Déplacer la racine (maximum) à la fin
            (array[0], array[i]) = (array[i], array[0]);
            
            // Restaurer la propriété de tas sur le tas réduit
            Heapify(array, i, 0);
        }
    }
    
    private static void Heapify(int[] array, int heapSize, int rootIndex)
    {
        int largest = rootIndex;
        int leftChild = 2 * rootIndex + 1;
        int rightChild = 2 * rootIndex + 2;
        
        // Si l'enfant gauche est plus grand que la racine
        if (leftChild < heapSize && array[leftChild] > array[largest])
            largest = leftChild;
        
        // Si l'enfant droit est plus grand que le plus grand jusqu'à présent
        if (rightChild < heapSize && array[rightChild] > array[largest])
            largest = rightChild;
        
        // Si le plus grand n'est pas la racine
        if (largest != rootIndex)
        {
            (array[rootIndex], array[largest]) = (array[largest], array[rootIndex]);
            
            // Heapify récursivement le sous-arbre affecté
            Heapify(array, heapSize, largest);
        }
    }
    
    // Version itérative pour éviter la récursion
    public static void SortIterative(int[] array)
    {
        int n = array.Length;
        
        // Construire le tas
        for (int i = n / 2 - 1; i >= 0; i--)
        {
            HeapifyIterative(array, n, i);
        }
        
        // Extraire les éléments
        for (int i = n - 1; i > 0; i--)
        {
            (array[0], array[i]) = (array[i], array[0]);
            HeapifyIterative(array, i, 0);
        }
    }
    
    private static void HeapifyIterative(int[] array, int heapSize, int startIndex)
    {
        int parent = startIndex;
        
        while (true)
        {
            int largest = parent;
            int leftChild = 2 * parent + 1;
            int rightChild = 2 * parent + 2;
            
            if (leftChild < heapSize && array[leftChild] > array[largest])
                largest = leftChild;
            
            if (rightChild < heapSize && array[rightChild] > array[largest])
                largest = rightChild;
            
            if (largest == parent)
                break;
            
            (array[parent], array[largest]) = (array[largest], array[parent]);
            parent = largest;
        }
    }
}
```

---

## **7. Tri par Comptage (Counting Sort)**

### **Q: Quand utiliser le tri par comptage ?**

**Cas d'usage :** Entiers dans une plage limitée connue (ex: âges 0-120).

```csharp
public class CountingSort
{
    public static void Sort(int[] array, int maxValue)
    {
        int[] count = new int[maxValue + 1];
        
        // Compter les occurrences
        foreach (int num in array)
        {
            count[num]++;
        }
        
        // Reconstruire le tableau trié
        int index = 0;
        for (int i = 0; i <= maxValue; i++)
        {
            while (count[i]-- > 0)
            {
                array[index++] = i;
            }
        }
    }
    
    // Version stable avec positions cumulatives
    public static void StableSort(int[] array, int maxValue)
    {
        int[] count = new int[maxValue + 1];
        int[] output = new int[array.Length];
        
        // Compter les occurrences
        foreach (int num in array)
        {
            count[num]++;
        }
        
        // Transformer en positions cumulatives
        for (int i = 1; i <= maxValue; i++)
        {
            count[i] += count[i - 1];
        }
        
        // Construire le tableau de sortie (stable)
        for (int i = array.Length - 1; i >= 0; i--)
        {
            output[count[array[i]] - 1] = array[i];
            count[array[i]]--;
        }
        
        // Copier le résultat
        Array.Copy(output, array, array.Length);
    }
}
```

---

## **8. Comparaison et Choix d'Algorithme**

### **Q: Comment choisir l'algorithme de tri approprié ?**

```csharp
public class SortingStrategy
{
    public static void OptimalSort<T>(T[] array, IComparer<T> comparer = null) where T : IComparable<T>
    {
        comparer ??= Comparer<T>.Default;
        
        // Cas spéciaux
        if (array.Length <= 1) return;
        
        if (array.Length <= 16)
        {
            // Insertion sort pour petits tableaux
            InsertionSort.Sort(array, comparer);
            return;
        }
        
        // Vérifier si presque trié
        if (IsNearlySorted(array, comparer))
        {
            InsertionSort.Sort(array, comparer);
            return;
        }
        
        // Pour les gros tableaux, utiliser un tri hybride
        IntroSort(array, comparer);
    }
    
    private static bool IsNearlySorted<T>(T[] array, IComparer<T> comparer)
    {
        int inversions = 0;
        int threshold = array.Length / 4; // 25% d'inversions max
        
        for (int i = 0; i < array.Length - 1; i++)
        {
            if (comparer.Compare(array[i], array[i + 1]) > 0)
            {
                inversions++;
                if (inversions > threshold)
                    return false;
            }
        }
        
        return true;
    }
    
    // Introsort - Hybride QuickSort/HeapSort/InsertionSort
    private static void IntroSort<T>(T[] array, IComparer<T> comparer)
    {
        int maxDepth = (int)(2 * Math.Log2(array.Length));
        IntroSortRecursive(array, 0, array.Length - 1, maxDepth, comparer);
    }
    
    private static void IntroSortRecursive<T>(T[] array, int low, int high, int maxDepth, IComparer<T> comparer)
    {
        while (high > low)
        {
            int size = high - low + 1;
            
            if (size <= 16)
            {
                InsertionSort.SortRange(array, low, high, comparer);
                return;
            }
            
            if (maxDepth == 0)
            {
                HeapSort.SortRange(array, low, high, comparer);
                return;
            }
            
            int pivot = Partition(array, low, high, comparer);
            
            // Optimisation tail call
            if (pivot - low < high - pivot)
            {
                IntroSortRecursive(array, low, pivot - 1, maxDepth - 1, comparer);
                low = pivot + 1;
            }
            else
            {
                IntroSortRecursive(array, pivot + 1, high, maxDepth - 1, comparer);
                high = pivot - 1;
            }
        }
    }
    
    private static int Partition<T>(T[] array, int low, int high, IComparer<T> comparer)
    {
        // Implémentation similaire à QuickSort
        // ... code de partition
        return (low + high) / 2; // Placeholder
    }
}

// Tableau de décision pour le choix d'algorithme
public static class SortingDecisionTable
{
    public static string RecommendAlgorithm(int size, bool isStabilityRequired, bool isSpaceConstrained, DataCharacteristics characteristics)
    {
        return characteristics switch
        {
            DataCharacteristics.Random when size < 50 => "Insertion Sort",
            DataCharacteristics.Random when size < 1000 => "Quick Sort",
            DataCharacteristics.Random => "Intro Sort (hybride)",
            
            DataCharacteristics.NearlySorted => "Insertion Sort",
            DataCharacteristics.Reversed => "Selection Sort puis reverse",
            
            DataCharacteristics.ManyDuplicates => "3-way Quick Sort",
            DataCharacteristics.IntegerRange when !isStabilityRequired => "Counting Sort",
            
            _ when isStabilityRequired && !isSpaceConstrained => "Merge Sort",
            _ when isStabilityRequired && isSpaceConstrained => "Tim Sort",
            _ when isSpaceConstrained => "Heap Sort",
            
            _ => "Intro Sort"
        };
    }
}

public enum DataCharacteristics
{
    Random,
    NearlySorted,
    Reversed,
    ManyDuplicates,
    IntegerRange
}
```

---

## **📊 Résumé des Complexités**

| Algorithme | Meilleur cas | Cas moyen | Pire cas | Espace | Stable |
|------------|--------------|-----------|----------|--------|--------|
| Bubble Sort | O(n) | O(n²) | O(n²) | O(1) | ✅ |
| Insertion Sort | O(n) | O(n²) | O(n²) | O(1) | ✅ |
| Selection Sort | O(n²) | O(n²) | O(n²) | O(1) | ❌ |
| Quick Sort | O(n log n) | O(n log n) | O(n²) | O(log n) | ❌ |
| Merge Sort | O(n log n) | O(n log n) | O(n log n) | O(n) | ✅ |
| Heap Sort | O(n log n) | O(n log n) | O(n log n) | O(1) | ❌ |
| Counting Sort | O(n + k) | O(n + k) | O(n + k) | O(k) | ✅ |

Cette documentation complète vous donne tous les outils pour maîtriser les algorithmes de tri en C# ! 🚀
