using Grpc.Net.Client;
using GrpcCore; // Utilisez le namespace défini dans le fichier .proto

var channel = GrpcChannel.ForAddress("http://localhost:5152");
var client = new Greeter.GreeterClient(channel);
var reply = await client.SayHelloAsync(new HelloRequest { Name = "Nicolas" });

Console.WriteLine("Greeting: " + reply.Message);
