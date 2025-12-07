namespace ReportApi.Models
{
    public class ReportModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TaskCount { get; set; }
        public double AverageGrade { get; set; }
    }
}
