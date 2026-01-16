using Microsoft.AspNetCore.Mvc;
using ReportApi.Models;
using System.Text;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ReportApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IntegrationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static bool _questPdfConfigured;

        public IntegrationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            // ✅ Чтобы не ловить окно про лицензию QuestPDF
            if (!_questPdfConfigured)
            {
                QuestPDF.Settings.License = LicenseType.Community;
                _questPdfConfigured = true;
            }
        }

        /// <summary>
        /// Отправить PDF-отчёт на почту через OtpravkaApi
        /// POST: /api/integration/send-pdf?recipient=mail@example.com
        /// Body: ReportModel
        /// </summary>
        [HttpPost("send-pdf")]
        public async Task<IActionResult> SendPdfToEmail(
            [FromQuery] string recipient,
            [FromBody] ReportModel model)
        {
            if (string.IsNullOrWhiteSpace(recipient))
                return BadRequest(new { error = "recipient is required" });

            // 1) Считаем Performance (как в ReportController)
            var performance = model.AverageGrade switch
            {
                >= 4.5 => "Отлично",
                >= 3.5 => "Хорошо",
                _ => "Удовлетворительно"
            };

            var report = new ReportResponseModel
            {
                StudentId = model.StudentId,
                StudentName = model.StudentName,
                TaskCount = model.TaskCount,
                AverageGrade = model.AverageGrade,
                Performance = performance,
                GeneratedAt = DateTime.UtcNow
            };

            // 2) Генерим PDF
            byte[] pdfBytes;
            try
            {
                pdfBytes = GeneratePdfBytes(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "PDF generation failed", details = ex.Message });
            }

            // 3) Готовим запрос в OtpravkaApi
            var emailRequest = new SendEmailRequest
            {
                Recipient = recipient,
                Subject = $"Отчёт по успеваемости (StudentId={report.StudentId})",
                Body =
                    $"Отправляем отчёт по успеваемости.\n\n" +
                    $"StudentId: {report.StudentId}\n" +
                    $"StudentName: {report.StudentName}\n" +
                    $"TaskCount: {report.TaskCount}\n" +
                    $"AverageGrade: {report.AverageGrade}\n" +
                    $"Performance: {report.Performance}\n" +
                    $"Дата формирования (UTC): {report.GeneratedAt:dd.MM.yyyy HH:mm}\n",
                AttachmentBase64 = Convert.ToBase64String(pdfBytes),
                AttachmentFileName = $"report_{report.StudentId}.pdf",
                AttachmentContentType = "application/pdf"
            };

            var client = _httpClientFactory.CreateClient("OtpravkaApi");

            var json = JsonSerializer.Serialize(emailRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

          
            var resp = await client.PostAsync("/api/EmailReport/send", content);
            var respBody = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                return StatusCode((int)resp.StatusCode, new
                {
                    error = "OtpravkaApi failed",
                    status = (int)resp.StatusCode,
                    details = respBody
                });
            }

            // успех
            return Ok(new
            {
                message = "Email sent",
                recipient,
                file = emailRequest.AttachmentFileName,
                otpravkaApiResponse = respBody
            });
        }

        private static byte[] GeneratePdfBytes(ReportResponseModel report)
        {
            using var stream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text("Отчёт по успеваемости")
                            .FontSize(20)
                            .SemiBold();

                        col.Item().Text($"StudentId: {report.StudentId}");
                        col.Item().Text($"StudentName: {report.StudentName}");
                        col.Item().Text($"TaskCount: {report.TaskCount}");
                        col.Item().Text($"AverageGrade: {report.AverageGrade}");
                        col.Item().Text($"Performance: {report.Performance}");
                        col.Item().Text($"Дата формирования (UTC): {report.GeneratedAt:dd.MM.yyyy HH:mm}");
                    });
                });
            })
            .GeneratePdf(stream);

            return stream.ToArray();
        }
    }
}