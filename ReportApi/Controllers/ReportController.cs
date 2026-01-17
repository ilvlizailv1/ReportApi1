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

 
        [HttpGet("group/sample")]
        public IActionResult GetGroupSample()
        {
            var sample = new GroupReportRequest
            {
                GroupName = "ВПК7-01",
                Students = new List<StudentInputModel>
                {
                    new StudentInputModel { StudentId = 1, StudentName = "Иванов Иван", TaskCount = 5, AverageGrade = 4.6 },
                    new StudentInputModel { StudentId = 2, StudentName = "Петров Петр", TaskCount = 4, AverageGrade = 3.9 },
                    new StudentInputModel { StudentId = 3, StudentName = "Сидорова Анна", TaskCount = 6, AverageGrade = 4.2 }
                }
            };

            return Ok(sample);
        }

        [HttpPost("group")]
        public IActionResult GenerateGroup([FromBody] GroupReportRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Request is null" });

            if (string.IsNullOrWhiteSpace(request.GroupName))
                return BadRequest(new { message = "GroupName is required" });

            if (request.Students == null || request.Students.Count == 0)
                return BadRequest(new { message = "Students list is required" });

            // Детализация по каждому студенту
            var students = request.Students.Select(s => new StudentReportItemResponse
            {
                StudentId = s.StudentId,
                StudentName = s.StudentName,
                TaskCount = s.TaskCount,
                AverageGrade = s.AverageGrade,
                Performance = GetPerformance(s.AverageGrade)
            }).ToList();

            // Сводка по группе
            var response = new GroupReportResponseModel
            {
                GroupName = request.GroupName,
                StudentCount = students.Count,
                GroupAverageGrade = Math.Round(students.Average(x => x.AverageGrade), 2),
                TotalTaskCount = students.Sum(x => x.TaskCount),
                Students = students,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(response);
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
    }
}
