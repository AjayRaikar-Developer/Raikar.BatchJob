namespace Raikar.BatchJob.Models
{
    public class BatchResponse<T>
    {
        public BatchResponse()
        {
            ErrorDetails = new List<BatchErrorDetailsDto<T>>();
        }
        public int? TotalCount { get; set; }
        public int? SuccessCount { get; set; }
        public int? FailCount { get; set; }
        public List<BatchErrorDetailsDto<T>> ErrorDetails { get; set; }
        public string? BatchReportHtml { get; set; }
    }
}
