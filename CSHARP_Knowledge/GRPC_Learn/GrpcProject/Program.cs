using GrpcProject.Services; // Assurez-vous que ce namespace est correct

var builder = WebApplication.CreateBuilder(args);

// Ajoutez les services gRPC au conteneur.
builder.Services.AddGrpc();

var app = builder.Build();

// Configurez le pipeline de requête HTTP.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<HealthCheckService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
