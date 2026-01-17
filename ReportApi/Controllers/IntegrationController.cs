using Microsoft.AspNetCore.Mvc;
using ReportApi.Models;
using System.Text;
using System.Text.Json;

namespace ReportApi.Controllers
{
    [ApiController]
    [Route("api/integration")]
    public class IntegrationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IntegrationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

    
        [HttpPost("send-csv-to-email")]
        public async Task<IActionResult> SendCsvToEmail([FromQuery] string email, [FromBody] ReportModel model)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "email is required" });

            var performance = GetPerformance(model.AverageGrade);

            var csv = new StringBuilder();
            csv.AppendLine("StudentId;StudentName;TaskCount;AverageGrade;Performance");
            csv.AppendLine($"{model.StudentId};{Escape(model.StudentName)};{model.TaskCount};{model.AverageGrade};{performance}");

            var req = new SendEmailRequest
            {
                Recipient = email.Trim(),
                Subject = $"Индивидуальный CSV-отчёт (StudentId: {model.StudentId})",
                Body = "Отчёт сформирован.\n\nCSV (скопируй и сохрани как report.csv):\n\n" + csv.ToString()
            };

            return await SendViaOtpravka(req);
        }

        // Новое
        [HttpPost("send-group-csv-to-email")]
        public async Task<IActionResult> SendGroupCsvToEmail([FromQuery] string email, [FromBody] GroupReportRequest request)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "email is required" });

            if (request == null || string.IsNullOrWhiteSpace(request.GroupName))
                return BadRequest(new { message = "GroupName is required" });

            if (request.Students == null || request.Students.Count == 0)
                return BadRequest(new { message = "Students list is required" });

            var sb = new StringBuilder();
            sb.AppendLine("GroupName;StudentId;StudentName;TaskCount;AverageGrade;Performance");

            foreach (var s in request.Students)
            {
                var perf = GetPerformance(s.AverageGrade);
                sb.AppendLine($"{Escape(request.GroupName)};{s.StudentId};{Escape(s.StudentName)};{s.TaskCount};{s.AverageGrade};{perf}");
            }

            var req = new SendEmailRequest
            {
                Recipient = email.Trim(),
                Subject = $"Групповой CSV-отчёт ({request.GroupName})",
                Body = "Групповой отчёт сформирован.\n\nCSV (скопируй и сохрани как report.csv):\n\n" + sb.ToString()
            };

            return await SendViaOtpravka(req);
        }

        private async Task<IActionResult> SendViaOtpravka(SendEmailRequest req)
        {
            var client = _httpClientFactory.CreateClient("OtpravkaApi");

            if (client.BaseAddress == null)
                return StatusCode(500, new { message = "OtpravkaApi:BaseUrl is not configured in appsettings.json" });

            var json = JsonSerializer.Serialize(req);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync("api/EmailReport/send", content);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode, new { message = "OtpravkaApi error", details = body });

            return Ok(new { message = "Отправлено", recipient = req.Recipient });
        }

        private static string GetPerformance(double avg)
        {
            return avg switch
            {
                >= 4.5 => "Отлично",
                >= 3.5 => "Хорошо",
                _ => "Удовлетворительно"
            };
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.Contains(';') || s.Contains('"') || s.Contains('\n'))
            {
                s = s.Replace("\"", "\"\"");
                return $"\"{s}\"";
            }
            return s;
        }
    }
}
