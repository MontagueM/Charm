using System.Diagnostics;
using Internal.Fbx;
using Tiger.Schema;
using Tiger.Schema.Entity;

namespace Tiger.Exporters;

// TODO: Clean this up
public class FbxExporter : AbstractExporter
{
    private readonly FbxManager _manager = FbxManager.Create();
    private ConfigSubsystem _config => ConfigSubsystem.Get();

    /// <summary>
    /// Must be single-threaded
    /// </summary>
    public override void Export(Exporter.ExportEventArgs args)
    {
        foreach (ExporterScene scene in args.Scenes)
        {
            FbxScene fbxScene = FbxScene.Create(_manager, scene.Name);
            string outputDirectory = args.OutputDirectory;
            string modelSubDirectory = $"Models/{scene.Type.ToString()}";

            switch (scene.DataType)
            {
                case DataExportType.Map:
                    if (_config.GetSingleFolderMapAssetsEnabled())
                        outputDirectory = Path.Join($"{_config.GetExportSavePath()}/Maps/Assets", modelSubDirectory);
                    else
                        outputDirectory = Path.Join(outputDirectory, modelSubDirectory);
                    break;
                default:
                    outputDirectory = Path.Join(outputDirectory, scene.Name);
                    break;
            }

            if (scene.Type is ExportType.API or ExportType.D1API)
            {
                if (args.AggregateOutput)
                    outputDirectory = args.OutputDirectory;

                FbxScene apiScene = FbxScene.Create(_manager, scene.Name);
                foreach (ExporterEntity entity in scene.Entities)
                {
                    if (entity.Mesh.Parts.Count == 0)
                        continue;
                    AddEntity(apiScene, entity);
                }
                ExportScene(apiScene, Path.Join(outputDirectory, scene.Name));
                continue;
            }

            foreach (ExporterMesh mesh in scene.StaticMeshes)
            {
                if (mesh.Parts.Count == 0)
                    continue;

                // this is dumb
                string savePath = scene.Type == ExportType.Statics
                && scene.DataType == DataExportType.Individual
                && args.AggregateOutput ? args.OutputDirectory : outputDirectory;

                FbxScene fbxIndivScene = FbxScene.Create(_manager, mesh.Hash);
                AddMesh(fbxIndivScene, mesh);
                ExportScene(fbxIndivScene, Path.Join(savePath, mesh.Hash));

                if (_config.GetSBoxExportEnabled() && scene.Type != ExportType.Terrain)
                {
                    string fbxPath = scene.DataType == DataExportType.Map ? modelSubDirectory : "Models";
                    Source2Handler.SaveStaticVMDL(savePath, fbxPath, mesh);
                }
            }

            foreach (ExporterEntity entity in scene.Entities)
            {
                if (entity.Mesh.Parts.Count == 0)
                    continue;

                // this is dumb
                string savePath = scene.Type == ExportType.Entities
                && scene.DataType == DataExportType.Individual
                && args.AggregateOutput ? args.OutputDirectory : outputDirectory;

                FbxScene fbxIndivScene = FbxScene.Create(_manager, entity.Mesh.Hash);
                AddEntity(fbxIndivScene, entity);
                ExportScene(fbxIndivScene, Path.Join(savePath, entity.Mesh.Hash));

                if (_config.GetSBoxExportEnabled() && scene.Type != ExportType.Terrain)
                {
                    string fbxPath = scene.DataType == DataExportType.Map ? modelSubDirectory : "Models";
                    Source2Handler.SaveEntityVMDL(savePath, fbxPath, entity);
                }
            }

            foreach (ExporterMesh mesh in scene.TerrainMeshes)
            {
                if (mesh.ID != null)
                    AddVertexAO(mesh, (ulong)mesh.ID);

                FbxScene fbxIndivScene = FbxScene.Create(_manager, $"{mesh.Hash}_{mesh.Index}");
                AddMesh(fbxIndivScene, mesh);
                ExportScene(fbxIndivScene, Path.Join(outputDirectory, $"{mesh.Hash}_{mesh.Index}"));
            }

            //foreach (var meshInstance in scene.ArrangedStaticMeshInstances)
            //{
            //    AddInstancedMesh(fbxScene, scene.StaticMeshes.First(s => s.Hash == meshInstance.Key).Parts, meshInstance.Value);
            //}

            //ExportScene(fbxScene, Path.Join(outputDirectory, scene.Name));
        }
    }

    public void AddVertexAO(ExporterMesh mesh, ulong identifier)
    {
        if (!Exporter.Get().GetOrCreateGlobalScene().TryGetItem<SStaticAOResource>(out SStaticAOResource mapAOHash))
            return;

        Tag<SStaticAmbientOcclusion> mapAO = FileResourcer.Get().GetSchemaTag<SStaticAmbientOcclusion>(mapAOHash.MapAO);
        uint offset = mapAO.TagData.AO_1.Value.Mappings.First(x => x.Identifier == identifier).Offset;
        using TigerReader handle = mapAO.TagData.AO_1.Value.Buffer.GetReferenceReader();

        foreach (MeshPart? part in mesh.Parts.Select(x => x.MeshPart))
        {
            for (int i = 0; i < part.VertexIndices.Count; i++)
            {
                int index = (int)part.VertexIndices[i];
                handle.BaseStream.Seek(index + offset, SeekOrigin.Begin);
                byte value = handle.ReadByte();
                part.VertexAO.Add(new Vector4((byte)1, (byte)1, (byte)1, value));
            }
        }

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
        if (fbxScene.GetRootNode().GetChildCount() > 0) // Only export if theres actually something to export
            exporter.Export(fbxScene);
        exporter.Destroy();
        fbxScene.Clear();
    }

    private void AddMesh(FbxScene fbxScene, ExporterMesh mesh)
    {
        foreach (ExporterPart part in mesh.Parts)
        {
            if (part.Material != null)
                AddPart(fbxScene, part);
        }
    }

    private void AddEntity(FbxScene fbxScene, ExporterEntity entity)
    {
        List<FbxNode> skeletonNodes = entity.BoneNodes == null ? new() : AddSkeleton(fbxScene, entity.BoneNodes);
        foreach (ExporterPart part in entity.Mesh.Parts)
        {
            var dynamicMeshPart = part.MeshPart as DynamicMeshPart;

            FbxMesh fbxMesh = AddPart(fbxScene, part, true);

            if (dynamicMeshPart.VertexColourSlots.Count > 0 || dynamicMeshPart.GearDyeChangeColorIndex != 0xFF)
            {
                fbxMesh.AddSlotColours(dynamicMeshPart);
                fbxMesh.AddTexcoords1(part);
            }

            if (dynamicMeshPart.VertexWeights.Count > 0 && skeletonNodes.Count > 0)
            {
                Debug.Assert(dynamicMeshPart.VertexWeights.Count == dynamicMeshPart.VertexPositions.Count);
                AddWeightsToMesh(fbxMesh, dynamicMeshPart, skeletonNodes);
            }
        }
    }

    private List<FbxNode> AddSkeleton(FbxScene fbxScene, List<BoneNode> boneNodes)
    {
        FbxNode rootNode = null;
        List<FbxNode> skeletonNodes = new();
        List<BoneNode> nodes = new();
        foreach (BoneNode boneNode in boneNodes)
        {
            BoneNode newNode = boneNode;
            FbxSkeleton skeleton = FbxSkeleton.Create(_manager, boneNode.HashString);
            newNode.Node = FbxNode.Create(_manager, boneNode.HashString);
            skeleton.SetSkeletonType(FbxSkeleton.EType.eLimbNode);
            newNode.Node.SetNodeAttribute(skeleton);
            Vector3 location = boneNode.DefaultObjectSpaceTransform.Translation;
            if (boneNode.ParentNodeIndex != -1)
            {
                location -= boneNodes[newNode.ParentNodeIndex].DefaultObjectSpaceTransform.Translation;
            }
            newNode.Node.LclTranslation.Set(new FbxDouble3(location.X, location.Y, location.Z));
            nodes.Add(newNode);
        }

        foreach (BoneNode node in nodes)
        {
            if (node.ParentNodeIndex != -1)
                nodes[node.ParentNodeIndex].Node.AddChild(node.Node);
            else
            {
                FbxSkeleton nodeatt = FbxSkeleton.Create(_manager, node.HashString);
                nodeatt.SetSkeletonType(FbxSkeleton.EType.eRoot);
                rootNode = FbxNode.Create(_manager, "Armature");
                rootNode.AddChild(node.Node);
                rootNode.SetNodeAttribute(nodeatt);
            }
            if (rootNode != null)
                fbxScene.GetRootNode().AddChild(rootNode);
            else
                skeletonNodes[node.ParentNodeIndex].AddChild(node.Node);
            skeletonNodes.Add(node.Node);
        }
        return skeletonNodes;
    }

    private void AddWeightsToMesh(FbxMesh mesh, DynamicMeshPart meshPart, List<FbxNode> skeletonNodes)
    {
        FbxSkin skin = FbxSkin.Create(_manager, "skinName");
        HashSet<int> seen = new();

        List<FbxCluster> weightClusters = new();
        foreach (FbxNode node in skeletonNodes)
        {
            FbxCluster weightCluster = FbxCluster.Create(_manager, node.GetName());
            weightCluster.SetLink(node);
            weightCluster.SetLinkMode(FbxCluster.ELinkMode.eTotalOne);
            FbxAMatrix transform = node.EvaluateGlobalTransform();
            weightCluster.SetTransformLinkMatrix(transform);
            weightClusters.Add(weightCluster);
        }

        // Conversion lookup table
        Dictionary<uint, int> lookup = new();
        for (int i = 0; i < meshPart.VertexIndices.Count; i++)
        {
            lookup[meshPart.VertexIndices[i]] = i;
        }
        foreach (uint v in meshPart.VertexIndices)
        {
            VertexWeight vw = meshPart.VertexWeights[lookup[v]];
            for (int j = 0; j < 4; j++)
            {
                if (vw.WeightValues[j] != 0)
                {
                    if (vw.WeightIndices[j] < weightClusters.Count)
                    {
                        seen.Add(vw.WeightIndices[j]);
                        weightClusters[vw.WeightIndices[j]].AddControlPointIndex(lookup[v], (float)vw.WeightValues[j] / 255);
                    }
                }
            }
        }

        foreach (FbxCluster c in weightClusters)
        {
            skin.AddCluster(c);
        }

        mesh.AddDeformer(skin);
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

    private FbxMesh AddPart(FbxScene fbxScene, ExporterPart part, bool isEntity = false)
    {
        FbxMesh fbxMesh = AddVerticesAndIndices(part);
        FbxNode node = FbxNode.Create(_manager, fbxMesh.GetName());
        node.SetNodeAttribute(fbxMesh);
        fbxScene.GetRootNode().AddChild(node);

        fbxMesh.AddTexcoords0(part);
        fbxMesh.AddNormals(part, isEntity);
        fbxMesh.AddTangents(part);
        fbxMesh.AddColours(part);
        fbxMesh.AddVertexAO(part);
        fbxMesh.AddExtraData(part);

        if (part.Material != null)
        {
            fbxMesh.AddMaterial(fbxScene, part.Material.Hash, node, part.Index);
        }

        fbxMesh.AddSmoothing();

        return fbxMesh;
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
            Vector4 pos = part.MeshPart.VertexPositions[lookup[vertexIndex]];
            mesh.SetControlPointAt(new FbxVector4(pos.X, pos.Y, pos.Z, 1), lookup[vertexIndex]);
        }
        foreach (UIntVector3 face in part.MeshPart.Indices)
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
        foreach (Vector2 tx in part.MeshPart.VertexTexcoords0)
        {
            uvLayer.GetDirectArray().Add(new FbxVector2(tx.X, tx.Y));
        }
        fbxMesh.GetLayer(0).SetUVs(uvLayer);
    }

    public static void AddTexcoords1(this FbxMesh fbxMesh, ExporterPart part)
    {
        if (!part.MeshPart.VertexTexcoords1.Any())
        {
            return;
        }

        FbxLayerElementUV uvLayer = FbxLayerElementUV.Create(fbxMesh, "uv1");
        uvLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        uvLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        foreach (Vector2 tx in part.MeshPart.VertexTexcoords1)
        {
            uvLayer.GetDirectArray().Add(new FbxVector2(tx.X, tx.Y));
        }

        if (fbxMesh.GetLayer(1) == null)
        {
            fbxMesh.CreateLayer();
        }

        fbxMesh.GetLayer(1).SetUVs(uvLayer);
    }

    public static void AddNormals(this FbxMesh fbxMesh, ExporterPart part, bool isEntity = false)
    {
        if (!part.MeshPart.VertexNormals.Any())
        {
            return;
        }

        FbxLayerElementNormal normalsLayer = FbxLayerElementNormal.Create(fbxMesh, "normalLayerName");
        normalsLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        normalsLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        foreach (Vector4 normal in part.MeshPart.VertexNormals)
        {
            FbxVector4 vec4;

            if (Strategy.CurrentStrategy > TigerStrategy.DESTINY2_SHADOWKEEP_2999)
            {
                Vector3 euler = part.MeshPart is DynamicMeshPart ? new Vector3(normal.X, normal.Y, normal.Z) : Vector4.ConsiderQuatToEulerConvert(normal);
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
        foreach (Vector4 tangent in part.MeshPart.VertexTangents)
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
        foreach (Vector4 colour in part.MeshPart.VertexColours)
        {
            colLayer.GetDirectArray().Add(new FbxColor(colour.X, colour.Y, colour.Z, colour.W));
        }
        fbxMesh.GetLayer(0).SetVertexColors(colLayer);
    }

    public static void AddExtraData(this FbxMesh fbxMesh, ExporterPart part)
    {
        if (!part.MeshPart.VertexExtraData.Any())
        {
            return;
        }

        foreach (KeyValuePair<int, List<Vector4>> entry in part.MeshPart.VertexExtraData)
        {
            FbxLayerElementVertexColor dataLayer = FbxLayerElementVertexColor.Create(fbxMesh, $"data{entry.Key}");
            dataLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
            dataLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
            foreach (Vector4 data in entry.Value)
            {
                dataLayer.GetDirectArray().Add(new FbxColor(data.X, data.Y, data.Z, data.W));
            }

            if (fbxMesh.GetLayer(entry.Key + 1) == null)
            {
                fbxMesh.CreateLayer();
                fbxMesh.GetLayer(entry.Key + 1).SetVertexColors(dataLayer);
            }
        }
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
            foreach (Vector4 colour in meshPart.VertexColourSlots)
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

    public static void AddVertexAO(this FbxMesh fbxMesh, ExporterPart part)
    {
        if (!part.MeshPart.VertexAO.Any())
            return;

        FbxLayerElementVertexColor colLayer = FbxLayerElementVertexColor.Create(fbxMesh, "VertexAO");
        colLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        colLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        foreach (Vector4 colour in part.MeshPart.VertexAO)
        {
            colLayer.GetDirectArray().Add(new FbxColor(colour.X, colour.Y, colour.Z, colour.W));
        }

        if (fbxMesh.GetLayer(1) == null)
            fbxMesh.CreateLayer();

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
