namespace LoggerExtensions;

using System;
using System.Collections.Concurrent;

public class Program
{
    public static void Main(string[] args)
    {
        ILoggerFactory factory = new LoggerFactory(new LoggerProvider());

        ILogger<Program>? logger1 = factory.CreateLogger<Program>();
        //Specific provider:
        //ILogger<Program>? logger1 = factory.CreateLogger<Program>(providerName: "MyLoggingProvider");
        logger1?.LogError("Logging an error");


        ILogger? logger2 = factory.CreateLogger(context: "AnotherProgram");
        //Specific provider:
        //ILogger? logger2 = factory.CreateLogger(context: "AnotherProgram", providerName: "MyLoggingProvider");
        logger2?.LogError("Logging another error");
    }

}

public interface ILoggerFactory
{
    ILogger<TContext>? CreateLogger<TContext>() where TContext : class;
    ILogger<TContext>? CreateLogger<TContext>(string providerName) where TContext : class;
    ILogger? CreateLogger(string context);
    ILogger? CreateLogger(string context, string providerName);
    void AddProvider(ILoggerProvider provider);
}

public class LoggerFactory : ILoggerFactory
{
    private readonly ConcurrentDictionary<string, ILoggerProvider> loggerProviders = new();

    public LoggerFactory()
    {
        //Default constructor so we can add Providers after creation
    }

    public LoggerFactory(ILoggerProvider provider) => AddProvider(provider);

    public ILogger<TContext>? CreateLogger<TContext>() where TContext : class
    {
        ILoggerProvider? provider = loggerProviders.GetValueOrDefault(loggerProviders.Keys.First());
        ILogger<TContext>? logger = provider?.CreateLogger<TContext>();

        return logger;
    }

    public ILogger<TContext>? CreateLogger<TContext>(string providerName) where TContext : class
    {
        ILoggerProvider? provider = loggerProviders.GetValueOrDefault(providerName);
        ILogger<TContext>? logger = provider?.CreateLogger<TContext>();

        return logger;
    }

    public ILogger? CreateLogger(string context)
    {
        ILoggerProvider? provider = loggerProviders.GetValueOrDefault(loggerProviders.Keys.First());
        ILogger? logger = provider?.CreateLogger(context);

        return logger;
    }

    public ILogger? CreateLogger(string context, string providerName)
    {
        ILoggerProvider? provider = loggerProviders.GetValueOrDefault(providerName);
        ILogger? logger = provider?.CreateLogger(context);

        return logger;
    }

    public void AddProvider(ILoggerProvider provider)
    {
        Type? providerType = provider.GetType();
        loggerProviders.AddOrUpdate(providerType.ToString(), name => provider, (name, provider) => provider);
    }
}

public interface ILoggerProvider
{
    ILogger<TContext>? CreateLogger<TContext>() where TContext : class;
    ILogger? CreateLogger(string context);
}

public class LoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, ILogger> loggers = new();

    public ILogger<TContext>? CreateLogger<TContext>() where TContext : class
    {
        ILogger<TContext>? logger = loggers.GetOrAdd(typeof(TContext).Name, name => new Logger<TContext>()) as ILogger<TContext>;

        return logger;
    }

    public ILogger? CreateLogger(string context)
    {
        ILogger? logger = loggers.GetOrAdd(context, name => new Logger(context));

        return logger;
    }
}

public interface ILogger
{
    void Log(string text);
    void Log(string context, string text);
}

public class Logger : ILogger
{
    private readonly string context;

    public Logger(string context) => this.context = context;

    public void Log(string text) => Console.WriteLine($"[{context}] - {text}");
    public void Log(string context, string text) => Console.WriteLine($"[{context}] - {text}");
}

public interface ILogger<TContext> : ILogger where TContext : class
{
}

public class Logger<TContext> : Logger, ILogger<TContext> where TContext : class
{
    public Logger()
        : base(typeof(TContext).Name)
    { }
}

public static class LoggerExtensionMethods
{
    //Add enum for different levels and implement in Logger.Log method
    public static void LogTrace(this ILogger logger, string text) => logger.Log(text);
    public static void LogTrace<TContext>(this ILogger<TContext> logger, string text) where TContext : class => logger.Log(text);
    public static void LogDebug(this ILogger logger, string text) => logger.Log(text);
    public static void LogDebug<TContext>(this ILogger<TContext> logger, string text) where TContext : class => logger.Log(text);
    public static void LogInformation(this ILogger logger, string text) => logger.Log(text);
    public static void LogInformation<TContext>(this ILogger<TContext> logger, string text) where TContext : class => logger.Log(text);
    public static void LogWarning(this ILogger logger, string text) => logger.Log(text);
    public static void LogWarning<TContext>(this ILogger<TContext> logger, string text) where TContext : class => logger.Log(text);
    public static void LogError(this ILogger logger, string text) => logger.Log(text);
    public static void LogError<TContext>(this ILogger<TContext> logger, string text) where TContext : class => logger.Log(text);
    public static void LogCritical(this ILogger logger, string text) => logger.Log(text);
    public static void LogCritical<TContext>(this ILogger<TContext> logger, string text) where TContext : class => logger.Log(text);
}
