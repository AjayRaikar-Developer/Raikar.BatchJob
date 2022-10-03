using Raikar.BatchJob.Models;

namespace Raikar.BatchJob.Test
{
    public class BatchJobTest
    {
        public BatchJobService<int> _batchJobService;
        public CancellationTokenSource _cancelToken = new CancellationTokenSource();

        public BatchJobTest()
        {
            BatchTxnProcess<int> process = new BatchTxnProcess<int>(TxnProcess);
            GetBatchKeyList<int> getKeyList = new GetBatchKeyList<int>(GetNumbers);


            BatchJobOptions options = new BatchJobOptions();
            options.BatchProcessMode = BatchProcessMode.ParallelForEach;
            options.GenerateBatchReport = true;
            options.CircuitBreakerLimit = 10;

            //Synchronous
            _batchJobService = new BatchJobService<int>(getKeyList, process, options);

            var token = _cancelToken.Token;

            //Asynchronous
            //BatchAsyncTxnProcess<int> asyncProcess = new BatchAsyncTxnProcess<int>(AsyncTxnProcess);
            //_batchJobService = new BatchJobService<int>(getKeyList, asyncProcess, token, options);
        }

        public BatchResponseDto<int> Test()
        {
            return _batchJobService.ExecuteBatchJob();
        }

        public async Task<BatchResponseDto<int>> Test2()
        {
            return await _batchJobService.ExecuteBatchJobAsync();
        }

        public List<int> GetNumbers()
        {
            var limit = 150;
            return Enumerable.Range(1, limit).ToList();
        }


        public TxnResponse TxnProcess(int Key)
        {
            TxnResponse response = new TxnResponse();
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

        public async Task<TxnResponse> AsyncTxnProcess(int Key)
        {
            TxnResponse response = new TxnResponse();
            response.TxnStatus = true;

            int evencheck = Key % 2;

            await Task.Delay(1);

            if (evencheck != 0)
            {
                response.TxnStatus = false;
                response.TxnDescription = "Even or Odd Check";
                response.TxnErrorDescription = "It's an Odd Number";
            }

            //Task Cancellation Testing
            //if(Key == 49)
            //{
            //    _cancelToken.Cancel();
            //}

            return response;
        }

    }
}
