using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Trivial.Text;

namespace Trivial.Diagnostics;

/// <summary>
/// JsonViewerWindow.xaml 的交互逻辑
/// </summary>
public partial class JsonViewerWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the JsonViewerWindow class.
    /// </summary>
    public JsonViewerWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the JSON object or array.
    /// </summary>
    /// <param name="value">The JSON object or array to display.</param>
    public void Set(BaseJsonValueNode value)
        => MainViewer.Set(value);

    /// <summary>
    /// Sets the JSON object.
    /// </summary>
    /// <param name="value">The JSON to display.</param>
    public void Set(JsonNode value)
        => MainViewer.Set((JsonObjectNode)value);

    /// <summary>
    /// Sets the JSON array.
    /// </summary>
    /// <param name="value">The JSON to display.</param>
    public void Set(JsonArray value)
        => MainViewer.Set((JsonArrayNode)value);
}
