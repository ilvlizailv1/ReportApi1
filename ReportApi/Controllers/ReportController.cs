using Microsoft.AspNetCore.Mvc;
using ReportApi.Models;

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
            // Определяем словесную оценку успеваемости
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
            return Ok("Функция экспорта отчёта в PDF/Word будет реализована в следующих лабораторных работах.");
        }
    }
}
