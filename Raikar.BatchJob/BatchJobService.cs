using Raikar.BatchJob.Models;
using ShellProgressBar;

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
    public delegate TxnResponseDto BatchTxnProcess<KeyDataType>(KeyDataType Key);

    /// <summary>
    /// It calls the batch job transaction async method which manages key wise processing
    /// </summary>
    /// <typeparam name="KeyDataType"></typeparam>
    /// <param name="Key"></param>
    /// <returns></returns>
    public delegate Task<TxnResponseDto> BatchAsyncTxnProcess<KeyDataType>(KeyDataType Key);
    
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
        BatchModeResponseDto<KeyDataType> _response = new BatchModeResponseDto<KeyDataType>();

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

        public BatchJobService(List<KeyDataType> keyList, BatchAsyncTxnProcess<KeyDataType> asyncMethodToProcess,CancellationToken cancellationToken)
        {
            _batchKeyList = keyList;
            BatchJobValidate();
            _asyncMethodToProcess = asyncMethodToProcess;
            _cancellationToken = cancellationToken;
            _progressBar = new ProgressBar(_batchKeyListCount, "Batch Job Async Service", options);
        }

        public BatchJobService(GetBatchKeyList<KeyDataType> getBatchList,BatchTxnProcess<KeyDataType> methodToProcess, BatchProcessMode batchProcessMode)
        {
            _getBatchKeyList = getBatchList;
            _batchKeyList = _getBatchKeyList();
             BatchJobValidate();
            _methodToProcess = methodToProcess;
            _batchProcessMode = batchProcessMode;
            _progressBar = new ProgressBar(_batchKeyListCount, "Batch Job Service", options);
        }

        public BatchJobService(GetBatchKeyList<KeyDataType> getBatchList,BatchAsyncTxnProcess<KeyDataType> asyncMethodToProcess, CancellationToken cancellationToken)
        {
            _getBatchKeyList = getBatchList;
            _batchKeyList = _getBatchKeyList();
             BatchJobValidate();
            _asyncMethodToProcess = asyncMethodToProcess;
            _cancellationToken = cancellationToken;
            _progressBar = new ProgressBar(_batchKeyListCount, "Batch Job Async Service", options);
        }        
        #endregion

        public void GetBatchKeyList(GetBatchKeyList<KeyDataType> getBatchList)
        {
            _getBatchKeyList = getBatchList;
            _batchKeyList = _getBatchKeyList();
            BatchJobValidate();
        }       

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
                case BatchProcessMode.Single:
                    batchProcessFunc = new BatchProcessFunc<KeyDataType>(ForEach);
                    batchResult = batchProcessFunc(_batchKeyList);
                    break;

                case BatchProcessMode.Parallel:
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

            return response;
        }

        #region Private Methods
        private BatchModeResponseDto<KeyDataType> ForEach(List<KeyDataType> batchKeyList)
        {
            foreach (var key in batchKeyList)
            {
                TxnResponseDto txnResponse = new TxnResponseDto();
                Thread.Sleep(1000);
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
                    _progressBar.Tick();

                }
                catch (Exception ex)
                {
                    Error(key, ex);
                }
            }

            return _response;
        }


        private BatchModeResponseDto<KeyDataType> ParallelForEach(List<KeyDataType> batchKeyList)
        {  
            Parallel.ForEach(batchKeyList, key =>
            {
                TxnResponseDto txnResponse = new TxnResponseDto();

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

                }
                catch (Exception ex)
                {
                    Error(key, ex);
                }
            });

            return _response;
        }

        private async Task<BatchModeResponseDto<KeyDataType>> ParallelForEachAsync(List<KeyDataType> batchKeyList, CancellationToken cancellationToken)
        {  
            await Parallel.ForEachAsync(batchKeyList, cancellationToken, async (key, cancellationToken) =>
            {
                TxnResponseDto txnResponse = new TxnResponseDto();

                cancellationToken.ThrowIfCancellationRequested();

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

                }
                catch (Exception ex)
                {
                    Error(key, ex);
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

        private void Error(KeyDataType key, TxnResponseDto txnResponse)
        {
            _response.FailCount++;
            _response.ErrorDetails.Add(new BatchErrorDetailsDto<KeyDataType>()
            {
                TxnKey = key,
                TxnDescription = txnResponse.TxnDescription,
                TxnErrorDescription = txnResponse.TxnErrorDescription
            });
        }

        private void Error(KeyDataType key, Exception ex)
        {
            _response.FailCount++;
            _response.ErrorDetails.Add(new BatchErrorDetailsDto<KeyDataType>()
            {
                TxnKey = key,
                TxnDescription = "Processing method call failed with an exception",
                TxnErrorDescription = ex.ToString()
            });
        }
        #endregion
    }
}

