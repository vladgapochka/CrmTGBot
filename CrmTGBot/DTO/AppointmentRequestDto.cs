namespace CrmTGBot.DTO
{
    public class AppointmentRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public long TelegramChatId { get; set; }
    }
}
