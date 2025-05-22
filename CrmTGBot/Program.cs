using CrmTGBot.Interfaces;
using CrmTGBot.Services;
using Telegram.Bot;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var token = Environment.GetEnvironmentVariable("BOT_TOKEN");


        // Регистрируем TelegramBotClient
        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token!));
        services.AddHttpClient();

        // Регистрируем необходимые сервисы
        services.AddScoped<ITelegramService, TelegramService>();
        services.AddScoped<IMessageHandler, AppointmentMessageHandler>();
        services.AddScoped<IMessageHandler, StartMessageHandler>();
    })
    .Build();



// Запуск бота
var telegramService = host.Services.GetRequiredService<ITelegramService>();
await telegramService.RunAsync();

// Запуск Host для постоянного прослушивания
await host.RunAsync();
