using Newtonsoft.Json;
using Raikar.BatchJob.Models;

namespace Raikar.BatchJob.Test
{
    public class BatchJobTest
    {
        public GetBatchKeyList<int> _getkeyList;
        public List<int> _keyList;
        public BatchTxnProcess<int> _process;
        public BatchAsyncTxnProcess<int> _asyncProcess;
        public BatchJobService<int> _batchJobService;
        SubscribeBatchEventsFunc<int> _subscribeBatchEvent;
        public CancellationTokenSource _cancelToken = new CancellationTokenSource();

        public BatchJobTest()
        {
            _process = new BatchTxnProcess<int>(TxnProcess);
            _asyncProcess = new BatchAsyncTxnProcess<int>(AsyncTxnProcess);
            _getkeyList = new GetBatchKeyList<int>(GetNumbers);
            _keyList = GetNumbers();
            _subscribeBatchEvent = new SubscribeBatchEventsFunc<int>(BatchSubscriber);
        }

        #region All Constructor Functions 
        public BatchResponseDto<int> SyncForEachBatch()
        {            
            _batchJobService = new BatchJobService<int>(_keyList, _process, BatchProcessMode.Foreach);
            return _batchJobService.ExecuteBatchJob();
        }

        public BatchResponseDto<int> SyncForEachParallelBatch()
        {
            _batchJobService = new BatchJobService<int>(_keyList, _process, BatchProcessMode.ParallelForEach);
            return _batchJobService.ExecuteBatchJob();
        }

        public async Task<BatchResponseDto<int>> ForEachParallelAsyncBatch()
        {
            var token = _cancelToken.Token;          
            _batchJobService = new BatchJobService<int>(_keyList, _asyncProcess, token);
            
            return await _batchJobService.ExecuteBatchJobAsync();
        }

        public BatchResponseDto<int> SyncForEachBatch_WithOptions()
        {
            BatchJobOptions options = new BatchJobOptions()
            {
                BatchName = "SyncForEachBatch_WithOptions",
                BatchProcessMode = BatchProcessMode.Foreach,
                GenerateBatchReport = true,
                CircuitBreakerLimit = 100
            };

            _batchJobService = new BatchJobService<int>(_keyList, _process, options);
            return _batchJobService.ExecuteBatchJob();
        }

        public BatchResponseDto<int> SyncForEachParallelBatch_WithOptions()
        {
            BatchJobOptions options = new BatchJobOptions()
            {
                BatchName = "SyncForEachParallelBatch_WithOptions",
                BatchProcessMode = BatchProcessMode.Foreach,
                GenerateBatchReport = true,
                CircuitBreakerLimit = 100
            };

            _batchJobService = new BatchJobService<int>(_keyList, _process, options);
            return _batchJobService.ExecuteBatchJob();
        }

        public async Task<BatchResponseDto<int>> ForEachParallelAsyncBatch_WithOptions()
        {
            var token = _cancelToken.Token;
            BatchJobOptions options = new BatchJobOptions()
            {
                BatchName = "ForEachParallelAsyncBatch_WithOptions",
                GenerateBatchReport = true,
                CircuitBreakerLimit = 100
            };

            _batchJobService = new BatchJobService<int>(_keyList, _asyncProcess, token, options);

            return await _batchJobService.ExecuteBatchJobAsync();
        }       

        public BatchResponseDto<int> SyncForEachBatch_WithGetKeyMethod()
        {
            BatchJobOptions options = new BatchJobOptions()
            {
                BatchName = "SyncForEachBatch_WithGetKeyMethod",
                BatchProcessMode = BatchProcessMode.Foreach,
                GenerateBatchReport = true,
                CircuitBreakerLimit = 100
            };

            _batchJobService = new BatchJobService<int>(_getkeyList, _process, options);
            return _batchJobService.ExecuteBatchJob();
        }

        public BatchResponseDto<int> SyncForEachParallelBatch_WithGetKeyMethod()
        {
            BatchJobOptions options = new BatchJobOptions()
            {
                BatchName = "SyncForEachParallelBatch_WithGetKeyMethod",
                BatchProcessMode = BatchProcessMode.Foreach,
                GenerateBatchReport = true,
                CircuitBreakerLimit = 100
            };
            _batchJobService = new BatchJobService<int>(_getkeyList, _process, options);
            return _batchJobService.ExecuteBatchJob();
        }

        public async Task<BatchResponseDto<int>> ForEachParallelAsyncBatch_WithGetKeyMethod()
        {
            var token = _cancelToken.Token;
            BatchJobOptions options = new BatchJobOptions()
            {
                BatchName = "ForEachParallelAsyncBatch_WithGetKeyMethod",
                GenerateBatchReport = true,
                CircuitBreakerLimit = 100
            };

            _batchJobService = new BatchJobService<int>(_getkeyList, _asyncProcess, token, options);

            return await _batchJobService.ExecuteBatchJobAsync();
        }

        public BatchResponseDto<int> SyncForEachBatch_WithSubscriberMethod()
        {
            BatchJobOptions options = new BatchJobOptions()
            {
                BatchName = "SyncForEachBatch_WithSubscriberMethod",
                BatchProcessMode = BatchProcessMode.Foreach,
                GenerateBatchReport = true,
                CircuitBreakerLimit = 100
            };

            _batchJobService = new BatchJobService<int>(_getkeyList, _process, _subscribeBatchEvent, options);
            
            return _batchJobService.ExecuteBatchJob();
        }

        public BatchResponseDto<int> SyncForEachParallelBatch_WithSubscriberMethod()
        {
            BatchJobOptions options = new BatchJobOptions()
            {
                BatchName = "SyncForEachParallelBatch_WithSubscriberMethod",
                BatchProcessMode = BatchProcessMode.Foreach,
                GenerateBatchReport = true,
                CircuitBreakerLimit = 100
            };

            _batchJobService = new BatchJobService<int>(_getkeyList, _process, _subscribeBatchEvent, options);

            return _batchJobService.ExecuteBatchJob();
        }

        public async Task<BatchResponseDto<int>> ForEachParallelAsyncBatch_WithSubscriberMethod()
        {
            var token = _cancelToken.Token;
            
            BatchJobOptions options = new BatchJobOptions()
            {
                BatchName = "ForEachParallelAsyncBatch_WithSubscriberMethod",
                GenerateBatchReport = true,
                CircuitBreakerLimit = 100
            };
 
            _batchJobService = new BatchJobService<int>(_getkeyList, _asyncProcess, _subscribeBatchEvent, token, options);


            return await _batchJobService.ExecuteBatchJobAsync();
        }

        #endregion

        #region Use Cases

        public BatchResponseDto<int> LoaderTest()
        {
            _process = new BatchTxnProcess<int>(TxnProcessLoader);
            _batchJobService = new BatchJobService<int>(_keyList, _process, BatchProcessMode.Foreach);
            return _batchJobService.ExecuteBatchJob();
        }

        public BatchResponseDto<int> CircuitBreakerTest()
        {
            BatchJobOptions options = new BatchJobOptions()
            {
                BatchName = "CircuitBreakerTest",
                BatchProcessMode = BatchProcessMode.Foreach,
                GenerateBatchReport = true,
                CircuitBreakerLimit = 10
            };

            _batchJobService = new BatchJobService<int>(_keyList, _process, options);
            return _batchJobService.ExecuteBatchJob();
        }

        public async Task<BatchResponseDto<int>> ForEachParallelAsyncBatch_TaskCancel()
        {
            try
            {
                var token = _cancelToken.Token;
                BatchJobOptions options = new BatchJobOptions()
                {
                    BatchName = "ForEachParallelAsyncBatch_WithOptions_TaskCancel",
                    GenerateBatchReport = true,
                    CircuitBreakerLimit = 100
                };

                BatchAsyncTxnProcess<int> asyncProcess = new BatchAsyncTxnProcess<int>(AsyncTxnCancelProcess);

                _batchJobService = new BatchJobService<int>(_keyList, asyncProcess, token, options);

                return await _batchJobService.ExecuteBatchJobAsync();
            }
            catch
            {
                return new BatchResponseDto<int>() { BatchReportHtml = "<h1>Batch was cancelled</h1>"};
            }
        }
        #endregion

        public List<int> GetNumbers()
        {
            var limit = 50;
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

        public TxnResponse TxnProcessLoader(int Key)
        {
            TxnResponse response = new TxnResponse();
            response.TxnStatus = true;

            int evencheck = Key % 2;

            Thread.Sleep(1);

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

            return response;
        }

        public async Task<TxnResponse> AsyncTxnCancelProcess(int Key)
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
            if(Key == 10)
            {
                _cancelToken.Cancel();
            }

            return response;
        }


        public void BatchSubscriber(BatchTxnEvent<int> batchEvent)
        {
            Console.WriteLine(JsonConvert.SerializeObject(batchEvent));
        }

    }
}
