using ChatSupportAPI.Models;
using ChatSupportAPI.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddSingleton<IChatSupportEngine, ChatSupportEngine>();

builder.Services.AddSingleton<PeriodicHostedService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<PeriodicHostedService>());

builder.Services.Configure<ChatSupportSettings>(builder.Configuration.GetSection(nameof(ChatSupportSettings)));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();

// chat API
app.MapGet("/monitor", (IChatSupportEngine engine)
    => engine.GetInfo());

app.MapPost("/chat", (string userName, IChatSupportEngine engine) 
    => engine.StartChat(userName));

app.MapGet("/chat", (string sessionId, IChatSupportEngine engine) 
    => engine.GetChat(sessionId));

app.MapPost("/chat/send", (string sessionId, string message, IChatSupportEngine engine) 
    => engine.SendChat(sessionId, message));

app.MapDelete("/chat", (string sessionId, IChatSupportEngine engine) => 
    engine.DisconnectChat(sessionId));

app.Run();
