using Telegram.Bot;
using Telegram.Bot.Types;

namespace CrmTGBot.Interfaces
{
    public interface IMessageHandler
    {
        /// <summary>
        /// Проверяет, нужно ли обрабатывать текущее сообщение, 
        /// и если да — обрабатывает его и возвращает true.
        /// </summary>
        /// <param name="client">Экземпляр ITelegramBotClient</param>
        /// <param name="update">Update из Telegram</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Возвращает true, если сообщение обработано (дальше искать не надо), иначе false.</returns>
        Task<bool> HandleAsync(ITelegramBotClient client, Update update, CancellationToken ct);
    }
}
