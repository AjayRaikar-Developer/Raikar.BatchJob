# Raikar.BatchJob
This is a batchjob library which help us in processing the data in bulk. 

With feature like - dynamic strongly key data type mapping, circuit breaker, batch report & many more..!

.NET 6 ready!

It has 3 modes of operation -  
 - **Foreach** - Synchronous single threaded 
 - **Parallel-Foreach** - Synchronous multi threaded
 - **Parallel-Foreach-Async** - Asynchronous multi threaded

> *Default operation mode of batch is **Foreach*** 

# Install
Get it on nuget: https://www.nuget.org/packages/Raikar.BatchJob/

Using Pacakage Manager Console: 
```PM
 NuGet\Install-Package Raikar.BatchJob -Version 1.0.0
 ```

# Usage  
Usage is really straightforward

## Dynamic Key List Synchronous Example 
```csharp
//Key List
List<int> _keyList = Enumerable.Range(1, 50).ToList();

//Transaction Method
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

//Transaction Method mapping to the delegate BatchTxnProcess
//This _process is passed as input to the batch configuration 
//Declare it in a constructor or a method
BatchTxnProcess<int> _process = new BatchTxnProcess<int>(TxnProcess);

//Configuring the BatchJob and Executing
 public BatchResponse<int> SyncForEachBatch()
{            
    BatchJobService<int> _batchJobService = new BatchJobService<int>(_keyList, _process, BatchProcessMode.Foreach);

    return _batchJobService.ExecuteBatchJob();
}       
```


- **Step-1:** Input forming beforing configuring the batch 

    1. Key List - Against which key's the batch should run.
    2. Transaction Method - This is the processing method which you have to create which takes key as input to process and returns ```TxnResponse``` model as response with correct status either success or fail mapped to the field ```TxnStatus```
    3. **BatchProcessMode** - Selecting any one synchronous operation mode. In the above example I have used **Foreach**

- **Step-2:** Configuring the Batch & Executing it 
    1. Map all the field from the above inputs configured to the BatchJobService method
    2. Call the ```ExecuteBatchJob()``` method by instating the BatchJobService


## Dynamic Key List Asynchronous Example 
```csharp
//Key List
List<int> _keyList = Enumerable.Range(1, 50).ToList();

//Async Transaction Method
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

//Transaction Method mapping to the delegate BatchTxnProcess
//This _process is passed as input to the batch configuration
BatchAsyncTxnProcess<int> _asyncProcess = new BatchAsyncTxnProcess<int>(AsyncTxnProcess);

//Cancellation Token
public CancellationTokenSource _cancelToken = new CancellationTokenSource();

//Configuring the BatchJob and Executing
public async Task<BatchResponse<int>> ForEachParallelAsyncBatch()
{
    var token = _cancelToken.Token;          
    BatchJobService<int> _batchJobService = new BatchJobService<int>(_keyList, _asyncProcess, token);
    
    return await _batchJobService.ExecuteBatchJobAsync();
}
```


## Batch Options Example
**Batch Options -**
- **BatchProcessMode** - Its an enum. This helps the library to decide in which operation mode it has to run either Foreach or ParallelForEach.
- **GenerateBatchReport** - If true then HTML batch report would be sent in response else null.
- **CircuitBreakerLimit** - It's the limit where the batch will break when error count matchs the limit configured.
- **BatchName** - Configure the batch with a name.

```csharp
public BatchResponse<int> SyncForEachBatch_WithOptions()
{
    BatchJobOptions options = new BatchJobOptions()
    {
        BatchName = "SyncForEachBatch With Options",
        BatchProcessMode = BatchProcessMode.Foreach,
        GenerateBatchReport = true,
        CircuitBreakerLimit = 100
    };

    BatchJobService<int> _batchJobService = new BatchJobService<int>(_keyList, _process, options);
    return _batchJobService.ExecuteBatchJob();
}
```

> For more examples please check out the Test Project - ***Raikar.BatchJob.Test***

# Features
- 3 Operation modes - Foreach, Paralle-Foreach & Parallel-Foreach-Async
- Dynamic Key List transaction processing 
- Gets the Key List from the user's custom method
- Circuit Breaker Option 
- Batch Report Option
- Batch events publish method which can be used by the user's to subscribe for live events from the batch
- Console Loader Untill the batch is progressing

> ***Hit that Star button to show some ❤️***