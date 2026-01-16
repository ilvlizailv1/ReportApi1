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

        // POST: /api/integration/send-csv-to-email?email=...
        [HttpPost("send-csv-to-email")]
        public async Task<IActionResult> SendCsvToEmail([FromQuery] string email, [FromBody] ReportModel model)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "email is required" });

            // 1) Формируем CSV по данным отчёта (по сути — “мини-таблица”)
            var csv = BuildCsv(model);

            // 2) Готовим запрос в OtpravkaApi (оно отправляет текст, вложений нет)
            var req = new SendEmailRequest
            {
                Recipient = email.Trim(),
                Subject = $"CSV-отчёт по успеваемости (StudentId: {model.StudentId})",
                Body =
                    "Автоматический отчёт сформирован.\n\n" +
                    "CSV (скопируй и сохрани как report.csv):\n\n" +
                    csv
            };

            // 3) Отправляем в OtpravkaApi
            var client = _httpClientFactory.CreateClient("OtpravkaApi");

            // если BaseUrl не задан — будет ошибка, поэтому проверим
            if (client.BaseAddress == null)
                return StatusCode(500, new { message = "OtpravkaApi:BaseUrl is not configured in appsettings.json" });

            var json = JsonSerializer.Serialize(req);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync("api/EmailReport/send", content);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode, new { message = "OtpravkaApi error", details = body });

            return Ok(new { message = "Отправлено", email, note = "OtpravkaApi не поддерживает вложения, поэтому CSV отправлен текстом." });
        }

        private static string BuildCsv(ReportModel m)
        {
            // CSV с разделителем ; (удобно для RU Excel)
            // AverageGrade заменим точку/запятую
            var avg = m.AverageGrade.ToString().Replace(",", "."); // чтобы не ломалось

            var sb = new StringBuilder();
            sb.AppendLine("StudentId;StudentName;TaskCount;AverageGrade");
            sb.AppendLine($"{m.StudentId};{Escape(m.StudentName)};{m.TaskCount};{avg}");
            return sb.ToString();
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            // если есть ; или кавычки — экранируем
            if (s.Contains(';') || s.Contains('"') || s.Contains('\n'))
            {
                s = s.Replace("\"", "\"\"");
                return $"\"{s}\"";
            }
            return s;
        }
    }
}
