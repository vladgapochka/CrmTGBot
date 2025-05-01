namespace CrmTGBot.DTO
{
    public class AppointmentResponseDto
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
