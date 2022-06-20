using System.Drawing;
using Shipwreck.Phash;
using Wabbajack.DTOs.Texture;
using Wabbajack.Hashing.PHash;
using Wabbajack.Hashing.xxHash64;
using Wabbajack.Paths;

namespace cesi.Analyzers;

public class DDS : Analyzer<cesi.DTOs.DDS>
{
    protected override async Task<DTOs.DDS?> Analyze(Stream stream, Func<AbsolutePath, Task> analyzeSubFile, CancellationToken token)
    {
        try
        {

            var data = await ImageLoader.Load(stream);
            return new DTOs.DDS
            {
                Width = data.Width,
                Height = data.Height,
                Format = data.Format.ToString(),
                PHash = data.PerceptualHash.Data.ToHex()
            };
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public override string Name => "DDS";
}