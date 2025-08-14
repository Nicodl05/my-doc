# LINQ et Expressions Lambda

## 1. Qu'est-ce que LINQ et pourquoi l'utiliser ?

**Définition :**
LINQ (Language Integrated Query) est une technologie Microsoft qui permet d'écrire des requêtes directement dans le code C#. Elle offre une syntaxe uniforme pour interroger différentes sources de données (collections, bases de données, XML, etc.).

**Avantages :**
- **IntelliSense** : Support complet de l'IDE avec auto-complétion
- **Vérification à la compilation** : Les erreurs sont détectées lors de la compilation
- **Lisibilité** : Code plus expressif et facile à comprendre
- **Performance** : Optimisations automatiques, notamment avec Entity Framework

**Exemple :**
```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Sans LINQ
var evenNumbers = new List<int>();
foreach (var number in numbers)
{
    if (number % 2 == 0)
        evenNumbers.Add(number);
}

// Avec LINQ
var evenNumbersLinq = numbers.Where(n => n % 2 == 0).ToList();
```

## 2. Quelle est la différence entre les syntaxes de méthode et de requête LINQ ?

**Syntaxe de Méthode :**
- Utilise des méthodes d'extension avec des expressions lambda
- Plus concise pour des opérations simples
- Accès à toutes les méthodes LINQ

```csharp
var result = students
    .Where(s => s.Age > 18)
    .OrderBy(s => s.Name)
    .Select(s => s.Name)
    .ToList();
```

**Syntaxe de Requête :**
- Ressemble au SQL
- Plus lisible pour des requêtes complexes
- Limitée aux opérateurs de base

```csharp
var result = (from s in students
              where s.Age > 18
              orderby s.Name
              select s.Name).ToList();
```

## 3. Qu'est-ce qu'une expression lambda et comment l'utiliser ?

**Définition :**
Une expression lambda est une fonction anonyme que vous pouvez utiliser pour créer des délégués ou des arbres d'expression. Elle utilise l'opérateur `=>` (va vers).

**Syntaxes :**
```csharp
// Expression simple
x => x * 2

// Avec paramètres multiples
(x, y) => x + y

// Avec corps de méthode
x => 
{
    var result = x * 2;
    return result;
}

// Sans paramètres
() => DateTime.Now
```

**Utilisations courantes :**
```csharp
// Avec LINQ
var adults = people.Where(p => p.Age >= 18);

// Avec délégués
Action<string> print = message => Console.WriteLine(message);

// Avec événements
button.Click += (sender, e) => MessageBox.Show("Clicked!");
```

## 4. Quelles sont les principales méthodes LINQ et leurs utilisations ?

**Méthodes de Filtrage :**
```csharp
// Where - filtre les éléments
var adults = people.Where(p => p.Age >= 18);

// Take - prend les n premiers éléments
var firstFive = numbers.Take(5);

// Skip - ignore les n premiers éléments
var afterFive = numbers.Skip(5);

// Distinct - éléments uniques
var uniqueNumbers = numbers.Distinct();
```

**Méthodes de Projection :**
```csharp
// Select - transforme chaque élément
var names = people.Select(p => p.Name);

// SelectMany - aplatit les collections imbriquées
var allPhones = people.SelectMany(p => p.PhoneNumbers);
```

**Méthodes d'Agrégation :**
```csharp
// Count/Sum/Average/Min/Max
var count = numbers.Count();
var sum = numbers.Sum();
var average = numbers.Average();
var min = numbers.Min();
var max = numbers.Max();

// Aggregate - agrégation personnalisée
var product = numbers.Aggregate((a, b) => a * b);
```

**Méthodes d'Ordonnancement :**
```csharp
// OrderBy/OrderByDescending
var sortedByAge = people.OrderBy(p => p.Age);
var sortedByAgeDesc = people.OrderByDescending(p => p.Age);

// ThenBy/ThenByDescending - tri secondaire
var sorted = people
    .OrderBy(p => p.LastName)
    .ThenBy(p => p.FirstName);
```

## 5. Qu'est-ce que l'exécution différée (Deferred Execution) ?

**Définition :**
L'exécution différée signifie que l'évaluation d'une requête LINQ est retardée jusqu'à ce que les résultats soient réellement énumérés.

**Exemple :**
```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5 };
var query = numbers.Where(n => n > 2); // Pas d'exécution ici

numbers.Add(6); // Modification de la source

foreach (var num in query) // Exécution maintenant
{
    Console.WriteLine(num); // Affiche: 3, 4, 5, 6
}
```

**Méthodes avec exécution immédiate :**
```csharp
// Ces méthodes forcent l'exécution immédiate
var list = query.ToList();
var array = query.ToArray();
var count = query.Count();
var first = query.First();
```

---

# Jeu de Questions-Réponses : LINQ et Lambda

## Questions de Base (Débutant)

**Q1:** Que signifie l'acronyme LINQ ?
<details>
<summary>Réponse</summary>
Language Integrated Query (Requête Intégrée au Langage)
</details>

**Q2:** Quel opérateur est utilisé dans les expressions lambda ?
<details>
<summary>Réponse</summary>
L'opérateur => (va vers)
</details>

**Q3:** Écrivez une expression lambda qui double un nombre.
<details>
<summary>Réponse</summary>
x => x * 2
</details>

## Questions Intermédiaires

**Q4:** Quelle est la différence entre `IEnumerable<T>` et `IQueryable<T>` ?
<details>
<summary>Réponse</summary>
- IEnumerable<T> : Exécution en mémoire, utilisé pour les collections LINQ to Objects
- IQueryable<T> : Peut être traduit en requêtes (SQL), utilisé pour LINQ to SQL/EF
</details>

**Q5:** Que va afficher ce code ?
```csharp
var numbers = new List<int> { 1, 2, 3 };
var query = numbers.Select(x => x * 2);
numbers.Add(4);
Console.WriteLine(string.Join(", ", query));
```
<details>
<summary>Réponse</summary>
"2, 4, 6, 8" - L'exécution différée inclut le 4 ajouté après la création de la requête
</details>

## Questions Avancées

**Q6:** Écrivez une requête LINQ qui groupe des personnes par ville et calcule l'âge moyen par ville.
<details>
<summary>Réponse</summary>
```csharp
var result = people
    .GroupBy(p => p.City)
    .Select(g => new { 
        City = g.Key, 
        AverageAge = g.Average(p => p.Age) 
    });
```
</details>

**Q7:** Comment éviter les exceptions lors de l'utilisation de `First()` ?
<details>
<summary>Réponse</summary>
Utiliser `FirstOrDefault()` qui retourne la valeur par défaut au lieu de lever une exception, ou vérifier avec `Any()` avant d'utiliser `First()`.
</details>

**Q8:** Quelle est la performance relative de `Count()` vs `Any()` pour vérifier si une collection contient des éléments ?
<details>
<summary>Réponse</summary>
`Any()` est plus performant car il s'arrête dès qu'il trouve un élément, tandis que `Count()` peut énumérer toute la collection.
</details>
