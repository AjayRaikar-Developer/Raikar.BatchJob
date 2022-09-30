// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using Raikar.BatchJob.Test;
using ShellProgressBar;
using System.Drawing;

Console.WriteLine("Raikar BatchJob Testing!");

BatchJobTest batchJob = new BatchJobTest();
var result = batchJob.Test();

Console.WriteLine("\n");

Console.WriteLine();
Console.WriteLine(JsonConvert.SerializeObject(result));

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("=====================================");
Console.BackgroundColor = ConsoleColor.Magenta;
Console.WriteLine($"     Batch Result      ");
Console.BackgroundColor = ConsoleColor.DarkBlue;
Console.WriteLine($"\n Total Count - {result.TotalCount}");
Console.BackgroundColor = ConsoleColor.DarkGreen;
Console.WriteLine($"\n Success Count - {result.SuccessCount}");
Console.BackgroundColor = ConsoleColor.Red;
Console.WriteLine($"\n Failed Count - {result.FailCount}");
Console.ForegroundColor = ConsoleColor.Black;
Console.ResetColor();
Console.WriteLine("=====================================");



BatchJobTest.PrintLine();
BatchJobTest.PrintRow("Key", "Error Description");
BatchJobTest.PrintLine();
foreach(var x in result.ErrorDetails)
{
    BatchJobTest.PrintRow(x.TxnKey.ToString(), x.TxnErrorDescription);
}
BatchJobTest.PrintLine();

Console.ReadLine();