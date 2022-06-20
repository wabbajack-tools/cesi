namespace cesi.DTOs;

public class Source
{
    public NexusSource? Nexus { get; set; }
}

public class NexusSource
{
    public string Game { get; set; }
    public long ModId { get; set; }
    public long FileId { get; set; }
}