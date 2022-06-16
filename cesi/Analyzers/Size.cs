using Wabbajack.Common.FileSignatures;
using Wabbajack.Paths;

namespace cesi.Analyzers;

public class Size : Analyzer<long>
{
    protected override async Task<long> Analyze(Stream stream, Func<AbsolutePath, Task> analyzeSubFile, CancellationToken token)
    {
        return stream.Length;
    }

    public override string Name => "Size";
}