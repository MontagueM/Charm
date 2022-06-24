using Autodesk.Fbx;
using Field.Entities;
using Field.Models;
using Field.Textures;

namespace Field.General;


public class FbxHandler
{
    private static FbxManager _manager;
    private static FbxScene _scene;

    public static void Initialise()
    {
        _manager = FbxManager.Create();
        _scene = FbxScene.Create(_manager, "");
    }

    public static FbxMesh AddMeshPartToScene(DynamicPart part, int index)
    {
        FbxMesh mesh = CreateMeshPart(part);
        FbxNode node = FbxNode.Create(_manager, mesh.GetName());
        node.SetNodeAttribute(mesh);

        if (part.VertexNormals.Count > 0)
        {
            AddNormalsToMesh(mesh, part);
        }

        if (part.VertexTexcoords.Count > 0)
        {
            AddTexcoordsToMesh(mesh, part);
        }

        if (part.VertexColours.Count > 0)
        {
            AddColoursToMesh(mesh, part);
        }

        // for importing to other engines
        if (InfoConfigHandler.bOpen)
        {
            InfoConfigHandler.AddMaterial(part.Material);
            InfoConfigHandler.AddPart(part, node.GetName());   
        }


        AddMaterial(mesh, node, index);

        _scene.GetRootNode().AddChild(node);
        return mesh;
    }

    private static FbxMesh CreateMeshPart(DynamicPart part)
    {
        FbxMesh mesh = FbxMesh.Create(_manager, $"{part.IndexOffset}_{part.IndexCount}_{part.DetailLevel}");
        // Conversion lookup table
        Dictionary<int, int> lookup = new Dictionary<int, int>();
        for (int i = 0; i < part.VertexIndices.Count; i++)
        {
            lookup[(int)part.VertexIndices[i]] = i;
        }
        foreach (int vertexIndex in part.VertexIndices)
        {
            // todo utilise dictionary to make this control point thing better maybe?
            var pos = part.VertexPositions[lookup[vertexIndex]];
            mesh.SetControlPointAt(new FbxVector4(pos.X, pos.Y, pos.Z, 1), lookup[vertexIndex]);
        }
        foreach (var face in part.Indices)
        {
            mesh.BeginPolygon();
            mesh.AddPolygon(lookup[(int)face.X]);
            mesh.AddPolygon(lookup[(int)face.Y]);
            mesh.AddPolygon(lookup[(int)face.Z]);
            mesh.EndPolygon();
        }

        mesh.CreateLayer();
        return mesh;
    }

    private static void AddNormalsToMesh(FbxMesh mesh, DynamicPart part)
    {
        FbxLayerElementNormal normalsLayer = FbxLayerElementNormal.Create(mesh, "normalLayerName");
        normalsLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        normalsLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        foreach (var normal in part.VertexNormals)
        {
            normalsLayer.GetDirectArray().Add(new FbxVector4(normal.X, normal.Y, normal.Z));
        }
        mesh.GetLayer(0).SetNormals(normalsLayer);
    }

    
    private static void AddTexcoordsToMesh(FbxMesh mesh, DynamicPart part)
    {
        FbxLayerElementUV uvLayer = FbxLayerElementUV.Create(mesh, "uvLayerName");
        uvLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        uvLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        foreach (var tx in part.VertexTexcoords)
        {
            uvLayer.GetDirectArray().Add(new FbxVector2(tx.X, tx.Y));
        }
        mesh.GetLayer(0).SetUVs(uvLayer);
    }
    
    
    private static void AddColoursToMesh(FbxMesh mesh, DynamicPart part)
    {
        FbxLayerElementVertexColor colLayer = FbxLayerElementVertexColor.Create(mesh, "colourLayerName");
        colLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        colLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        foreach (var colour in part.VertexColours)
        {
            colLayer.GetDirectArray().Add(new FbxVector4(colour.X, colour.Y, colour.Z, colour.W));
        }
        mesh.GetLayer(0).SetVertexColors(colLayer);
    }


    private static void AddWeightsToMesh(FbxMesh mesh, DynamicPart part, List<FbxNode> skeletonNodes)
    {
        FbxSkin skin = FbxSkin.Create(_manager, "skinName");
        HashSet<int> seen = new HashSet<int>();
        
        List<FbxCluster> weightClusters = new List<FbxCluster>();
        foreach (var node in skeletonNodes)
        {
            FbxCluster weightCluster = FbxCluster.Create(_manager, node.GetName());
            weightCluster.SetLink(node);
            weightCluster.SetLinkMode(FbxCluster.ELinkMode.eTotalOne);
            FbxAMatrix transform = node.EvaluateGlobalTransform();
            weightCluster.SetTransformLinkMatrix(transform);
            weightClusters.Add(weightCluster);
        }
        
        // for (int i = 0; i < part.VertexWeights.Count; i++)
        // Conversion lookup table
        Dictionary<int, int> lookup = new Dictionary<int, int>();
        for (int i = 0; i < part.VertexIndices.Count; i++)
        {
            lookup[(int)part.VertexIndices[i]] = i;
        }
        foreach (int v in part.VertexIndices)
        {
            VertexWeight vw = part.VertexWeights[lookup[v]];
            for (int j = 0; j < 4; j++)
            {
                if (vw.WeightValues[j] != 0)
                {
                    if (vw.WeightIndices[j] < weightClusters.Count)
                    {
                        seen.Add(vw.WeightIndices[j]);
                        weightClusters[vw.WeightIndices[j]].AddControlPointIndex(lookup[v], (float)vw.WeightValues[j]/255);
                    }
                }
            }
        }
        
        foreach (var c in weightClusters)
        {
            skin.AddCluster(c);
        }
        
        mesh.AddDeformer(skin);
    }

    private static void AddMaterial(FbxMesh mesh, FbxNode node, int index)
    {
        FbxSurfacePhong fbxMaterial = FbxSurfacePhong.Create(_scene, $"{node.GetName()}_{index}");
        fbxMaterial.ShadingModel.Set("Phong");
        fbxMaterial.DiffuseFactor.Set(1);
        node.SetShadingMode(FbxNode.EShadingMode.eTextureShading);
        node.AddMaterial(fbxMaterial);

        // if this doesnt exist, it wont load the material slots in unreal
        FbxLayerElementMaterial materialLayer = FbxLayerElementMaterial.Create(mesh, $"matlayer_{node.GetName()}_{index}");
        materialLayer.SetMappingMode(FbxLayerElement.EMappingMode.eAllSame);
        mesh.GetLayer(0).SetMaterials(materialLayer);
    }

    public static List<FbxNode> AddSkeleton(List<BoneNode> boneNodes)
    {
        FbxNode rootNode = null;
        List<FbxNode> skeletonNodes = new List<FbxNode>();
        foreach (var boneNode in boneNodes)
        {
            FbxSkeleton skeleton = FbxSkeleton.Create(_manager, boneNode.Hash.ToString());
            skeleton.SetSkeletonType(FbxSkeleton.EType.eLimbNode);
            FbxNode node = FbxNode.Create(_manager, boneNode.Hash.ToString());
            node.SetNodeAttribute(skeleton);
            Vector3 location = boneNode.DefaultObjectSpaceTransform.Translation;
            if (boneNode.ParentNodeIndex != -1)
            {
                location -= boneNodes[boneNode.ParentNodeIndex].DefaultObjectSpaceTransform.Translation;
            }
            node.LclTranslation.Set(new FbxDouble3(location.X, location.Y, location.Z));
            if (rootNode == null)
            {
                skeleton.SetSkeletonType(FbxSkeleton.EType.eRoot);
                rootNode = node;
            }
            else
            {
                skeletonNodes[boneNode.ParentNodeIndex].AddChild(node);
            }
            skeletonNodes.Add(node);
        }

        _scene.GetRootNode().AddChild(rootNode);
        // rootNode.LclRotation.Set(new FbxDouble3(-90, 0, 0));
        return skeletonNodes;
    }

    public static void ExportScene(string fileName)
    {
        if (_manager.GetIOSettings() == null)
        {
            FbxIOSettings ios = FbxIOSettings.Create(_manager, Globals.IOSROOT);
            _manager.SetIOSettings(ios);
        }
        _manager.GetIOSettings().SetBoolProp(Globals.EXP_FBX_MATERIAL, true);
        _manager.GetIOSettings().SetBoolProp(Globals.EXP_FBX_TEXTURE, true);
        _manager.GetIOSettings().SetBoolProp(Globals.EXP_FBX_EMBEDDED, true);
        _manager.GetIOSettings().SetBoolProp(Globals.EXP_FBX_ANIMATION, true);
        _manager.GetIOSettings().SetBoolProp(Globals.EXP_FBX_GLOBAL_SETTINGS, true);
        FbxExporter exporter = FbxExporter.Create(_manager, "");
        exporter.Initialize(fileName, -1);  // -1 == use binary not ascii, binary is more space efficient
        exporter.Export(_scene);
        exporter.Destroy();
    }

    public static void AddEntityToScene(Entity entity, List<DynamicPart> dynamicParts, ELOD detailLevel)
    {
        // _scene.GetRootNode().LclRotation.Set(new FbxDouble3(90, 0, 0));
        List<FbxNode> skeletonNodes = new List<FbxNode>();
        if (entity.Skeleton != null)
        {
            skeletonNodes = AddSkeleton(entity.Skeleton.GetBoneNodes());
        }
        for( int i = 0; i < dynamicParts.Count; i++)
        {
            var dynamicPart = dynamicParts[i];
            FbxMesh mesh = AddMeshPartToScene(dynamicPart, i);
            
            if (dynamicPart.VertexWeights.Count > 0)
            {
                if (skeletonNodes.Count > 0)
                {
                    AddWeightsToMesh(mesh, dynamicPart, skeletonNodes);
                }
            }
        }
    }

    public static void Clear()
    {
        _scene.Clear();
    }
}