using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Arithmic;
using Tiger;
using Tiger.Schema.Entity;

namespace Charm;

public partial class EntityView : UserControl
{
    public FileHash Hash;
    private bool _isEntityModel = false;
    private HelixModelView HelixMV;

    public EntityView()
    {
        InitializeComponent();
    }

    public bool LoadEntity(FileHash entityHash)
    {
        if (_isEntityModel)
            LoadEntityModel(entityHash);

        Log.Info($"Loading Entity {entityHash}");

        Hash = entityHash;
        SetupCheckboxHandlers();

        Entity entity = FileResourcer.Get().GetFile<Entity>(entityHash);

        List<Entity> entities = new() { entity };
        entities.AddRange(entity.GetEntityChildren());

        HelixMV ??= (HelixModelView)ModelView.UCModelView.Resources["HelixMV"];

        HelixMV.Clear();
        List<HelixModelView.DisplayPart> displayParts = ModelView.MakeEntityDisplayParts(entities, ModelView.GetSelectedLod());
        HelixMV.SetChildren(displayParts);
        HelixMV.Title = entityHash;
        HelixMV.SubTitle = $"{displayParts.Sum(p => p.BasePart.Indices.Count)} triangles";

        return true;
    }

    public bool LoadEntityModel(FileHash entityModelHash)
    {
        Hash = entityModelHash;
        _isEntityModel = true;
        SetupCheckboxHandlers();

        EntityModel entityModel = FileResourcer.Get().GetFile<EntityModel>(entityModelHash);

        if (HelixMV is null)
            HelixMV = (HelixModelView)ModelView.UCModelView.Resources["HelixMV"];

        HelixMV.Clear();
        List<HelixModelView.DisplayPart> displayParts = ModelView.MakeEntityModelDisplayParts(entityModel, ModelView.GetSelectedLod());
        HelixMV.SetChildren(displayParts);
        HelixMV.Title = entityModelHash;
        HelixMV.SubTitle = $"{displayParts.Sum(p => p.BasePart.Indices.Count)} triangles";

        return true;
    }

    private void SetupCheckboxHandlers()
    {
        ModelView.TextureCheckBox.Visibility = Visibility.Visible;
        ModelView.SkeletonCheckBox.Visibility = Visibility.Visible;

        // Detach first to prevent multiple subscriptions
        ModelView.TextureCheckBox.Checked -= ReloadEntity;
        ModelView.TextureCheckBox.Unchecked -= ReloadEntity;

        ModelView.SkeletonCheckBox.Checked -= ReloadEntity;
        ModelView.SkeletonCheckBox.Unchecked -= ReloadEntity;

        ModelView.TextureCheckBox.Checked += ReloadEntity;
        ModelView.TextureCheckBox.Unchecked += ReloadEntity;

        ModelView.SkeletonCheckBox.Checked += ReloadEntity;
        ModelView.SkeletonCheckBox.Unchecked += ReloadEntity;
    }

    private void ReloadEntity(object sender, RoutedEventArgs e) =>
        LoadEntity(Hash);
}
