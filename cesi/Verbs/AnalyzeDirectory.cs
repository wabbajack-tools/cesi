using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Text.Json;
using cesi.Analyzers;
using Microsoft.Extensions.Logging;
using Wabbajack.Common;
using Wabbajack.FileExtractor;
using Wabbajack.Hashing.xxHash64;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;

namespace cesi.Verbs;

public class AnalyzeDirectory : IVerb
{
    private readonly ILogger<AnalyzeDirectory> _logger;
    private readonly FileExtractor _extractor;
    private readonly IEnumerable<IAnalyzer> _analyzers;

    public AnalyzeDirectory(ILogger<AnalyzeDirectory> logger, IEnumerable<IAnalyzer> analyzers)
    {
        _logger = logger;
        _analyzers = analyzers;
    }
    public Command MakeCommand()
    {
        var command = new Command("analyze-directory");
        command.Add(new Option<AbsolutePath>(new[] {"-i", "-input"}, "Input Archive"));
        command.Add(new Option<AbsolutePath>(new[] {"-o", "-output"}, "Output folder"));
        command.Description = "Extracts the contents of an archive into a folder";
        command.Handler = CommandHandler.Create(Run);
        return command;
    }
    
    private async Task<int> Run(AbsolutePath input, AbsolutePath output, CancellationToken token)
    {
        if (!output.DirectoryExists())
            output.CreateDirectory();

        var opts = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        foreach (var file in input.EnumerateFiles().Where(f => f.Extension != Ext.Meta).Take(100))
        {
            //_logger.LogInformation("Analyzing {file}", file.RelativeTo(input));
            _logger.LogInformation("Analyzing {Name}", file.FileName);
            await AnalyzeFile(output, token, file, opts);
        }

        return 0;
    }

    private async Task AnalyzeFile(AbsolutePath output, CancellationToken token, AbsolutePath file,
        JsonSerializerOptions opts)
    {
        var initialHash = await file.Hash();
        var outPath = SplitPath(initialHash).RelativeTo(output.Combine("analysis")).WithExtension(Ext.Json);
        outPath.Parent.CreateDirectory();

        await using var os = outPath.Open(FileMode.Create, FileAccess.Write, FileShare.None);
        await using var utf8Writer = new Utf8JsonWriter(os, new JsonWriterOptions() {Indented = true});
        utf8Writer.WriteStartObject();
        utf8Writer.WriteString("xxHash64", initialHash.ToCompatibleHex());
        foreach (var analyzer in _analyzers)
        {
            _logger.LogInformation("- Start: {Name}", analyzer.Name);
            var stopwatch = Stopwatch.StartNew();
            await analyzer.Analyze(utf8Writer, opts, file, async path =>
            {
                await AnalyzeFile(output, token, path, opts);
            }, token);
            _logger.LogInformation("- End: {Elapsed}ms {Name}", stopwatch.ElapsedMilliseconds, analyzer.Name);
        }

        utf8Writer.WriteEndObject();
    }

    private RelativePath SplitPath(Hash initialHash)
    {
        var hash = initialHash.ToArray().Reverse().ToArray();
        RelativePath path = default;
        for (var offset = 0; offset < 6; offset+=2)
        {
            if (offset == 0)
            {
                path = hash[offset..(offset + 2)].ToHex().ToRelativePath();
            }
            else
            {
                path = path.Combine(hash[offset..(offset + 2)].ToHex());
            }
        }
        path = path.Combine(hash.ToHex());
        return path;
    }
}