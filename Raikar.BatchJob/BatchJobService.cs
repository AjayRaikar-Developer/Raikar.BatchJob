using Raikar.BatchJob.Helper;
using Raikar.BatchJob.Models;
using ShellProgressBar;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Raikar.BatchJob
{
    /// <summary>
    /// Gets the list of keys for the batch job processing against the mapped method
    /// </summary>
    /// <typeparam name="KeyDataType"></typeparam>
    /// <returns></returns>
    public delegate List<KeyDataType> GetBatchKeyList<KeyDataType>();
    
    /// <summary>
    /// It calls the batch job transaction method which manages key wise processing
    /// </summary>
    /// <typeparam name="KeyDataType"></typeparam>
    /// <param name="Key"></param>
    /// <returns></returns>
    public delegate TxnResponse BatchTxnProcess<KeyDataType>(KeyDataType Key);

    /// <summary>
    /// It calls the batch job transaction async method which manages key wise processing
    /// </summary>
    /// <typeparam name="KeyDataType"></typeparam>
    /// <param name="Key"></param>
    /// <returns></returns>
    public delegate Task<TxnResponse> BatchAsyncTxnProcess<KeyDataType>(KeyDataType Key);
    
    /// <summary>
    /// Generic Batch Job Main Methods
    /// </summary>
    /// <typeparam name="KeyDataType"></typeparam>
    /// <param name="batchKeyList"></param>
    /// <returns></returns>
    public delegate BatchModeResponseDto<KeyDataType> BatchProcessFunc<KeyDataType>(List<KeyDataType> batchKeyList);

    /// <summary>
    /// Async Batch Job Main Method with Cancellation Token 
    /// </summary>
    /// <typeparam name="KeyDataType"></typeparam>
    /// <param name="batchKeyList"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public delegate Task<BatchModeResponseDto<KeyDataType>> BatchProcessAsyncFunc<KeyDataType>(List<KeyDataType> batchKeyList, CancellationToken cancellationToken);
    
    
    public delegate void SubscribeBatchEventsFunc<KeyDataType>(BatchTxnEvent<KeyDataType> batchTxnEvents);
    
    
    public class BatchJobService<KeyDataType>
    {
        #region Private Properties
        private BatchTxnProcess<KeyDataType>? _methodToProcess;
        private BatchAsyncTxnProcess<KeyDataType>? _asyncMethodToProcess;
        private CancellationToken _cancellationToken;
        private BatchProcessMode _batchProcessMode;
        private List<KeyDataType> _batchKeyList = new List<KeyDataType>();
        private int _batchKeyListCount;
        private GetBatchKeyList<KeyDataType>? _getBatchKeyList;
        private BatchModeResponseDto<KeyDataType> _response = new BatchModeResponseDto<KeyDataType>();
        private bool _generateBatchReport;
        private int _circuitBreakerLimit;
        private bool _circuitBreakerLimitHit = false;
        private bool _eventSubscribed = false;
        private SubscribeBatchEventsFunc<KeyDataType>? _subscribeBatchEvents = null;

        /// <summary>
        /// Progress bar options
        /// </summary>
        private static ProgressBarOptions options = new ProgressBarOptions
        {
            ForegroundColor = ConsoleColor.Yellow,
            ForegroundColorDone = ConsoleColor.DarkGreen,
            BackgroundColor = ConsoleColor.DarkGray,
            BackgroundCharacter = '\u2593'
        };

        /// <summary>
        /// Progress bar configuration 
        /// </summary>
        private ProgressBar _progressBar;
        #endregion

        #region Constructors
        public BatchJobService(List<KeyDataType> keyList, BatchTxnProcess<KeyDataType> methodToProcess, BatchProcessMode batchProcessMode)
        {
            _batchKeyList = keyList;
            BatchJobValidate();
            _methodToProcess = methodToProcess;
            _batchProcessMode = batchProcessMode;
            _progressBar = new ProgressBar(_batchKeyListCount, "Batch Job Service", options);
        }

        /// <summary>
        /// Batch job service with Options
        /// </summary>
        /// <param name="keyList">Pass the list of keys to process</param>
        /// <param name="methodToProcess">Its a delegate pass the transaction method to be called for processing all keys</param>
        /// <param name="batchOptions"></param>
        public BatchJobService(List<KeyDataType> keyList, BatchTxnProcess<KeyDataType> methodToProcess, BatchJobOptions batchJobOptions)
        {
            _batchKeyList = keyList;
             BatchJobValidate();
            _methodToProcess = methodToProcess;
            _batchProcessMode = batchJobOptions.BatchProcessMode;
            _generateBatchReport = batchJobOptions.GenerateBatchReport;
            _circuitBreakerLimit = batchJobOptions.CircuitBreakerLimit;
            _progressBar = new ProgressBar(_batchKeyListCount, batchJobOptions.BatchName, options);
        }        

        public BatchJobService(List<KeyDataType> keyList, BatchAsyncTxnProcess<KeyDataType> asyncMethodToProcess, 
            CancellationToken cancellationToken, BatchJobOptions batchJobOptions)
        {
            _batchKeyList = keyList;
            BatchJobValidate();
            _asyncMethodToProcess = asyncMethodToProcess;
            _cancellationToken = cancellationToken;
            _generateBatchReport = batchJobOptions.GenerateBatchReport;
            _circuitBreakerLimit = batchJobOptions.CircuitBreakerLimit;
            _progressBar = new ProgressBar(_batchKeyListCount, batchJobOptions.BatchName, options);
        }

        public BatchJobService(GetBatchKeyList<KeyDataType> getBatchList,BatchTxnProcess<KeyDataType> methodToProcess, 
            BatchJobOptions batchJobOptions)
        {
            _getBatchKeyList = getBatchList;
            _batchKeyList = _getBatchKeyList();
             BatchJobValidate();
            _methodToProcess = methodToProcess;
            _batchProcessMode = batchJobOptions.BatchProcessMode;
            _generateBatchReport = batchJobOptions.GenerateBatchReport;
            _circuitBreakerLimit = batchJobOptions.CircuitBreakerLimit;
            _progressBar = new ProgressBar(_batchKeyListCount, batchJobOptions.BatchName, options);
        }

        public BatchJobService(GetBatchKeyList<KeyDataType> getBatchList,BatchAsyncTxnProcess<KeyDataType> asyncMethodToProcess, 
            CancellationToken cancellationToken, BatchJobOptions batchJobOptions)
        {
            _getBatchKeyList = getBatchList;
            _batchKeyList = _getBatchKeyList();
             BatchJobValidate();
            _asyncMethodToProcess = asyncMethodToProcess;
            _cancellationToken = cancellationToken;
            _generateBatchReport = batchJobOptions.GenerateBatchReport;
            _circuitBreakerLimit = batchJobOptions.CircuitBreakerLimit;
            _progressBar = new ProgressBar(_batchKeyListCount, batchJobOptions.BatchName, options);
        }

        public BatchJobService(GetBatchKeyList<KeyDataType> getBatchList, BatchTxnProcess<KeyDataType> methodToProcess, 
            SubscribeBatchEventsFunc<KeyDataType> subscribeEvents, BatchJobOptions batchJobOptions)
        {
            _getBatchKeyList = getBatchList;
            _batchKeyList = _getBatchKeyList();
            BatchJobValidate();
            _methodToProcess = methodToProcess;
            _batchProcessMode = batchJobOptions.BatchProcessMode;
            _generateBatchReport = batchJobOptions.GenerateBatchReport;
            _circuitBreakerLimit = batchJobOptions.CircuitBreakerLimit;
            _eventSubscribed = true;
            _subscribeBatchEvents = subscribeEvents;
            _progressBar = new ProgressBar(_batchKeyListCount, batchJobOptions.BatchName, options);
        }

        public BatchJobService(GetBatchKeyList<KeyDataType> getBatchList, BatchAsyncTxnProcess<KeyDataType> asyncMethodToProcess, 
            SubscribeBatchEventsFunc<KeyDataType> subscribeEvents, CancellationToken cancellationToken, BatchJobOptions batchJobOptions)
        {
            _getBatchKeyList = getBatchList;
            _batchKeyList = _getBatchKeyList();
            BatchJobValidate();
            _asyncMethodToProcess = asyncMethodToProcess;
            _cancellationToken = cancellationToken;
            _generateBatchReport = batchJobOptions.GenerateBatchReport;
            _circuitBreakerLimit = batchJobOptions.CircuitBreakerLimit;
            _eventSubscribed = true;
            _subscribeBatchEvents = subscribeEvents;
            _progressBar = new ProgressBar(_batchKeyListCount, batchJobOptions.BatchName, options);
        }

        #endregion

        /// <summary>
        /// Synchoronous Batch Job Execution Method
        /// </summary>
        /// <param name="batchRequest"></param>
        /// <returns></returns>
        public BatchResponseDto<KeyDataType> ExecuteBatchJob()
        {
            BatchResponseDto<KeyDataType> response = new BatchResponseDto<KeyDataType>();
            BatchProcessFunc<KeyDataType> batchProcessFunc;
            BatchModeResponseDto<KeyDataType> batchResult = new BatchModeResponseDto<KeyDataType>();
            response.TotalCount = _batchKeyListCount;

            BatchJobValidate();

            switch (_batchProcessMode)
            {
                case BatchProcessMode.Foreach:
                    batchProcessFunc = new BatchProcessFunc<KeyDataType>(ForEach);
                    batchResult = batchProcessFunc(_batchKeyList);
                    break;

                case BatchProcessMode.ParallelForEach:
                    batchProcessFunc = new BatchProcessFunc<KeyDataType>(ParallelForEach);
                    batchResult = batchProcessFunc(_batchKeyList);
                    break;

                default:
                    batchProcessFunc = new BatchProcessFunc<KeyDataType>(ForEach);
                    batchResult = batchProcessFunc(_batchKeyList);
                    break;
            }

            response.SuccessCount = batchResult.SuccessCount;
            response.FailCount = batchResult.FailCount;
            response.ErrorDetails = batchResult.ErrorDetails;

            if (_generateBatchReport)
            {
                var report = BatchReport<KeyDataType>.Generate(response);
                if (report.Status)
                {
                    response.BatchReportHtml = report.HtmlReport;
                }
            }

            return response;
        }

        /// <summary>
        /// Async Batch Job Execution Method
        /// </summary>
        /// <param name="batchRequest"></param>
        /// <returns></returns>
        public async Task<BatchResponseDto<KeyDataType>> ExecuteBatchJobAsync()
        {
            BatchResponseDto<KeyDataType> response = new BatchResponseDto<KeyDataType>();
            BatchProcessAsyncFunc<KeyDataType> batchProcessAsyncFunc = new BatchProcessAsyncFunc<KeyDataType>(ParallelForEachAsync);
            BatchModeResponseDto<KeyDataType> batchResult = new BatchModeResponseDto<KeyDataType>();
            response.TotalCount = _batchKeyListCount;

            BatchJobValidate();

            batchResult = await batchProcessAsyncFunc(_batchKeyList, _cancellationToken);

            response.SuccessCount = batchResult.SuccessCount;
            response.FailCount = batchResult.FailCount;
            response.ErrorDetails = batchResult.ErrorDetails;

            if (_generateBatchReport)
            {
                var report = BatchReport<KeyDataType>.Generate(response);
                if (report.Status)
                {
                    response.BatchReportHtml = report.HtmlReport;
                }
            }

            return response;
        }

        #region Private Methods
        private BatchModeResponseDto<KeyDataType> ForEach(List<KeyDataType> batchKeyList)
        {
            foreach (var key in batchKeyList)
            {
                TxnResponse txnResponse = new TxnResponse();

                //Loader testing
                Thread.Sleep(1);
                try
                {
                    if (_methodToProcess != null)
                        txnResponse = _methodToProcess(key);

                    if (txnResponse.TxnStatus)
                    {
                        Success();
                    }
                    else
                    {
                        Error(key, txnResponse);
                    }

                    PublishEvents();

                    _progressBar.Tick();
                }
                catch (TaskCanceledException taskCancelException)
                {
                    Error(key, taskCancelException);
                    PublishEvents();
                    break;
                }
                catch (Exception ex)
                {
                    Error(key, ex);
                    PublishEvents();
                }
            }

            return _response;
        }


        private BatchModeResponseDto<KeyDataType> ParallelForEach(List<KeyDataType> batchKeyList)
        {  
            Parallel.ForEach(batchKeyList, (key, state) =>
            {
                TxnResponse txnResponse = new TxnResponse();

                try
                {
                    if (_methodToProcess != null)
                        txnResponse = _methodToProcess(key);

                    if (txnResponse.TxnStatus)
                    {
                        Success();
                    }
                    else
                    {
                        Error(key, txnResponse);
                    }

                    PublishEvents();

                    _progressBar.Tick();
                }
                catch (TaskCanceledException taskCancelException)
                {
                    Error(key, taskCancelException);
                    PublishEvents();
                    state.Break();
                    return;
                }
                catch (Exception ex)
                {
                    Error(key, ex);
                    PublishEvents();
                }
            });

            return _response;
        }

        private async Task<BatchModeResponseDto<KeyDataType>> ParallelForEachAsync(List<KeyDataType> batchKeyList, CancellationToken cancellationToken)
        {
            await Parallel.ForEachAsync(batchKeyList, cancellationToken, async (key, cancellationToken) =>
            {
                TxnResponse txnResponse = new TxnResponse();

                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                try
                {
                    if (_asyncMethodToProcess != null)
                        txnResponse = await _asyncMethodToProcess(key);

                    if (txnResponse.TxnStatus)
                    {
                        Success();
                    }
                    else
                    {
                        Error(key, txnResponse);
                    }

                    PublishEvents();
                    _progressBar.Tick();
                }
                catch (TaskCanceledException taskCancelException)
                {
                    Error(key, taskCancelException);
                    PublishEvents();
                    return;                    
                }
                catch (Exception ex)
                {
                    Error(key, ex);
                    PublishEvents();
                }
            });

            return _response;
        }

        private void BatchJobValidate()
        {
            _batchKeyListCount = _batchKeyList.Count();

            if (_batchKeyListCount <= 0)
            {
                throw new ArgumentException("No key list to Process");
            }
        }

        private void Success()
        {
            _response.SuccessCount++;
        }

        private void Error(KeyDataType key, TxnResponse txnResponse)
        {
            _response.FailCount++;
            _response.ErrorDetails.Add(new BatchErrorDetailsDto<KeyDataType>()
            {
                TxnKey = key,
                TxnDescription = txnResponse.TxnDescription,
                TxnErrorDescription = txnResponse.TxnErrorDescription
            });

            if(_response.FailCount >= _circuitBreakerLimit)
            {
                _circuitBreakerLimitHit = true;
                throw new TaskCanceledException("Circuit breaker limit reached cancelling the batch");
            }
        }

        private void Error(KeyDataType key, Exception ex)
        {
            _response.FailCount++;
            _response.ErrorDetails.Add(new BatchErrorDetailsDto<KeyDataType>()
            {
                TxnKey = key,
                TxnDescription = (_circuitBreakerLimitHit) ?  "Circuit Breaker" : "Processing method call failed with an exception",
                TxnErrorDescription = (_circuitBreakerLimitHit)? ex.Message : ex.ToString()
            });
        }

        private void PublishEvents()
        {
            if (_subscribeBatchEvents != null && _eventSubscribed)
            {
                string response = JsonSerializer.Serialize(_response);
                var txnEvent = JsonSerializer.Deserialize<BatchTxnEvent<KeyDataType>>(response);

                if(txnEvent != null)
                    _subscribeBatchEvents(txnEvent);
            }
        }

        #endregion
    }
}

