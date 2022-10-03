using System.ComponentModel.DataAnnotations;

namespace Raikar.BatchJob.Models
{
    public class TxnResponse
    {
        /// <summary>
        /// Transaction details which is being performed 
        /// </summary>
        public string?  TxnDescription { get; set; }

        /// <summary>
        /// Transaction error details if anythings occurs
        /// </summary>
        public string? TxnErrorDescription { get; set; }

        /// <summary>
        /// Transaction status was it success or fail
        /// </summary>
        [Required]
        public bool TxnStatus {get;set;}
    }
}
