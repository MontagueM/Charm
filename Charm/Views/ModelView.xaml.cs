using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using HelixToolkit.SharpDX;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;
using Tiger.Schema.Entity;
using Tiger.Schema.Static;

namespace Charm;

public partial class ModelView : UserControl
{
    public ModelView()
    {
        InitializeComponent();
    }

    public ExportDetailLevel GetSelectedLod()
    {
        ExportDetailLevel selected = (ExportDetailLevel)LodCombobox.SelectedIndex;
        return selected;
    }

    public int GetSelectedGroupIndex()
    {
        if (GroupsCombobox.SelectedItem == null)
            return -1;
        string selected = (GroupsCombobox.SelectedItem as ComboBoxItem).Content as string;
        if (selected == String.Empty || selected == "All")
            return -1;
        string i = selected.Split("Group ")[1].Split("/")[0];
        int index = int.Parse(i);
        return index - 1;
    }

    private Action _loadModelFunc = null;
    private bool _bFromSelectionChange = false;
    private bool _bFromSetGroupIndices = false;

    public void SetModelFunction(Action action)
    {
        _loadModelFunc = action;
    }

    private void LodCombobox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // We need the LoadEntity function bound with its data
        if (_loadModelFunc != null)
        {
            _loadModelFunc();
        }
    }

    private void GroupsCombobox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_bFromSetGroupIndices)
        {
            return;
        }

        _bFromSelectionChange = true;
        if (_loadModelFunc != null)
        {
            _loadModelFunc();
        }

        _bFromSelectionChange = false;
    }

    public void SetGroupIndices(HashSet<int> hashSet)
    {
        if (_bFromSelectionChange || hashSet.Count == 0)
            return;
        _bFromSetGroupIndices = true;

        GroupsCombobox.Items.Clear();
        var l = hashSet.ToList();
        if (l != null)
        {
            l.Sort();
            int max = l.Last();
            foreach (int i in l)
            {
                GroupsCombobox.Items.Add(new ComboBoxItem
                {
                    Content = $"Group {i + 1}/{max + 1}",
                    IsSelected = i == l.First()
                });
            }
        }
        GroupsCombobox.Items.Add(new ComboBoxItem
        {
            Content = $"All",
            IsSelected = true
        });
        _bFromSetGroupIndices = false;
    }

    public List<HelixModelView.DisplayPart> MakeEntityDisplayParts(List<Entity> entities, ExportDetailLevel detailLevel)
    {
        bool useTextures = TextureCheckBox.IsChecked == true;

        ConcurrentBag<HelixModelView.DisplayPart> displayParts = new();
        foreach (Entity ent in entities)
        {
            var offsetTrans = Vector3.Zero;
            var offsetRot = Vector4.Quaternion;
            if (ent.HasGeometry())
            {
                List<DynamicMeshPart> dynamicParts = ent.Load(detailLevel);
                SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
                if (GetSelectedGroupIndex() != -1)
                    dynamicParts = dynamicParts.Where(x => x.GroupIndex == GetSelectedGroupIndex()).ToList();

                offsetTrans = ent.Model.TranslationOffset.ToVec3();
                offsetRot = ent.Model.RotationOffset;
                foreach (DynamicMeshPart part in dynamicParts)
                {
                    HelixModelView.DisplayPart displayPart = new();
                    displayPart.BasePart = part;
                    displayPart.Translations.Add(Vector3.Zero + offsetTrans);
                    displayPart.Rotations.Add(new(System.Numerics.Quaternion.Identity * offsetRot.ToQuat()));
                    displayPart.Scales.Add(Vector3.One);

                    if (useTextures && part.Material?.Pixel.Textures.Any() == true)
                    {
                        Stream texture = TextureView.RemoveAlpha(part.Material.Pixel.Textures[0].Texture.GetTexture());
                        displayPart.DiffuseMaterial = new()
                        {
                            DiffuseMap = new TextureModel(texture, true),
                        };
                    }

                    displayParts.Add(displayPart);
                }
            }

            if (ent.Skeleton != null && SkeletonCheckBox.IsChecked == true)
            {
                HelixModelView.DisplayPart displayPart = new();
                displayPart.BoneNodes = ent.Skeleton.GetBoneNodes();
                displayPart.Translations.Add(offsetTrans);
                displayPart.Rotations.Add(offsetRot);
                displayPart.Scales.Add(Vector3.One);

                displayParts.Add(displayPart);
            }
        }

        return displayParts.ToList();
    }

    // TODO combine with above, I don't like this
    public List<HelixModelView.DisplayPart> MakeEntityModelDisplayParts(EntityModel entModel, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<HelixModelView.DisplayPart> displayParts = new();

        List<DynamicMeshPart> dynamicParts = entModel.Load(detailLevel, null);
        SetGroupIndices(new HashSet<int>(dynamicParts.Select(x => x.GroupIndex)));
        if (GetSelectedGroupIndex() != -1)
            dynamicParts = dynamicParts.Where(x => x.GroupIndex == GetSelectedGroupIndex()).ToList();

        foreach (DynamicMeshPart part in dynamicParts)
        {
            HelixModelView.DisplayPart displayPart = new();
            displayPart.BasePart = part;
            displayPart.Translations.Add(Vector3.Zero);
            displayPart.Rotations.Add(Vector4.Zero);
            displayPart.Scales.Add(Vector3.One);

            displayParts.Add(displayPart);
        }

        return displayParts.ToList();
    }

    // TODO: Merge all this into one, or simplify it?
    public List<HelixModelView.DisplayPart> MakeStaticMapDisplayParts(StaticMapData staticMap, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<HelixModelView.DisplayPart> displayParts = new();
        if (Strategy.IsD1())
        {
            if (staticMap.TagData.D1StaticMapData is not null)
            {
                StaticMapData_D1 d1MapData = staticMap.TagData.D1StaticMapData;
                Dictionary<FileHash, List<StaticMapData_D1.MeshInfo>> statics = d1MapData.GetStatics();
                List<StaticMapData_D1.InstanceTransform> instances = d1MapData.ParseTransforms();
                Parallel.ForEach(statics, mesh =>
                {
                    List<StaticPart> parts = d1MapData.Load(mesh.Value, instances);
                    foreach (StaticMapData_D1.MeshInfo info in mesh.Value)
                    {
                        for (int i = info.TransformIndex; i < info.TransformIndex + info.InstanceCount; i++)
                        {
                            foreach (StaticPart part in parts)
                            {
                                HelixModelView.DisplayPart displayPart = new();
                                displayPart.BasePart = part;
                                displayPart.Translations.Add(instances[i].Translation.ToVec3());
                                displayPart.Rotations.Add(instances[i].Rotation);
                                displayPart.Scales.Add(instances[i].Scale.ToVec3());
                                displayParts.Add(displayPart);
                            }
                        }
                    }
                });
            }
        }
        else
        {
            Parallel.ForEach(staticMap.TagData.InstanceCounts, c =>
            {
                // inefficiency as sometimes there are two instance count entries with same hash. why? idk
                StaticMesh model = staticMap.TagData.Statics[c.StaticIndex].Static;
                List<StaticPart> parts = model.Load(ExportDetailLevel.MostDetailed);
                for (int i = c.InstanceOffset; i < c.InstanceOffset + c.InstanceCount; i++)
                {
                    foreach (StaticPart part in parts)
                    {
                        HelixModelView.DisplayPart displayPart = new();
                        displayPart.BasePart = part;
                        displayPart.Translations.Add(staticMap.TagData.Instances[i].Position);
                        displayPart.Rotations.Add(staticMap.TagData.Instances[i].Rotation);
                        displayPart.Scales.Add(new Vector3(staticMap.TagData.Instances[i].Scale.X));
                        displayParts.Add(displayPart);
                    }

                }
            });
        }

        return displayParts.ToList();
    }

    public List<HelixModelView.DisplayPart> MakeTerrainDisplayParts(Terrain terrain, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<HelixModelView.DisplayPart> displayParts = new();
        List<StaticPart> parts = new();
        foreach (STerrainPart partEntry in terrain.TagData.StaticParts)
        {
            if (partEntry.DetailLevel == 0)
            {
                StaticPart part = terrain.MakePart(partEntry);
                terrain.TransformPositions(part);
                terrain.TransformTexcoords(part);
                parts.Add(part);
            }
        }

        foreach (StaticPart part in parts)
        {
            HelixModelView.DisplayPart displayPart = new();
            displayPart.BasePart = part;
            displayPart.Translations.Add(Vector3.Zero);
            displayPart.Rotations.Add(Vector4.Zero);
            displayPart.Scales.Add(Vector3.One);
            displayParts.Add(displayPart);
        }
        return displayParts.ToList();
    }

    public List<HelixModelView.DisplayPart> MakeMapDataTableDisplayParts(FileHash hash, ExportDetailLevel detailLevel)
    {
        ConcurrentBag<HelixModelView.DisplayPart> displayParts = new();

        List<SMapDataEntry> dataEntries = new();
        if (Strategy.IsD1() && hash.GetReferenceHash().Hash32 == 0x808003F6) //F6038080
            dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<S808003F6>(hash).TagData.EntityComponent.CollapseIntoDataEntry());
        else
            dataEntries.AddRange(FileResourcer.Get().GetSchemaTag<SMapDataTable>(hash).TagData.DataEntries);

        Parallel.ForEach(dataEntries, entry =>
        {
            Entity entity = FileResourcer.Get().GetFile(typeof(Entity), entry.Entity.Hash);
            List<Entity> entities = new() { entity };
            entities.AddRange(entity.GetEntityChildren());
            foreach (Entity ent in entities)
            {
                if (ent.HasGeometry())
                {
                    List<DynamicMeshPart> parts = ent.Load(ExportDetailLevel.MostDetailed);

                    foreach (DynamicMeshPart part in parts)
                    {
                        HelixModelView.DisplayPart displayPart = new();
                        displayPart.BasePart = part;
                        displayPart.Translations.Add(entry.Transfrom.Translation.ToVec3());
                        displayPart.Rotations.Add(entry.Transfrom.Rotation);
                        displayPart.Scales.Add(new Tiger.Schema.Vector3(entry.Transfrom.Translation.W, entry.Transfrom.Translation.W, entry.Transfrom.Translation.W));
                        displayParts.Add(displayPart);
                    }
                }
            }
        });
        return displayParts.ToList();
    }
}
