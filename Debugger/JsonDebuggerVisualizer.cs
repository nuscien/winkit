using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Diagnostics;
using Trivial.Reflection;
using Trivial.Text;

#if NETFRAMEWORK
[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(JsonDebuggerVisualizer),
typeof(JsonObjectSource),
Target = typeof(JsonObjectNode),
Description = "JSON object node visualizer")]
[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(JsonDebuggerVisualizer),
typeof(JsonObjectSource),
Target = typeof(JsonArrayNode),
Description = "JSON array node visualizer")]
#endif

namespace Trivial.Diagnostics;

#if NETFRAMEWORK
/// <summary>
/// The Visual Studio debugger visualizer for JSON.
/// </summary>
internal class JsonDebuggerVisualizer : DialogDebuggerVisualizer
{
    /// <summary>
    /// Initializes a new instance of the JsonDebuggerVisualizer class.
    /// </summary>
    public JsonDebuggerVisualizer()
        : base(FormatterPolicy.Json)
    {
    }

    /// <inheritdoc />
    protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
    {
        //var data = (objectProvider as IVisualizerObjectProvider3)?.GetObject<JsonObjectNode>();
        var data = (objectProvider as IVisualizerObjectProvider3)?.GetObject<JsonObjectNode>();
        var form = new JsonViewerWindow();
        form.Set(data);
        form.ShowDialog();
    }
}
#endif

/// <summary>
/// The visualizer object source for JSON.
/// </summary>
internal class JsonObjectSource : VisualizerObjectSource
{
    /// <inheritdoc/>
    public override void GetData(object target, Stream outgoingData)
    {
        var o = target as BaseJsonValueNode;
        var writer = new Utf8JsonWriter(outgoingData);
        o.WriteTo(writer);
    }

    /// <inheritdoc/>
    public override object CreateReplacementObject(object target, Stream incomingData)
        => JsonSerializer.Deserialize<BaseJsonValueNode>(incomingData);
}
