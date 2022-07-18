using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Trivial.UI;
using DependencyObjectProxy = DependencyObjectProxy<SingleWebView>;

/// <summary>
/// The web view with navigation bar.
/// </summary>
public sealed partial class SingleWebView : UserControl
{
    /// <summary>
    /// The dependency property of source
    /// </summary>
    public static readonly DependencyProperty SourceProperty = DependencyObjectProxy.RegisterProperty<Uri>(nameof(Source), UpdateUrlSource);

    /// <summary>
    /// The dependency property of command bar height.
    /// </summary>
    public static readonly DependencyProperty NavigationBarHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(NavigationBarHeight), new GridLength(40));

    /// <summary>
    /// The dependency property of command bar padding.
    /// </summary>
    public static readonly DependencyProperty CommandBarPaddingProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(CommandBarPadding));

    /// <summary>
    /// The dependency property of the flag indicating whether the navigation input box is read-only.
    /// </summary>
    public static readonly DependencyProperty IsReadOnlyProperty = DependencyObjectProxy.RegisterProperty(nameof(IsReadOnly), UpdateReadOnly, false);

    /// <summary>
    /// The dependency property of the default web view background color.
    /// </summary>
    public static readonly DependencyProperty DefaultWebViewBackgroundColorProperty = DependencyObjectProxy.RegisterProperty(nameof(DefaultWebViewBackgroundColor), Microsoft.UI.Colors.Transparent);

    /// <summary>
    /// The dependency property of the command bar.
    /// </summary>
    public static readonly DependencyProperty NavigationBarBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(NavigationBarBackground));

    /// <summary>
    /// The dependency property of the button foreground.
    /// </summary>
    public static readonly DependencyProperty ButtonForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(ButtonForeground));

    /// <summary>
    /// The dependency property of the button hover foreground.
    /// </summary>
    public static readonly DependencyProperty ButtonHoverForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(ButtonHoverForeground));

    /// <summary>
    /// The dependency property of the button disabled foreground.
    /// </summary>
    public static readonly DependencyProperty ButtonDisabledForegroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(ButtonDisabledForeground));

    /// <summary>
    /// The dependency property of the button background.
    /// </summary>
    public static readonly DependencyProperty ButtonBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(ButtonBackground));

    /// <summary>
    /// The dependency property of the button hover background.
    /// </summary>
    public static readonly DependencyProperty ButtonHoverBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(ButtonHoverBackground));

    /// <summary>
    /// The dependency property of the button disabled background.
    /// </summary>
    public static readonly DependencyProperty ButtonDisabledBackgroundProperty = DependencyObjectProxy.RegisterProperty<Brush>(nameof(ButtonDisabledBackground));

    /// <summary>
    /// The dependency property of the button font size.
    /// </summary>
    public static readonly DependencyProperty ButtonFontSizeProperty = DependencyObjectProxy.RegisterProperty(nameof(ButtonFontSize), 16d);

    /// <summary>
    /// The dependency property of the button padding.
    /// </summary>
    public static readonly DependencyProperty ButtonPaddingProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(ButtonPadding));

    /// <summary>
    /// The dependency property of the button margin.
    /// </summary>
    public static readonly DependencyProperty ButtonMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(ButtonMargin));

    /// <summary>
    /// The dependency property of the URL input font size.
    /// </summary>
    public static readonly DependencyProperty UrlInputFontSizeProperty = DependencyObjectProxy.RegisterProperty(nameof(UrlInputFontSize), 14d);

    /// <summary>
    /// The dependency property of the URL input padding.
    /// </summary>
    public static readonly DependencyProperty UrlInputPaddingProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(UrlInputPadding));

    /// <summary>
    /// The dependency property of the URL input margin.
    /// </summary>
    public static readonly DependencyProperty UrlInputMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(UrlInputMargin));

    /// <summary>
    /// The dependency property of the navigation bar padding.
    /// </summary>
    public static readonly DependencyProperty NavigationBarPaddingProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(NavigationBarPadding));

    /// <summary>
    /// The dependency property of the navigation bar margin.
    /// </summary>
    public static readonly DependencyProperty NavigationBarMarginProperty = DependencyObjectProxy.RegisterProperty<Thickness>(nameof(NavigationBarMargin));

    /// <summary>
    /// The dependency property of the navigation bar corner radius.
    /// </summary>
    public static readonly DependencyProperty NavigationBarCornerRadiusProperty = DependencyObjectProxy.RegisterProperty<CornerRadius>(nameof(NavigationBarCornerRadius));

    /// <summary>
    /// The dependency property of the loading style.
    /// </summary>
    public static readonly DependencyProperty LoadingStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(LoadingStyle));

    /// <summary>
    /// The flag indicating whether the webpage is loading.
    /// </summary>
    private bool isLoading;

    /// <summary>
    /// Initialized a new instance of the SingleWebView class.
    /// </summary>
    public SingleWebView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Occurs when the user submits a search query.
    /// </summary>
    public event TypedEventHandler<SingleWebView, AutoSuggestBoxQuerySubmittedEventArgs> UrlQuerySubmitted;

    /// <summary>
    /// Occurs before the text content of the editable control component is updated.
    /// </summary>
    public event TypedEventHandler<SingleWebView, AutoSuggestBoxSuggestionChosenEventArgs> UrlSuggestionChosen;

    /// <summary>
    /// Occurs after the text content of the editable control component is updated.
    /// </summary>
    public event TypedEventHandler<SingleWebView, AutoSuggestBoxTextChangedEventArgs> UrlTextChanged;

    /// <summary>
    /// Occurs when the web view core processes failed.
    /// </summary>
    public event TypedEventHandler<SingleWebView, CoreWebView2ProcessFailedEventArgs> CoreProcessFailed;

    /// <summary>
    /// Occurs when the web view core has initialized.
    /// </summary>
    public event TypedEventHandler<SingleWebView, CoreWebView2InitializedEventArgs> CoreWebView2Initialized;

    /// <summary>
    /// Occurs when the navigation is completed.
    /// </summary>
    public event TypedEventHandler<SingleWebView, CoreWebView2NavigationCompletedEventArgs> NavigationCompleted;

    /// <summary>
    /// Occurs when the navigation is starting.
    /// </summary>
    public event TypedEventHandler<SingleWebView, CoreWebView2NavigationStartingEventArgs> NavigationStarting;

    /// <summary>
    /// Occurs when the web message core has received.
    /// </summary>
    public event TypedEventHandler<SingleWebView, CoreWebView2WebMessageReceivedEventArgs> WebMessageReceived;

    /// <summary>
    /// Occurs when the new window request sent.
    /// </summary>
    public event TypedEventHandler<SingleWebView, CoreWebView2NewWindowRequestedEventArgs> NewWindowRequested;

    /// <summary>
    /// Occurs when the webpage send request to close itself.
    /// </summary>
    public event TypedEventHandler<SingleWebView, object> WindowCloseRequested;

    /// <summary>
    /// Occurs when the downloading is starting.
    /// </summary>
    public event TypedEventHandler<SingleWebView, CoreWebView2DownloadStartingEventArgs> DownloadStarting;

    /// <summary>
    /// Occurs when the navigation of a frame in web page is created.
    /// </summary>
    public event TypedEventHandler<SingleWebView, CoreWebView2FrameCreatedEventArgs> FrameCreated;

    /// <summary>
    /// Occurs when the navigation of a frame in web page is completed.
    /// </summary>
    public event TypedEventHandler<SingleWebView, CoreWebView2NavigationCompletedEventArgs> FrameNavigationCompleted;

    /// <summary>
    /// Occurs when the navigation of a frame in web page is starting.
    /// </summary>
    public event TypedEventHandler<SingleWebView, CoreWebView2NavigationStartingEventArgs> FrameNavigationStarting;

    /// <summary>
    /// Occurs when the history has changed.
    /// </summary>
    public event TypedEventHandler<SingleWebView, object> HistoryChanged;

    /// <summary>
    /// Occurs when fullscreen request sent including to enable and disable.
    /// </summary>
    public event DataEventHandler<bool> ContainsFullScreenElementChanged;

    /// <summary>
    /// Occurs when the document title has changed.
    /// </summary>
    public event DataEventHandler<string> DocumentTitleChanged;

    /// <summary>
    /// Gets the download list.
    /// </summary>
    public List<CoreWebView2DownloadOperation> DownloadList { get; internal set; } = new();

    /// <summary>
    /// Gets the web view element.
    /// </summary>
    public WebView2 WebView2 => Browser;

    /// <summary>
    /// Gets the core of web view instance.
    /// </summary>
    public CoreWebView2 CoreWebView2 => Browser.CoreWebView2;

    /// <summary>
    /// The height of command bar.
    /// </summary>
    public GridLength NavigationBarHeight
    {
        get => (GridLength)GetValue(NavigationBarHeightProperty);
        set => SetValue(NavigationBarHeightProperty, value);
    }

    /// <summary>
    /// The padding of command bar.
    /// </summary>
    public Thickness CommandBarPadding
    {
        get => (Thickness)GetValue(CommandBarPaddingProperty);
        set => SetValue(CommandBarPaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    public Uri Source
    {
        get
        {
            return Browser.Source;
        }

        set
        {
            Browser.Source = value;
            UpdateUrlInput(value);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the navigation input box is read-only..
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Gets or set the default background color in web view.
    /// </summary>
    public Windows.UI.Color DefaultWebViewBackgroundColor
    {
        get => (Windows.UI.Color)GetValue(DefaultWebViewBackgroundColorProperty);
        set => SetValue(DefaultWebViewBackgroundColorProperty, value);
    }

    /// <summary>
    /// Gets or set the foreground of command bar.
    /// </summary>
    public Brush NavigationBarBackground
    {
        get => (Brush)GetValue(NavigationBarBackgroundProperty);
        set => SetValue(NavigationBarBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or set the foreground of navigation button.
    /// </summary>
    public Brush ButtonForeground
    {
        get => (Brush)GetValue(ButtonForegroundProperty);
        set => SetValue(ButtonForegroundProperty, value);
    }

    /// <summary>
    /// Gets or set the hover foreground of navigation button.
    /// </summary>
    public Brush ButtonHoverForeground
    {
        get => (Brush)GetValue(ButtonHoverForegroundProperty);
        set => SetValue(ButtonHoverForegroundProperty, value);
    }

    /// <summary>
    /// Gets or set the disabled foreground of navigation button.
    /// </summary>
    public Brush ButtonDisabledForeground
    {
        get => (Brush)GetValue(ButtonDisabledForegroundProperty);
        set => SetValue(ButtonDisabledForegroundProperty, value);
    }

    /// <summary>
    /// Gets or set the foreground of navigation button.
    /// </summary>
    public Brush ButtonBackground
    {
        get => (Brush)GetValue(ButtonBackgroundProperty);
        set => SetValue(ButtonBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or set the hover foreground of navigation button.
    /// </summary>
    public Brush ButtonHoverBackground
    {
        get => (Brush)GetValue(ButtonHoverBackgroundProperty);
        set => SetValue(ButtonHoverBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or set the disabled background of navigation button.
    /// </summary>
    public Brush ButtonDisabledBackground
    {
        get => (Brush)GetValue(ButtonDisabledBackgroundProperty);
        set => SetValue(ButtonDisabledBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or set the font size of navigation button.
    /// </summary>
    public double ButtonFontSize
    {
        get => (double)GetValue(ButtonFontSizeProperty);
        set => SetValue(ButtonFontSizeProperty, value);
    }

    /// <summary>
    /// Gets or set the padding of navigation button.
    /// </summary>
    public Thickness ButtonPadding
    {
        get => (Thickness)GetValue(ButtonPaddingProperty);
        set => SetValue(ButtonPaddingProperty, value);
    }

    /// <summary>
    /// Gets or set the margin of navigation button.
    /// </summary>
    public Thickness ButtonMargin
    {
        get => (Thickness)GetValue(ButtonMarginProperty);
        set => SetValue(ButtonMarginProperty, value);
    }

    /// <summary>
    /// Gets or set the font size of URL input element.
    /// </summary>
    public double UrlInputFontSize
    {
        get => (double)GetValue(UrlInputFontSizeProperty);
        set => SetValue(UrlInputFontSizeProperty, value);
    }

    /// <summary>
    /// Gets or set the padding of URL input element.
    /// </summary>
    public Thickness UrlInputPadding
    {
        get => (Thickness)GetValue(UrlInputPaddingProperty);
        set => SetValue(UrlInputPaddingProperty, value);
    }

    /// <summary>
    /// Gets or set the margin of URL input element.
    /// </summary>
    public Thickness UrlInputMargin
    {
        get => (Thickness)GetValue(UrlInputMarginProperty);
        set => SetValue(UrlInputMarginProperty, value);
    }

    /// <summary>
    /// Gets or set the padding of navigation bar.
    /// </summary>
    public Thickness NavigationBarPadding
    {
        get => (Thickness)GetValue(NavigationBarPaddingProperty);
        set => SetValue(NavigationBarPaddingProperty, value);
    }

    /// <summary>
    /// Gets or set the margin of navigation bar.
    /// </summary>
    public Thickness NavigationBarMargin
    {
        get => (Thickness)GetValue(NavigationBarMarginProperty);
        set => SetValue(NavigationBarMarginProperty, value);
    }

    /// <summary>
    /// Gets or set the corner radius of navigation bar.
    /// </summary>
    public CornerRadius NavigationBarCornerRadius
    {
        get => (CornerRadius)GetValue(NavigationBarCornerRadiusProperty);
        set => SetValue(NavigationBarCornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or set the style of loading element.
    /// </summary>
    public Style LoadingStyle
    {
        get => (Style)GetValue(LoadingStyleProperty);
        set => SetValue(LoadingStyleProperty, value);
    }

    /// <summary>
    /// Gets the document title.
    /// </summary>
    public string DocumentTitle => Browser.CoreWebView2?.DocumentTitle;

    /// <summary>
    /// Gets the settings of the web view core.
    /// </summary>
    public CoreWebView2Settings Settings => Browser.CoreWebView2?.Settings;

    /// <summary>
    /// Gets the a value indicating whether contains the full screen element.
    /// </summary>
    public bool ContainsFullScreenElement => Browser.CoreWebView2?.ContainsFullScreenElement ?? false;

    /// <summary>
    /// Gets a value indicating whether can go back in history.
    /// </summary>
    public bool CanGoBack => Browser.CanGoBack;

    /// <summary>
    /// Gets a value indicating whether can go forward in history.
    /// </summary>
    public bool CanGoForward => Browser.CanGoForward;

    /// <summary>
    /// Gets or sets the handler for searching.
    /// </summary>
    public Func<string, Uri> SearchHandler { get; set; }

    /// <summary>
    /// Ensures the core of web view initialized.
    /// </summary>
    /// <returns>The async task.</returns>
    public async Task EnsureCoreWebView2Async()
    {
        await Browser.EnsureCoreWebView2Async();
    }

    /// <summary>
    /// Focues browser.
    /// </summary>
    /// <param name="value">The state.</param>
    public bool FocusBrowser(FocusState value)
        => Browser.Focus(value);

    /// <summary>
    /// Focues URL input element.
    /// </summary>
    /// <param name="value">The state.</param>
    public bool FocusUrlInput(FocusState value)
        => UrlInput.Focus(value);

    /// <summary>
    /// Reloads web page.
    /// </summary>
    public void Reload()
        => Browser.Reload();

    /// <summary>
    /// Stops processing of the web page.
    /// </summary>
    public void Stop()
    {
        UpdateLoadingState(false);
        if (Browser.CoreWebView2 != null) Browser.CoreWebView2.Stop();
    }

    /// <summary>
    /// Turns back history.
    /// </summary>
    public void GoBack()
    {
        if (Browser.CanGoBack) Browser.GoBack();
    }

    /// <summary>
    /// Turns forward history.
    /// </summary>
    public void GoForward()
    {
        if (Browser.CanGoForward) Browser.GoForward();
    }

    /// <summary>
    /// Executes JavaScript.
    /// </summary>
    /// <param name="javascriptCode">The script.</param>
    /// <returns>The result.</returns>
    public async Task<string> ExecuteScriptAsync(string javascriptCode)
    {
        await Browser.EnsureCoreWebView2Async();
        return await Browser.ExecuteScriptAsync(javascriptCode);
    }

    /// <summary>
    /// Executes JavaScript on document created.
    /// </summary>
    /// <param name="javascriptCode">The script.</param>
    /// <returns>The result.</returns>
    public async Task<string> AddScriptToExecuteOnDocumentCreatedAsync(string javascriptCode)
    {
        await Browser.EnsureCoreWebView2Async();
        return await Browser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(javascriptCode);
    }

    /// <summary>
    /// Navagates to a specific static HTML.
    /// </summary>
    /// <param name="htmlContent">The HTML content to load.</param>
    public void NavigateToString(string htmlContent)
        => Browser.NavigateToString(htmlContent);

    /// <summary>
    /// Posts message to webpage.
    /// </summary>
    /// <param name="json">The content body.</param>
    public void PostWebMessage(JsonObjectNode json)
    {
        if (json == null) return;
        Browser.CoreWebView2.PostWebMessageAsJson(json.ToString());
    }

    /// <summary>
    /// Adds object to JS environment.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="json">The value.</param>
    public void AddHostObjectToScript(string name, JsonObjectNode json)
        => VisualUtility.AddHostObjectToScript(Browser, name, json);

    /// <summary>
    /// Adds object to JS environment.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="json">The value.</param>
    public Task AddHostObjectToScriptAsync(string name, JsonObjectNode json)
        => VisualUtility.AddHostObjectToScriptAsync(Browser, name, json);

    /// <summary>
    /// Adds object to JS environment.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="json">The value.</param>
    public void AddExternalObjectToScript(string name, JsonObjectNode json)
        => VisualUtility.AddExternalObjectToScript(Browser, name, json);

    /// <summary>
    /// Adds object to JS environment.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="json">The value.</param>
    public Task AddExternalObjectToScriptAsync(string name, JsonObjectNode json)
        => VisualUtility.AddExternalObjectToScriptAsync(Browser, name, json);

    /// <summary>
    /// Hides loading element.
    /// </summary>
    public void HideLoading()
        => LoadingElement.IsActive = false;

    /// <summary>
    /// Closes.
    /// </summary>
    public void Close()
        => Browser.Close();

    private void OnCoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
    {
        sender.CoreWebView2.DocumentTitleChanged += OnDocumentTitleChanged;
        sender.CoreWebView2.DownloadStarting += OnDownloadStarting;
        sender.CoreWebView2.FrameCreated += OnFrameCreated;
        sender.CoreWebView2.FrameNavigationStarting += OnFrameNavigationStarting;
        sender.CoreWebView2.FrameNavigationCompleted += OnFrameNavigationCompleted;
        sender.CoreWebView2.HistoryChanged += OnHistoryChanged;
        sender.CoreWebView2.ContainsFullScreenElementChanged += OnContainsFullScreenElementChanged;
        sender.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
        sender.CoreWebView2.WindowCloseRequested += OnWindowCloseRequested;
        CoreWebView2Initialized?.Invoke(this, args);
    }

    private void OnDocumentTitleChanged(CoreWebView2 sender, object args)
    {
        var uri = Browser.Source;
        var title = sender.DocumentTitle;
        UpdateUrlInput(uri);
        DocumentTitleChanged?.Invoke(this, new DataEventArgs<string>(title));
    }

    private void OnNavigationCompleted(WebView2 _, CoreWebView2NavigationCompletedEventArgs args)
    {
        UpdateLoadingState(false);
        NavigationCompleted?.Invoke(this, args);
    }

    private void OnNavigationStarting(WebView2 _, CoreWebView2NavigationStartingEventArgs args)
    {
        UpdateLoadingState(!string.IsNullOrEmpty(args?.Uri));
        NavigationStarting?.Invoke(this, args);
    }

    private void UpdateLoadingState(bool value)
    {
        if (isLoading == value) return;
        LoadingElement.IsActive = isLoading = value;
        RefreshButton.SetFontIcon(isLoading ? "\xE106" : "\xE149");
    }

    private void OnWebMessageReceived(WebView2 _, CoreWebView2WebMessageReceivedEventArgs args)
        => WebMessageReceived?.Invoke(this, args);

    private void OnCoreProcessFailed(WebView2 _, CoreWebView2ProcessFailedEventArgs args)
        => CoreProcessFailed?.Invoke(this, args);

    private void OnContainsFullScreenElementChanged(CoreWebView2 sender, object args)
        => ContainsFullScreenElementChanged?.Invoke(this, new DataEventArgs<bool>(Browser.CoreWebView2.ContainsFullScreenElement));

    private void OnFrameCreated(CoreWebView2 sender, CoreWebView2FrameCreatedEventArgs args)
        => FrameCreated?.Invoke(this, args);

    private void OnFrameNavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        => FrameNavigationStarting?.Invoke(this, args);

    private void OnFrameNavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        => FrameNavigationCompleted?.Invoke(this, args);

    private void OnHistoryChanged(CoreWebView2 sender, object args)
    {
        BackButton.IsEnabled = Browser.CanGoBack;
        ForwardButton.Visibility = Browser.CanGoForward ? Visibility.Visible : Visibility.Collapsed;
        HistoryChanged?.Invoke(this, args);
    }

    private void OnDownloadStarting(CoreWebView2 sender, CoreWebView2DownloadStartingEventArgs args)
    {
        DownloadList.Add(args.DownloadOperation);
        DownloadStarting?.Invoke(this, args);
    }

    private void OnNewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        => NewWindowRequested?.Invoke(this, args);

    private void OnWindowCloseRequested(CoreWebView2 sender, object args)
        => WindowCloseRequested?.Invoke(this, args);

    private void OnUrlQuerySubmitted(AutoSuggestBox _, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var s = UrlInput.Text?.Trim();
        if (string.IsNullOrEmpty(s))
        {
            UpdateUrlInput(Browser.Source);
            return;
        }

        var uri = VisualUtility.TryCreateUri(s);
        if (uri == null)
        {
            var search = SearchHandler;
            if (search != null)
            {
                uri = search(s);
            }
            else if (!s.Contains("://"))
            {
                if (!s.Contains(' ') || s.IndexOf('.') < s.IndexOf(' '))
                    uri = VisualUtility.TryCreateUri(string.Concat(s.StartsWith("//") ? "https:" : "https://", s));
            }
        }

        if (uri == null) return;
        Source = uri;
        UrlQuerySubmitted?.Invoke(this, args);
    }

    private void OnUrlTextChanged(AutoSuggestBox _, AutoSuggestBoxTextChangedEventArgs args)
        => UrlTextChanged?.Invoke(this, args);

    private void OnUrlSuggestionChosen(AutoSuggestBox _, AutoSuggestBoxSuggestionChosenEventArgs args)
        => UrlSuggestionChosen?.Invoke(this, args);

    private void BackButton_Click(object sender, RoutedEventArgs e)
        => GoBack();

    private void ForwardButton_Click(object sender, RoutedEventArgs e)
        => GoForward();

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        var webview = Browser.CoreWebView2;
        if (webview == null) return;
        if (isLoading) webview.Stop();
        else Browser.Reload();
    }

    private void UpdateUrlInput(Uri uri)
    {
        try
        {
            UrlInput.Text = UrlElement.Text = uri?.OriginalString ?? "about:blank";
        }
        catch (InvalidOperationException)
        {
        }
        catch (FormatException)
        {
        }
    }

    private static void UpdateUrlSource(SingleWebView c, ChangeEventArgs<Uri> e, DependencyProperty d)
        => c.Source = e.NewValue;

    private static void UpdateReadOnly(SingleWebView c, ChangeEventArgs<bool> e, DependencyProperty d)
    {
        if (e.NewValue)
        {
            c.UrlInput.Visibility = Visibility.Collapsed;
            c.UrlElement.Visibility = Visibility.Visible;
        }
        else
        {
            c.UrlElement.Visibility = Visibility.Collapsed;
            c.UrlInput.Visibility = Visibility.Visible;
        }
    }
}
