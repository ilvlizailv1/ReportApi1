using Microsoft.AspNetCore.Mvc;
using ReportApi.Models;
using System.IO;

// Алиасы, чтобы НЕ было конфликта Document
using PdfDoc = QuestPDF.Fluent.Document;
using WordprocessingDocument = DocumentFormat.OpenXml.Packaging.WordprocessingDocument;
using DocumentFormat.OpenXml;
using W = DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;

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
            string performance = model.AverageGrade switch
            {
                >= 4.5 => "Отлично",
                >= 3.5 => "Хорошо",
                _ => "Удовлетворительно"
            };

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

        // GET api/report/export
        [HttpGet("export")]
        public IActionResult Export()
        {
            return Ok("Используйте POST /api/report/export/pdf или POST /api/report/export/docx для экспорта отчёта.");
        }

        // POST api/report/export/pdf
        [HttpPost("export/pdf")]
        public IActionResult ExportPdf([FromBody] ReportModel model)
        {
            var report = BuildReport(model);

            using var stream = new MemoryStream();

            
            PdfDoc.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Отчёт по успеваемости").FontSize(18).SemiBold();

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Text($"StudentId: {report.StudentId}");
                        col.Item().Text($"StudentName: {report.StudentName}");
                        col.Item().Text($"TaskCount: {report.TaskCount}");
                        col.Item().Text($"AverageGrade: {report.AverageGrade}");
                        col.Item().Text($"Performance: {report.Performance}");
                        col.Item().Text($"Дата формирования (UTC): {report.GeneratedAt:dd.MM.yyyy HH:mm}");
                    });
                });
            }).GeneratePdf(stream);

            stream.Position = 0;
            return File(stream.ToArray(), "application/pdf", $"report_{report.StudentId}.pdf");
        }

        // POST api/report/export/docx
        [HttpPost("export/docx")]
        public IActionResult ExportDocx([FromBody] ReportModel model)
        {
            var report = BuildReport(model);

            using var ms = new MemoryStream();

            using (var wordDoc = WordprocessingDocument.Create(ms, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new W.Document();
                var body = mainPart.Document.AppendChild(new W.Body());

                body.AppendChild(new W.Paragraph(new W.Run(new W.Text("Отчёт по успеваемости"))));
                body.AppendChild(new W.Paragraph(new W.Run(new W.Text($"StudentId: {report.StudentId}"))));
                body.AppendChild(new W.Paragraph(new W.Run(new W.Text($"StudentName: {report.StudentName}"))));
                body.AppendChild(new W.Paragraph(new W.Run(new W.Text($"TaskCount: {report.TaskCount}"))));
                body.AppendChild(new W.Paragraph(new W.Run(new W.Text($"AverageGrade: {report.AverageGrade}"))));
                body.AppendChild(new W.Paragraph(new W.Run(new W.Text($"Performance: {report.Performance}"))));
                body.AppendChild(new W.Paragraph(new W.Run(new W.Text($"Дата формирования (UTC): {report.GeneratedAt:dd.MM.yyyy HH:mm}"))));

                mainPart.Document.Save();
            }

            var bytes = ms.ToArray();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"report_{report.StudentId}.docx");
        }

        private ReportResponseModel BuildReport(ReportModel model)
        {
            string performance = model.AverageGrade switch
            {
                >= 4.5 => "Отлично",
                >= 3.5 => "Хорошо",
                _ => "Удовлетворительно"
            };

            return new ReportResponseModel
            {
                StudentId = model.StudentId,
                StudentName = model.StudentName,
                TaskCount = model.TaskCount,
                AverageGrade = model.AverageGrade,
                Performance = performance,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}
