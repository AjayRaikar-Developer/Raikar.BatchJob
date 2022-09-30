namespace Raikar.BatchJob.Models
{
    public class BatchErrorDetailsDto<T>
    {
        public T? TxnKey { get; set; }
        public string? TxnDescription { get; set; }
        public string? TxnErrorDescription { get; set; }
    }
}
