using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using Tiger;

namespace Charm;

public partial class TagListViewerView : UserControl
{
    public TagListViewerView()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        if (ConfigSubsystem.Get().GetAnimatedBackground())
        {
            SpinnerShader _spinner = new SpinnerShader();
            Spinner.Effect = _spinner;
            SizeChanged += _spinner.OnSizeChanged;
            _spinner.ScreenWidth = (float)ActualWidth;
            _spinner.ScreenHeight = (float)ActualHeight;
            _spinner.Scale = new(0, 0);
            _spinner.Offset = new(-1, -1);
            SpinnerContainer.Visibility = Visibility.Visible;
        }
    }

    public void LoadContent(ETagListType tagListType, FileHash contentValue = null, bool bFromBack = false,
        ConcurrentBag<TagItem> overrideItems = null)
    {
        TagList.LoadContent(tagListType, contentValue, bFromBack, overrideItems);
    }
}
