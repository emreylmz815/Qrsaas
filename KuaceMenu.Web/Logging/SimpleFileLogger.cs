using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;

namespace KuaceMenu.Web.Logging;

public class SimpleFileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly ConcurrentDictionary<string, SimpleFileLogger> _loggers = new();
    private readonly object _sync = new();

    public SimpleFileLoggerProvider(string filePath)
    {
        _filePath = filePath;
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new SimpleFileLogger(name, _filePath, _sync));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public class SimpleFileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _filePath;
    private readonly object _sync;

    public SimpleFileLogger(string categoryName, string filePath, object sync)
    {
        _categoryName = categoryName;
        _filePath = filePath;
        _sync = sync;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        var line = $"{DateTime.UtcNow:O} [{logLevel}] {_categoryName}: {message}";
        if (exception is not null)
        {
            line += Environment.NewLine + exception;
        }

        lock (_sync)
        {
            System.IO.File.AppendAllText(_filePath, line + Environment.NewLine);
        }
    }
}
