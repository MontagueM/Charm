using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Tiger;

namespace Charm.Objects;


public class ListItem
{
    public TigerHash Hash { get; set; }
    public string HashString { get => $"[{Hash}]"; }
    public string Title { get; set; }
    public string Subtitle { get; set; }

    public ListItem()
    {
    }

    public ListItem(TigerHash hash)
    {
        Hash = hash;
    }
}

public interface IAbstractListItem<TData>
{
    public void Initialise(TigerFile file);
    public void OnClick();
}

public static class NestedTypeHelpers
{
    public static Type? FindNestedGenericType<T>()
    {
        Type? nestedType = null;

        Type testType = typeof(T);
        while (nestedType == null && testType != null && testType != typeof(object))
        {
            if (testType.IsGenericType)
            {
                nestedType = testType.GenericTypeArguments[0];
            }
            else
            {
                testType = testType.BaseType;
            }
        }

        return nestedType;
    }

    public static Type? GetNonGenericParent(this Type inTestType, Type inheritParentType)
    {
        Type? testType = inTestType;
        while (testType != null && testType != typeof(object))
        {
            if (testType.IsGenericType && testType.GenericTypeArguments.Length > 0 && testType.GetGenericTypeDefinition() == inheritParentType)
            {
                return testType;
            }
            else
            {
                testType = testType.BaseType;
            }
        }

        return null;
    }
}

public interface IAbstractFileView<in TView, in TData>
{
    public abstract void LoadView(TData dataToView);
}

/// <summary>
/// Takes some data of type <see cref="TData"/> and generates a display item for a list based on the data given.
/// On click, it returns a routed  <see cref="TView"/> and populates it with the data.
/// </summary>
/// <typeparam name="TData">The type of data type to use for generating the information of this item. Must be a schema struct or class of type <see cref="Tag"/>.</typeparam>
/// <typeparam name="TView">The type of view this item opens and populates on click. Must be of type <see cref="IAbstractFileView{TData}"/>.</typeparam>
public abstract class AbstractList<TData> : BaseViewModel, IAbstractFileView<ListControl, TData>
{
    private ObservableCollection<ListItem> items = new();

    public ObservableCollection<ListItem> Items
    {
        get => items;
        set
        {
            items = value;
            OnPropertyChanged();
        }
    }
    public void LoadView(TData dataToView)
    {
        Items = GetAllItems(dataToView);
    }

    public abstract ObservableCollection<ListItem> GetAllItems(TData data);

    public abstract void OnClick();
}


public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string memberName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
    }
}
