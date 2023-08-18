namespace Tiger.Exporters;

public abstract class AbstractExporter : Subsystem
{
    protected internal override bool Initialise()
    {
        Exporter.ExportEvent += Export;
        return true;
    }

    public abstract void Export(Exporter.ExportEventArgs args);
}
