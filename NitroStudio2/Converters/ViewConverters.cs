using Avalonia.Controls;
using Avalonia.Data.Converters;
using NitroStudio2.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NitroStudio2.ViewModels
{
    /// <summary>Shows the tree or the sequence text depending on the editor's right-pane mode.</summary>
    public static class EnumConverters
    {
        public static IValueConverter IsTree { get; } =
            new FuncValueConverter<RightPaneMode, bool>(mode => mode == RightPaneMode.Tree);

        public static IValueConverter IsSequenceEditor { get; } =
            new FuncValueConverter<RightPaneMode, bool>(mode =>
                mode == RightPaneMode.SequenceEditor
            );
    }

    /// <summary>
    /// Turns a node's action list into a context menu. Each tree node carries only the subset of
    /// actions that applies to it, the way CreateMenuStrip built a trimmed ContextMenuStrip.
    /// </summary>
    public static class MenuConverters
    {
        public static IValueConverter ToContextMenu { get; } = new ContextMenuConverter();

        private sealed class ContextMenuConverter : IValueConverter
        {
            public object Convert(
                object value,
                Type targetType,
                object parameter,
                CultureInfo culture
            )
            {
                if (value is not IReadOnlyList<MenuAction> actions || actions.Count == 0)
                {
                    return null;
                }
                ContextMenu menu = new();
                List<MenuItem> items = [];
                foreach (MenuAction action in actions)
                {
                    MenuItem item = new() { Header = action.Header, Command = action.Command };
                    if (action.IconName is not null)
                    {
                        item.Icon = new Image { Source = action.Icon, Width = 16, Height = 16 };
                    }
                    items.Add(item);
                }
                menu.ItemsSource = items;
                return menu;
            }

            public object ConvertBack(
                object value,
                Type targetType,
                object parameter,
                CultureInfo culture
            ) => throw new NotSupportedException();
        }
    }
}
