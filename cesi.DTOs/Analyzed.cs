using CouchDB.Driver.Types;

namespace cesi.DTOs;

public class Analyzed : CouchDocument
{
    
    public string xxHash64 { get; set; }
    public string MD5 { get; set; }
    public string SHA1 { get; set; }
    public string SHA256 { get; set; }
    public string SHA512 { get; set; }
    public string CRC32 { get; set; }
    public long Size { get; set; }
    public Archive? Archive { get; set; }
    public Plugin? Plugin { get; set; }
    public DDS? DDS { get; set; }
    public Source[]? Source { get; set; }
}