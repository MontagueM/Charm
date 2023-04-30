namespace Tiger;

public class SchemaHandle : Strategy.StrategistSingleton<SchemaHandle>
{
    SchemaHandle(TigerStrategy strategy) : base(strategy)
    {
        var a = 0;
    }
}