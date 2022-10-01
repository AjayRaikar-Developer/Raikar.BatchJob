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
            _batchJobService = new BatchJobService<int>(getKeyList, process, Models.BatchProcessMode.Single,true);
        }

        public BatchResponseDto<int> Test()
        {
            return _batchJobService.ExecuteBatchJob();
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

            int evencheck = Key % 2;

            if (evencheck != 0)
            {
                response.TxnStatus = false;
                response.TxnDescription = "Even or Odd Check";
                response.TxnErrorDescription = "It's an Odd Number";
            }

            return response;
        }
    }
}
