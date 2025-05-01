using CrmTGBot.Interfaces;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CrmTGBot.Services
{
    public class StartMessageHandler : IMessageHandler
    {
        [Obsolete]
        public async Task<bool> HandleAsync(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            if (update.Type != UpdateType.Message || update.Message?.Text is not { } text)
                return false;

            if (text.ToLower() != "/start")
                return false;

            var chatId = update.Message.Chat.Id;

            string welcome = "👋 Добро пожаловать в *Салон красоты «АртСтиль»*!\n\n" +
                             "💇 Мы предлагаем стрижки, укладки, маникюр, педикюр и многое другое.\n\n" +
                             "📅 Чтобы записаться на приём, нажмите кнопку ниже:";

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("📅 Записаться") }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };

            await client.SendTextMessageAsync(
                chatId,
                welcome,
                replyMarkup: keyboard,
                parseMode: ParseMode.Markdown,
                cancellationToken: ct
            );

            return true;
        }
    }
}
