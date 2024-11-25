using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Trivial.Diagnostics;

/// <summary>
/// PropertyViewer.xaml 的交互逻辑
/// </summary>
internal partial class PropertyViewer : UserControl
{
    /// <summary>
    /// Initializes a new instance of the PropertyViewer class.
    /// </summary>
    public PropertyViewer()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the header.
    /// </summary>
    public object Header
    {
        get => HeaderElement.Content;
        set => HeaderElement.Content = value;
    }

    /// <summary>
    /// Gets or sets the string value.
    /// </summary>
    public string Value
    {
        get => ValueElement.Text;
        set => ValueElement.Text = value;
    }
}
