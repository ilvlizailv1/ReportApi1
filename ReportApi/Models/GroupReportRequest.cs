using System.Collections.Generic;

namespace ReportApi.Models
{
    public class GroupReportRequest
    {
        // Способ указания группы 
        public string GroupName { get; set; } = "";

        // Список студентов группы
        public List<StudentInputModel> Students { get; set; } = new();
    }
}
