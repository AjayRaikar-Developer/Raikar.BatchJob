using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raikar.BatchJob.Models
{
    public class BatchReportResponseDto
    {
        public bool Status { get; set; }
        public string? HtmlReport { get; set; }
    }
}
