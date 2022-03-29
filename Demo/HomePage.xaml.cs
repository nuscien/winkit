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
using System.Threading.Tasks;
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
        }

        _ = ThenAsync();
        FileBrowserElement.NavigateAsync(new DirectoryInfo("C:\\"));
    }

    private async Task ThenAsync()
    {
        await Task.Delay(5000);
        TextViewElement.ScrollIntoView(240);
        TextViewElement.SelectedLineNumber = 250;
    }
}
