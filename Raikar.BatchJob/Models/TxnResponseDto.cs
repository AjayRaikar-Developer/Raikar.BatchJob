using System.ComponentModel.DataAnnotations;

namespace Raikar.BatchJob.Models
{
    public class TxnResponseDto
    {
        public string?  TxnDescription { get; set; }

        public string? TxnErrorDescription { get; set; }

        [Required]
        public bool TxnStatus {get;set;}
    }
}
