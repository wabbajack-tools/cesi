using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Text.Json;
using cesi.Analyzers;
using cesi.DTOs;
using CouchDB.Driver;
using CouchDB.Driver.Exceptions;
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
    private readonly CouchClient _client;

    public AnalyzeDirectory(ILogger<AnalyzeDirectory> logger, IEnumerable<IAnalyzer> analyzers, CouchClient client)
    {
        _logger = logger;
        _analyzers = analyzers;
        _client = client;
    }
    public Command MakeCommand()
    {
        var command = new Command("analyze-directory");
        command.Add(new Option<AbsolutePath>(new[] {"-i", "-input"}, "Input Archive"));
        command.Description = "Extracts the contents of an archive into a folder";
        command.Handler = CommandHandler.Create(Run);
        return command;
    }
    
    private async Task<int> Run(AbsolutePath input, CancellationToken token)
    {

        var db = _client.GetDatabase<DTOs.Analyzed>("cesi");

        var opts = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        foreach (var file in input.EnumerateFiles().Where(f => f.Extension != Ext.Meta))
        {
            //_logger.LogInformation("Analyzing {file}", file.RelativeTo(input));
            _logger.LogInformation("Analyzing {Name}", file.FileName);
            await AnalyzeFile(db, token, file, opts);
        }

        return 0;
    }

    private async Task AnalyzeFile(ICouchDatabase<Analyzed> db, CancellationToken token, AbsolutePath file,
        JsonSerializerOptions opts)
    {
        _logger.LogInformation("Analyzing {File}", file.FileName);
        var initialHash = await file.Hash();

        var ms = new MemoryStream();
        await using var utf8Writer = new Utf8JsonWriter(ms, new JsonWriterOptions() {Indented = true});
        utf8Writer.WriteStartObject();
        utf8Writer.WriteString("xxHash64", initialHash.ToCompatibleHex());
        utf8Writer.WriteString("Id", initialHash.ToCompatibleHex());
        foreach (var analyzer in _analyzers)
        {
            try
            {
                await analyzer.Analyze(utf8Writer, opts, file,
                    async path => { await AnalyzeFile(db, token, path, opts); }, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "While running Analyzer {Name}", analyzer.Name);
            }
        }

        utf8Writer.WriteEndObject();
        await utf8Writer.FlushAsync(token);

        ms.Position = 0;
        try
        {
            var doc = await JsonSerializer.DeserializeAsync<Analyzed>(ms, cancellationToken: token)!;
            await db.AddOrUpdateAsync(doc, false, token);
        }
        catch (CouchConflictException ex)
        {
            
        }
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