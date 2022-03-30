using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Trivial.Data;
using Trivial.IO;
using Trivial.Tasks;
using Trivial.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<FileListView>;

/// <summary>
/// The file list view.
/// </summary>
public sealed partial class FileListView : UserControl
{
    /// <summary>
    /// The dependency property of path height.
    /// </summary>
    public static readonly DependencyProperty PathHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(PathHeight), new GridLength(30));

    /// <summary>
    /// The dependency property of path style.
    /// </summary>
    public static readonly DependencyProperty PathStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(PathStyle));

    /// <summary>
    /// The dependency property of item container style.
    /// </summary>
    public static readonly DependencyProperty ItemContainerStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(ItemContainerStyle));

    /// <summary>
    /// The dependency property of selection mode.
    /// </summary>
    public static readonly DependencyProperty SelectionModeProperty = DependencyObjectProxy.RegisterProperty(nameof(SelectionMode), ListViewSelectionMode.Extended);

    /// <summary>
    /// The dependency property of title container style.
    /// </summary>
    public static readonly DependencyProperty TitleStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(TitleStyle));

    /// <summary>
    /// The dependency property of hyperlink style.
    /// </summary>
    public static readonly DependencyProperty HyperlinkStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(HyperlinkStyle));

    /// <summary>
    /// The dependency property of file name style.
    /// </summary>
    public static readonly DependencyProperty FileNameStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(FileNameStyle));

    /// <summary>
    /// The dependency property of icon style.
    /// </summary>
    public static readonly DependencyProperty IconStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(IconStyle));

    /// <summary>
    /// The dependency property of spacing between file name and icon.
    /// </summary>
    public static readonly DependencyProperty TitleSpacingProperty = DependencyObjectProxy.RegisterProperty(nameof(TitleSpacing), 8d);

    /// <summary>
    /// The dependency property of description style.
    /// </summary>
    public static readonly DependencyProperty DescriptionStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(DescriptionStyle));

    /// <summary>
    /// The dependency property of description style.
    /// </summary>
    public static readonly DependencyProperty DirectoryNavigationProperty = DependencyObjectProxy.RegisterProperty(nameof(DirectoryNavigation), true);

    private const int DefaultDepth = 24;
    private Guid tracing = Guid.Empty;

    /// <summary>
    /// The text.
    /// </summary>
    private readonly ObservableCollection<IFileSystemReferenceInfo> collection = new();

    /// <summary>
    /// The text.
    /// </summary>
    private readonly ObservableCollection<IFileContainerReferenceInfo> path = new();

    /// <summary>
    /// Initializes a new instance of the FileListView class.
    /// </summary>
    public FileListView()
    {
        InitializeComponent();
        FileBrowser.ItemsSource = collection;
        PathBar.ItemsSource = path;
    }

    /// <summary>
    /// Adds or removes the click event on item.
    /// </summary>
    public event ItemClickEventHandler ItemClick;

    /// <summary>
    /// Adds or removes the click event on file item.
    /// </summary>
    public event DataEventHandler<IFileContainerReferenceInfo> Navigating;

    /// <summary>
    /// Adds or removes the click event on file item.
    /// </summary>
    public event DataEventHandler<IFileContainerReferenceInfo> Navigated;

    /// <summary>
    /// Adds or removes the click event on file item.
    /// </summary>
    public event DataEventHandler<IFileReferenceInfo> FileOpened;

    /// <summary>
    /// Adds or removes the click event on directory item.
    /// </summary>
    public event DataEventHandler<IDirectoryReferenceInfo> DirectoryOpened;

    /// <summary>
    /// Adds or removes the change event on item.
    /// </summary>
    public event SelectionChangedEventHandler SelectionChanged;

    /// <summary>
    /// Gets the count of line.
    /// </summary>
    public int Count => FileBrowser.Items.Count;

    /// <summary>
    /// Gets the selected item.
    /// </summary>
    public IFileSystemReferenceInfo SelectedItem
    {
        get => FileBrowser.SelectedItem as IFileSystemReferenceInfo;
        set => FileBrowser.SelectedItem = value;
    }

    /// <summary>
    /// Gets the selected index.
    /// </summary>
    public int SelectedIndex
    {
        get => FileBrowser.SelectedIndex;
        set => FileBrowser.SelectedIndex = value;
    }

    /// <summary>
    /// Gets the selected items.
    /// </summary>
    public IList<IFileSystemReferenceInfo> SelectedItems => FileBrowser.SelectedItems?.OfType<IFileSystemReferenceInfo>().ToList();

    /// <summary>
    /// Gets the selected items.
    /// </summary>
    public IReadOnlyList<ItemIndexRange> SelectedIndexes => FileBrowser.SelectedRanges;

    /// <summary>
    /// Gets or sets the width of line number.
    /// </summary>
    public GridLength PathHeight
    {
        get => (GridLength)GetValue(PathHeightProperty);
        set => SetValue(PathHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the path style.
    /// </summary>
    public Style PathStyle
    {
        get => (Style)GetValue(PathStyleProperty);
        set => SetValue(PathStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the item container style.
    /// </summary>
    public Style ItemContainerStyle
    {
        get => (Style)GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the title container style.
    /// </summary>
    public Style TitleStyle
    {
        get => (Style)GetValue(TitleStyleProperty);
        set => SetValue(TitleStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the title container style.
    /// </summary>
    public Style HyperlinkStyle
    {
        get => (Style)GetValue(HyperlinkStyleProperty);
        set => SetValue(HyperlinkStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of file name.
    /// </summary>
    public double TitleSpacing
    {
        get => (double)GetValue(TitleSpacingProperty);
        set => SetValue(TitleSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the file name style.
    /// </summary>
    public Style FileNameStyle
    {
        get => (Style)GetValue(FileNameStyleProperty);
        set => SetValue(FileNameStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon style.
    /// </summary>
    public Style IconStyle
    {
        get => (Style)GetValue(IconStyleProperty);
        set => SetValue(IconStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the description style.
    /// </summary>
    public Style DescriptionStyle
    {
        get => (Style)GetValue(DescriptionStyleProperty);
        set => SetValue(DescriptionStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the selection behavior for the element.
    /// </summary>
    public ListViewSelectionMode SelectionMode
    {
        get => (ListViewSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether need navigate to target directory.
    /// </summary>
    public bool DirectoryNavigation
    {
        get => (bool)GetValue(DirectoryNavigationProperty);
        set => SetValue(DirectoryNavigationProperty, value);
    }

    /// <summary>
    /// Gets or sets the content for the file list header.
    /// </summary>
    public object Header
    {
        get => FileBrowser.Header;
        set => FileBrowser.Header = value;
    }

    /// <summary>
    /// Gets or sets the content for the file list footer.
    /// </summary>
    public object Footer
    {
        get => FileBrowser.Footer;
        set => FileBrowser.Footer = value;
    }

    /// <summary>
    /// Gets the file reference client.
    /// </summary>
    public IFileReferenceClient FileReferenceClient { get; private set; }

    /// <summary>
    /// Gets the directory navigated.
    /// </summary>
    public IFileContainerReferenceInfo Directory { get; private set; }

    /// <summary>
    /// Gets all files.
    /// </summary>
    /// <returns>The file collection.</returns>
    public IEnumerable<IFileSystemReferenceInfo> GetFiles()
        => FileBrowser.Items?.OfType<IFileSystemReferenceInfo>()?.ToList() ?? new List<IFileSystemReferenceInfo>();

    /// <summary>
    /// Sets data.
    /// </summary>
    /// <param name="client">The file reference client.</param>
    /// <param name="directory">The start directory.</param>
    /// <param name="depth">The depth to load parent initialized.</param>
    public async Task NavigateAsync(IFileReferenceClient client, IFileContainerReferenceInfo directory, int depth = DefaultDepth)
    {
        if (directory == null || !directory.Exists) return;
        var id = tracing = Guid.NewGuid();
        Navigating?.Invoke(this, new DataEventArgs<IFileContainerReferenceInfo>(directory));
        FileReferenceClient = client;
        Directory = directory;
        collection.Clear();
        if (client == null) client = FileReferenceClientFactory.Instance;
        var path = new List<IFileContainerReferenceInfo>
        {
            directory
        };
        for (var i = 0; i < depth; i++)
        {
            var item = path.LastOrDefault();
            if (item == null || !client.Test(item)) break;
            item = await client.GetParentAsync(item);
            if (item == null || path.Contains(item)) break;
            if (id != tracing) return;
            path.Add(item);
        }

        path.Reverse();
        if (id != tracing) return;
        this.path.Clear();
        foreach (var item in path)
        {
            this.path.Add(item);
        }

        var directories = await client.GetDirectoriesAsync(directory);
        if (id != tracing) return;
        if (directories.Count > 0)
        {
            foreach (var item in directories)
            {
                if (string.IsNullOrWhiteSpace(item.Name)) continue;
                collection.Add(item);
            }
        }

        if (id != tracing) return;
        var files = await client.GetFilesAsync(directory);
        if (files.Count > 0)
        {
            foreach (var item in files)
            {
                if (string.IsNullOrWhiteSpace(item.Name)) continue;
                collection.Add(item);
            }
        }

        Navigated?.Invoke(this, new DataEventArgs<IFileContainerReferenceInfo>(directory));
    }

    /// <summary>
    /// Sets data.
    /// </summary>
    /// <param name="directory">The start directory.</param>
    /// <param name="depth">The depth to load parent initialized.</param>
    public Task NavigateAsync(DirectoryInfo directory, int depth = DefaultDepth)
        => NavigateAsync(null, directory == null ? null : new LocalDirectoryReferenceInfo(directory), depth);

    private void FileBrowser_ItemClick(object sender, ItemClickEventArgs e)
        => ItemClick?.Invoke(this, e);

    private void FileBrowser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => SelectionChanged?.Invoke(this, e);

    /// <summary>
    /// Gets description text.
    /// </summary>
    /// <param name="info">The file reference information instance.</param>
    /// <returns>The description text.</returns>
    public static string GetDescription(BaseFileSystemReferenceInfo info)
    {
        if (info is IFileReferenceInfo file && file.Size >= 0)
            return $"{file.LastModification:g} \t{FileSystemInfoUtility.ToFileSizeString(file.Size)}";
        return info.LastModification.ToString("g");
    }

    /// <summary>
    /// Gets glyph of file icon.
    /// </summary>
    /// <param name="info">The file reference information instance.</param>
    /// <returns>The glyph.</returns>
    public static string GetGlyph(BaseFileSystemReferenceInfo info)
        => info is IFileReferenceInfo ? VisualUtility.IconSet.GetFileGlyph(info.Name) : VisualUtility.IconSet.FolderGlyph;

    private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not HyperlinkButton button) return;
        FileBrowser.SelectedItem = button.DataContext;
        if (button.DataContext is IFileReferenceInfo file)
        {
            FileOpened?.Invoke(this, new DataEventArgs<IFileReferenceInfo>(file));
            if (DirectoryNavigation && file is IFileContainerReferenceInfo container) _ = NavigateAsync(FileReferenceClient, container);
        }
        else if (button.DataContext is IDirectoryHostReferenceInfo dir)
        {
            DirectoryOpened?.Invoke(this, new DataEventArgs<IDirectoryReferenceInfo>(dir));
            if (DirectoryNavigation) _ = NavigateAsync(FileReferenceClient, dir);
        }
    }

    private void PathBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args?.Item is not IFileContainerReferenceInfo info) return;
        if (info is IDirectoryReferenceInfo dir) DirectoryOpened?.Invoke(this, new DataEventArgs<IDirectoryReferenceInfo>(dir));
        if (DirectoryNavigation) _ = NavigateAsync(FileReferenceClient, info);
    }
}
