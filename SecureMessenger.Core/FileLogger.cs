using System;
using System.IO;

public static class FileLogger
{
    private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_log.txt");
    private static readonly object _lock = new object();

    static FileLogger()
    {
        try { File.WriteAllText(logFilePath, string.Empty); }
        catch { /* Ignore errors */ }
    }

    public static void Log(string message)
    {
        lock (_lock)
        {
            try
            {
                File.AppendAllText(logFilePath, $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
            catch { /* Fail silently */ }
        }
    }
}