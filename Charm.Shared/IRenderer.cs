using System.Runtime.CompilerServices;
using Arithmic;
using Tiger;
using Tiger.Schema.Investment;

namespace Charm.Shared;

public delegate void ActionRef<T>(ref T value);

public interface IRenderer
{
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
}
