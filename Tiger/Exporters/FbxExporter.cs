using System.Diagnostics;
using Internal.Fbx;
using Tiger.Schema;
using Tiger.Schema.Entity;

namespace Tiger.Exporters;

public class FbxExporter : AbstractExporter
{
    private FbxManager _manager = FbxManager.Create();

    /// <summary>
    /// Must be single-threaded
    /// </summary>
    public override void Export(Exporter.ExportEventArgs args)
    {
        foreach (ExporterScene scene in args.Scenes)
        {
            FbxScene fbxScene = FbxScene.Create(_manager, scene.Name);

            foreach (ExporterMesh mesh in scene.StaticMeshes)
            {
                AddMesh(fbxScene, mesh);
            }
            foreach (var meshInstance in scene.StaticMeshInstances)
            {
                Debug.Assert(scene.StaticMeshes.Count(s => s.Hash == meshInstance.Key) == 1);
                AddInstancedMesh(fbxScene, scene.StaticMeshes.First(s => s.Hash == meshInstance.Key).Parts, meshInstance.Value);
            }
            foreach (ExporterMesh mesh in scene.EntityMeshes)
            {
                AddEntityMesh(fbxScene, mesh);
            }

            foreach (var p in scene.EntityPoints)
            {
                AddDynamicPoint(fbxScene, p.Key, p.Value);
            }

            string outputDirectory = args.OutputDirectory;
            if (scene.Type is ExportType.Static or ExportType.Entity)
            {
                outputDirectory = Path.Join(outputDirectory, scene.Name);
            }
            else if (scene.Type is ExportType.Map or ExportType.Terrain or ExportType.EntityPoints)
            {
                outputDirectory = Path.Join(outputDirectory, "Maps");
            }
            else if (scene.Type is ExportType.StaticInMap)
            {
                outputDirectory = Path.Join(outputDirectory, "Maps", "Statics");
            }

            ExportScene(fbxScene, Path.Join(outputDirectory, scene.Name));
        }
    }

    private void AddDynamicPoint(FbxScene fbxScene, string pointName, SMapDataEntry point)
    {
        FbxNode node = FbxNode.Create(_manager, $"{pointName}");
        Vector3 eulerRot = Vector4.QuaternionToEulerAngles(point.Rotation);

        node.LclTranslation.Set(new FbxDouble3(point.Translation.X * 100, point.Translation.Z * 100, -point.Translation.Y * 100));
        node.LclRotation.Set(new FbxDouble3(eulerRot.X, eulerRot.Y, eulerRot.Z));
        node.LclScaling.Set(new FbxDouble3(100, 100, 100));

        // Scale and rotate
        //ScaleAndRotateForBlender(node);

        fbxScene.GetRootNode().AddChild(node);
    }

    private void ExportScene(FbxScene fbxScene, string outputPath)
    {
        // Make directory for file
        string? directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (_manager.GetIOSettings() == null)
        {
            FbxIOSettings ios = FbxIOSettings.Create(_manager, FbxWrapperNative.IOSROOT);
            _manager.SetIOSettings(ios);
        }
        _manager.GetIOSettings().SetBoolProp(FbxWrapperNative.EXP_FBX_MATERIAL, true);
        _manager.GetIOSettings().SetBoolProp(FbxWrapperNative.EXP_FBX_TEXTURE, true);
        _manager.GetIOSettings().SetBoolProp(FbxWrapperNative.EXP_FBX_EMBEDDED, true);
        _manager.GetIOSettings().SetBoolProp(FbxWrapperNative.EXP_FBX_ANIMATION, true);
        _manager.GetIOSettings().SetBoolProp(FbxWrapperNative.EXP_FBX_GLOBAL_SETTINGS, true);
        var exporter = Internal.Fbx.FbxExporter.Create(_manager, "");
        exporter.Initialize(outputPath + ".fbx", -1);  // -1 == detect via extension ie binary not ascii, binary is more space efficient
        exporter.Export(fbxScene);
        exporter.Destroy();
        fbxScene.Clear();
    }

    private void AddMesh(FbxScene fbxScene, ExporterMesh mesh)
    {
        foreach (ExporterPart part in mesh.Parts)
        {
            AddPart(fbxScene, part);
        }
    }

    private void AddEntityMesh(FbxScene fbxScene, ExporterMesh mesh)
    {
        foreach (ExporterPart part in mesh.Parts)
        {
            var dynamicMeshPart = part.MeshPart as DynamicMeshPart;
            AddPart(fbxScene, part);
        }
    }

    private void AddInstancedMesh(FbxScene fbxScene, List<ExporterPart> parts, List<Transform> transforms)
    {
        for (int i = 0; i < parts.Count; i++)
        {
            FbxMesh fbxMesh = AddVerticesAndIndices(parts[i]);
            for (int j = 0; j < transforms.Count; j++)
            {
                Transform transform = transforms[j];
                FbxNode node = FbxNode.Create(_manager, $"{parts[i].Name}_{i}_{j}");

                node.SetNodeAttribute(fbxMesh);

                node.LclTranslation.Set(new FbxDouble3(transform.Position.X, transform.Position.Y, transform.Position.Z));
                node.LclRotation.Set(new FbxDouble3(transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z));
                node.LclScaling.Set(new FbxDouble3(transform.Scale.X, transform.Scale.X, transform.Scale.X));

                fbxScene.GetRootNode().AddChild(node);
            }
        }
    }

    private void AddPart(FbxScene fbxScene, ExporterPart part)
    {
        FbxMesh fbxMesh = AddVerticesAndIndices(part);
        FbxNode node = FbxNode.Create(_manager, fbxMesh.GetName());
        node.SetNodeAttribute(fbxMesh);
        fbxScene.GetRootNode().AddChild(node);

        fbxMesh.AddTexcoords0(part);
        fbxMesh.AddNormals(part);
        fbxMesh.AddTangents(part);
        fbxMesh.AddColours(part);

        if (part.Material != null)
        {
            fbxMesh.AddMaterial(fbxScene, part.Material.FileHash, node, part.Index);
        }

        fbxMesh.AddSmoothing();

        if (part.MeshPart is DynamicMeshPart dynamicMeshPart)
        {
            if (dynamicMeshPart.VertexColourSlots.Count > 0 || dynamicMeshPart.GearDyeChangeColorIndex != 0xFF)
            {
                fbxMesh.AddSlotColours(dynamicMeshPart);
                fbxMesh.AddTexcoords1(dynamicMeshPart);
            }
        }
    }

    private FbxMesh AddVerticesAndIndices(ExporterPart part)
    {
        FbxMesh mesh = FbxMesh.Create(_manager, part.Name);

        // Conversion lookup table
        Dictionary<uint, int> lookup = new();
        for (int i = 0; i < part.MeshPart.VertexIndices.Count; i++)
        {
            lookup[part.MeshPart.VertexIndices[i]] = i;
        }
        foreach (uint vertexIndex in part.MeshPart.VertexIndices)
        {
            // todo utilise dictionary to make this control point thing better maybe?
            var pos = part.MeshPart.VertexPositions[lookup[vertexIndex]];
            mesh.SetControlPointAt(new FbxVector4(pos.X, pos.Y, pos.Z, 1), lookup[vertexIndex]);
        }
        foreach (var face in part.MeshPart.Indices)
        {
            mesh.BeginPolygon();
            mesh.AddPolygon(lookup[face.X]);
            mesh.AddPolygon(lookup[face.Y]);
            mesh.AddPolygon(lookup[face.Z]);
            mesh.EndPolygon();
        }

        mesh.CreateLayer();
        return mesh;
    }
}

public static class FbxMeshExtensions
{
    public static void AddTexcoords0(this FbxMesh fbxMesh, ExporterPart part)
    {
        if (!part.MeshPart.VertexTexcoords0.Any())
        {
            return;
        }

        FbxLayerElementUV uvLayer = FbxLayerElementUV.Create(fbxMesh, "uv0");
        uvLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        uvLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        foreach (var tx in part.MeshPart.VertexTexcoords0)
        {
            uvLayer.GetDirectArray().Add(new FbxVector2(tx.X, tx.Y));
        }
        fbxMesh.GetLayer(0).SetUVs(uvLayer);
    }

    public static void AddTexcoords1(this FbxMesh fbxMesh, DynamicMeshPart meshPart)
    {
        if (!meshPart.VertexTexcoords1.Any())
        {
            return;
        }

        FbxLayerElementUV uvLayer = FbxLayerElementUV.Create(fbxMesh, "uv1");
        uvLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        uvLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        foreach (var tx in meshPart.VertexTexcoords1)
        {
            uvLayer.GetDirectArray().Add(new FbxVector2(tx.X, tx.Y));
        }

        if (fbxMesh.GetLayer(1) == null)
        {
            fbxMesh.CreateLayer();
        }

        fbxMesh.GetLayer(1).SetUVs(uvLayer);
    }

    public static void AddNormals(this FbxMesh fbxMesh, ExporterPart part)
    {
        if (!part.MeshPart.VertexNormals.Any())
        {
            return;
        }

        FbxLayerElementNormal normalsLayer = FbxLayerElementNormal.Create(fbxMesh, "normalLayerName");
        normalsLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        normalsLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        foreach (var normal in part.MeshPart.VertexNormals)
        {
            FbxVector4 vec4;

            if (Strategy.CurrentStrategy > TigerStrategy.DESTINY2_SHADOWKEEP_2999)
            {
                Vector3 euler = Vector4.ConsiderQuatToEulerConvert(normal);
                vec4 = new FbxVector4(euler.X, euler.Y, euler.Z);
            }
            else
            {
                vec4 = new FbxVector4(normal.X, normal.Y, normal.Z);
            }
            normalsLayer.GetDirectArray().Add(vec4);
        }
        fbxMesh.GetLayer(0).SetNormals(normalsLayer);
    }

    public static void AddTangents(this FbxMesh fbxMesh, ExporterPart part)
    {
        if (!part.MeshPart.VertexTangents.Any())
        {
            return;
        }

        FbxLayerElementTangent tangentsLayer = FbxLayerElementTangent.Create(fbxMesh, "tangentLayerName");
        tangentsLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        tangentsLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        // todo more efficient to do AddMultiple
        foreach (var tangent in part.MeshPart.VertexTangents)
        {
            FbxVector4 vec4;

            if (Strategy.CurrentStrategy > TigerStrategy.DESTINY2_SHADOWKEEP_2999)
            {
                Vector3 euler = Vector4.QuaternionToEulerAngles(tangent);
                vec4 = new FbxVector4(euler.X, euler.Y, euler.Z);
            }
            else
            {
                vec4 = new FbxVector4(tangent.X, tangent.Y, tangent.Z);
            }
            tangentsLayer.GetDirectArray().Add(vec4);
        }
        fbxMesh.GetLayer(0).SetTangents(tangentsLayer);
    }

    public static void AddColours(this FbxMesh fbxMesh, ExporterPart part)
    {
        if (!part.MeshPart.VertexColours.Any())
        {
            return;
        }

        FbxLayerElementVertexColor colLayer = FbxLayerElementVertexColor.Create(fbxMesh, "colourLayerName");
        colLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        colLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        foreach (var colour in part.MeshPart.VertexColours)
        {
            colLayer.GetDirectArray().Add(new FbxColor(colour.X, colour.Y, colour.Z, colour.W));
        }
        fbxMesh.GetLayer(0).SetVertexColors(colLayer);
    }

    public static void AddSlotColours(this FbxMesh fbxMesh, DynamicMeshPart meshPart)
    {
        if (!meshPart.VertexColourSlots.Any())
        {
            return;
        }

        FbxLayerElementVertexColor colLayer = FbxLayerElementVertexColor.Create(fbxMesh, "slots");
        colLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        colLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        if (meshPart.PrimitiveType == PrimitiveType.Triangles)
        {
            DynamicMeshPart.AddVertexColourSlotInfo(meshPart, meshPart.GearDyeChangeColorIndex);
            for (int i = 0; i < meshPart.VertexPositions.Count; i++)
            {
                colLayer.GetDirectArray().Add(new FbxColor(meshPart.VertexColourSlots[0].X, meshPart.VertexColourSlots[0].Y, meshPart.VertexColourSlots[0].Z, meshPart.VertexColourSlots[0].W));
            }
        }
        else
        {
            foreach (var colour in meshPart.VertexColourSlots)
            {
                colLayer.GetDirectArray().Add(new FbxColor(colour.X, colour.Y, colour.Z, colour.W));
            }
        }

        if (fbxMesh.GetLayer(1) == null)
        {
            fbxMesh.CreateLayer();
        }

        fbxMesh.GetLayer(1).SetVertexColors(colLayer);
    }

    public static void AddMaterial(this FbxMesh fbxMesh, FbxScene scene, FileHash materialHash, FbxNode fbxNode, int index)
    {
        // todo why scene here?
        FbxSurfacePhong fbxMaterial = FbxSurfacePhong.Create(scene, materialHash.ToString());
        FbxLayerElementMaterial materialLayer =
            FbxLayerElementMaterial.Create(fbxMesh, $"matlayer_{fbxNode.GetName()}_{index}");
        fbxMaterial.DiffuseFactor.Set(1);
        fbxNode.SetShadingMode(FbxNode.EShadingMode.eTextureShading);
        fbxNode.AddMaterial(fbxMaterial);

        // if this doesnt exist, it wont load the material slots in unreal
        materialLayer.SetMappingMode(FbxLayerElement.EMappingMode.eAllSame);
        fbxMesh.GetLayer(0).SetMaterials(materialLayer);
    }

    public static void AddSmoothing(this FbxMesh fbxMesh)
    {
        FbxLayerElementSmoothing smoothingLayer = FbxLayerElementSmoothing.Create(fbxMesh, $"smoothingLayer");
        smoothingLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByEdge);
        smoothingLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);

        FbxArrayInt edges = fbxMesh.mEdgeArray;
        int numEdges = edges.GetCount();
        for (int i = 0; i < numEdges; i++)
        {
            smoothingLayer.GetDirectArray().Add(i);
        }

        fbxMesh.GetLayer(0).SetSmoothing(smoothingLayer);
        fbxMesh.SetMeshSmoothness(FbxMesh.ESmoothness.eFine);
    }
}
