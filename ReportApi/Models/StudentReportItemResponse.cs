namespace ReportApi.Models
{
    public class StudentReportItemResponse
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public int TaskCount { get; set; }
        public double AverageGrade { get; set; }
        public string Performance { get; set; } = "";
    }
}
