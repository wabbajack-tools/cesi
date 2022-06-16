using System.Security.Cryptography;
using Wabbajack.Common.FileSignatures;
using Wabbajack.Hashing.xxHash64;
using Wabbajack.Paths;

namespace cesi.Analyzers;

public abstract class CryptoHashAnalyzer : Analyzer<string>
{
    private readonly string _algo;

    protected CryptoHashAnalyzer(string algo)
    {
        _algo = algo;
    }
    protected override async Task<string?> Analyze(Stream stream, Func<AbsolutePath, Task> analyzeSubFile, CancellationToken token)
    {
        var algo = HashAlgorithm.Create(_algo);
        return (await algo.ComputeHashAsync(stream, token)).ToHex();
    }

    public override string Name => _algo;
    public override IEnumerable<FileType> LimitSignatures => new FileType[] { };
}

public class MD5 : CryptoHashAnalyzer
{
    public MD5() : base("MD5")
    {
    }
}

public class SHA1 : CryptoHashAnalyzer
{
    public SHA1() : base("SHA1")
    {
    }
}

public class SHA256 : CryptoHashAnalyzer
{
    public SHA256() : base("SHA256")
    {
    }
}

public class SHA512 : CryptoHashAnalyzer
{
    public SHA512() : base("SHA512")
    {
    }
}