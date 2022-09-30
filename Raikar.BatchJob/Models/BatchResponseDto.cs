namespace Raikar.BatchJob.Models
{
    public class BatchResponseDto<T>
    {
        public BatchResponseDto()
        {
            ErrorDetails = new List<BatchErrorDetailsDto<T>>();
        }
        public string? BatchName { get; set; }
        public BatchMode? Mode { get; set; }
        public DateTime? Date { get; set; }
        public int? TotalCount { get; set; }
        public int? SuccessCount { get; set; }
        public int? FailCount { get; set; }
        public List<BatchErrorDetailsDto<T>>? ErrorDetails { get; set; }
        public string? CorrelationId { get; set; }
    }
}
