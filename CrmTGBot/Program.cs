using CrmTGBot.Interfaces;
using CrmTGBot.Services;
using Telegram.Bot;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// === �������� ������������ ===
var token = Environment.GetEnvironmentVariable("BOT_TOKEN");
if (string.IsNullOrEmpty(token))
{
    Console.WriteLine("? BOT_TOKEN �� ������ � ���������� �����.");
    return;
}

// === ����������� ������������ ===
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token));

// Telegram-�������
builder.Services.AddScoped<ITelegramService, TelegramService>();
builder.Services.AddScoped<IMessageHandler, StartMessageHandler>();
builder.Services.AddScoped<IMessageHandler, AppointmentMessageHandler>();

// ����������� API
builder.Services.AddControllers();

// === ���������� ���������� ===
var app = builder.Build();

// === ��������� ������������� ===
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // ��������� ��������� ������������
});

// === ������ Telegram-���� ����� scope ===
using (var scope = app.Services.CreateScope())
{
    var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
    await telegramService.RunAsync();
}

// === ������ ASP.NET Core API ===
await app.RunAsync();




//using CrmTGBot.Interfaces;
//using CrmTGBot.Services;
//using Telegram.Bot;

//var host = Host.CreateDefaultBuilder(args)
//    .ConfigureServices((context, services) =>
//    {
//        var token = Environment.GetEnvironmentVariable("BOT_TOKEN");


//        // ������������ TelegramBotClient
//        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token!));
//        services.AddHttpClient();

//        // ������������ ����������� �������
//        services.AddScoped<ITelegramService, TelegramService>();
//        services.AddScoped<IMessageHandler, AppointmentMessageHandler>();
//        services.AddScoped<IMessageHandler, StartMessageHandler>();
//    })
//    .Build();



//// ������ ����
//var telegramService = host.Services.GetRequiredService<ITelegramService>();
//await telegramService.RunAsync();

//// ������ Host ��� ����������� �������������
//await host.RunAsync();
