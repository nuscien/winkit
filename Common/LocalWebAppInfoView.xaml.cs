using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Text;
using Trivial.Web;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<LocalWebAppInfoView>;

/// <summary>
/// The local web app information view.
/// </summary>
public sealed partial class LocalWebAppInfoView : UserControl
{
    /// <summary>
    /// The dependency property of data model.
    /// </summary>
    public static readonly DependencyProperty ModelProperty = DependencyObjectProxy.RegisterProperty<LocalWebAppManifest>(nameof(Model), OnModelChanged);

    /// <summary>
    /// The dependency property of title font size.
    /// </summary>
    public static readonly DependencyProperty TitleFontSizeProperty = DependencyObjectProxy.RegisterProperty(nameof(TitleFontSize), 18d);

    /// <summary>
    /// Initializes a new instance of the local web app information view.
    /// </summary>
    public LocalWebAppInfoView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the data model.
    /// </summary>
    public LocalWebAppManifest Model
    {
        get => (LocalWebAppManifest)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    /// <summary>
    /// Gets or sets the font size of title.
    /// </summary>
    public double TitleFontSize
    {
        get => (double)GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets element to apply to the further container.
    /// </summary>
    public UIElement FurtherElement
    {
        get => FurtherContainer.Child;
        set => FurtherContainer.Child = value;
    }

    private static void OnModelChanged(LocalWebAppInfoView c, ChangeEventArgs<LocalWebAppManifest> e, DependencyProperty d)
    {
        if (c == null || e == null) return;
        var m = e.NewValue;
        if (m == null)
        {
            return;
        }

        c.TitleElement.Text = string.IsNullOrWhiteSpace(m.DisplayName) ? "App" : m.DisplayName;
        c.PublisherElement.Visibility = GetVisibility(m.PublisherName);
        c.DescriptionElement.Visibility = GetVisibility(m.Description);
        var uri = VisualUtility.TryCreateUri(m.Website);
        c.WebsiteElement.NavigateUri = uri;
        c.WebsiteElement.Visibility = uri == null ? Visibility.Collapsed : Visibility.Visible;
        var hasVer = string.IsNullOrWhiteSpace(m.Version);
        var hasId = string.IsNullOrWhiteSpace(m.Id);
        c.VersionElement.Visibility = Visibility.Visible;
        if (hasId && hasVer)
        {
            c.VersionElement.Text = null;
            c.VersionElement.Visibility = Visibility.Collapsed;
        }
        else if (hasId)
        {
            c.VersionElement.Text = m.Version;
        }
        else if (hasVer)
        {
            c.VersionElement.Text = m.Id;
        }
        else
        {
            c.VersionElement.Text = $"{m.Id.Trim()}  ·  {m.Version.Trim()}";
        }

        c.CopyrightElement.Visibility = GetVisibility(m.Copyright);
    }

    private static Visibility GetVisibility(string s)
        => string.IsNullOrWhiteSpace(s) ? Visibility.Collapsed : Visibility.Visible;
}
