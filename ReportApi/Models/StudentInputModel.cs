namespace ReportApi.Models
{
    public class StudentInputModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public int TaskCount { get; set; }
        public double AverageGrade { get; set; }
    }
}
