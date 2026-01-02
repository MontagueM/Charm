using Tiger;
using Tiger.Schema;
using Tiger.Schema.Investment;

namespace Charm.Shared;
public interface IRenderer
{
    //void Initialize();
    //void Start();
    //void Stop();

    void LoadStatic(FileHash hash, MapTransform transform);
    void LoadEntity(FileHash hash);
    void LoadInvestmentItem(InventoryItem item);
}
