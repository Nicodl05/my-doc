using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Début des opérations asynchrones");

        Task task1 = PerformMathOperationAsync(1, "Addition", (a, b) => a + b);
        Task task2 = PerformMathOperationAsync(2, "Soustraction", (a, b) => a - b);
        Task task3 = PerformMathOperationAsync(3, "Multiplication", (a, b) => a * b);

        await Task.WhenAll(task1, task2, task3);

        Console.WriteLine("Fin des opérations asynchrones");
    }

    static async Task PerformMathOperationAsync(int id, string operationName, Func<int, int, int> operation)
    {
        Console.WriteLine($"Début de l'opération {id}: {operationName}");
        await Task.Delay(2000); // Simule une opération longue
        int result = operation(10, 5);
        Console.WriteLine($"Résultat de l'opération {id}: {result}");
    }
}
