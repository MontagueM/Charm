using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Charm.ViewModels;

namespace Charm;

internal class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        else
        {
            return new TextBlock { Text = name };
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
