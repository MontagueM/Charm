using System.Windows;
using System.Windows.Controls;

namespace Charm.Objects;

public partial class DefaultListItem : UserControl
{
    public DefaultListItem()
    {
        InitializeComponent();
    }

    public string Hash { get; set; } = "Hash";
    public string HashString { get; set; } = "HashString";
    public string Title { get; set; } = "Title";

}

