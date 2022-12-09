using System.Collections.Concurrent;
using Field.General;

namespace Field.Textures;

enum ShaderType { Vertex, Pixel };

public class ShaderLearningCommandlet : Commandlet
{
    public override void Main(string[] args)
    {
        CompareEnvironmentShaders();
    }

    private void CompareEnvironmentShaders()
    {
        
    }

    private ConcurrentDictionary<TagHash, TagHash> GetShaders(ShaderType shaderType)
    {
        ConcurrentDictionary<TagHash, TagHash> tags = new ConcurrentDictionary<TagHash, TagHash>();
        
        List<TagHash> allMaterials = PackageHandler.GetAllTagsWithReference(0x80806daa);
        PackageHandler.CacheHashDataList(allMaterials.Select(x => x.Hash).ToArray());
        Parallel.ForEach(allMaterials, material =>
        {
            using (MemoryStream ms = new MemoryStream(PackageHandler.BytesCache[material]))
            using (BinaryReader br = new BinaryReader(ms))
            {
                if (shaderType == ShaderType.Vertex)
                {
                    br.BaseStream.Seek(0x70, SeekOrigin.Begin);
                }
                else if (shaderType == ShaderType.Pixel)
                {
                    br.BaseStream.Seek(0x2B0, SeekOrigin.Begin);
                }
                else
                {
                    throw new NotImplementedException();
                }
                uint hash = br.ReadUInt32();
                if (hash == 0xFFFFFFFF)
                    return;
                TagHash reff = PackageHandler.GetEntryReference(new TagHash(hash));
                if (reff == 0xFFFFFFFF)
                    return;
                TagHash dxbcFile = new TagHash(reff);
                tags.TryAdd(material, dxbcFile);
            }
        });
        return tags;
    }
    
    private void SaveAllShaders()
    {
        List<TagHash> allMaterials = PackageHandler.GetAllTagsWithReference(0x80806daa);
        PackageHandler.CacheHashDataList(allMaterials.Select(x => x.Hash).ToArray());
        Parallel.ForEach(allMaterials, material =>
        {
            using (MemoryStream ms = new MemoryStream(PackageHandler.BytesCache[material]))
            using (BinaryReader br = new BinaryReader(ms))
            {
                // Get the PS and decompile it
                br.BaseStream.Seek(0x70, SeekOrigin.Begin);
                uint psHash = br.ReadUInt32();
                if (psHash == 0xFFFFFFFF)
                    return;
                TagHash reff = PackageHandler.GetEntryReference(new TagHash(psHash));
                if (reff == 0xFFFFFFFF)
                    return;
                DestinyFile psFile = new DestinyFile(reff);
                Material.Decompile(psFile.GetData(), material, "vs", "C:/T/all_vs/");
            }
        });
    }
}