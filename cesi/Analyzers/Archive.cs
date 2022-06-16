using System.Text.Json;
using Microsoft.Extensions.Logging;
using Wabbajack.Common;
using Wabbajack.Common.FileSignatures;
using Wabbajack.FileExtractor;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;

namespace cesi.Analyzers;

public class Archive : IAnalyzer
{
    private readonly ILogger<Archive> _logger;
    private readonly TemporaryFileManager _temporaryFileManager;
    private readonly FileExtractor _fileExtractor;

    public Archive(ILogger<Archive> logger, TemporaryFileManager temporaryFileManager, FileExtractor fileExtractor)
    {
        _logger = logger;
        _temporaryFileManager = temporaryFileManager;
        _fileExtractor = fileExtractor;
    }

    public string Name => "Archive";
    public Type DTOType => throw new NotImplementedException();
    public IEnumerable<FileType> LimitSignatures => throw new NotImplementedException();

    public async Task Analyze(Utf8JsonWriter writer, JsonSerializerOptions options, AbsolutePath path, Func<AbsolutePath, Task> analyzeSubFile,
        CancellationToken token)
    {
        var archiveType = await FileExtractor.ArchiveSigs.MatchesAsync(path);
        if (archiveType == null) return;

        await using var tempPath = _temporaryFileManager.CreateFolder();
        await _fileExtractor.ExtractAll(path, tempPath.Path, token);
        writer.WritePropertyName("Archive");
        writer.WriteStartObject();
        writer.WriteString("Type", archiveType.ToString());
        writer.WritePropertyName("Entries");
        writer.WriteStartObject();
        foreach (var file in tempPath.Path.EnumerateFiles())
        {
            var hash = await file.Hash();
            writer.WriteString(file.RelativeTo(tempPath.Path).ToString(), hash.ToHex());
            await analyzeSubFile(file);
        }
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}