using static Telegram.Bot.TelegramBotClient;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using CrmTGBot.Interfaces;
using Telegram.Bot.Types;

namespace CrmTGBot.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IEnumerable<IMessageHandler> _messageHandlers;


        public TelegramService(ITelegramBotClient botClient, IEnumerable<IMessageHandler> messageHandlers)
        {


            _botClient = botClient;
            _messageHandlers = messageHandlers;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Starting the bot...");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: OnErrorAsync,
                receiverOptions: receiverOptions
            );

            Console.WriteLine("Bot is running. Press any key to exit.");
            Console.ReadKey();
        }

        [Obsolete]
        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct)
        {

            foreach (var handler in _messageHandlers)
            {
                bool handled = await handler.HandleAsync(client, update, ct);
                if (handled) break;
            }


         }

        private Task OnErrorAsync(ITelegramBotClient client, Exception ex, CancellationToken ct)
        {
            Console.WriteLine($"Bot error: {ex.Message}");
            return Task.CompletedTask;
        }
    }
}
