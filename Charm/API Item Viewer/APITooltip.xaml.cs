﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
    }

    public void MakeTooltip(PlugItem item) // TODO: Use a number ordering system instead
    {
        item.Name = item.Name.ToUpper();

        InfoBox.Visibility = Visibility.Visible;
        var fadeIn = FindResource("FadeIn 0.2") as Storyboard;
        InfoBox.BeginStoryboard(fadeIn);
        InfoBox.DataContext = item;

        if (item.Item?.GetItemStrings().TagData.Unk38.GetValue(item.Item.GetItemStrings().GetReader()) is D2Class_D8548080 warning)
        {
            foreach (var rule in warning.InsertionRules)
            {
                if (rule.FailureMessage.Value is null)
                    continue;

                AddToTooltip(new PlugItem
                {
                    Description = rule.FailureMessage.Value,
                    PlugRarityColor = Color.FromArgb(255, 255, 0, 0)
                }, TooltipType.WarningBlock);
            }
        }

        //if (item.PlugDamageType != DestinyDamageTypeEnum.None)
        //{
        //    AddToTooltip(item, TooltipType.Element);
        //}

        if (item.Item?.GetItemStrings().TagData.Unk40.GetValue(item.Item.GetItemStrings().GetReader()) is D2Class_D7548080 preview)
        {
            if (preview.ScreenStyleHash.Hash32 == 3797307284) // 'screen_style_emblem'
            {
                AddToTooltip(new PlugItem
                {
                    PlugImageSource = ApiImageUtils.MakeFullIcon(item.Item.GetItemStrings().TagData.EmblemContainerIndex)
                }, TooltipType.Emblem);
            }
        }

        if (item.Description is not null && item.Description != "")
            AddToTooltip(item, TooltipType.TextBlock);

        if (item.Item?.TagData.Unk38.GetValue(item.Item.GetReader()) is D2Class_B0738080 objectives)
        {
            foreach (var objective in objectives.Objectives)
            {
                var obj = Investment.Get().GetObjective(objective.ObjectiveIndex);
                if (obj is null)
                    continue;

                PlugItem objItem = new PlugItem
                {
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

                if (item.PlugStyle == DestinySocketCategoryStyle.Reusable)
                    AddToTooltip(null, TooltipType.Separator);

                AddToTooltip(objItem, tooltipType); // TODO: Other styles
            }
        }

        if (item.Item?.TagData.Unk78.GetValue(item.Item.GetReader()) is D2Class_81738080 stats)
        {
            if (item.PlugStyle == DestinySocketCategoryStyle.Reusable)
                return;

            foreach (var perk in stats.Perks)
            {
                var perkStrings = Investment.Get().SandboxPerkStrings[perk.PerkIndex];
                if (perkStrings.IconIndex == -1)
                    continue;

                PlugItem perkItem = new PlugItem
                {
                    Hash = perkStrings.SandboxPerkHash,
                    Description = perkStrings.SandboxPerkDescription.Value,
                    PlugImageSource = ApiImageUtils.MakeIcon(perkStrings.IconIndex)
                };

                AddToTooltip(perkItem, TooltipType.Grid);
            }
        }

        if (item.Item is not null)
        {
            foreach (var notif in item.Item?.GetItemStrings().TagData.TooltipNotifications)
            {
                PlugItem notifItem = new PlugItem
                {
                    Description = $"★ {notif.DisplayString.Value}",
                    PlugImageSource = null
                };
                AddToTooltip(notifItem, TooltipType.InfoBlock);
            }
        }
    }

    public void ClearTooltip()
    {
        InfoBoxStackPanel.Children.Clear();
        WarningBoxStackPanel.Children.Clear();
        InfoBox.Visibility = Visibility.Collapsed;
    }

    public void AddToTooltip(PlugItem item, TooltipType type)
    {
        switch (type)
        {
            case TooltipType.TextBlock:
                DataTemplate infoTextTemplate = (DataTemplate)FindResource("InfoBoxTextTemplate");
                FrameworkElement textUI = (FrameworkElement)infoTextTemplate.LoadContent();
                textUI.DataContext = item;
                InfoBoxStackPanel.Children.Add(textUI);
                break;
            case TooltipType.Grid:
                DataTemplate infoGridTemplate = (DataTemplate)FindResource("InfoBoxGridTemplate");
                FrameworkElement gridUi = (FrameworkElement)infoGridTemplate.LoadContent();
                gridUi.DataContext = item;
                InfoBoxStackPanel.Children.Add(gridUi);
                break;
            case TooltipType.InfoBlock:
                if (InfoBoxStackPanel.Children.Count != 0)
                {
                    DataTemplate infoBlockSepTemplate = (DataTemplate)FindResource("InfoBoxSeparatorTemplate");
                    FrameworkElement infoBlockSepUi = (FrameworkElement)infoBlockSepTemplate.LoadContent();
                    InfoBoxStackPanel.Children.Add(infoBlockSepUi);
                }

                DataTemplate infoBlockTemplate = (DataTemplate)FindResource("InfoBoxGridTemplate");
                FrameworkElement infoBlockUi = (FrameworkElement)infoBlockTemplate.LoadContent();
                infoBlockUi.DataContext = item;
                InfoBoxStackPanel.Children.Add(infoBlockUi);
                break;
            case TooltipType.WarningBlock:
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
    }

    public void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        Dispatcher.InvokeAsync(new Action(() =>
        {
            Point position = Mouse.GetPosition(this);
            if (InfoBox.Visibility == Visibility.Visible && ActiveItem != null)
            {
                float xOffset = 25;
                float yOffset = 25;
                float padding = 25;

                // this is stupid
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
        }), System.Windows.Threading.DispatcherPriority.Input);
    }

    public enum TooltipType // TODO: Simplify styles/templates
    {
        InfoBlock,
        TextBlock,
        Grid,
        WarningBlock,
        Separator,
        Emblem,
        Element,

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
                return new SolidColorBrush(Color.FromArgb(230, color.R, color.G, color.B));

            System.Drawing.Color col = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            System.Drawing.Color newColor = ColorUtility.GenerateShades(col, 1, brightnessFactor).First();
            return new SolidColorBrush(Color.FromArgb(230, newColor.R, newColor.G, newColor.B));
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