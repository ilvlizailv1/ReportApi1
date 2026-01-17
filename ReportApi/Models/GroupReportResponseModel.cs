using System;
using System.Collections.Generic;

namespace ReportApi.Models
{
    public class GroupReportResponseModel
    {
        public string GroupName { get; set; } = "";
        public int StudentCount { get; set; }
        public double GroupAverageGrade { get; set; }
        public int TotalTaskCount { get; set; }

        public List<StudentReportItemResponse> Students { get; set; } = new();

        public DateTime GeneratedAt { get; set; }
    }
}
