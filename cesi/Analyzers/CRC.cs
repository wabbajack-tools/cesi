using DamienG.Security.Cryptography;
using Wabbajack.Hashing.xxHash64;
using Wabbajack.Paths;

namespace cesi.Analyzers;

public class CRC32 : Analyzer<string>
{
    protected override async Task<string> Analyze(Stream stream, Func<AbsolutePath, Task> analyzeSubFile, CancellationToken token)
    {
        var crc = new Crc32();
        var result = await crc.ComputeHashAsync(stream, token);
        return result.ToHex();
    }

    public override string Name => "CRC32";
}
