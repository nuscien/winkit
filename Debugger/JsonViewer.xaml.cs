using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
using Trivial.Maths;
using Trivial.Text;

namespace Trivial.Diagnostics;

/// <summary>
/// The viewer for JSON.
/// </summary>
public partial class JsonViewer : UserControl
{
    private BaseJsonValueNode value;

    /// <summary>
    /// Initializes a new instance of the JsonViewer.
    /// </summary>
    public JsonViewer()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the JSON object or array.
    /// </summary>
    /// <param name="value">The JSON to display.</param>
    public void Set(BaseJsonValueNode value)
    {
        this.value = value;
        if (value is JsonObjectNode json)
        {
            DiagnosticsUtility.ToTreeViewItems(json, DocumentTree.Items);
            TreeColumn.Width = new(1, GridUnitType.Star);
            DetailsColumn.Width = new(3, GridUnitType.Star);
        }
        else if (value is JsonArrayNode arr)
        {
            DiagnosticsUtility.ToTreeViewItems(arr, DocumentTree.Items);
            TreeColumn.Width = new(1, GridUnitType.Star);
            DetailsColumn.Width = new(3, GridUnitType.Star);
        }
        else
        {
            TreeColumn.Width = new(0, GridUnitType.Star);
            DetailsColumn.Width = new(4, GridUnitType.Star);
        }

        Show(value);
    }

    /// <summary>
    /// Sets the JSON object.
    /// </summary>
    /// <param name="value">The JSON to display.</param>
    public void Set(JsonNode value)
        => Set((JsonObjectNode)value);

    /// <summary>
    /// Sets the JSON array.
    /// </summary>
    /// <param name="value">The JSON to display.</param>
    public void Set(JsonArray value)
        => Set((JsonArrayNode)value);

    private void DocumentTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is null) Show(value);
        else if (e.NewValue is TreeViewItem item) Show(item);
    }

    private void Show(TreeViewItem item)
    {
        var data = item.DataContext;
        if (data is null) Show(value);
        else if (data is JsonObjectNode json) Show(json);
        else if (data is JsonArrayNode arr) Show(arr);
        else if (data is BaseJsonValueNode node) Show(node);
        else if (data is KeyValuePair<string, BaseJsonValueNode> kvp1) Show(kvp1);
        else if (data is KeyValuePair<int, BaseJsonValueNode> kvp2) Show(kvp2);
    }

    private void Show(KeyValuePair<string, BaseJsonValueNode> kvp)
    {
        DetailsPanel.Children.Clear();
        DiagnosticsUtility.AddKeyValuePair(DetailsPanel.Children, InternalResource.PropertyKey, kvp.Key);
        DetailsPanel.Children.Add(new Separator());
        DiagnosticsUtility.AddKeyValuePairs(kvp.Value, DetailsPanel.Children);
    }

    private void Show(KeyValuePair<int, BaseJsonValueNode> kvp)
    {
        DetailsPanel.Children.Clear();
        DiagnosticsUtility.AddKeyValuePair(DetailsPanel.Children, InternalResource.Index, kvp.Key.ToString());
        DetailsPanel.Children.Add(new Separator());
        DiagnosticsUtility.AddKeyValuePairs(kvp.Value, DetailsPanel.Children);
    }

    private void Show(BaseJsonValueNode json)
    {
        DetailsPanel.Children.Clear();
        DiagnosticsUtility.AddKeyValuePairs(json, DetailsPanel.Children);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (DocumentTree.SelectedItem is not TreeViewItem item) Show(value);
        else item.IsSelected = false;
    }
}
