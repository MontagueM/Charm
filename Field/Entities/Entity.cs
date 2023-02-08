using System.Collections.Generic;
using Field;
using Field.General;
using Field.Models;

namespace Field.Entities;

public class Entity : Tag
{
    public D2Class_D89A8080 Header;
    public List<EntityResource> Resources;
    
    // Entity features
    public EntitySkeleton Skeleton = null;
    public EntityModel Model = null;
    public EntityResource ModelParentResource = null;
    public EntityModel PhysicsModel = null;
    public EntityResource PatternAudio = null;
    public EntityResource PatternAudioUnnamed = null;
    public EntityControlRig ControlRig = null;
    
    public Entity(TagHash hash) : base(hash)
    {
    }
    
    public Entity(TagHash hash, bool disableLoad) : base(hash, disableLoad)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_D89A8080>();
    }

    protected override void ParseData()
    {
        foreach (var resource in Header.EntityResources)
        {
            switch (resource.ResourceHash.Header.Unk10)
            {
                case D2Class_8A6D8080:  // Entity model
                    Model = ((D2Class_8F6D8080)resource.ResourceHash.Header.Unk18).Model;
                    ModelParentResource = resource.ResourceHash;
                    break;
                case D2Class_5B6D8080:  // Entity physics model
                    PhysicsModel = ((D2Class_6C6D8080)resource.ResourceHash.Header.Unk18).PhysicsModel;
                    break;
                case D2Class_DD818080:  // Entity skeleton FK
                    Skeleton = PackageHandler.GetTag(typeof(EntitySkeleton), resource.ResourceHash.Hash);
                    break;
                case D2Class_668B8080:  // Entity skeleton IK
                    ControlRig = PackageHandler.GetTag(typeof(EntityControlRig), resource.ResourceHash.Hash);
                    break;
                case D2Class_97318080:
                    PatternAudio = resource.ResourceHash;
                    break;
                case D2Class_F62C8080:
                    PatternAudioUnnamed = resource.ResourceHash;
                    break;
                default:
                    // throw new NotImplementedException($"Implement parsing for {resource.ResourceHash.Header.Unk08}");
                    break;
            }
        }
    }

    public List<DynamicPart> Load(ELOD detailLevel, bool forceLoad = false)
    {
        if(forceLoad) //force the entity to re-parse its data because it's dumb
        {
            ParseStructs(); 
            ParseData();
        }

        var dynamicParts = new List<DynamicPart>();
        if (Model != null)
        {
            dynamicParts = dynamicParts.Concat(Model.Load(detailLevel, ModelParentResource)).ToList();
        }
        if (PhysicsModel != null)
        {
            dynamicParts = dynamicParts.Concat(PhysicsModel.Load(detailLevel, ModelParentResource)).ToList();
        }
        return dynamicParts;
    }

    public void SaveMaterialsFromParts(string saveDirectory, List<DynamicPart> dynamicParts, bool bSaveShaders)
    {
        Directory.CreateDirectory($"{saveDirectory}/Textures");
        Directory.CreateDirectory($"{saveDirectory}/Shaders");
        foreach (var dynamicPart in dynamicParts)
        {
            if (dynamicPart.Material == null) continue;
            dynamicPart.Material.SaveAllTextures($"{saveDirectory}/Textures");
            // dynamicPart.Material.SaveVertexShader(saveDirectory);
            if (bSaveShaders)
            {
                dynamicPart.Material.SavePixelShader($"{saveDirectory}/Shaders");
                dynamicPart.Material.SaveVertexShader($"{saveDirectory}/Shaders");
            }
            // Environment.Exit(5);
        }
    }

    public void SaveTexturePlates(string saveDirectory)
    {
        Directory.CreateDirectory($"{saveDirectory}/Textures/");
        if (((D2Class_8F6D8080) ModelParentResource.Header.Unk18).TexturePlates is null)
        {
            return;
        }
        var rsrc = ((D2Class_8F6D8080) ModelParentResource.Header.Unk18).TexturePlates.Header;
        rsrc.AlbedoPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_albedo");
        rsrc.NormalPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_normal");
        rsrc.GStackPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_gstack");
        rsrc.DyemapPlate.SavePlatedTexture($"{saveDirectory}/Textures/{Hash}_dyemap");
    }

    public static bool HasGeometry(Entity ent) //yoinked from LoadEntityList in TagListView
    { 
        // Check the entity has geometry
        bool bHasGeometry = false;
        using (var handle = ent.GetHandle())//new Tag(PackageHandler.GetTag(typeof(Entity), ent.Hash)).GetHandle())
        {
            handle.BaseStream.Seek(8, SeekOrigin.Begin);
            int resourceCount = handle.ReadInt32();
            if (resourceCount > 2)
            {
                handle.BaseStream.Seek(0x10, SeekOrigin.Begin);
                int resourcesOffset = handle.ReadInt32() + 0x20;
                for (int i = 0; i < 2; i++)
                {
                    handle.BaseStream.Seek(resourcesOffset + i * 0xC, SeekOrigin.Begin);
                    using (var handle2 = new Tag(new TagHash(handle.ReadUInt32())).GetHandle())
                    {
                        handle2.BaseStream.Seek(0x10, SeekOrigin.Begin);
                        int checkOffset = handle2.ReadInt32() + 0x10 - 4;
                        handle2.BaseStream.Seek(checkOffset, SeekOrigin.Begin);
                        if (handle2.ReadUInt32() == 0x80806d8a)
                        {
                            bHasGeometry = true;
                        }
                    }
                }
            }
        }
        return bHasGeometry;
    }
}