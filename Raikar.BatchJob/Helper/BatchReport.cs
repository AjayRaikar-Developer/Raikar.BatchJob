using Raikar.BatchJob.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raikar.BatchJob.Helper
{
    internal static class BatchReport<KeyDataType>
    {
        public static BatchReportResponseDto Generate(BatchResponse<KeyDataType> batchResponse)
        {
            BatchReportResponseDto response = new BatchReportResponseDto();
            try
            {
                string body = string.Empty;
                string batchMessage = "Batch successfully executed";
                var reportTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "Helper\\BatchTemplate.html");

                using (StreamReader reader = new StreamReader(reportTemplatePath))
                {
                    body = reader.ReadToEnd();
                }


                body = body.Replace("{TotalCount}", batchResponse.TotalCount.ToString());
                body = body.Replace("{SuccessCount}", batchResponse.SuccessCount.ToString());
                body = body.Replace("{FailCount}", batchResponse.FailCount.ToString());

                if(batchResponse.FailCount > 0)
                {
                    batchMessage = "Batch has few failed transactions please re-try";
                }

                body = body.Replace("{BatchMessage}", batchMessage);


                string errorDetails = "";

                foreach (var item in batchResponse.ErrorDetails)
                {
                    errorDetails += $"<tr><td>" + item.TxnKey + "</td><td>" + item.TxnDescription + "</td><td>" + item.TxnErrorDescription + "</td></tr>";
                }

                var failedKeyList = batchResponse.ErrorDetails.Select(x => x.TxnKey).ToList();
                string strlist = "[" + string.Join(",", failedKeyList) + "]";

                body = body.Replace("{FailedKeysList}", strlist);
                body = body.Replace("{ErrorDetails}", errorDetails);

                response.Status = true;
                response.HtmlReport = body;
            }
            catch
            {
                response.Status = false;
            }

            return response;
        }
    }
}
