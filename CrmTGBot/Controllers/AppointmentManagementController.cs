using CrmTGBot.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;

namespace CrmTGBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentManagementController : ControllerBase
    {
        private readonly ITelegramBotClient _botClient;

        public AppointmentManagementController(ITelegramBotClient telegramBotClient)
        {
            _botClient = telegramBotClient;
        }

        [Obsolete]
        [HttpPost("send")]
        public async Task<IActionResult> NotifyUser([FromBody] BotNotificationDto dto)
        {
            if (dto.ChatId == 0 || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Некорректные данные");

            try
            {
                await _botClient.SendTextMessageAsync(
                    chatId: dto.ChatId,
                    text: dto.Message
                );

                return Ok(new { message = "Уведомление отправлено" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
