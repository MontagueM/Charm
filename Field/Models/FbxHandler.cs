using System.Runtime.InteropServices;
using Field.Entities;
using Field.General;
using Field.Statics;
using Internal.Fbx;

namespace Field.Models;


public class FbxHandler
{
    private FbxManager _manager;
    private FbxScene _scene;
    public InfoConfigHandler InfoHandler;
    private static object _fbxLock = new object();
    public List<FbxNode> _globalSkeletonNodes = new List<FbxNode>();  // used for attaching all models to one skeleton
    public List<BoneNode> _globalBoneNodes = new List<BoneNode>();
    public FbxHandler(bool bMakeInfoHandler=true)
    {
        lock (_fbxLock) // bc fbx is not thread-safe
        {
            _manager = FbxManager.Create();
            _scene = FbxScene.Create(_manager, "");
        }

        if (bMakeInfoHandler)
            InfoHandler = new InfoConfigHandler();
    }

    public FbxMesh AddMeshPartToScene(Part part, int index, string meshName)
    {
        FbxMesh mesh = CreateMeshPart(part, index, meshName);
        FbxNode node;
        lock (_fbxLock)
        {
            node = FbxNode.Create(_manager, mesh.GetName());
        }
        node.SetNodeAttribute(mesh);

        if (part.VertexNormals.Count > 0)
        {
            AddNormalsToMesh(mesh, part);
        }
        
        if (part.VertexTangents.Count > 0)
        {
            AddTangentsToMesh(mesh, part);
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
        if (InfoHandler != null && part.Material != null) // todo consider why some materials are null
        {
            InfoHandler.AddMaterial(part.Material);
            InfoHandler.AddPart(part, node.GetName());   
        }


        AddMaterial(mesh, node, index);
        AddSmoothing(mesh);

        lock (_fbxLock)
        {
            _scene.GetRootNode().AddChild(node);
        }
        return mesh;
    }

    private FbxMesh CreateMeshPart(Part part, int index, string meshName)
    {
        bool done = false;
        FbxMesh mesh;
        lock (_fbxLock)
        {
            mesh = FbxMesh.Create(_manager, $"{meshName}_Group{part.GroupIndex}_{index}");
        }

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

    private void AddNormalsToMesh(FbxMesh mesh, Part part)
    {
        FbxLayerElementNormal normalsLayer;
        lock (_fbxLock)
        {
            normalsLayer = FbxLayerElementNormal.Create(mesh, "normalLayerName");
        }
        normalsLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        normalsLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        // Check if quaternion
        foreach (var normal in part.VertexNormals)
        {
            normalsLayer.GetDirectArray().Add(new FbxVector4(normal.X, normal.Y, normal.Z));
        }
        mesh.GetLayer(0).SetNormals(normalsLayer);
    }
    
    private void AddTangentsToMesh(FbxMesh mesh, Part part)
    {
        FbxLayerElementTangent tangentsLayer;
        lock (_fbxLock)
        {
            tangentsLayer = FbxLayerElementTangent.Create(mesh, "tangentLayerName");
        }
        tangentsLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        tangentsLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        // todo more efficient to do AddMultiple
        foreach (var tangent in part.VertexTangents)
        {
            tangentsLayer.GetDirectArray().Add(new FbxVector4(tangent.X, tangent.Y, tangent.Z));
        }
        mesh.GetLayer(0).SetTangents(tangentsLayer);
    }

    
    private void AddTexcoordsToMesh(FbxMesh mesh, Part part)
    {
        FbxLayerElementUV uvLayer;
        lock (_fbxLock)
        {
            uvLayer = FbxLayerElementUV.Create(mesh, "uvLayerName");
        }
        uvLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        uvLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        foreach (var tx in part.VertexTexcoords)
        {
            uvLayer.GetDirectArray().Add(new FbxVector2(tx.X, tx.Y));
        }
        mesh.GetLayer(0).SetUVs(uvLayer);
    }
    
    
    private void AddColoursToMesh(FbxMesh mesh, Part part)
    {
        FbxLayerElementVertexColor colLayer;
        lock (_fbxLock)
        {
            colLayer = FbxLayerElementVertexColor.Create(mesh, "colourLayerName");
        }
        colLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
        colLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
        foreach (var colour in part.VertexColours)
        {
            colLayer.GetDirectArray().Add(new FbxColor(colour.X, colour.Y, colour.Z, colour.W));
        }
        mesh.GetLayer(0).SetVertexColors(colLayer);
    }

    /// <summary>
    /// Bind pose uses global transforms?
    /// </summary>
    private void AddBindPose(List<FbxNode> clusterNodes)
    {
        FbxPose pose = FbxPose.Create(_scene, "bindPoseName");
        pose.SetIsBindPose(true);
        
        for (int i = 0; i < clusterNodes.Count; i++)
        {
            // Setting the global transform for each cluster (but really its node link)
            var node = clusterNodes[i];
            var boneNode = _globalBoneNodes[i];
            // setting the bind matrix from DOST
            FbxMatrix bindMatrix = new FbxMatrix();
            bindMatrix.SetIdentity();
            bindMatrix.SetTQS(
                boneNode.DefaultObjectSpaceTransform.Translation.ToFbxVector4(),
                boneNode.DefaultObjectSpaceTransform.QuaternionRotation.ToFbxQuaternion(),
                new FbxVector4(boneNode.DefaultObjectSpaceTransform.Scale, boneNode.DefaultObjectSpaceTransform.Scale, boneNode.DefaultObjectSpaceTransform.Scale)
                );
            pose.Add(node, bindMatrix);
        }
        
        _scene.AddPose(pose);
    }

    private void AddWeightsToMesh(FbxMesh mesh, DynamicPart part, List<FbxNode> skeletonNodes)
    {
        FbxSkin skin;
        lock (_fbxLock)
        {
            skin = FbxSkin.Create(_manager, "skinName");
        }
        HashSet<int> seen = new HashSet<int>();
        
        List<FbxCluster> weightClusters = new List<FbxCluster>();
        foreach (var node in skeletonNodes)
        {
            FbxCluster weightCluster;
            lock (_fbxLock)
            {
                weightCluster = FbxCluster.Create(_manager, node.GetName());
            }
            weightCluster.SetLink(node);
            weightCluster.SetLinkMode(FbxCluster.ELinkMode.eTotalOne);
            FbxAMatrix transform = node.EvaluateGlobalTransform(); // dodgy?
            weightCluster.SetTransformLinkMatrix(transform);
            
            
            
            weightClusters.Add(weightCluster);
        }
        
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
    
    private FbxAMatrix GetGeometry(FbxNode pNode)
    {
        FbxVector4 lT = pNode.GetGeometricTranslation(FbxNode.EPivotSet.eSourcePivot);
        FbxVector4 lR = pNode.GetGeometricRotation(FbxNode.EPivotSet.eSourcePivot);
        FbxVector4 lS = pNode.GetGeometricScaling(FbxNode.EPivotSet.eSourcePivot);

        return new FbxAMatrix(lT, lR, lS);
    }

    private void AddMaterial(FbxMesh mesh, FbxNode node, int index)
    {
        FbxSurfacePhong fbxMaterial;
        FbxLayerElementMaterial materialLayer;
        lock (_fbxLock)
        {
            fbxMaterial = FbxSurfacePhong.Create(_scene, $"{node.GetName()}_{index}");
            materialLayer = FbxLayerElementMaterial.Create(mesh, $"matlayer_{node.GetName()}_{index}");
        }
        fbxMaterial.DiffuseFactor.Set(1);
        node.SetShadingMode(FbxNode.EShadingMode.eTextureShading);
        node.AddMaterial(fbxMaterial);

        // if this doesnt exist, it wont load the material slots in unreal
        materialLayer.SetMappingMode(FbxLayerElement.EMappingMode.eAllSame);
        mesh.GetLayer(0).SetMaterials(materialLayer);
    }

    private void AddSmoothing(FbxMesh mesh)
    {
        FbxLayerElementSmoothing smoothingLayer;
        lock (_fbxLock)
        {
            smoothingLayer = FbxLayerElementSmoothing.Create(mesh, $"smoothingLayerName");
        }
        smoothingLayer.SetMappingMode(FbxLayerElement.EMappingMode.eByEdge);
        smoothingLayer.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);

        FbxArrayInt edges = mesh.mEdgeArray;
        List<int> sharpEdges = new List<int>();
        var numEdges = edges.GetCount();
        for (int i = 0; i < numEdges; i++)
        {
            smoothingLayer.GetDirectArray().Add(i);
        }
        
        mesh.GetLayer(0).SetSmoothing(smoothingLayer);
        
        mesh.SetMeshSmoothness(FbxMesh.ESmoothness.eFine);
    }

    public List<FbxNode> AddSkeleton(List<BoneNode> boneNodes)
    {
        FbxNode rootNode = null;
        List<FbxNode> skeletonNodes = new List<FbxNode>();
        foreach (var boneNode in boneNodes)
        {
            FbxSkeleton skeleton;
            FbxNode node;
            lock (_fbxLock)
            {
                skeleton = FbxSkeleton.Create(_manager, boneNode.Hash.ToString());
                node = FbxNode.Create(_manager, boneNode.Hash.ToString());
            }
            skeleton.SetSkeletonType(FbxSkeleton.EType.eLimbNode);
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
    
    public List<FbxNode> MakeFbxSkeletonHierarchy(List<BoneNode> boneNodes)
    {
        var jointNodes = new List<FbxNode>();
        
        for (int i = 0; i < boneNodes.Count; i++)
        {
            var node = boneNodes[i];
            
            FbxSkeleton skeleton;
            FbxNode joint;
            lock (_fbxLock)
            {
                skeleton = FbxSkeleton.Create(_manager, node.Hash.ToString());
                joint = FbxNode.Create(_manager, node.Hash.ToString());
            }
            skeleton.SetSkeletonType(FbxSkeleton.EType.eLimbNode);
            joint.SetNodeAttribute(skeleton);
            jointNodes.Add(joint);

            if (node.ParentNodeIndex >= 0)
            {
                var parentNode = jointNodes[node.ParentNodeIndex];
                parentNode.AddChild(joint);
            }
            else
            {
                lock (_fbxLock)
                {
                    FbxSkeleton rootNodeSkeleton;
                    FbxNode rootNode;
                    rootNodeSkeleton = FbxSkeleton.Create(_manager, node.Hash.ToString());
                    rootNode = FbxNode.Create(_manager, node.Hash.ToString());
                    rootNode.AddChild(joint);
                    rootNode.SetNodeAttribute(rootNodeSkeleton);
                    _scene.GetRootNode().AddChild(rootNode);
                }
            }
            
            // Set the transform
            FbxAMatrix globalTransform = joint.EvaluateGlobalTransform();
            FbxAMatrix objectSpaceTransform = new FbxAMatrix();
            objectSpaceTransform.SetIdentity();
            objectSpaceTransform.SetT(node.DefaultObjectSpaceTransform.Translation.ToFbxVector4());
            objectSpaceTransform.SetQ(node.DefaultObjectSpaceTransform.QuaternionRotation.ToFbxQuaternion());

            FbxAMatrix localTransform = globalTransform.Inverse().mul(objectSpaceTransform);
            var localTranslation = localTransform.GetT();
            var localRotation = localTransform.GetR();
            joint.LclTranslation.Set(localTranslation.ToDouble3());
            joint.LclRotation.Set(localRotation.ToDouble3());
        }

        return jointNodes;
    }


    public void ExportScene(string fileName)
    {
        // Make directory for file
        string directory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        lock (_fbxLock)
        {
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
            var exporter = FbxExporter.Create(_manager, "");
            exporter.Initialize(fileName, -1);  // -1 == use binary not ascii, binary is more space efficient
            exporter.Export(_scene);
            exporter.Destroy();
        }
        _scene.Clear();
        if (InfoHandler != null)
            InfoHandler.WriteToFile(directory);

    }

    public void AddEntityToScene(Entity entity, List<DynamicPart> dynamicParts, ELOD detailLevel, Animation animation=null)
    {
        List<FbxNode> skeletonNodes = new List<FbxNode>();
        if (entity.Skeleton != null)
        {
            // skeletonNodes = AddSkeleton(entity.Skeleton.GetBoneNodes());
            skeletonNodes = MakeFbxSkeletonHierarchy(entity.Skeleton.GetBoneNodes());
        }
        for( int i = 0; i < dynamicParts.Count; i++)
        {
            var dynamicPart = dynamicParts[i];
            FbxMesh mesh = AddMeshPartToScene(dynamicPart, i, entity.Hash);
            
            if (dynamicPart.VertexWeights.Count > 0)
            {
                if (skeletonNodes.Count > 0)
                {
                    AddWeightsToMesh(mesh, dynamicPart, skeletonNodes);
                }
                else if (_globalSkeletonNodes.Count > 0)
                {
                    AddWeightsToMesh(mesh, dynamicPart, _globalSkeletonNodes);
                    AddBindPose(_globalSkeletonNodes);
                }
            }
        }
        if (animation != null)
            AddAnimationToEntity(animation, skeletonNodes);
    }

    public void AddStaticToScene(List<Part> parts, string meshName)
    {
        for( int i = 0; i < parts.Count; i++)
        {
            Part part = parts[i];
            AddMeshPartToScene(part, i, meshName);
        }
    }
    
    public void AddAnimationToEntity(Animation animation, List<FbxNode> skeletonNodes)
    {
        animation.Load();
        

        FbxAnimStack animStack;
        FbxAnimLayer animLayer;
        FbxTime time;
        lock (_fbxLock)
        {
            animStack = FbxAnimStack.Create(_scene, "animStackName");
            animLayer = FbxAnimLayer.Create(_scene, "animLayerName");
            time = new FbxTime();
            animStack.AddMember(animLayer);        
        }
        string[] dims = { "X", "Y", "Z" };
        foreach (var track in animation.Tracks)
        {
            var scale = dims.Select(x => skeletonNodes[track.TrackIndex].LclScaling.GetCurve(animLayer, x, true)).ToList();
            var rotation = dims.Select(x => skeletonNodes[track.TrackIndex].LclRotation.GetCurve(animLayer, x, true)).ToList();
            var translation = dims.Select(x => skeletonNodes[track.TrackIndex].LclTranslation.GetCurve(animLayer, x, true)).ToList();

            scale.ForEach(x => x.KeyModifyBegin());
            rotation.ForEach(x => x.KeyModifyBegin());
            translation.ForEach(x => x.KeyModifyBegin());

            for (int d = 0; d < dims.Length; d++)
            {
                for (int i = 0; i < track.TrackTimes.Count; i++)
                {
                    float frameTime = track.TrackTimes[i];
                    time.SetSecondDouble(frameTime);
                    
                    var scaleKeyIndex = scale[d].KeyAdd(time);
                    scale[d].KeySetValue(scaleKeyIndex, track.TrackScales[i]);
                    scale[d].KeySetInterpolation(scaleKeyIndex, FbxAnimCurveDef.EInterpolationType.eInterpolationLinear);
                    
                    var rotDim = Array.FindIndex(dims, x => x == animation.rotXYZ[d]);
                    var rotationKeyIndex = rotation[d].KeyAdd(time);
                    rotation[d].KeySetValue(rotationKeyIndex, (animation.flipRot[d] == 1 ? -1 : 1) * track.TrackRotations[i][rotDim] + animation.rot[d]);
                    rotation[d].KeySetInterpolation(rotationKeyIndex, FbxAnimCurveDef.EInterpolationType.eInterpolationLinear);
                    
                    var traDim = Array.FindIndex(dims, x => x == animation.traXYZ[d]);
                    var translationKeyIndex = translation[d].KeyAdd(time);
                    translation[d].KeySetValue(translationKeyIndex, (animation.flipTra[d] == 1 ? -1 : 1) * track.TrackTranslations[i][traDim] + animation.tra[d]);
                    translation[d].KeySetInterpolation(translationKeyIndex, FbxAnimCurveDef.EInterpolationType.eInterpolationLinear);
                } 
            }

            scale.ForEach(x => x.KeyModifyEnd());
            rotation.ForEach(x => x.KeyModifyEnd());
            translation.ForEach(x => x.KeyModifyEnd());     
        }
    }

    public void Clear()
    {
        _scene.Clear();
        _globalSkeletonNodes.Clear();
        _globalBoneNodes.Clear();
    }

    public void Dispose()
    {
        lock (_fbxLock)
        {
            _scene.Destroy();
            _manager.Destroy();
        }
        InfoHandler.Dispose();
    }

    public void SetGlobalSkeleton(TagHash tagHash)
    {
        EntitySkeleton skeleton = PackageHandler.GetTag(typeof(EntitySkeleton), tagHash);
        _globalBoneNodes = skeleton.GetBoneNodes();
        _globalSkeletonNodes = MakeFbxSkeletonHierarchy(_globalBoneNodes);
    }
}