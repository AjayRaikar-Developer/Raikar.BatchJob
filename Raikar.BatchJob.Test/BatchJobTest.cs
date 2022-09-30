using Raikar.BatchJob.Models;
using System.Diagnostics;

namespace Raikar.BatchJob.Test
{
    public class BatchJobTest
    {
        public BatchJobService<int> _batchJobService;

        public BatchJobTest()
        {
            var limit = 50;
            var numbers = Enumerable.Range(1, limit).ToList();
            BatchTxnProcess<int> process = new BatchTxnProcess<int>(TxnProcess);
            GetBatchKeyList<int> getKeyList = new GetBatchKeyList<int>(GetNumbers);
            //_batchJobService = new BatchJobService<int>(numbers,process, Models.BatchProcessMode.Single);
            _batchJobService = new BatchJobService<int>(getKeyList, process, Models.BatchProcessMode.Single);
        }

        public dynamic Test()
        {
           return  _batchJobService.ExecuteBatchJob();
        }

        public List<int> GetNumbers()
        {
            var limit = 50;
            return Enumerable.Range(1, limit).ToList();
        }


        public TxnResponseDto TxnProcess(int Key)
        {
            TxnResponseDto response = new TxnResponseDto();
            response.TxnStatus = true;

            if (Key > 10)
            {
                response.TxnStatus = false;
                response.TxnErrorDescription = "Key greater than 10";
            }

            return response;
        }

        static int tableWidth = 73;
        public static void PrintLine()
        {
            Console.WriteLine(new string('-', tableWidth));
        }

        public static void PrintRow(params string[] columns)
        {
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns)
            {
                row += AlignCentre(column, width) + "|";
            }

            Console.WriteLine(row);
        }

        public static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        }
    }
}
