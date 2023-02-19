// namespace Tiger;
//
// public interface SchemaHandle
// {
//     
// }
//
// []
// struct D2S_CE978080
// {
//     private ulong ThisSize;
//     [Schema(0x10)] public List<D2S_SourceChannel> Channels;
//     [Schema(0x20)] public List<D2S_SourceChannel> ExposedChannels;
//     [Schema(0x38)] public List<D2S_SourceChannel> StorageChannels;
//     [Schema(0x48)] public List<D2S_SourceChannel> ScriptableChannels;
// }
//
//
//
// public class SchemaHandle
// {
//     private static TigerStrategy CurrentStrategy { get; set; }
//
//     private TigerStrategy Strategy;
//
//     private static SchemaHandle Instance;
//     
//     // Factory method; to avoid passing the strategy around, we use the settings singleton
//     public static SchemaHandle Get()
//     {
//         if (Instance.GetStrategy() == CurrentStrategy)
//         {
//             return Instance;
//         }
//         switch (CurrentStrategy)
//         {
//             case TigerStrategy.DESTINY2_LATEST:
//                 Instance = new Destiny2_Latest.SchemaHandle();
//                 break;
//             case TigerStrategy.DESTINY2_111894:
//                 Instance = new Destiny2_111894.SchemaHandle();
//                 break;
//             default:
//                 throw new Exception($"Unknown strategy '{CurrentStrategy}'.");
//         }
//
//         return Instance;
//     }
//     
//     public void SetStrategy(TigerStrategy strategy)
//     {
//         Strategy = strategy;
//     }
//     
//     public TigerStrategy GetStrategy()
//     {
//         return Strategy;
//     }
// }