namespace Raikar.BatchJob.Models
{
    public class BatchTxnEvent<T>
    {
        public BatchTxnEvent()
        {
            ErrorDetails = new List<BatchErrorDetailsDto<T>>();
        }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public List<BatchErrorDetailsDto<T>> ErrorDetails { get; set; }
    }
}
