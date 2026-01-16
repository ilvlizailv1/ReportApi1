using Microsoft.AspNetCore.Mvc;
using ReportApi.Models;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

using DocumentFormat.OpenXml.Packaging;

// Алиасы, чтобы не было конфликта "Document" (QuestPDF vs OpenXml)
using WordDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using WordBody = DocumentFormat.OpenXml.Wordprocessing.Body;
using WordParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WordRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WordText = DocumentFormat.OpenXml.Wordprocessing.Text;
using WordRunProperties = DocumentFormat.OpenXml.Wordprocessing.RunProperties;
using WordBold = DocumentFormat.OpenXml.Wordprocessing.Bold;
using DocumentFormat.OpenXml;
using QuestPDF.Helpers;

namespace ReportApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        // GET api/report
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Сервис ReportApi работает");
        }

        // GET api/report/sample
        [HttpGet("sample")]
        public IActionResult GetSample()
        {
            var sample = new ReportResponseModel
            {
                StudentId = 1,
                StudentName = "Пример студента",
                TaskCount = 5,
                AverageGrade = 4.2,
                Performance = "Хорошо",
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(sample);
        }

        // POST api/report
        [HttpPost]
        public IActionResult Generate([FromBody] ReportModel model)
        {
            string performance = GetPerformance(model.AverageGrade);

            var result = new ReportResponseModel
            {
                StudentId = model.StudentId,
                StudentName = model.StudentName,
                TaskCount = model.TaskCount,
                AverageGrade = model.AverageGrade,
                Performance = performance,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(result);
        }

        // POST api/report/export/pdf
        [HttpPost("export/pdf")]
        public IActionResult ExportPdf([FromBody] ReportModel model)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string performance = GetPerformance(model.AverageGrade);
            var generatedAt = DateTime.UtcNow;

            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("Отчёт по успеваемости")
                        .SemiBold()
                        .FontSize(18);

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);

                        // Всё на русском
                        col.Item().Text($"ID студента: {model.StudentId}");
                        col.Item().Text($"ФИО студента: {model.StudentName}");
                        col.Item().Text($"Количество заданий: {model.TaskCount}");
                        col.Item().Text($"Средний балл: {FormatGrade(model.AverageGrade)}");
                        col.Item().Text($"Успеваемость: {performance}");

                        col.Item().PaddingTop(10)
                            .Text("Сформировано автоматически в ReportApi.")
                            .Italic()
                            .FontSize(10);
                    });

                    page.Footer()
                        .AlignRight()
                        .Text($"Дата формирования (UTC): {generatedAt:dd.MM.yyyy HH:mm}");
                });
            }).GeneratePdf();

            return File(bytes, "application/pdf", $"report_{model.StudentId}.pdf");
        }

        // POST api/report/export/docx
        [HttpPost("export/docx")]
        public IActionResult ExportDocx([FromBody] ReportModel model)
        {
            string performance = GetPerformance(model.AverageGrade);
            var generatedAt = DateTime.UtcNow;

            using var ms = new MemoryStream();

            using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new WordDocument();
                var body = mainPart.Document.AppendChild(new WordBody());

                void AddLine(string text, bool bold = false)
                {
                    var run = new WordRun();

                    if (bold)
                        run.RunProperties = new WordRunProperties(new WordBold());

                    run.AppendChild(new WordText(text));
                    body.AppendChild(new WordParagraph(run));
                }

                // Всё на русском
                AddLine("Отчёт по успеваемости", bold: true);
                AddLine($"ID студента: {model.StudentId}");
                AddLine($"ФИО студента: {model.StudentName}");
                AddLine($"Количество заданий: {model.TaskCount}");
                AddLine($"Средний балл: {FormatGrade(model.AverageGrade)}");
                AddLine($"Успеваемость: {performance}");
                AddLine($"Дата формирования (UTC): {generatedAt:dd.MM.yyyy HH:mm}");

                mainPart.Document.Save();
            }

            var bytes = ms.ToArray();

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"report_{model.StudentId}.docx"
            );
        }

        // GET api/report/export (инфо)
        [HttpGet("export")]
        public IActionResult ExportInfo()
        {
            return Ok("Используйте POST /api/report/export/pdf или POST /api/report/export/docx для экспорта отчёта.");
        }

        private static string GetPerformance(double averageGrade)
        {
            return averageGrade switch
            {
                >= 4.5 => "Отлично",
                >= 3.5 => "Хорошо",
                _ => "Удовлетворительно"
            };
        }

        private static string FormatGrade(double grade)
        {
            // Чтобы не было "4,2" vs "4.2" путаницы — форматируем красиво
            return grade.ToString("0.0");
        }
    }
}
