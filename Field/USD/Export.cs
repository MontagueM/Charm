using Autodesk.Fbx;

namespace Field.USD;


public class UsdHandler
{
    public static void Test()
    {
        // Scene scene = Scene.Create();
        // PrimvarBase b = new PrimvarBase();
        // UsdGeomPrimvar g = new UsdGeomPrimvar();
        

        // scene.Save(/);
        // scene.Close();
        FbxManager manager = FbxManager.Create();
        FbxScene scene = FbxScene.Create(manager, "");
        FbxExporter exporter = FbxExporter.Create(manager, "");
        exporter.Initialize("C:/T/test.fbx", -1);
        exporter.Export(scene);
        exporter.Destroy();
    }
}