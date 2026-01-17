using Tiger;
using Tiger.Schema.Investment;

namespace Charm.Shared;
public interface IRenderer
{
    //void Initialize();
    //void Start();
    //void Stop();

    void LoadStatic(FileHash hash);
    void LoadEntity(FileHash hash);
    void LoadInvestmentItem(InventoryItem item);
    void LoadInvestmentItems(IEnumerable<InventoryItem> items);
}
