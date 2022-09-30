// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using Raikar.BatchJob.Test;
using ShellProgressBar;

Console.WriteLine("Raikar BatchJob Testing!");

BatchJobTest batchJob = new BatchJobTest();

Console.WriteLine();
Console.WriteLine(JsonConvert.SerializeObject(batchJob.Test()));


Console.ReadLine();