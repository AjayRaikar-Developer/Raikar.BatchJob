// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using Raikar.BatchJob.Test;
using ShellProgressBar;
using System.Diagnostics;
using System.Drawing;
using System.Text;

Console.WriteLine("Raikar BatchJob Testing!\n");
Console.WriteLine("Even or Odd Number Check - \n");

BatchJobTest batchJob = new BatchJobTest();
var result = batchJob.Test();

Console.WriteLine("Batch Job Response\n");
Console.WriteLine(JsonConvert.SerializeObject(result));
Console.WriteLine("\n");

ConsoleTable.PrintLine();
ConsoleTable.PrintRow("", "Error Details", "");
ConsoleTable.PrintLine();
ConsoleTable.PrintRow("Key", "Txn Description", "Error Description");
ConsoleTable.PrintLine();
foreach (var x in result.ErrorDetails)
{
    ConsoleTable.PrintRow(x.TxnKey.ToString(), x.TxnDescription, x.TxnErrorDescription);
    ConsoleTable.PrintLine();
}

Console.WriteLine("=======================");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"     Batch Result      ");
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"\n Total Count - {result.TotalCount}");
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"\n Success Count - {result.SuccessCount}");
Console.ForegroundColor = ConsoleColor.DarkRed;
Console.WriteLine($"\n Failed Count - {result.FailCount}");
Console.ForegroundColor = ConsoleColor.Black;
Console.ResetColor();
Console.WriteLine("=======================");

if (result.BatchReportHtml != null)
{

    var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "BatchReport.html");

    using (FileStream fs = new FileStream(reportPath, FileMode.Open))
    {
        using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
        {
            w.WriteLine(result.BatchReportHtml);
        }
    }

    var uri = reportPath;
    var psi = new System.Diagnostics.ProcessStartInfo();
    psi.UseShellExecute = true;
    psi.FileName = uri;
    System.Diagnostics.Process.Start(psi);
}