namespace cesi.DTOs;

public class Archive
{
    public string Type { get; set; }
    public Dictionary<string, string> Entries { get; set; } = new();
}
