﻿using Microsoft.UI.Xaml;
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
using Trivial.UI;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Trivial.Demo;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "Demo - WindowsKit - Trivial";
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var key = (args.SelectedItem as NavigationViewItem)?.Tag as string;
        if (string.IsNullOrWhiteSpace(key)) return;
        ContentFrame.Navigate(key.ToLowerInvariant() switch
        {
            "nbc" => typeof(Nbc.NewsPage),
            "bilibili" => typeof(Bilibili.ChannelPage),
            _ => typeof(HomePage)
        }, null);
    }
}