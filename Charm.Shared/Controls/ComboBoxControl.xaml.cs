using System.Windows.Controls;

namespace Charm.Shared;

public partial class ComboBoxControl : UserControl
{
    public ComboBoxControl()
    {
        InitializeComponent();
        DataContext = this;
    }

    public ComboBox Box => Combobox;

    public string Text { get; set; }
    public int TextFontSize { get; set; } = 16;

    public string Label { get; set; }
    public int LabelFontSize { get; set; } = 12;
}
