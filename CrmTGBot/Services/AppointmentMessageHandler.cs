using CrmTGBot.Interfaces;
using CrmTGBot.DTO;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CrmTGBot.Services
{
    public class AppointmentMessageHandler : IMessageHandler
    {
        private readonly HttpClient _http;
        private readonly string _api;
        private static readonly Dictionary<long, AppointmentConversation> _conv = new();

        public AppointmentMessageHandler(HttpClient http)
        {
            _http = http;
            _api = Environment.GetEnvironmentVariable("CrmService_api")!;
        }

        [Obsolete]
        public async Task<bool> HandleAsync(ITelegramBotClient bot, Update upd, CancellationToken ct)
        {
            if (upd.Type != UpdateType.Message || upd.Message?.Text is null)
                return false;

            var chat = upd.Message.Chat.Id;
            var msg = upd.Message.Text.Trim();

            // ---------- старт диалога ----------
            if (!_conv.ContainsKey(chat))
            {
                if (msg != "📅 Записаться") return false;

                _conv[chat] = new();
                await bot.SendTextMessageAsync(chat, "Как вас зовут?", cancellationToken: ct);
                return true;
            }

            var c = _conv[chat];

            // ---------- шаги диалога ----------
            switch (c.Step)
            {
                case 0:                                             // ФИО
                    c.FullName = msg;
                    c.Step = 1;
                    await bot.SendTextMessageAsync(chat, "Ваш номер телефона? (формат +79991234567)", cancellationToken: ct);
                    return true;

                case 1:                                             // телефон
                    c.PhoneNumber = msg;
                    c.Step = 2;

                    // получаем услуги
                    c.Services = await _http.GetFromJsonAsync<List<ServiceItemDto>>($"{_api}/serviceitems", ct);
                    var list = string.Join('\n', c.Services.Select((s, i) => $"{i + 1}. {s.Name}"));
                    await bot.SendTextMessageAsync(chat, $"Выберите услугу, ответив её номером:\n{list}", cancellationToken: ct);
                    return true;

                case 2:                                             // выбор услуги
                    if (!int.TryParse(msg, out var sNum) || sNum < 1 || sNum > c.Services.Count)
                    {
                        await bot.SendTextMessageAsync(chat, "Номер услуги не распознан, попробуйте ещё раз:", cancellationToken: ct);
                        return true;
                    }
                    c.ServiceItemId = c.Services[sNum - 1].Id;
                    c.Step = 3;

                    // получаем мастеров
                    c.Masters = await _http.GetFromJsonAsync<List<MasterDto>>($"{_api}/masters", ct) ?? [];
                    var mList = string.Join('\n', c.Masters.Select((m, i) => $"{i + 1}. {m.FullName}"));
                    await bot.SendTextMessageAsync(chat, $"К какому мастеру? Ответьте номером:\n{mList}", cancellationToken: ct);
                    return true;

                case 3:                                             // выбор мастера
                    if (!int.TryParse(msg, out var mNum) || mNum < 1 || mNum > c.Masters.Count)
                    {
                        await bot.SendTextMessageAsync(chat, "Номер мастера не распознан, попробуйте ещё раз:", cancellationToken: ct);
                        return true;
                    }
                    c.MasterId = c.Masters[mNum - 1].Id;
                    c.Step = 4;

                    await bot.SendTextMessageAsync(chat,
                        "Дата и время в формате `ДД.ММ.ГГГГ ЧЧ:ММ`",
                        parseMode: ParseMode.Markdown, cancellationToken: ct);
                    return true;

                case 4:                                             // дата/время
                    if (!DateTime.TryParse(msg, out var dt))
                    {
                        await bot.SendTextMessageAsync(chat, "Неверный формат даты, попробуйте ещё раз:", cancellationToken: ct);
                        return true;
                    }
                    c.Time = DateTime.SpecifyKind(dt, DateTimeKind.Utc);

                    // ---------- отправляем в CRM ----------
                    var req = new AppointmentRequestDto
                    {
                        FullName = c.FullName,
                        PhoneNumber = c.PhoneNumber,
                        ServiceItemId = c.ServiceItemId!.Value,
                        MasterId = c.MasterId!.Value,
                        Time = c.Time.Value,
                        TelegramChatId = chat
                    };

                    try
                    {
                        var resp = await _http.PostAsJsonAsync($"{_api}/appointments", req, ct);
                        if (!resp.IsSuccessStatusCode)
                        {
                            await bot.SendTextMessageAsync(chat, "🚫 Ошибка при создании заявки.", cancellationToken: ct);
                            return true;
                        }

                        await bot.SendTextMessageAsync(chat,
                            $"✅ Заявка создана!\n\n" +
                            $"👤 {req.FullName}\n" +
                            $"📞 {req.PhoneNumber}\n" +
                            $"💅 {c.Services.First(s => s.Id == req.ServiceItemId).Name}\n" +
                            $"🧑‍🔧 {c.Masters.First(m => m.Id == req.MasterId).FullName}\n" +
                            $"📅 {req.Time:dd.MM.yyyy HH:mm}\n" +
                            $"Статус: На подтверждении",
                            cancellationToken: ct);

                    }
                    catch (Exception ex)
                    {
                        await bot.SendTextMessageAsync(chat, $"Ошибка: {ex.Message}", cancellationToken: ct);
                    }

                    _conv.Remove(chat);          // конец диалога
                    return true;
            }

            return false;
        }
    }

}

