# **Délégués, Événements et Programmation Asynchrone - Guide Avancé**

> **🔗 Références :**
> - [LINQ et Lambda](./LINQ%20et%20Expressions%20Lambda.md) pour les expressions lambda avec délégués
> - [POO](./POO.md) pour les concepts d'événements dans les classes
> - [Performance](./Complexité%20Algorithmique%20et%20Performance.md) pour l'optimisation async/await

---

## 1. Qu'est-ce qu'un délégué en C# ?

**Définition :**
Un délégué est un type qui représente des références à des méthodes avec une signature spécifique. C'est comme un pointeur de fonction en C++, mais en version type-safe.

> **💡 Lambda :** Les délégués sont souvent utilisés avec les expressions lambda, voir [LINQ - Expressions Lambda](./LINQ%20et%20Expressions%20Lambda.md#expressions-lambda)

**Types de délégués :**
```csharp
// Délégué personnalisé
public delegate int Calculator(int x, int y);

// Délégués prédéfinis
Action<T>        // Méthode qui ne retourne rien
Func<T, TResult> // Méthode qui retourne quelque chose
Predicate<T>     // Méthode qui retourne un bool
```

**Exemple d'utilisation :**
```csharp
public delegate void MessageDelegate(string message);

class Program
{
    static void Main()
    {
        MessageDelegate del = PrintToConsole;
        del += PrintToFile; // Multicast delegate
        
        del("Hello World!"); // Appelle les deux méthodes
    }
    
    static void PrintToConsole(string msg) => Console.WriteLine(msg);
    static void PrintToFile(string msg) => File.WriteAllText("log.txt", msg);
}
```

## 2. Quelle est la différence entre un délégué et un événement ?

**Délégué :**
- Peut être appelé directement depuis l'extérieur de la classe
- Peut être assigné avec `=` (écrase les abonnements existants)
- Accès complet aux méthodes abonnées

**Événement :**
- Ne peut être déclenché que depuis la classe qui le déclare
- Ne peut être modifié que avec `+=` et `-=` depuis l'extérieur
- Encapsulation et sécurité renforcées

```csharp
public class Publisher
{
    // Délégué public - DANGEREUX
    public Action<string> OnMessageDelegate;
    
    // Événement - SÉCURISÉ
    public event Action<string> OnMessageEvent;
    
    public void SendMessage(string message)
    {
        // Les deux peuvent être appelés depuis l'intérieur
        OnMessageDelegate?.Invoke(message);
        OnMessageEvent?.Invoke(message);
    }
}

class Program
{
    static void Main()
    {
        var pub = new Publisher();
        
        // Avec délégué - possible mais dangereux
        pub.OnMessageDelegate = Console.WriteLine;
        pub.OnMessageDelegate = null; // Supprime tous les abonnements !
        pub.OnMessageDelegate("Test"); // Peut être appelé directement
        
        // Avec événement - sécurisé
        pub.OnMessageEvent += Console.WriteLine;
        // pub.OnMessageEvent = null; // ERREUR de compilation
        // pub.OnMessageEvent("Test"); // ERREUR de compilation
    }
}
```

## 3. Qu'est-ce que la programmation asynchrone en C# ?

**Définition :**
La programmation asynchrone permet d'exécuter des opérations longues sans bloquer le thread principal. Elle utilise les mots-clés `async` et `await`.

**Avantages :**
- Améliore la réactivité des applications UI
- Meilleure utilisation des ressources serveur
- Évite le blocage des threads

**Pattern async/await :**
```csharp
public async Task<string> GetDataAsync()
{
    using (var client = new HttpClient())
    {
        // await libère le thread pendant l'attente
        string result = await client.GetStringAsync("https://api.example.com/data");
        return result;
    }
}

// Utilisation
public async Task ProcessDataAsync()
{
    try
    {
        string data = await GetDataAsync();
        Console.WriteLine(data);
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Erreur: {ex.Message}");
    }
}
```

## 4. Quelle est la différence entre Task et Task<T> ?

**Task :**
- Représente une opération asynchrone qui ne retourne pas de valeur
- Équivalent à un `void` asynchrone
- Utilisé pour les opérations d'écriture, de mise à jour, etc.

**Task<T> :**
- Représente une opération asynchrone qui retourne une valeur de type T
- Équivalent à un `T` asynchrone
- Utilisé pour les opérations de lecture, de calcul, etc.

```csharp
// Task - pas de valeur de retour
public async Task SaveDataAsync(string data)
{
    await File.WriteAllTextAsync("data.txt", data);
    Console.WriteLine("Données sauvegardées");
}

// Task<T> - retourne une valeur
public async Task<string> LoadDataAsync()
{
    string content = await File.ReadAllTextAsync("data.txt");
    return content;
}

// Utilisation
public async Task MainAsync()
{
    await SaveDataAsync("Hello World");
    string data = await LoadDataAsync();
    Console.WriteLine(data);
}
```

## 5. Comment gérer les exceptions dans le code asynchrone ?

**Gestion d'exception simple :**
```csharp
public async Task<string> GetDataWithErrorHandlingAsync()
{
    try
    {
        var client = new HttpClient();
        return await client.GetStringAsync("https://api.example.com/data");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Erreur HTTP: {ex.Message}");
        return "Données par défaut";
    }
    catch (TaskCanceledException ex)
    {
        Console.WriteLine($"Timeout: {ex.Message}");
        return "Timeout";
    }
}
```

**Gestion d'exceptions avec Task.WhenAll :**
```csharp
public async Task ProcessMultipleTasksAsync()
{
    var tasks = new[]
    {
        GetDataAsync("url1"),
        GetDataAsync("url2"),
        GetDataAsync("url3")
    };
    
    try
    {
        string[] results = await Task.WhenAll(tasks);
        // Traiter les résultats
    }
    catch (Exception ex)
    {
        // Une seule exception est remontée, même si plusieurs ont échoué
        Console.WriteLine($"Au moins une tâche a échoué: {ex.Message}");
        
        // Pour voir toutes les exceptions
        foreach (var task in tasks)
        {
            if (task.IsFaulted)
            {
                Console.WriteLine($"Erreur: {task.Exception?.InnerException?.Message}");
            }
        }
    }
}
```

## 6. Qu'est-ce que ConfigureAwait(false) et quand l'utiliser ?

**Problème :**
Par défaut, `await` capture le contexte de synchronisation (SynchronizationContext) pour reprendre l'exécution sur le même thread.

**Solution :**
`ConfigureAwait(false)` indique qu'on n'a pas besoin de reprendre sur le thread original.

```csharp
// Dans une bibliothèque - utilisez ConfigureAwait(false)
public async Task<string> LibraryMethodAsync()
{
    var client = new HttpClient();
    // Pas besoin du contexte UI ici
    string result = await client.GetStringAsync("https://api.example.com")
                                .ConfigureAwait(false);
    return result.ToUpper();
}

// Dans une application UI - n'utilisez PAS ConfigureAwait(false)
private async void Button_Click(object sender, EventArgs e)
{
    string data = await LibraryMethodAsync();
    // On a besoin du contexte UI pour mettre à jour l'interface
    textBox.Text = data; // Doit s'exécuter sur le thread UI
}
```

---

# Jeu de Questions-Réponses : Délégués et Async

## Questions de Base

**Q1:** Quelle est la signature du délégué `Action<T>` ?
<details>
<summary>Réponse</summary>
void Action<T>(T obj) - Une méthode qui prend un paramètre de type T et ne retourne rien.
</details>

**Q2:** Que fait le mot-clé `await` ?
<details>
<summary>Réponse</summary>
Il suspend l'exécution de la méthode jusqu'à ce que la tâche asynchrone soit terminée, puis reprend l'exécution avec le résultat.
</details>

**Q3:** Peut-on utiliser `await` dans une méthode synchrone ?
<details>
<summary>Réponse</summary>
Non, `await` ne peut être utilisé que dans une méthode marquée `async`.
</details>

## Questions Intermédiaires

**Q4:** Quelle est la différence entre ces deux appels ?
```csharp
// Version A
await task1;
await task2;

// Version B
await Task.WhenAll(task1, task2);
```
<details>
<summary>Réponse</summary>
- Version A : Exécution séquentielle, task2 attend que task1 soit terminée
- Version B : Exécution parallèle, les deux tâches s'exécutent en même temps
</details>

**Q5:** Que se passe-t-il si on n'utilise pas `await` devant un appel de méthode async ?
<details>
<summary>Réponse</summary>
La méthode s'exécute de manière "fire-and-forget". On obtient une Task non attendue, et les exceptions peuvent être perdues.
</details>

## Questions Avancées

**Q6:** Écrivez un délégué multicast qui additionne et multiplie deux nombres.
<details>
<summary>Réponse</summary>
```csharp
delegate void MathOperation(int a, int b);

MathOperation ops = (x, y) => Console.WriteLine($"Addition: {x + y}");
ops += (x, y) => Console.WriteLine($"Multiplication: {x * y}");
ops(5, 3); // Affiche les deux résultats
```
</details>

**Q7:** Comment annuler une opération asynchrone ?
<details>
<summary>Réponse</summary>
En utilisant `CancellationToken` :
```csharp
public async Task<string> GetDataAsync(CancellationToken cancellationToken)
{
    var client = new HttpClient();
    return await client.GetStringAsync("url", cancellationToken);
}
```
</details>

**Q8:** Pourquoi éviter `async void` sauf pour les gestionnaires d'événements ?
<details>
<summary>Réponse</summary>
- Pas de retour de Task, donc impossible d'attendre la completion
- Gestion d'exceptions difficile (peut crasher l'application)
- Pas de composition possible avec d'autres méthodes async
</details>
