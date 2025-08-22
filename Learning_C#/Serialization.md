# **Sérialisation et Désérialisation en C#**

La **sérialisation** et la **désérialisation** sont des mécanismes essentiels pour convertir des objets en un format stockable ou transmissible (comme du JSON, du XML ou du binaire), puis pour les reconstituer. Ces techniques sont utilisées pour :
- Sauvegarder l'état d'un objet (dans un fichier, une base de données).
- Transmettre des objets sur un réseau (API, microservices).
- Stocker des données dans un cache.

---

## **1. Concepts de Base**

### **Sérialisation**
Processus de conversion d'un **objet** en un **flux de données** (JSON, XML, binaire, etc.).

### **Désérialisation**
Processus inverse : reconstitution d'un objet à partir d'un flux de données.

---

## **2. Formats de Sérialisation Courants en C#**

| **Format** | **Avantages** | **Inconvénients** | **Cas d'usage** |
|------------|--------------|------------------|----------------|
| **JSON** | Lisible, léger, largement utilisé | Moins performant que le binaire | APIs REST, configuration, stockage |
| **XML** | Lisible, standardisé | Verbosité, moins performant | Fichiers de configuration, SOAP |
| **Binaire** | Rapide, compact | Illisible, dépendant de la plateforme | Performances critiques, stockage interne |

---

## **3. Sérialisation JSON avec `System.Text.Json`**

### **Installation**
Ajoutez le package NuGet :
```bash
dotnet add package System.Text.Json
```

### **Exemple de classe**
```csharp
public class Personne
{
    public string Nom { get; set; }
    public int Age { get; set; }
    public List<string> Hobbies { get; set; }
}
```

### **Sérialisation (Objet → JSON)**
```csharp
using System.Text.Json;

// Créer un objet
var personne = new Personne
{
    Nom = "Alice",
    Age = 30,
    Hobbies = new List<string> { "Lecture", "Randonnée" }
};

// Sérialiser en JSON
string jsonString = JsonSerializer.Serialize(personne);
Console.WriteLine(jsonString);
// Résultat : {"Nom":"Alice","Age":30,"Hobbies":["Lecture","Randonnée"]}
```

### **Désérialisation (JSON → Objet)**
```csharp
string json = @"{""Nom"":""Alice"",""Age"":30,""Hobbies"":[""Lecture"",""Randonnée""]}";

Personne personneDeserialisee = JsonSerializer.Deserialize<Personne>(json);
Console.WriteLine(personneDeserialisee.Nom); // Affiche : Alice
```

---

### **Options de Sérialisation**
```csharp
var options = new JsonSerializerOptions
{
    WriteIndented = true, // JSON formaté (lisible)
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // nomDesProprietes → nomDesProprietes
};

string jsonString = JsonSerializer.Serialize(personne, options);
```

---

## **4. Sérialisation XML avec `System.Xml.Serialization`**

### **Installation**
Aucun package supplémentaire nécessaire (disponible dans .NET par défaut).

### **Exemple de classe**
```csharp
[Serializable]
public class Personne
{
    public string Nom { get; set; }
    public int Age { get; set; }
    public List<string> Hobbies { get; set; }
}
```

### **Sérialisation (Objet → XML)**
```csharp
using System.Xml.Serialization;

var serializer = new XmlSerializer(typeof(Personne));
using (var writer = new StringWriter())
{
    serializer.Serialize(writer, personne);
    string xmlString = writer.ToString();
    Console.WriteLine(xmlString);
}
// Résultat :
// <?xml version="1.0" encoding="utf-16"?>
// <Personne>
//   <Nom>Alice</Nom>
//   <Age>30</Age>
//   <Hobbies>
//     <string>Lecture</string>
//     <string>Randonnée</string>
//   </Hobbies>
// </Personne>
```

### **Désérialisation (XML → Objet)**
```csharp
string xml = @"<Personne>
                <Nom>Alice</Nom>
                <Age>30</Age>
                <Hobbies>
                  <string>Lecture</string>
                  <string>Randonnée</string>
                </Hobbies>
              </Personne>";

using (var reader = new StringReader(xml))
{
    Personne personneDeserialisee = (Personne)serializer.Deserialize(reader);
    Console.WriteLine(personneDeserialisee.Nom); // Affiche : Alice
}
```

---

## **5. Sérialisation Binaire avec `BinaryFormatter`**

⚠️ **Attention** : `BinaryFormatter` est **obsolète** depuis .NET 5+ pour des raisons de sécurité. Préférez d'autres solutions comme **Protocol Buffers** ou des bibliothèques tierces.

### **Alternative moderne : `System.Text.Json` en binaire**
```csharp
byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(personne);
Personne personneDeserialisee = JsonSerializer.Deserialize<Personne>(bytes);
```

---

## **6. Attributs pour Contrôler la Sérialisation**

### **Ignorer une propriété**
```csharp
public class Personne
{
    public string Nom { get; set; }

    [JsonIgnore] // Ignoré en JSON
    public string MotDePasse { get; set; }
}
```

### **Renommer une propriété**
```csharp
public class Personne
{
    [JsonPropertyName("full_name")] // Nom personnalisé en JSON
    public string Nom { get; set; }
}
```

---

## **7. Sérialisation Personnalisée**

### **Implémenter `ISerializable` (pour le binaire)**
```csharp
[Serializable]
public class Personne : ISerializable
{
    public string Nom { get; set; }
    public int Age { get; set; }

    // Constructeur pour la désérialisation
    protected Personne(SerializationInfo info, StreamingContext context)
    {
        Nom = info.GetString("nom");
        Age = info.GetInt32("age");
    }

    // Méthode pour la sérialisation
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("nom", Nom);
        info.AddValue("age", Age);
    }
}
```

---

## **8. Bonnes Pratiques**

1. **Évitez de sérialiser des données sensibles** (mots de passe, clés secrètes).
2. **Utilisez des versions tolérantes** pour éviter les erreurs si le format change.
3. **Préférez JSON** pour les APIs et le stockage moderne.
4. **Gérez les exceptions** lors de la désérialisation (ex : données corrompues).

---

## **9. Cas Pratique : Sauvegarder dans un Fichier**

### **Sauvegarder un objet en JSON**
```csharp
string cheminFichier = "personne.json";
string jsonString = JsonSerializer.Serialize(personne);
File.WriteAllText(cheminFichier, jsonString);
```

### **Charger un objet depuis un fichier**
```csharp
string jsonFromFile = File.ReadAllText(cheminFichier);
Personne personneChargee = JsonSerializer.Deserialize<Personne>(jsonFromFile);
```

---

## **10. Performances et Alternatives**

- **Pour des performances maximales** : Utilisez **Protocol Buffers** (`Google.Protobuf`) ou **MessagePack**.
- **Pour des objets complexes** : Utilisez des bibliothèques comme **Newtonsoft.Json** (plus flexible que `System.Text.Json`).

---

## **Résumé**

| **Action** | **JSON** | **XML** | **Binaire** |
|------------|----------|---------|-------------|
| **Sérialiser** | `JsonSerializer.Serialize()` | `XmlSerializer.Serialize()` | `JsonSerializer.SerializeToUtf8Bytes()` |
| **Désérialiser** | `JsonSerializer.Deserialize<T>()` | `XmlSerializer.Deserialize()` | `JsonSerializer.Deserialize<T>(bytes)` |
| **Avantages** | Lisible, moderne | Standardisé | Compact, rapide |
| **Inconvénients** | Moins performant | Verbosité | Illisible |

---

## **Exercice Pratique**
1. Créez une classe `Produit` avec des propriétés `Id`, `Nom` et `Prix`.
2. Sérialisez un objet `Produit` en JSON et sauvegardez-le dans un fichier.
3. Chargez le fichier et désérialisez-le pour reconstituer l'objet.

