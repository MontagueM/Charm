using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Tiger;
using Tiger.Schema.Investment;
using static Charm.APIItemView;

namespace Charm;

public partial class APITooltip : UserControl
{
    public dynamic? ActiveItem;

    public APITooltip()
    {
        InitializeComponent();
        CompositionTarget.Rendering += OnRender;
    }

    public async void MakeTooltip(PlugItem item)
    {
        item.Name = item.Name.ToUpper();
        var itemStrings = item.Item?.GetItemStrings();

        // TODO: Make this better, it sucks
        if (itemStrings is not null &&
            ((DestinyTooltipStyle)itemStrings.TagData.TooltipStyle.Hash32 != DestinyTooltipStyle.None
            && (DestinyTooltipStyle)itemStrings.TagData.TooltipStyle.Hash32 != DestinyTooltipStyle.Record))
        {
            item.PlugRarity = DestinyTierType.Unknown;
            item.PlugRarityColor = DestinyTierType.Unknown.GetColor();
        }

        // Idk if Task.Run is actually doing anything here but it maybeeee feels a little more responsive?
        await Task.Run(() =>
        {
            Dispatcher.Invoke(() =>
            {
                InfoBox.Visibility = Visibility.Visible;
                //UIHelper.AnimateFadeIn(InfoBox, 0.1f, 1f, 0f);
                InfoBox.DataContext = item;
            });

            if (itemStrings?.TagData.Unk38.GetValue(itemStrings.GetReader()) is D2Class_D8548080 warning)
            {
                foreach (var rule in warning.InsertionRules)
                {
                    if (rule.FailureMessage.Value is null || rule.FailureMessage.Value.Value == "Requires Mod Item")
                        continue;

                    AddToTooltip(new PlugItem
                    {
                        Description = rule.FailureMessage.Value,
                        PlugRarityColor = Color.FromArgb(255, 174, 65, 65)
                    }, TooltipType.Warning);
                }
            }

            //if (item.PlugDamageType != DestinyDamageTypeEnum.None)
            //{
            //    AddToTooltip(item, TooltipType.Element);
            //}

            if (itemStrings?.TagData.Unk40.GetValue(itemStrings.GetReader()) is D2Class_D7548080 preview)
            {
                if (preview.ScreenStyleHash.Hash32 == 3797307284) // 'screen_style_emblem'
                {
                    AddToTooltip(new PlugItem
                    {
                        PlugOrderIndex = 1,
                        PlugImageSource = ApiImageUtils.MakeFullIcon(itemStrings.TagData.EmblemContainerIndex)
                    }, TooltipType.Emblem);
                }

                PlugItem inputItem = new PlugItem
                {
                    PlugOrderIndex = 0,
                    Name = $"", // Key glyph
                    Type = $"", // 2nd key glyph (mouse left/right)
                    Description = $"{(preview.PreviewActionString.Value ?? "Details")}"
                };
                AddToTooltip(inputItem, TooltipType.Input);
            }

            if (item.Description is not null && item.Description != "")
            {
                item.PlugOrderIndex = 2;
                AddToTooltip(item, TooltipType.TextBlock);
            }

            if (item.Item is not null)
            {
                if (item.Item.TagData.Unk38.GetValue(item.Item.GetReader()) is D2Class_B0738080 objectives)
                {
                    foreach (var objective in objectives.Objectives)
                    {
                        var obj = Investment.Get().GetObjective(objective.ObjectiveIndex);
                        if (obj is null)
                            continue;

                        PlugItem objItem = new PlugItem
                        {
                            PlugOrderIndex = 4,
                            Description = obj.Value.ProgressDescription.Value,
                            Type = $"0/{Investment.Get().GetObjectiveValue(objective.ObjectiveIndex)}",
                            PlugImageSource = obj.Value.IconIndex != -1 ? ApiImageUtils.MakeIcon(obj.Value.IconIndex) : null
                        };

                        TooltipType tooltipType = TooltipType.ObjectiveInteger;
                        switch ((DestinyUnlockValueUIStyle)obj.Value.InProgressValueStyle)
                        {
                            case DestinyUnlockValueUIStyle.Percentage:
                                tooltipType = TooltipType.ObjectivePercentage;
                                break;
                            case DestinyUnlockValueUIStyle.Integer:
                                objItem.Type = $"{Investment.Get().GetObjectiveValue(objective.ObjectiveIndex)}";
                                tooltipType = TooltipType.ObjectiveInteger;
                                break;
                        }

                        //if (item.PlugStyle == DestinySocketCategoryStyle.Reusable)
                        //    AddToTooltip(null, TooltipType.Separator);

                        AddToTooltip(objItem, tooltipType); // TODO: Other styles
                    }
                }

                if (item.Item.TagData.Unk78.GetValue(item.Item.GetReader()) is D2Class_81738080 stats)
                {
                    if (item.PlugStyle == DestinySocketCategoryStyle.Reusable)
                        return;

                    if (itemStrings is not null && (DestinyUIDisplayStyle)itemStrings.TagData.DisplayStyle.Hash32 == DestinyUIDisplayStyle.EnergyMod)
                    {
                        foreach (var stat in stats.InvestmentStats)
                        {
                            var statItem = Investment.Get().StatStrings[stat.StatTypeIndex];
                            if (statItem.StatHash.Hash32 is 3578062600 or 514071887)
                            {
                                PlugItem energy = new PlugItem
                                {
                                    PlugOrderIndex = -1,
                                    Hash = statItem.StatHash,
                                    Name = $"{stat.Value}",
                                    Description = "ENERGY COST",
                                    PlugImageSource = ApiImageUtils.MakeIcon(statItem.StatIconIndex, 3, 0)
                                };
                                AddToTooltip(energy, TooltipType.Cost);
                            }
                        }
                    }

                    foreach (var perk in stats.Perks)
                    {
                        var perkStrings = Investment.Get().SandboxPerkStrings[perk.PerkIndex];
                        if (perkStrings.IconIndex == -1)
                            continue;

                        PlugItem perkItem = new PlugItem
                        {
                            PlugOrderIndex = 5,
                            Hash = perkStrings.SandboxPerkHash,
                            Description = perkStrings.SandboxPerkDescription.Value,
                            PlugImageSource = ApiImageUtils.MakeIcon(perkStrings.IconIndex, 0, 1)
                        };

                        AddToTooltip(perkItem, TooltipType.Grid);
                    }
                }


                foreach (var notif in itemStrings.TagData.TooltipNotifications)
                {
                    PlugItem notifItem = new PlugItem
                    {
                        PlugOrderIndex = 6,
                        Description = $"{notif.DisplayString.Value}",
                        PlugImageSource = null,

                        TooltipNotificationStyle = (DestinyUIDisplayStyle)notif.DisplayStyle.Hash32
                    };
                    AddToTooltip(notifItem, TooltipType.Notification);
                }
            }
        });
    }

    public void ClearTooltip()
    {
        InfoBoxStackPanel.Children.Clear();
        UserInputStackPanel.Children.Clear();
        WarningBoxStackPanel.Children.Clear();
        InfoBox.Visibility = Visibility.Collapsed;
    }

    public void AddToTooltip(PlugItem item, TooltipType type)
    {
        Dispatcher.Invoke(() =>
        {
            switch (type)
            {
                case TooltipType.Cost:
                    DataTemplate infoCostTemplate = (DataTemplate)FindResource("InfoBoxCostTemplate");
                    FrameworkElement costUI = (FrameworkElement)infoCostTemplate.LoadContent();
                    costUI.DataContext = item;
                    InfoBoxStackPanel.Children.Add(costUI);
                    break;

                case TooltipType.TextBlock:
                    DataTemplate infoTextTemplate = (DataTemplate)FindResource("InfoBoxTextTemplate");
                    FrameworkElement textUI = (FrameworkElement)infoTextTemplate.LoadContent();
                    textUI.DataContext = item;
                    InfoBoxStackPanel.Children.Add(textUI);
                    break;

                case TooltipType.Source:
                    DataTemplate sourceTextTemplate = (DataTemplate)FindResource("InfoBoxSourceTemplate");
                    FrameworkElement sourceTextUI = (FrameworkElement)sourceTextTemplate.LoadContent();
                    sourceTextUI.DataContext = item;
                    InfoBoxStackPanel.Children.Add(sourceTextUI);
                    break;

                case TooltipType.TextBlockItalic:
                    DataTemplate infoTextItalicTemplate = (DataTemplate)FindResource("InfoBoxTextItalicTemplate");
                    FrameworkElement textItalicUI = (FrameworkElement)infoTextItalicTemplate.LoadContent();
                    textItalicUI.DataContext = item;
                    InfoBoxStackPanel.Children.Add(textItalicUI);
                    break;

                case TooltipType.Grid:
                    DataTemplate infoGridTemplate = (DataTemplate)FindResource("InfoBoxGridTemplate");
                    FrameworkElement gridUi = (FrameworkElement)infoGridTemplate.LoadContent();
                    gridUi.DataContext = item;
                    InfoBoxStackPanel.Children.Add(gridUi);
                    break;

                case TooltipType.Notification:
                    DataTemplate notificationTemplate = (DataTemplate)FindResource("InfoBoxNotificationTemplate");
                    FrameworkElement notifUi = (FrameworkElement)notificationTemplate.LoadContent();
                    notifUi.DataContext = item;
                    InfoBoxStackPanel.Children.Add(notifUi);
                    break;

                case TooltipType.Warning:
                    DataTemplate warningTextTemplate = (DataTemplate)FindResource("InfoBoxWarningTextTemplate");
                    FrameworkElement warningTextUI = (FrameworkElement)warningTextTemplate.LoadContent();
                    warningTextUI.DataContext = item;
                    WarningBoxStackPanel.Children.Add(warningTextUI);
                    break;

                case TooltipType.Separator:
                    DataTemplate separatorTemplate = (DataTemplate)FindResource("InfoBoxSeparatorTemplate");
                    FrameworkElement separatorUi = (FrameworkElement)separatorTemplate.LoadContent();
                    InfoBoxStackPanel.Children.Add(separatorUi);
                    break;

                case TooltipType.Emblem:
                    DataTemplate emblemTemplate = (DataTemplate)FindResource("InfoBoxEmblemTemplate");
                    FrameworkElement emblemUi = (FrameworkElement)emblemTemplate.LoadContent();
                    emblemUi.DataContext = item;
                    InfoBoxStackPanel.Children.Add(emblemUi);
                    break;

                case TooltipType.Element:
                    DataTemplate elementTemplate = (DataTemplate)FindResource("InfoBoxElementTemplate");
                    FrameworkElement elementUi = (FrameworkElement)elementTemplate.LoadContent();
                    elementUi.DataContext = item;
                    InfoBoxStackPanel.Children.Add(elementUi);
                    break;

                case TooltipType.Input:
                    DataTemplate inputTemplate = (DataTemplate)FindResource("InfoBoxInputTemplate");
                    FrameworkElement inputUi = (FrameworkElement)inputTemplate.LoadContent();
                    inputUi.DataContext = item;
                    UserInputStackPanel.Children.Add(inputUi);
                    break;

                // Objective stuff
                case TooltipType.ObjectivePercentage:
                    DataTemplate objPercentTemplate = (DataTemplate)FindResource("InfoBoxObjectivePercentageTemplate");
                    FrameworkElement objPercentUi = (FrameworkElement)objPercentTemplate.LoadContent();
                    objPercentUi.DataContext = item;
                    InfoBoxStackPanel.Children.Add(objPercentUi);
                    break;

                case TooltipType.ObjectiveInteger:
                    DataTemplate objIntTemplate = (DataTemplate)FindResource("InfoBoxObjectiveIntegerTemplate");
                    FrameworkElement objIntUi = (FrameworkElement)objIntTemplate.LoadContent();
                    objIntUi.DataContext = item;
                    InfoBoxStackPanel.Children.Add(objIntUi);
                    break;
            }

            List<UIElement> elementList = InfoBoxStackPanel.Children.Cast<UIElement>().ToList();
            InfoBoxStackPanel.Children.Clear();
            foreach (var element in elementList.OrderBy(x => (((FrameworkElement)x).DataContext as PlugItem).PlugOrderIndex))
            {
                InfoBoxStackPanel.Children.Add(element);
            }

            elementList = UserInputStackPanel.Children.Cast<UIElement>().ToList();
            UserInputStackPanel.Children.Clear();
            foreach (var element in elementList.OrderBy(x => (((FrameworkElement)x).DataContext as PlugItem).PlugOrderIndex))
            {
                UserInputStackPanel.Children.Add(element);
            }
        });
    }

    private void OnRender(object sender, EventArgs e)
    {
        if (ActiveItem == null || InfoBox.Visibility != Visibility.Visible)
            return;

        Point position = Mouse.GetPosition(this);

        float xOffset = 25;
        float yOffset = 25;
        float padding = 25;

        if (position.X >= ActualWidth / 2)
            xOffset = (-1 * xOffset + 10) - (float)InfoBox.ActualWidth;

        if (position.Y <= ActualHeight / 2)
            yOffset = (-1 * yOffset - 10) - (float)InfoBox.ActualHeight;

        if (position.Y - yOffset - padding - (float)InfoBox.ActualHeight <= 0)
            yOffset += (float)(position.Y - yOffset - padding - (float)InfoBox.ActualHeight);

        TranslateTransform infoBoxTransform = (TranslateTransform)InfoBox.RenderTransform;
        infoBoxTransform.X = position.X + xOffset;
        infoBoxTransform.Y = position.Y - yOffset - ActualHeight;
    }

    public enum TooltipType // TODO: Simplify styles/templates
    {
        TextBlock,
        TextBlockItalic,
        Cost,
        Source,
        Notification,
        Grid,
        Warning,
        Separator,
        Emblem,
        Element,
        Input,

        ObjectivePercentage,
        ObjectiveInteger,
    }
}


public class TransparentColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color color && double.TryParse(parameter?.ToString(), out double alphaFactor))
        {
            byte alpha = (byte)(color.A * alphaFactor);
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InfoBoxColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color color && float.TryParse(parameter?.ToString(), out float brightnessFactor))
        {
            // hacky 'fix'
            if (brightnessFactor == 0.5f)
                return new SolidColorBrush(Color.FromArgb(235, color.R, color.G, color.B));

            System.Drawing.Color col = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            System.Drawing.Color newColor = ColorUtility.GenerateShades(col, 1, brightnessFactor).First();
            return new SolidColorBrush(Color.FromArgb(235, newColor.R, newColor.G, newColor.B));
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DamageTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string damageType = value.ToString();

        if (string.IsNullOrEmpty(damageType) || damageType == "Kinetic")
        {
            if (parameter.ToString() == "Visibility")
                return Visibility.Collapsed;
            if (parameter.ToString() == "Text")
                return string.Empty;
            if (parameter.ToString() == "Foreground")
                return new SolidColorBrush(Colors.Transparent); // Or some default color
        }

        switch (damageType)
        {
            //case "Kinetic":
            //    if (parameter.ToString() == "Text")
            //        return "Kinetic";
            //    if (parameter.ToString() == "Foreground")
            //        return new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            //    break;
            case "Arc":
                if (parameter.ToString() == "Text")
                    return "";
                if (parameter.ToString() == "Foreground")
                    return new SolidColorBrush(Color.FromRgb(0x85, 0xc5, 0xec)); // #85c5ec
                break;
            case "Solar":
                if (parameter.ToString() == "Text")
                    return "";
                if (parameter.ToString() == "Foreground")
                    return new SolidColorBrush(Color.FromRgb(0xf2, 0x71, 0x1b)); // #f2711b
                break;
            case "Void":
                if (parameter.ToString() == "Text")
                    return "";
                if (parameter.ToString() == "Foreground")
                    return new SolidColorBrush(Color.FromRgb(0xb1, 0x84, 0xc5)); // #b184c5
                break;
            case "Stasis":
                if (parameter.ToString() == "Text")
                    return "";
                if (parameter.ToString() == "Foreground")
                    return new SolidColorBrush(Color.FromRgb(0x4d, 0x88, 0xff)); // #4d88ff
                break;
            case "Strand":
                if (parameter.ToString() == "Text")
                    return "";
                if (parameter.ToString() == "Foreground")
                    return new SolidColorBrush(Color.FromRgb(0x35, 0xe3, 0x66)); // #35e366
                break;
            default:
                return DependencyProperty.UnsetValue;
        }

        if (parameter.ToString() == "Visibility")
            return Visibility.Visible;

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
