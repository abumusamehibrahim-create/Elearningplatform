namespace ELearningPlatform.Models
{
    public class LicenseDashboardViewModel
    {
        public string ClientName { get; set; }
        public string Domain { get; set; }
        public string LicenseKey { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsActive { get; set; }
        public int DaysLeft { get; set; }
        public string Status { get; set; }
    }
}
