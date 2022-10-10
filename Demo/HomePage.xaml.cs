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
using Trivial.Collection;
using Trivial.IO;
using Trivial.Security;
using Trivial.Text;
using Trivial.UI;
using Trivial.Web;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Trivial.Demo;

/// <summary>
/// The homepage
/// </summary>
public sealed partial class HomePage : Page
{
    /// <summary>
    /// Initializes a new instance of the HomePage class.
    /// </summary>
    public HomePage()
    {
        InitializeComponent();
        for (var i = 0; i < 1000; i++)
        {
            TextViewElement.Append(new[]
            {
                "Welcome!",
                "This is a demo for Trivial.WindowsKit library.",
                "It can be used for pages of news, video and product.",
                string.Empty,
                "This demo shows basic visual samples based on some 3rd-party data sources.",
                "Copyrights of these data sources are reserved by their owner.",
                string.Empty
            });
            if (i > 0) continue;
            TextViewElement.Append(new JsonObjectNode
            {
                { "title", "Trivial.WindowsKit" },
                { "3rd-party-notes", new JsonArrayNode
                {
                    "This demo shows basic visual samples based on some 3rd-party data sources.",
                    "Copyrights of these data sources are reserved by their owner."
                } },
                { "data", new JsonObjectNode
                {
                    { "number", 9876543210 },
                    { "true", true },
                    { "false", false },
                    { "null", null as string }
                } },
            });
            TextViewElement.Append(string.Empty);
        }

        FileBrowserElement.NavigateAsync(new DirectoryInfo("C:\\"));
    }
}
