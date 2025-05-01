namespace CrmTGBot.DTO
{
    public class AppointmentConversation
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Service { get; set; }
        public DateTime? Time { get; set; }
        public int Step { get; set; } = 0; // 0 - имя, 1 - телефон, 2 - услуга, 3 - дата/время
    }
}
