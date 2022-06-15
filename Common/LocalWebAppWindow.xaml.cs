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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Text;
using Trivial.UI;
using Trivial.Web;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Trivial.UI;

/// <summary>
/// The local standalone web app window.
/// </summary>
public sealed partial class LocalWebAppWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the LocalWebAppWindow class.
    /// </summary>
    public LocalWebAppWindow()
    {
        InitializeComponent();
        var appWin = VisualUtility.TryGetAppWindow(this);
        try
        {
            if (appWin.TitleBar != null) appWin.TitleBar.IconShowOptions = Microsoft.UI.Windowing.IconShowOptions.HideIconAndSystemMenu;
        }
        catch (ArgumentException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (NotImplementedException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (ExternalException)
        {
        }
    }

    /// <summary>
    /// Initializes a new instance of the LocalWebAppWindow class.
    /// </summary>
    public LocalWebAppWindow(LocalWebAppOptions options) : this()
        => _ = MainElement.LoadAsync(options);

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="options">The options of the standalone web app.</param>
    public Task LoadAsync(LocalWebAppOptions options)
        => MainElement.LoadAsync(options);

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="host">The standalone web app host.</param>
    public Task LoadAsync(LocalWebAppHost host)
        => MainElement.LoadAsync(host);

    /// <summary>
    /// Loads data.
    /// </summary>
    /// <param name="host">The standalone web app host.</param>
    public Task LoadAsync(Task<LocalWebAppHost> host)
        => MainElement.LoadAsync(host);

    private void OnClosed(object sender, WindowEventArgs args)
        => MainElement.Close();

    private void OnTitleChanged(object sender, Data.DataEventArgs<string> e)
        => Title = e.Data;
}
