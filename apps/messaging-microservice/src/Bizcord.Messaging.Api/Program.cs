using Bizcord.MessageClient;
using Bizcord.Messaging.Api.Application;
using Bizcord.Messaging.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHealthChecks();

var connectionString = builder.Configuration.GetConnectionString("Messages") ?? "Data Source=data/messages.db";
builder.Services.AddSingleton(new SqliteMessageRepository(connectionString));
builder.Services.AddSingleton<IMessageRepository>(provider => provider.GetRequiredService<SqliteMessageRepository>());
builder.Services.AddScoped<MessageService>();

var brokerHost = builder.Configuration["RABBITMQ_HOST"] ?? builder.Configuration["RabbitMq:Host"] ?? "localhost";
var brokerUser = builder.Configuration["RabbitMq:Username"] ?? "bizcord";
var brokerPassword = builder.Configuration["RabbitMq:Password"] ?? "bizcord-local-dev";
builder.Services.AddMessageClient($"host={brokerHost};username={brokerUser};password={brokerPassword};timeout=5");

var app = builder.Build();
await app.Services.GetRequiredService<SqliteMessageRepository>().InitializeAsync();
app.UseExceptionHandler();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => new { service = "Bizcord Messaging", messages = "/api/messages", health = "/health" });
app.Run();

public partial class Program;
