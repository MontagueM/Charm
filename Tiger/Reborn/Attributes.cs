namespace Tiger.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class TigerSchema : Attribute
{
    internal int _val;
    public int Offset
    {
        get { return _val; }
    }


    public TigerSchema(int size)
    {
        _val = d2Offset;
    }
}