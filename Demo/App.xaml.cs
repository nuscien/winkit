using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Trivial.Web;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Trivial.Demo
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            //m_window = new MainWindow();
            //m_window.Activate();
            _ = OnInitAsync();
        }

        private static async Task OnInitAsync()
        {
            LocalWebAppHost.SetHostId("WinKitDemo");
            await LocalWebAppHost.LoadAsync(null as System.Reflection.Assembly);
            var win = UI.LocalWebAppHubPage.CreateWindow(null, out var page);
            page.AddAdditionalDevApp(new()
            {
                ResourcePackageId = "@",
                DisplayName = "List & Files",
                LocalPath = "List & Files"
            });
            page.AddAdditionalDevApp(new()
            {
                ResourcePackageId = "@",
                DisplayName = "NBC",
                Icon = "ms-appx:///Nbc/nbc_logo.png",
                LocalPath = "NBC"
            });
            page.AddAdditionalDevApp(new()
            {
                ResourcePackageId = "@",
                DisplayName = "Bilibili",
                Icon = "ms-appx:///Bilibili/bilibili_logo.png",
                LocalPath = "Bilibili"
            });
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(0, 10, 0, 10)
            };
            page.MoreContent = panel;
            page.PreventAppHandler = (p, info, dev) =>
            {
                if (info?.ResourcePackageId != "@") return false;
                var key = info.LocalPath?.Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(key)) return true;
                switch (key)
                {
                    case "list & files":
                        win.Add("List & Files", new HomePage());
                        break;
                    case "nbc":
                        win.Add("NBC", new Nbc.NewsPage());
                        break;
                    case "bilibili":
                        win.Add("Bilibili", new Bilibili.ChannelPage());
                        break;
                    case "refresh":
                        try
                        {
                            page.Refresh();
                            page.ShowDevPanel(false);
                        }
                        catch (InvalidOperationException)
                        {
                        }
                        catch (NullReferenceException)
                        {
                        }

                        break;
                }

                return true;
            };
            AddHyperlinkButton(panel, "Refresh", (sender, ev) =>
            {
                try
                {
                    page.Refresh();
                    page.ShowDevPanel(false);
                }
                catch (InvalidOperationException)
                {
                }
                catch (NullReferenceException)
                {
                }
            });
            win.Activate();
        }

        private static HyperlinkButton AddHyperlinkButton(Panel parent, string text, RoutedEventHandler click)
        {
            var button = new HyperlinkButton
            {
                Content = text,
                Padding = new Thickness(20, 10, 20, 10)
            };
            button.Click += click;
            if (parent != null) parent.Children.Add(button);
            return button;
        }
    }
}
