using System.CommandLine;
using System.CommandLine.IO;
using System.ComponentModel;
using System.Text.Json;
using cesi;
using cesi.Analyzers;
using cesi.Verbs;
using CouchDB.Driver;
using CouchDB.Driver.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using NLog.Targets;
using Wabbajack.Downloaders;
using Wabbajack.DTOs;
using Wabbajack.FileExtractor;
using Wabbajack.Networking.Http;
using Wabbajack.Networking.Http.Interfaces;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;
using Wabbajack.RateLimiter;
using Wabbajack.Services.OSIntegrated;

TypeDescriptor.AddAttributes(typeof(AbsolutePath),
    new TypeConverterAttribute(typeof(AbsolutePathTypeConverter)));

var host = Host.CreateDefaultBuilder(Array.Empty<string>())
    .ConfigureLogging(AddLogging)
    .ConfigureServices((host, services) =>
    {
        services.AddSingleton(new JsonSerializerOptions());
        
        services.AddSingleton<IVerb, AnalyzeDirectory>();

        services.AddSingleton<CommandLineBuilder>();
        services.AddSingleton<IConsole, SystemConsole>();
        
        /*
        services.AddSingleton<HttpClient, HttpClient>();
        services.AddSingleton<IHttpDownloader, SingleThreadedDownloader>();

        services.AddSingleton<TemporaryFileManager>();
        services.AddSingleton<FileExtractor>();
        services.AddSingleton<ParallelOptions>(s => new ParallelOptions());
        services.AddAllSingleton<IResource, IResource<FileExtractor>>(s =>
            new Resource<FileExtractor>("File Extractor", maxTasks:4));
            */

        services.AddSingleton<CouchClient>(s => new CouchClient("http://localhost:15984", builder =>
        {
            builder.UseBasicAuthentication("cesi", "password");
            builder.SetPropertyCase(PropertyCaseType.None);
            builder.SetJsonNullValueHandling(NullValueHandling.Ignore);
        }));

        services.AddSingleton<IAnalyzer, MD5>();
        services.AddSingleton<IAnalyzer, SHA1>();
        services.AddSingleton<IAnalyzer, SHA256>();
        services.AddSingleton<IAnalyzer, SHA512>();
        
        services.AddSingleton<IAnalyzer, CRC32>();
        services.AddSingleton<IAnalyzer, Size>();
        services.AddSingleton<IAnalyzer, cesi.Analyzers.Archive>();
        services.AddSingleton<IAnalyzer, Plugin>();
        services.AddSingleton<IAnalyzer, DDS>();
        services.AddSingleton<IAnalyzer, Source>();
        services.AddOSIntegrated();

    }).Build();

var service = host.Services.GetRequiredService<CommandLineBuilder>();
return await service!.Run(args);

void AddLogging(ILoggingBuilder loggingBuilder)
{
    var config = new NLog.Config.LoggingConfiguration();

    var fileTarget = new FileTarget("file")
    {
        FileName = "logs/cesi.current.log",
        ArchiveFileName = "logs/cesi.{##}.log",
        ArchiveOldFileOnStartup = true,
        MaxArchiveFiles = 10,
        Layout = "${processtime} [${level:uppercase=true}] (${logger}) ${message:withexception=true}",
        Header = "############ Creation Engine Survey Initiative log file - ${longdate} ############"
    };

    var consoleTarget = new ConsoleTarget("console")
    {
        Layout = "${processtime} [${level:uppercase=true}] ${message:withexception=true}",
    };
        

    config.AddRuleForAllLevels(fileTarget);
    config.AddRuleForAllLevels(consoleTarget);

    loggingBuilder.ClearProviders();
    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
    loggingBuilder.AddNLog(config);
}