using System.Text.Json;
using Wabbajack.Common.FileSignatures;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;

namespace cesi.Analyzers;

public interface IAnalyzer
{
    /// <summary>
    /// The name of this analyzer, the data for this analyzer is stored under a key of this name
    /// </summary>
    public string Name { get; }
    
    public Type DTOType { get; }
    
    /// <summary>
    /// File types that this analyzer should run on
    /// </summary>
    public IEnumerable<FileType> LimitSignatures { get; }
    
    public Task Analyze(Utf8JsonWriter writer, JsonSerializerOptions options, AbsolutePath stream, Func<AbsolutePath, Task> analyzeSubFile, CancellationToken token);
}

public abstract class Analyzer<T> : IAnalyzer
{
    public async Task Analyze(Utf8JsonWriter writer, JsonSerializerOptions options, AbsolutePath stream, Func<AbsolutePath, Task> analyzeSubfile, CancellationToken token)
    {
        await using var strm = stream.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        var result = await Analyze(strm, analyzeSubfile, token);
        if (result != null)
        {
            writer.WritePropertyName(Name);
            JsonSerializer.Serialize(writer, result!, options);
        }
    }

    protected abstract Task<T?> Analyze(Stream stream, Func<AbsolutePath, Task> analyzeSubFile, CancellationToken token);
    public abstract string Name { get; }

    public Type DTOType => typeof(T);
    public virtual IEnumerable<FileType> LimitSignatures => Array.Empty<FileType>();
}