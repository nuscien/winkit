using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Controls;
using Trivial.Maths;
using Trivial.Text;

namespace Trivial.Diagnostics;

/// <summary>
/// The utils for diagnostics.
/// </summary>
public static class DiagnosticsUtility
{
    internal static JsonObjectNode TestData => new()
    {
        { "0", 123456789 },
        { "Good", "morning!" },
        { "Nothing", DBNull.Value },
        { "JSON", new JsonObjectNode
        {
            { "id", "test data" },
            { "b", true },
            { "arr", new JsonArrayNode
            {
                "text", -100, false, 3.14159265
            } },
        } },
        { "str", "some words here… 还有中文。some words here… 还有中文。some words here… 还有中文。some words here… 还有中文。" }
    };

    /// <summary>
    /// Creates tree view item collection.
    /// </summary>
    /// <param name="json">The JSON object node instance.</param>
    /// <param name="maxLen">The maximum length of path.</param>
    /// <returns>A collection of tree view item.</returns>
    public static IEnumerable<TreeViewItem> ToTreeViewItems(JsonObjectNode json, int maxLen = 100)
    {
        if (json == null || maxLen < 1) yield break;
        maxLen--;
        foreach (var prop in json)
        {
            if (string.IsNullOrWhiteSpace(prop.Key)) continue;
            if (prop.Value == null)
            {
                yield return ToTreeViewItem(new(prop.Key, JsonValues.Null), "null");
                continue;
            }

            switch (prop.Value.ValueKind)
            {
                case JsonValueKind.Undefined:
                    yield return ToTreeViewItem(prop, "undefined");
                    break;
                case JsonValueKind.Null:
                    yield return ToTreeViewItem(prop, "null");
                    break;
                case JsonValueKind.True:
                    yield return ToTreeViewItem(prop, "true");
                    break;
                case JsonValueKind.False:
                    yield return ToTreeViewItem(prop, "false");
                    break;
                case JsonValueKind.Number:
                    yield return ToTreeViewItem(prop, prop.Value.ToString());
                    break;
                case JsonValueKind.String:
                    {
                        if (prop.Value is IJsonValueNode<string> s)
                            yield return ToTreeViewItem(prop, JsonStringNode.ToJson(s.Value));
                        else
                            yield return ToTreeViewItem(prop, "string");
                        break;
                    }
                case JsonValueKind.Object:
                    {
                        if (prop.Value is JsonObjectNode j)
                        {
                            var r = ToTreeViewItem(prop, ToHeader(j));
                            if (!ReferenceEquals(j, json)) ToTreeViewItems(j, r.Items, maxLen);
                            yield return r;
                        }
                        else
                        {
                            yield return ToTreeViewItem(prop, string.Concat("object"));
                        }

                        break;
                    }
                case JsonValueKind.Array:
                    {
                        if (prop.Value is JsonArrayNode a)
                        {
                            var r = ToTreeViewItem(prop, string.Concat("array (", a.Count, ')'));
                            ToTreeViewItems(a, r.Items, maxLen);
                            yield return r;
                        }
                        else
                        {
                            yield return ToTreeViewItem(prop, string.Concat("array"));
                        }

                        break;
                    }
            }
        }
    }

    /// <summary>
    /// Creates tree view item collection.
    /// </summary>
    /// <param name="json">The JSON object node instance.</param>
    /// <param name="items">The collection to add the tree view item.</param>
    /// <param name="maxLen">The maximum length of path.</param>
    public static void ToTreeViewItems(this JsonObjectNode json, ItemCollection items, int maxLen = 100)
    {
        var sub = ToTreeViewItems(json);
        foreach (var rec in sub)
        {
            items.Add(rec);
        }
    }

    /// <summary>
    /// Creates tree view item collection.
    /// </summary>
    /// <param name="json">The JSON array node instance.</param>
    /// <param name="maxLen">The maximum length of path.</param>
    /// <returns>A collection of tree view item.</returns>
    public static IEnumerable<TreeViewItem> ToTreeViewItems(this JsonArrayNode json, int maxLen = 100)
    {
        if (json == null || maxLen < 1) yield break;
        var i = -1;
        maxLen--;
        foreach (var item in json)
        {
            i++;
            if (item == null)
            {
                yield return ToTreeViewItem(i, JsonValues.Null, "null");
                continue;
            }

            switch (item.ValueKind)
            {
                case JsonValueKind.Undefined:
                    yield return ToTreeViewItem(i, item, "undefined");
                    break;
                case JsonValueKind.Null:
                    yield return ToTreeViewItem(i, item, "null");
                    break;
                case JsonValueKind.True:
                    yield return ToTreeViewItem(i, item, "true");
                    break;
                case JsonValueKind.False:
                    yield return ToTreeViewItem(i, item, "false");
                    break;
                case JsonValueKind.Number:
                    yield return ToTreeViewItem(i, item, item.ToString());
                    break;
                case JsonValueKind.String:
                    {
                        if (item is IJsonValueNode<string> s)
                            yield return ToTreeViewItem(i, item, JsonStringNode.ToJson(s.Value));
                        else
                            yield return ToTreeViewItem(i, item, "string");
                        break;
                    }
                case JsonValueKind.Object:
                    {
                        if (item is JsonObjectNode j)
                        {
                            var r = ToTreeViewItem(i, item, ToHeader(j));
                            ToTreeViewItems(j, r.Items, maxLen);
                            yield return r;
                        }
                        else
                        {
                            yield return ToTreeViewItem(i, item, string.Concat("object"));
                        }

                        break;
                    }
                case JsonValueKind.Array:
                    {
                        if (item is JsonArrayNode a)
                        {
                            var r = ToTreeViewItem(i, item, string.Concat("array (", a.Count, ')'));
                            if (!ReferenceEquals(a, json)) ToTreeViewItems(a, r.Items, maxLen);
                            yield return r;
                        }
                        else
                        {
                            yield return ToTreeViewItem(i, item, string.Concat("array"));
                        }

                        break;
                    }
            }
        }
    }

    /// <summary>
    /// Creates tree view item collection.
    /// </summary>
    /// <param name="json">The JSON array node instance.</param>
    /// <param name="items">The collection to add the tree view item.</param>
    /// <param name="maxLen">The maximum length of path.</param>
    public static void ToTreeViewItems(this JsonArrayNode json, ItemCollection items, int maxLen = 100)
    {
        var sub = ToTreeViewItems(json);
        foreach (var rec in sub)
        {
            items.Add(rec);
        }
    }

    /// <summary>
    /// Creates a stack panel with details information of JSON.
    /// </summary>
    /// <param name="json">The JSON node.</param>
    /// <returns>The stack panel with details information of JSON.</returns>
    public static StackPanel ToStackPanel(this BaseJsonValueNode json)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        AddKeyValuePairs(json, panel.Children);
        return panel;
    }

    internal static void AddKeyValuePairs(BaseJsonValueNode json, UIElementCollection panel)
    {
        var kind = json.ValueKind;
        var kindElement = AddKeyValuePair(panel, InternalResource.Kind, kind.ToString());
        switch (kind)
        {
            case JsonValueKind.Undefined:
                kindElement.Value = InternalResource.Undefined;
                AddKeyValuePair(panel, InternalResource.Value, "undefined");
                break;
            case JsonValueKind.Null:
                kindElement.Value = InternalResource.Null;
                AddKeyValuePair(panel, InternalResource.Value, "null");
                break;
            case JsonValueKind.True:
                {
                    kindElement.Value = InternalResource.Boolean;
                    var p = AddKeyValuePair(panel, InternalResource.Value, true);
                    if (!InternalResource.True.Equals(p.Value, StringComparison.OrdinalIgnoreCase))
                        AddKeyValuePair(panel, InternalResource.Value, InternalResource.True);
                }

                break;
            case JsonValueKind.False:
                {
                    kindElement.Value = InternalResource.Boolean;
                    var p = AddKeyValuePair(panel, InternalResource.Value, false);
                    if (!InternalResource.False.Equals(p.Value, StringComparison.OrdinalIgnoreCase))
                        AddKeyValuePair(panel, InternalResource.Value, InternalResource.False);
                }

                break;
            case JsonValueKind.Number:
                {
                    kindElement.Value = InternalResource.Number;
                    if (json is IJsonValueNode<long> l)
                    {
                        kindElement.Value = l.Value <= JsonIntegerNode.MaxSafeInteger && l.Value >= JsonIntegerNode.MinSafeInteger ? InternalResource.Integer : $"{InternalResource.Integer} ({InternalResource.Unsafe})";
                        AddKeyValuePair(panel, InternalResource.Value, l.Value.ToString());
                        AddLocaleString(panel, l.Value);
                    }
                    else if (json is IJsonValueNode<double> d)
                    {
                        if (double.IsNaN(d.Value)) kindElement.Value = InternalResource.Null;
                        AddKeyValuePair(panel, InternalResource.Value, d.Value.ToString());
                        AddLocaleString(panel, d.Value);
                    }
                    else if (json is IJsonValueNode<decimal> d2)
                    {
                        AddKeyValuePair(panel, InternalResource.Value, json.ToString());
                        AddLocaleString(panel, (double)d2.Value);
                    }
                    else if (json is IJsonValueNode<float> f)
                    {
                        if (float.IsNaN(f.Value)) kindElement.Value = InternalResource.Null;
                        AddKeyValuePair(panel, InternalResource.Value, f.Value.ToString());
                        AddLocaleString(panel, f.Value);
                    }
                    else if (json is IJsonValueNode<int> i)
                    {
                        kindElement.Value = InternalResource.Integer;
                        AddKeyValuePair(panel, InternalResource.Value, json.ToString());
                        AddLocaleString(panel, i.Value);
                    }
                    else if (json is IJsonValueNode<short> i2)
                    {
                        kindElement.Value = InternalResource.Integer;
                        AddKeyValuePair(panel, InternalResource.Value, json.ToString());
                        AddLocaleString(panel, i2.Value);
                    }
                    else
                    {
                        AddKeyValuePair(panel, InternalResource.Value, json.ToString());
                    }
                }

                break;
            case JsonValueKind.String:
                {
                    kindElement.Value = InternalResource.String;
                    var s = json is IJsonValueNode<string> str ? str.Value : json.As<string>();
                    if (s == null)
                    {
                        kindElement.Value = InternalResource.Null;
                        break;
                    }

                    AddKeyValuePair(panel, InternalResource.ArrayLength, s.Length.ToString());
                    AddNote(panel, s);
                }

                break;
            case JsonValueKind.Object:
                {
                    kindElement.Value = InternalResource.Object;
                    if (json is not JsonObjectNode j)
                    {
                        AddNote(panel, json.ToString());
                        break;
                    }

                    AddKeyValuePair(panel, InternalResource.PropertyCount, j.Count.ToString());
                    AddKeyValuePair(panel, InternalResource.Type, j.TypeDiscriminator);
                    AddKeyValuePair(panel, "ID", j.TryGetId(out _));
                    AddKeyValuePair(panel, InternalResource.Schema, j.Schema);
                    AddKeyValuePair(panel, InternalResource.Comment, j.CommentValue);
                    AddNote(panel, j.ToString(IndentStyles.Compact));
                }

                break;
            case JsonValueKind.Array:
                {
                    kindElement.Value = InternalResource.Array;
                    if (json is not JsonArrayNode a)
                    {
                        AddNote(panel, json.ToString());
                        break;
                    }


                    AddKeyValuePair(panel, InternalResource.Count, a.Count.ToString());
                    AddNote(panel, a.ToString(IndentStyles.Compact));
                }
                break;
        }
    }

    internal static PropertyViewer AddKeyValuePair(UIElementCollection panel, object header, string value)
    {
        if (value == null) return null;
        var c = new PropertyViewer
        {
            Header = header,
            Value = value,
            VerticalContentAlignment = System.Windows.VerticalAlignment.Center
        };
        panel.Add(c);
        return c;
    }

    internal static bool AddLocaleString(UIElementCollection panel, long number)
    {
        AddKeyValuePair(panel, InternalResource.Hex, string.Concat("0x", Numbers.ToPositionalNotationString(number, 16)));
        var cultureName = Thread.CurrentThread.CurrentUICulture?.Name;
        if (cultureName == null) return false;

        if (cultureName.StartsWith("en-"))
        {
            AddKeyValuePair(panel, "English", EnglishNumerals.Default.ToString(number));
            return true;
        }

        if (cultureName.StartsWith("zh-Hant") || cultureName.StartsWith("zh-TW") || cultureName.StartsWith("zh-HK") || cultureName.StartsWith("zh-MO"))
        {
            AddKeyValuePair(panel, "中文數字", ChineseNumerals.Traditional.ToString(number));
            AddKeyValuePair(panel, "大寫數字", ChineseNumerals.TraditionalUppercase.ToString(number));
            return true;
        }

        if (cultureName.StartsWith("zh-"))
        {
            AddKeyValuePair(panel, "汉字数字", ChineseNumerals.Simplified.ToString(number));
            AddKeyValuePair(panel, "大写数字", ChineseNumerals.SimplifiedUppercase.ToString(number));
            return true;
        }

        if (cultureName.StartsWith("ja-"))
        {
            AddKeyValuePair(panel, "漢数字", JapaneseNumerals.Default.ToString(number));
            AddKeyValuePair(panel, "かんすうじ", JapaneseNumerals.Kana.ToString(number));
            return true;
        }

        return false;
    }

    internal static bool AddLocaleString(UIElementCollection panel, double number)
    {
        var cultureName = Thread.CurrentThread.CurrentUICulture?.Name;
        if (cultureName == null) return false;

        if (cultureName.StartsWith("en-"))
        {
            AddKeyValuePair(panel, "English", EnglishNumerals.Default.ToString(number));
            return true;
        }

        if (cultureName.StartsWith("zh-Hant") || cultureName.StartsWith("zh-TW") || cultureName.StartsWith("zh-HK") || cultureName.StartsWith("zh-MO"))
        {
            AddKeyValuePair(panel, "中文", ChineseNumerals.Traditional.ToString(number));
            AddKeyValuePair(panel, "大写", ChineseNumerals.TraditionalUppercase.ToString(number));
            return true;
        }

        if (cultureName.StartsWith("zh-"))
        {
            AddKeyValuePair(panel, "中文", ChineseNumerals.Simplified.ToString(number));
            AddKeyValuePair(panel, "大写", ChineseNumerals.SimplifiedUppercase.ToString(number));
            return true;
        }

        if (cultureName.StartsWith("ja-"))
        {
            AddKeyValuePair(panel, "漢数字", JapaneseNumerals.Default.ToString(number));
            AddKeyValuePair(panel, "かんすうじ", JapaneseNumerals.Kana.ToString(number));
            return true;
        }

        return false;
    }

    internal static void AddNote(UIElementCollection panel, string s)
    {
        panel.Add(new Separator());
        panel.Add(new TextBox
        {
            Background = null,
            BorderBrush = null,
            Padding = new(8),
            Text = s,
            AcceptsReturn = true,
        });
    }

    internal static PropertyViewer AddKeyValuePair(UIElementCollection panel, object header, bool value)
        => AddKeyValuePair(panel, header, value.ToString());


    private static TreeViewItem ToTreeViewItem(KeyValuePair<string, BaseJsonValueNode> prop, string value)
        => new()
        {
            Header = string.Concat(prop.Key, ": ", value),
            DataContext = prop,
            VerticalContentAlignment = System.Windows.VerticalAlignment.Center
        };

    private static TreeViewItem ToTreeViewItem(int i, BaseJsonValueNode node, string value)
        => new()
        {
            Header = string.Concat(i, i < 10 ? ".  " : ". ", value),
            DataContext = new KeyValuePair<int, BaseJsonValueNode>(i, node),
            VerticalContentAlignment = System.Windows.VerticalAlignment.Center
        };

    private static string ToHeader(JsonObjectNode json)
    {
        var sb = new StringBuilder("object (");
        sb.Append(json.Count);
        var id = json.TryGetId(out var idKey);
        if (string.IsNullOrEmpty(id))
        {
            sb.Append(')');
        }
        else
        {
            sb.Append(") ");
            sb.Append(idKey);
            sb.Append('=');
            sb.Append(id);
        }

        return sb.ToString();
    }
}
