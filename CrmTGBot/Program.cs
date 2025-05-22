using CrmTGBot.Interfaces;
using CrmTGBot.Services;
using Telegram.Bot;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var token = Environment.GetEnvironmentVariable("BOT_TOKEN");


        // ������������ TelegramBotClient
        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token!));
        services.AddHttpClient();

        // ������������ ����������� �������
        services.AddScoped<ITelegramService, TelegramService>();
        services.AddScoped<IMessageHandler, AppointmentMessageHandler>();
        services.AddScoped<IMessageHandler, StartMessageHandler>();
    })
    .Build();



// ������ ����
var telegramService = host.Services.GetRequiredService<ITelegramService>();
await telegramService.RunAsync();

// ������ Host ��� ����������� �������������
await host.RunAsync();
