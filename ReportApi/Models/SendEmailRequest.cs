namespace ReportApi.Models
{
    public class SendEmailRequest
    {
        public string Recipient { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public string? AttachmentBase64 { get; set; }
        public string? AttachmentFileName { get; set; }
        public string? AttachmentContentType { get; set; }
    }
}
