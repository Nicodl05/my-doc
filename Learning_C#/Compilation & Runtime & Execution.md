# **Cours Complet : Compilation, Runtime et Exécution en Informatique**
*(Illustré avec des exemples en C#)*

---

## **Table des Matières**
1. [Introduction aux Concepts](#1-introduction-aux-concepts)
2. [La Compilation](#2-la-compilation)
   - [Compilation en C# : Du Code Source au IL](#21-compilation-en-c-exemple-complet)
   - [Types de Compilation](#22-types-de-compilation)
3. [Le Runtime](#3-le-runtime)
   - [Rôle du Runtime](#31-rôle-du-runtime)
   - [Le CLR en C#](#32-le-clr-common-language-runtime-en-c)
4. [L'Exécution](#4-l-exécution)
   - [De l'IL au Code Machine](#41-de-lil-au-code-machine-jit)
   - [Gestion de la Mémoire](#42-gestion-de-la-mémoire-stack-heap-et-garbage-collection)
5. [Exemple Complet en C#](#5-exemple-complet-en-c)
6. [Outils et Bonnes Pratiques](#6-outils-et-bonnes-pratiques)
7. [Comparaison avec d'Autres Langages](#7-comparaison-avec-d-autres-langages)
8. [Exercices Pratiques](#8-exercices-pratiques)

---

## **1. Introduction aux Concepts** <a name="1-introduction-aux-concepts"></a>

### **Pourquoi ces concepts sont-ils importants ?**
- Comprendre comment un programme **passe du code source à l'exécution**.
- Optimiser les **performances** et la **mémoire**.
- Déboguer efficacement (erreurs de compilation vs erreurs d'exécution).

### **Définitions Clés**
| **Terme**         | **Définition**                                                                 |
|-------------------|--------------------------------------------------------------------------------|
| **Compilation**   | Transformation du code source en code machine ou intermédiaire.                |
| **Runtime**       | Environnement qui exécute le programme (ex : CLR pour C#, JVM pour Java).      |
| **Exécution**     | Phase où le programme est effectivement exécuté par le processeur.             |
| **Code Source**   | Code écrit par le développeur (ex : `Program.cs`).                            |
| **Code Machine**  | Instructions binaires comprises par le processeur.                            |
| **IL (Intermediate Language)** | Code intermédiaire en .NET, indépendant de la plateforme.                |

---

## **2. La Compilation** <a name="2-la-compilation"></a>

### **2.1. Compilation en C# : Exemple Complet** <a name="21-compilation-en-c-exemple-complet"></a>

#### **Étape 1 : Code Source**
Fichier `Program.cs` :
```csharp
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Bonjour, monde !");
    }
}
```

#### **Étape 2 : Compilation avec Roslyn**
- Le **compilateur C# (Roslyn)** transforme le code source en **IL** (Intermediate Language).
- Commande :
  ```bash
  csc Program.cs
  ```
  ou avec .NET CLI :
  ```bash
  dotnet build
  ```
- Résultat : Un fichier `Program.exe` contenant du **IL**.

#### **Inspection du IL**
Utilisez **ILSpy** ou **ildasm** pour voir le IL généré :
```il
.method private hidebysig static void Main() cil managed
{
    .entrypoint
    ldstr "Bonjour, monde !"
    call void [System.Console]System.Console::WriteLine(string)
    ret
}
```

---

### **2.2. Types de Compilation** <a name="22-types-de-compilation"></a>

| **Type**               | **Description**                                                                 | **Exemple**                     |
|------------------------|---------------------------------------------------------------------------------|----------------------------------|
| **Compilation native** | Code source → Code machine (dépendant de l'OS/architecture).                   | C, C++                           |
| **Compilation en IL/Bytecode** | Code source → Code intermédiaire (portable).                                  | C#, Java                         |
| **Transpilation**      | Code source → Code source dans un autre langage.                               | TypeScript → JavaScript          |

**En C#** :
- Le compilateur génère du **IL**, pas du code machine.
- Le IL est **indépendant de la plateforme**.

---

## **3. Le Runtime** <a name="3-le-runtime"></a>

### **3.1. Rôle du Runtime** <a name="31-rôle-du-runtime"></a>
- Charge et exécute le programme.
- Gère la **mémoire**, les **threads**, et les **appels système**.
- Fournit des services comme la **sécurité** et le **garbage collection**.

### **3.2. Le CLR (Common Language Runtime) en C#** <a name="32-le-clr-common-language-runtime-en-c"></a>
- **Charge** le code IL.
- **Vérifie** la sécurité et la validité du IL.
- **Compile en temps réel (JIT)** le IL en code machine.
- **Gère la mémoire** (allocation, garbage collection).

**Schéma du CLR** :
```
Code IL (Program.exe)
       ↓
[CLR]
       ↓
Code Machine (x86/x64/ARM)
       ↓
Exécution par le CPU
```

---

## **4. L'Exécution** <a name="4-l-exécution"></a>

### **4.1. De l'IL au Code Machine (JIT)** <a name="41-de-lil-au-code-machine-jit"></a>
- Le **JIT Compiler** du CLR transforme le IL en **code machine** **à la volée**.
- **Avantages** :
  - Portabilité (un même `.exe` fonctionne sur Windows, Linux, macOS).
  - Optimisations dynamiques (le JIT adapte le code machine à l'environnement).

**Exemple** :
1. Quand vous exécutez `Program.exe`, le CLR :
   - Charge le IL.
   - Compile les méthodes appelées en code machine.
   - Exécute le code machine.

---

### **4.2. Gestion de la Mémoire** <a name="42-gestion-de-la-mémoire-stack-heap-et-garbage-collection"></a>

#### **Stack vs Heap**
| **Stack**                          | **Heap**                          |
|------------------------------------|-----------------------------------|
| Variables locales, paramètres.      | Objets (instances de classes).    |
| Allocation/désallocation automatique. | Allocation/désallocation dynamique (`new`). |
| Rapide, taille limitée.            | Plus lent, mais flexible.         |

**Exemple en C#** :
```csharp
void MaMethode()
{
    int x = 5; // Stack
    var liste = new List<int>(); // Heap
}
```

#### **Garbage Collection (GC)**
- Libère automatiquement la mémoire des objets inutilisés sur le **heap**.
- **Générations** :
  - **Génération 0** : Objets récents (collecte fréquente).
  - **Génération 1** : Objets survivants.
  - **Génération 2** : Objets longévifs (collecte rare).

**Exemple de GC** :
```csharp
var obj = new object(); // Alloué sur le heap
obj = null; // L'objet devient éligible pour le GC
// GC.Collect(); // À éviter en production
```

---

## **5. Exemple Complet en C#** <a name="5-exemple-complet-en-c"></a>

### **Étape 1 : Écriture du Code**
```csharp
using System;

class Personne
{
    public string Nom { get; set; }

    public Personne(string nom)
    {
        Nom = nom;
    }

    public void SePresenter()
    {
        Console.WriteLine($"Bonjour, je m'appelle {Nom}.");
    }
}

class Program
{
    static void Main()
    {
        var personne = new Personne("Alice");
        personne.SePresenter();
    }
}
```

### **Étape 2 : Compilation**
```bash
dotnet build
```
- Génère `Program.dll` (contenant le IL).

### **Étape 3 : Exécution**
```bash
dotnet run
```
1. Le **CLR** charge `Program.dll`.
2. Le **JIT** compile les méthodes en code machine.
3. Le programme s'exécute.

### **Étape 4 : Inspection du IL**
Avec **ildasm** :
```bash
ildasm bin/Debug/net8.0/Program.dll
```
- Vous verrez le IL pour la classe `Personne` et la méthode `Main`.

---

## **6. Outils et Bonnes Pratiques** <a name="6-outils-et-bonnes-pratiques"></a>

### **Outils Utiles**
| **Outil**               | **Utilité**                                      |
|-------------------------|--------------------------------------------------|
| **ILSpy**               | Décompiler un assembly en C# ou IL.              |
| **BenchmarkDotNet**     | Mesurer les performances.                        |
| **Visual Studio Diagnostic Tools** | Analyser la mémoire et le CPU.          |
| **SharpLab**            | Voir le IL généré en ligne.                      |

### **Bonnes Pratiques**
1. **Évitez les boxings** (conversion d'un `ValueType` en `ReferenceType`) :
   ```csharp
   int x = 42;
   object o = x; // Boxing → allocation sur le heap
   ```
2. **Utilisez `Span<T>`** pour réduire les allocations :
   ```csharp
   Span<int> nombres = stackalloc int[100]; // Alloué sur la stack
   ```
3. **Préférez les `struct`** pour les petites données immuables.
4. **Limitez les appels à `GC.Collect()`**.

---

## **7. Comparaison avec d'Autres Langages** <a name="7-comparaison-avec-d-autres-langages"></a>

| **Langage** | **Compilation**               | **Runtime**       | **Exécution**                     |
|-------------|-------------------------------|-------------------|-----------------------------------|
| **C**       | Native (code machine).        | Aucun.            | Exécutable direct.                |
| **C#**      | IL (portable).                | CLR.              | JIT → code machine.               |
| **Java**    | Bytecode (portable).          | JVM.              | JIT → code machine.               |
| **Python**  | Interprété (ou compilé avec PyPy). | CPython.      | Interprété ligne par ligne.       |

---

## **8. Exercices Pratiques** <a name="8-exercices-pratiques"></a>

### **Exercice 1 : Inspecter le IL**
1. Écrivez une classe `Calculatrice` avec une méthode `Additionner`.
2. Compilez avec `dotnet build`.
3. Utilisez **ILSpy** pour voir le IL généré.

### **Exercice 2 : Mesurer les Performances**
1. Comparez les performances d'une `struct` vs `class` avec **BenchmarkDotNet**.
   ```csharp
   [MemoryDiagnoser]
   public class BenchmarkStructVsClass
   {
       [Benchmark]
       public void AvecStruct()
       {
           var point = new PointStruct { X = 1, Y = 2 };
       }

       [Benchmark]
       public void AvecClass()
       {
           var point = new PointClass { X = 1, Y = 2 };
       }
   }

   public struct PointStruct { public int X; public int Y; }
   public class PointClass { public int X; public int Y; }
   ```
2. Exécutez avec :
   ```bash
   dotnet run -c Release
   ```

### **Exercice 3 : Analyser la Mémoire**
1. Utilisez **Visual Studio Diagnostic Tools** pour observer l'allocation mémoire :
   ```csharp
   var liste = new List<int>();
   for (int i = 0; i < 10000; i++)
   {
       liste.Add(i);
   }
   ```

---

## **Résumé**
1. **Compilation** : Le code C# est transformé en **IL** par Roslyn.
2. **Runtime (CLR)** : Charge le IL, le compile en code machine (JIT), et gère la mémoire.
3. **Exécution** : Le code machine est exécuté par le CPU.
4. **Gestion mémoire** : Le **GC** nettoie automatiquement le heap.

---
**Prochaine étape** : Approfondir le **JIT**, le **garbage collection**, ou les **optimisations** ? 😊