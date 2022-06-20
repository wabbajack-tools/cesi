using System.Text.Json;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Wabbajack.Common;
using Wabbajack.Common.FileSignatures;
using Wabbajack.Paths;

namespace cesi.Analyzers;

public class Plugin : IAnalyzer
{
    public string Name => "Plugin";
    public Type DTOType => throw new NotImplementedException();
    public IEnumerable<FileType> LimitSignatures { get; }

    private HashSet<Extension> Extensions = new() {Ext.Esp, Ext.Esm, new Extension(".esl")};
    public async Task Analyze(Utf8JsonWriter writer, JsonSerializerOptions options, AbsolutePath path, Func<AbsolutePath, Task> analyzeSubFile,
        CancellationToken token)
    {
        if (!Extensions.Contains(path.Extension)) return;
        
        var file = SkyrimMod.CreateFromBinary(new ModPath(path.ToString()), SkyrimRelease.SkyrimSE);
        
        writer.WritePropertyName("Plugin");
        writer.WriteStartObject();
        writer.WriteString("Name", file.ModKey.FileName);
        writer.WriteString("Author", file.ModHeader.Author);
        writer.WriteString("Description", file.ModHeader.Description);
        writer.WriteNumber("FormVersion", file.ModHeader.FormVersion);
        writer.WriteString("ModType", file.ModKey.Type.ToString());
        writer.WriteBoolean("IsLightMaster", file.ModHeader.Flags.HasFlag(SkyrimModHeader.HeaderFlag.LightMaster));
        writer.WriteBoolean("IsMaster", file.ModHeader.Flags.HasFlag(SkyrimModHeader.HeaderFlag.Master));
        if (file.ModHeader.MasterReferences.Any())
        {
            writer.WritePropertyName("MasterReferences");
            writer.WriteStartArray();
            foreach (var master in file.ModHeader.MasterReferences)
            {
                writer.WriteStringValue(master.Master.FileName);
            }
            writer.WriteEndArray();
        }

        writer.WriteEndObject();

        return;

    }
}