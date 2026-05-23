using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Arithmic;
using Tiger;
using Tiger.Schema.Investment;

namespace Charm.Shared;

public delegate void ActionRef<T>(ref T value);

public interface IRenderer
{
    public static Assembly? CharmRenderer = null;

    //void Initialize();
    //void Start();
    //void Stop();
    void Destroy(bool fullyDestroy = false);

    void LoadStatic(FileHash hash);
    void LoadEntity(FileHash hash);
    void LoadInvestmentItem(InventoryItem item);
    void LoadInvestmentItems(IEnumerable<InventoryItem> items);

    private static readonly object _lock = new();
    static HashSet<IRenderer> ActiveRenderers { get; } = new HashSet<IRenderer>();

    public static IRenderer CreateRenderer([CallerMemberName] string callerMethodName = "")
    {
        if (!CanUseRenderer())
            throw new InvalidOperationException("Tried to create renderer when it is not available. This should never happen.");

        Type renderer = CharmRenderer.GetType("Charm.Renderer.RendererViewport");
        var irenderer = Activator.CreateInstance(renderer) as IRenderer;
        RegisterRenderer(irenderer, callerMethodName);

        return irenderer;
    }

    /// <summary>
    /// MUST be called after creating any IRenderer
    /// </summary>
    /// <param name="renderer"></param>
    static void RegisterRenderer(IRenderer renderer, [CallerMemberName] string callerMethodName = "")
    {
        ActiveRenderers.Add(renderer);
        Log.Debug($"Registered Renderer from {callerMethodName}");
    }

    static void UnregisterRenderer(IRenderer renderer)
    {
        lock (_lock)
        {
            ActiveRenderers.Remove(renderer);
            renderer.Destroy(ActiveRenderers.Count == 0);
            //Log.Debug($"Unregistered Renderer");
        }
    }

    // Gotta load the renderer via reflection to avoid circular dependencies since it depends on Charm for styles
    public static void IsRendererDllAvailable()
    {
        string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Charm.Renderer.dll");
        if (File.Exists(dllPath))
            CharmRenderer = Assembly.LoadFrom(dllPath);
        else
            return;

        Log.Info("Loaded Charm.Renderer");
    }

    public static bool CanUseRenderer()
    {
        return CharmRenderer is not null
            && ConfigSubsystem.Get().GetCustomRenderer()
            && Strategy.IsLatest();
    }
}
