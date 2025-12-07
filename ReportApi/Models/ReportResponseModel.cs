public class ReportResponseModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int TaskCount { get; set; }
    public double AverageGrade { get; set; }
    public string Performance { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
