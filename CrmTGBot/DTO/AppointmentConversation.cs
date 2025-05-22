namespace CrmTGBot.DTO
{
    public class AppointmentConversation
    {
        public int Step { get; set; } = 0;

        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // списки «на время диалога»
        public List<ServiceItemDto> Services { get; set; } = new();
        public int? ServiceItemId { get; set; }

        public List<MasterDto> Masters { get; set; } = new();
        public int? MasterId { get; set; }

        public DateTime? Time { get; set; }
    }

}
