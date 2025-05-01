using CrmTGBot.Interfaces;
using CrmTGBot.DTO;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CrmTGBot.Services
{
    public class AppointmentMessageHandler : IMessageHandler
    {
        private readonly HttpClient _httpClient;
        private readonly string _serviceApi;
        private static readonly Dictionary<long, AppointmentConversation> _conversations = new();

        public AppointmentMessageHandler(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _serviceApi = Environment.GetEnvironmentVariable("CrmService_api");
        }

        [Obsolete]
        public async Task<bool> HandleAsync(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            if (update.Type != UpdateType.Message || update.Message?.Text is null)
                return false;

            var chatId = update.Message.Chat.Id;
            var text = update.Message.Text.Trim();
            if (!_conversations.ContainsKey(chatId))
            {
                if (text != "📅 Записаться")
                    return false;

                _conversations[chatId] = new AppointmentConversation();
                await client.SendTextMessageAsync(chatId, "Как вас зовут?", cancellationToken: ct);
                return true;
            }

            if (!_conversations.TryGetValue(chatId, out var convo))
            {
                convo = new AppointmentConversation();
                _conversations[chatId] = convo;
                await client.SendTextMessageAsync(chatId, "Как вас зовут?", cancellationToken: ct);
                return true;
            }

            switch (convo.Step)
            {
                case 0:
                    convo.FullName = text;
                    convo.Step++;
                    await client.SendTextMessageAsync(chatId, "Ваш номер телефона? (в формате +79991234567)", cancellationToken: ct);
                    return true;

                case 1:
                    convo.PhoneNumber = text;
                    convo.Step++;
                    await client.SendTextMessageAsync(chatId, "Какую услугу вы хотите? (например: маникюр)", cancellationToken: ct);
                    return true;

                case 2:
                    convo.Service = text;
                    convo.Step++;
                    await client.SendTextMessageAsync(chatId, "Введите дату и время в формате `ДД.ММ.ГГГГ ЧЧ:ММ`", parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return true;

                case 3:
                    if (!DateTime.TryParse(text, out var dt))
                    {
                        await client.SendTextMessageAsync(chatId, "Неверный формат даты. Повторите:", cancellationToken: ct);
                        return true;
                    }

                    convo.Time = DateTime.SpecifyKind(dt, DateTimeKind.Utc); // безопасно для PostgreSQL

                    // Отправка в API
                    var request = new AppointmentRequestDto
                    {
                        FullName = convo.FullName!,
                        PhoneNumber = convo.PhoneNumber!,
                        Service = convo.Service!,
                        Time = convo.Time.Value,
                        TelegramChatId = chatId
                    };

                    try
                    {
                        var response = await _httpClient.PostAsJsonAsync(_serviceApi, request, ct);

                        if (!response.IsSuccessStatusCode)
                        {
                            await client.SendTextMessageAsync(chatId, "Ошибка при создании заявки.", cancellationToken: ct);
                            return true;
                        }

                        var result = await response.Content.ReadFromJsonAsync<AppointmentResponseDto>(cancellationToken: ct);
                        await client.SendTextMessageAsync(chatId,
                            $"✅ Заявка создана!\n\n" +
                            $"👤 {result.ClientName}\n" +
                            $"📞 {request.PhoneNumber}\n" +
                            $"💅 {result.Service}\n" +
                            $"📅 {result.Time:G}\n" +
                            $"Статус: На подтверждении",
                            parseMode: ParseMode.Markdown, cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(chatId, $"Ошибка: {ex.Message}", cancellationToken: ct);
                    }

                    _conversations.Remove(chatId); // очистить состояние
                    return true;
            }

            return false;
        }
    }
}

