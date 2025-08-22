# **Boucles et Collections C# - Guide Pratique**

> **🔗 Voir aussi :** 
> - [Complexité Algorithmique et Performance](./Complexité%20Algorithmique%20et%20Performance.md) pour l'analyse de performance des collections
> - [Algorithmes de Tri et Graphes](./Algorithmes%20de%20Tri%20et%20Théorie%20des%20Graphes.md) pour les algorithmes sur les collections

---

### 1. Quels sont les prérequis pour utiliser `foreach` sur une collection ?

Pour utiliser `foreach` sur une collection en C#, la collection doit implémenter l'interface `IEnumerable` ou `IEnumerable<T>`. Ces interfaces définissent une méthode `GetEnumerator` qui retourne un énumérateur permettant d'itérer sur les éléments de la collection.

**Exemple :**
```csharp
public class MyCollection : IEnumerable<int>
{
    private int[] _items = { 1, 2, 3, 4, 5 };

    public IEnumerator<int> GetEnumerator()
    {
        return ((IEnumerable<int>)_items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _items.GetEnumerator();
    }
}

// Utilisation de foreach
MyCollection collection = new MyCollection();
foreach (var item in collection)
{
    Console.WriteLine(item);
}
```

### 2. Quelle est la différence entre une `List` et un `Dictionary` en C# ?

> **⚡ Performance :** Pour l'analyse détaillée des complexités O(n) vs O(1), voir [Complexité des Collections](./Complexité%20Algorithmique%20et%20Performance.md#collections-net-et-complexité)

- **`List<T>`** :
  - **Description** : Une liste est une collection ordonnée d'éléments de type `T`.
  - **Accès** : Les éléments sont accessibles par leur index (**O(1)**)
  - **Recherche** : Recherche linéaire (**O(n)**)
  - **Utilisation** : Idéale pour les collections où l'ordre des éléments est important et où les éléments sont accédés par leur position.
  - **Exemple** : `List<int> numbers = new List<int> { 1, 2, 3 };`

- **`Dictionary<TKey, TValue>`** :
  - **Description** : Un dictionnaire est une collection de paires clé-valeur, où chaque clé est unique.
  - **Accès** : Les éléments sont accessibles par leur clé (**O(1) average case**)
  - **Recherche** : Recherche par hash (**O(1) average case**)
  - **Utilisation** : Idéale pour les collections où les éléments doivent être associés à des clés uniques et où l'accès rapide par clé est nécessaire.
  - **Exemple** : `Dictionary<string, int> ages = new Dictionary<string, int> { { "Alice", 30 }, { "Bob", 25 } };`

### 3. Quels types peuvent être utilisés comme clés de dictionnaire ? Comment sont gérées les collisions ?

- **Types de Clés** :
  - Tout type qui implémente correctement les méthodes `GetHashCode` et `Equals` peut être utilisé comme clé de dictionnaire. Les types couramment utilisés incluent `string`, `int`, `Guid`, et les types personnalisés.

- **Gestion des Collisions** :
  - Les collisions se produisent lorsque deux clés différentes ont le même code de hachage. Le dictionnaire utilise une technique appelée "chaînage" pour gérer les collisions. Chaque emplacement dans la table de hachage contient une liste chaînée de paires clé-valeur ayant le même code de hachage. Lors de la recherche d'une clé, le dictionnaire parcourt la liste chaînée pour trouver la clé correspondante.

### 4. Comment assurer l’unicité des clés dans un dictionnaire quand celles-ci sont des objets complexes ?

Pour assurer l'unicité des clés dans un dictionnaire lorsque celles-ci sont des objets complexes, vous devez :

1. **Override `GetHashCode`** : Implémentez une méthode `GetHashCode` qui retourne un code de hachage unique pour chaque objet.
2. **Override `Equals`** : Implémentez une méthode `Equals` qui compare les objets pour déterminer s'ils sont égaux.

**Exemple :**
```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public override int GetHashCode()
    {
        return HashCode.Combine(FirstName, LastName);
    }

    public override bool Equals(object obj)
    {
        if (obj is Person other)
        {
            return FirstName == other.FirstName && LastName == other.LastName;
        }
        return false;
    }
}

// Utilisation dans un dictionnaire
var dictionary = new Dictionary<Person, int>();
var person1 = new Person { FirstName = "Alice", LastName = "Smith" };
var person2 = new Person { FirstName = "Alice", LastName = "Smith" };

dictionary[person1] = 30;
Console.WriteLine(dictionary.ContainsKey(person2)); // Affiche True
```

### 5. Qu’est-ce qu’une `LinkedList` et dans quels cas l’utiliser ?

- **Description** :
  - Une `LinkedList<T>` est une collection d'éléments où chaque élément contient une référence à l'élément suivant et précédent. Cela permet des insertions et suppressions rapides à n'importe quelle position dans la liste.

- **Cas d'Utilisation** :
  - Utilisez une `LinkedList` lorsque vous avez besoin d'effectuer des insertions et des suppressions fréquentes au milieu de la liste.
  - Évitez de l'utiliser lorsque vous avez besoin d'un accès aléatoire rapide, car l'accès à un élément spécifique nécessite de parcourir la liste.

**Exemple :**
```csharp
var linkedList = new LinkedList<int>();
linkedList.AddLast(1);
linkedList.AddLast(2);
linkedList.AddFirst(0);

foreach (var item in linkedList)
{
    Console.WriteLine(item); // Affiche 0, 1, 2
}
```

### 6. Quelle est la différence entre une `Stack` et une `Heap` ?

- **`Stack` (Pile)** :
  - **Description** : La pile est une zone de mémoire utilisée pour stocker des variables locales et des informations de contrôle de flux (comme les appels de méthode).
  - **Caractéristiques** : Les allocations et désallocations sont rapides, mais la taille de la pile est limitée.
  - **Utilisation** : Utilisée pour les variables locales et les appels de méthode.

- **`Heap` (Tas)** :
  - **Description** : Le tas est une zone de mémoire utilisée pour stocker des objets dynamiquement alloués.
  - **Caractéristiques** : Les allocations et désallocations sont plus lentes que sur la pile, mais la taille du tas est beaucoup plus grande.
  - **Utilisation** : Utilisée pour les objets qui doivent persister au-delà de la portée de la méthode.

### 7. Quelles sont les principales collections utilisées en .NET et dans quels cas les privilégier ?

- **`List<T>`** :
  - **Utilisation** : Pour les collections ordonnées où l'accès par index est fréquent.
  - **Cas d'Utilisation** : Listes d'éléments où l'ordre est important.

- **`Dictionary<TKey, TValue>`** :
  - **Utilisation** : Pour les collections de paires clé-valeur où l'accès rapide par clé est nécessaire.
  - **Cas d'Utilisation** : Mappage entre des clés uniques et des valeurs.

- **`HashSet<T>`** :
  - **Utilisation** : Pour les collections d'éléments uniques où l'ordre n'est pas important.
  - **Cas d'Utilisation** : Ensembles d'éléments uniques.

- **`Queue<T>`** :
  - **Utilisation** : Pour les collections où les éléments sont ajoutés à la fin et retirés du début (FIFO).
  - **Cas d'Utilisation** : Files d'attente de traitement.

- **`Stack<T>`** :
  - **Utilisation** : Pour les collections où les éléments sont ajoutés et retirés du sommet (LIFO).
  - **Cas d'Utilisation** : Piles d'appels de méthode, annulations d'actions.

- **`LinkedList<T>`** :
  - **Utilisation** : Pour les collections où les insertions et suppressions fréquentes au milieu sont nécessaires.
  - **Cas d'Utilisation** : Listes chaînées où l'ordre est important mais l'accès aléatoire n'est pas nécessaire.

- **`ConcurrentDictionary<TKey, TValue>`** :
  - **Utilisation** : Pour les collections de paires clé-valeur où l'accès concurrent est nécessaire.
  - **Cas d'Utilisation** : Dictionnaires partagés entre plusieurs threads.

- **`ConcurrentBag<T>`** :
  - **Utilisation** : Pour les collections non ordonnées où l'accès concurrent est nécessaire.
  - **Cas d'Utilisation** : Collections d'éléments où l'ordre n'est pas important et l'accès concurrent est fréquent.

En résumé, le choix de la collection dépend des besoins spécifiques de votre application, notamment en termes d'ordre, d'unicité, d'accès rapide et de concurrence.