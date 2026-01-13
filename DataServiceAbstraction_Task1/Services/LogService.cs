using DataServiceAbstraction_Task1.Constants;
using DataServiceAbstraction_Task1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServiceAbstraction_Task1.Services;
public class LogService : ILogger
{
    private readonly string _logFilePath;
    private readonly object _lock;

    public LogService()
    {
        _logFilePath = ContsantsVariables.LogFilePath;
        _lock = new object();
    }

    public void LogInformation(string message)
    {
        string logEntry = $"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        WriteFile(logEntry);
        Console.ResetColor();
        Console.WriteLine(logEntry);
    }

    public void LogError(string message,Exception exception)
    {
        string logEntry = $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        WriteFile(logEntry);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(logEntry);
        
        if (exception is not null)
        {
            string exDetails = $"Exception: {exception.Message}{Environment.NewLine} - StackTrace: {exception.StackTrace}";
            WriteFile(exDetails);

            logEntry += $"{Environment.NewLine}{exDetails}";

            Console.WriteLine(logEntry);
        }

        Console.ResetColor();
    }

    private void WriteFile(string logEntry)
    {
        try
        {
            using (var writer = new StreamWriter(_logFilePath, true))
            {
                lock (_lock)
                {
                    writer.WriteLine(logEntry);

                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write log to file: {ex.Message}");
        }
    }

    public void LogDataBass(string message)
    {
        // logic to log in Db or any other storage
    }
}
