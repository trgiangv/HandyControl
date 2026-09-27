using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace HandyControl.Controls;

/// <summary>
/// Applies the HandyControl menu styles to the text editor context menu.
/// WPF builds that menu as private subclasses (<c>EditorContextMenu</c> / <c>EditorMenuItem</c>).
/// Implicit styles match the exact type only, so those menus stay on the classic theme
/// unless Fluent is enabled. The text box theme sets <see cref="ActiveProperty"/>, which
/// listens for <see cref="FrameworkElement.ContextMenuOpeningEvent"/> on that instance.
/// </summary>
public static class TextEditorContextMenu
{
    private static readonly ContextMenuEventHandler OpeningHandler = OnContextMenuOpening;

    public static readonly DependencyProperty ActiveProperty = DependencyProperty.RegisterAttached(
        "Active",
        typeof(bool),
        typeof(TextEditorContextMenu),
        new PropertyMetadata(false, OnActiveChanged));

    public static void SetActive(DependencyObject element, bool value) => element.SetValue(ActiveProperty, value);

    public static bool GetActive(DependencyObject element) => (bool)element.GetValue(ActiveProperty);

    private static void OnActiveChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is not UIElement element || args.NewValue is not true)
            return;

        element.AddHandler(FrameworkElement.ContextMenuOpeningEvent, OpeningHandler, handledEventsToo: true);
    }

    private static void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (sender is not FrameworkElement editor)
            return;

        ApplyMenu(editor);
        editor.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => ApplyMenu(editor));
    }

    private static void ApplyMenu(FrameworkElement editor)
    {
        if (FindOpenMenu(editor) is not { } menu)
            return;

        ApplyThemeStyle(menu, menu, MenuStyleKey);
        foreach (var item in menu.Items)
        {
            switch (item)
            {
                case MenuItem menuItem:
                    ApplyThemeStyle(menu, menuItem, ItemStyleKey);
                    break;
                case Separator separator:
                    ApplyThemeStyle(menu, separator, SeparatorStyleKey);
                    break;
            }
        }
    }

    private const string MenuStyleKey = "TextEditorContextMenuStyle";
    private const string ItemStyleKey = "TextEditorMenuItemStyle";
    private const string SeparatorStyleKey = "TextEditorMenuSeparatorStyle";

    private static void ApplyThemeStyle(ContextMenu menu, FrameworkElement element, string styleKey)
    {
        var source = DependencyPropertyHelper.GetValueSource(element, FrameworkElement.StyleProperty);
        if (source.BaseValueSource is not (BaseValueSource.Default or BaseValueSource.DefaultStyle))
            return;

        var style = (menu.PlacementTarget as FrameworkElement)?.TryFindResource(styleKey) as Style
                    ?? Application.Current?.TryFindResource(styleKey) as Style;
        if (style is null)
            return;

        element.Style = style;
    }

    private static ContextMenu? FindOpenMenu(FrameworkElement editor)
    {
        foreach (PresentationSource source in PresentationSource.CurrentSources)
        {
            if (Match(source?.RootVisual, editor) is { } root)
                return root;

            if (source?.RootVisual is DependencyObject visual && Match(FindVisualChild<ContextMenu>(visual), editor) is { } nested)
                return nested;
        }

        return null;
    }

    private static ContextMenu? Match(DependencyObject? candidate, FrameworkElement editor) =>
        candidate is ContextMenu menu && menu.IsOpen && menu.PlacementTarget == editor ? menu : null;

    private static T? FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        if (parent is T match)
            return match;

        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var index = 0; index < count; index++)
        {
            var nested = FindVisualChild<T>(VisualTreeHelper.GetChild(parent, index));
            if (nested is not null)
                return nested;
        }

        return null;
    }
}
