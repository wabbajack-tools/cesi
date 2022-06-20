using System.Text.Json;
using Wabbajack.Common;
using Wabbajack.Common.FileSignatures;
using Wabbajack.Downloaders;
using Wabbajack.DTOs;
using Wabbajack.DTOs.DownloadStates;
using Wabbajack.Installer;
using Wabbajack.Paths;
using Wabbajack.Paths.IO;

namespace cesi.Analyzers;

public class Source : IAnalyzer
{
    private readonly DownloadDispatcher _dispatcher;
    public string Name { get; }
    public Type DTOType { get; }
    public IEnumerable<FileType> LimitSignatures { get; }

    public Source(DownloadDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Analyze(Utf8JsonWriter writer, JsonSerializerOptions options, AbsolutePath path, Func<AbsolutePath, Task> analyzeSubFile,
        CancellationToken token)
    {
        var meta = path.WithExtension(Ext.Meta);
        if (!meta.FileExists())
            return;

        var ini = await _dispatcher.ResolveArchive(meta.LoadIniFile()["General"]
            .ToDictionary(d => d.KeyName, d => d.Value));
        if (ini == null) return;

        writer.WritePropertyName("Source");
        writer.WriteStartArray();
        
        writer.WriteStartObject();
        switch (ini)
        {
            case Nexus n:
                writer.WritePropertyName("Nexus");
                writer.WriteStartObject();
                
                writer.WriteString("Game", n.Game.MetaData().NexusName);
                writer.WriteNumber("ModId", n.ModID);
                writer.WriteNumber("Number", n.FileID);
                
                writer.WriteEndObject();

                break;
            case GameFileSource g:
                writer.WritePropertyName("GameFile");
                writer.WriteStartObject();
                
                writer.WriteString("Game", g.Game.ToString());
                writer.WriteString("Version", g.GameVersion);
                writer.WriteString("Path", g.GameFile.ToString());

                writer.WriteEndObject();
                break;
            case Http h:
                writer.WritePropertyName("Http");
                writer.WriteStartObject();
                
                writer.WriteString("Url", h.Url.ToString());

                writer.WriteEndObject();
                break;
            default:
                throw new NotImplementedException();

        }
        
        writer.WriteEndObject();
        writer.WriteEndArray();
        
    }
}