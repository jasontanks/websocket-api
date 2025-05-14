using WebSocketRabbitMQ.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<RabbitMQService>(); // ✅ Ensure this is added

builder.Services.AddControllers();

var app = builder.Build();
app.UseWebSockets();

app.MapControllers(); // ✅ Directly mapping controllers

var rabbitService = app.Services.GetRequiredService<RabbitMQService>();
await rabbitService.InitializeAsync(); // ✅ Ensures async initialization
await Task.Run(() => rabbitService.ConsumeMessagesAsync());

app.Run();
