namespace ReportApi.Models
{
    public class SendEmailRequest
    {
        public string Recipient { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
    }
}
