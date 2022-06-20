namespace cesi.DTOs;

public class Plugin
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public uint FormVersion { get; set; }
    public string ModType { get; set; }
    public bool IsMaster { get; set; }
    public bool IsLightMaster { get; set; }
    public string[] MasterReferences { get; set; } = Array.Empty<string>();
}