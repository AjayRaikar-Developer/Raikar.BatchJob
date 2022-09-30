namespace Raikar.BatchJob.Models
{
    public class BatchModeResponseDto<T>
    {
        public BatchModeResponseDto()
        {
            ErrorDetails = new List<BatchErrorDetailsDto<T>>();
        }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public List<BatchErrorDetailsDto<T>> ErrorDetails { get; set; }
    }
}
