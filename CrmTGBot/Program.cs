using CrmTGBot.Interfaces;
using CrmTGBot.Services;
using Telegram.Bot;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// === Загрузка конфигурации ===
var token = Environment.GetEnvironmentVariable("BOT_TOKEN");
if (string.IsNullOrEmpty(token))
{
    Console.WriteLine("? BOT_TOKEN не найден в переменных среды.");
    return;
}

// === Регистрация зависимостей ===
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token));

// Telegram-сервисы
builder.Services.AddScoped<ITelegramService, TelegramService>();
builder.Services.AddScoped<IMessageHandler, StartMessageHandler>();
builder.Services.AddScoped<IMessageHandler, AppointmentMessageHandler>();

// Контроллеры API
builder.Services.AddControllers();

// === Построение приложения ===
var app = builder.Build();

// === Настройка маршрутизации ===
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // включение поддержки контроллеров
});

// === Запуск Telegram-бота через scope ===
using (var scope = app.Services.CreateScope())
{
    var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
    await telegramService.RunAsync();
}

// === Запуск ASP.NET Core API ===
await app.RunAsync();




//using CrmTGBot.Interfaces;
//using CrmTGBot.Services;
//using Telegram.Bot;

//var host = Host.CreateDefaultBuilder(args)
//    .ConfigureServices((context, services) =>
//    {
//        var token = Environment.GetEnvironmentVariable("BOT_TOKEN");


//        // Регистрируем TelegramBotClient
//        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token!));
//        services.AddHttpClient();

//        // Регистрируем необходимые сервисы
//        services.AddScoped<ITelegramService, TelegramService>();
//        services.AddScoped<IMessageHandler, AppointmentMessageHandler>();
//        services.AddScoped<IMessageHandler, StartMessageHandler>();
//    })
//    .Build();



//// Запуск бота
//var telegramService = host.Services.GetRequiredService<ITelegramService>();
//await telegramService.RunAsync();

//// Запуск Host для постоянного прослушивания
//await host.RunAsync();
