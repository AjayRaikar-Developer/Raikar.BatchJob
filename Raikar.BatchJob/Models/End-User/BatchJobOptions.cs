using System.ComponentModel.DataAnnotations;

namespace Raikar.BatchJob.Models
{
    public class BatchJobOptions
    {
        public BatchJobOptions()
        {
            BatchName = "Batch Job Service";
            BatchProcessMode = BatchProcessMode.Foreach;
            GenerateBatchReport = false;
            CircuitBreakerLimit = 100;
        }

        /// <summary>
        /// Batch Name
        /// </summary>
        public string BatchName { get; set; }

        /// <summary>
        /// Process Mode - Foreach & ForEachParallel. 
        /// Use the enum BatchProcessMode to set this value
        /// Ignore this field for async batch
        /// </summary>
        public BatchProcessMode BatchProcessMode { get; set; }

        /// <summary>
        /// Enabling this will return HTML Batch Report
        /// </summary>
        public bool GenerateBatchReport { get; set; }
        
        /// <summary>
        /// It will break the batch if the mentioned limit is hit.
        /// Default Limit = 100
        /// </summary>
        public int CircuitBreakerLimit { get; set; }
    }
}
