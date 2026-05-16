namespace Elearningplatform.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
       // public string Message { get; set; }
        //public string StackTrace { get; set; }
//        public string UserName { get; set; }
     //   public DateTime CreatedAt { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
