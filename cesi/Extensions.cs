using Wabbajack.Hashing.xxHash64;

namespace cesi;

public static class Extensions
{
    public static string ToCompatibleHex(this Hash hash)
    {
        return hash.ToArray().Reverse().ToArray().ToHex();
    }
}