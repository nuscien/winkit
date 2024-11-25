#if NETFRAMEWORK
using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(Trivial.Diagnostics.JsonObjectNodeDebuggerVisualizer),
typeof(VisualizerObjectSource),
Target = typeof(JsonObjectNode),
Description = "JSON object node visualizer")]
[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(Trivial.Diagnostics.JsonObjectNodeDebuggerVisualizer),
typeof(VisualizerObjectSource),
Target = typeof(JsonArrayNode),
Description = "JSON array node visualizer")]

namespace Trivial.Diagnostics;

/// <summary>
/// The Visual Studio debugger visualizer for JSON object node.
/// </summary>
public class JsonObjectNodeDebuggerVisualizer : DialogDebuggerVisualizer
{
    /// <summary>
    /// Initializes a new instance of the JsonObjectNodeDebuggerVisualizer class.
    /// </summary>
    public JsonObjectNodeDebuggerVisualizer()
        : base(FormatterPolicy.Json)
    {
    }

    /// <inheritdoc />
    protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
    {
        var data = (objectProvider as IVisualizerObjectProvider3)?.GetObject<JsonObjectNode>();
        var form = new JsonViewerWindow();
        form.Set(data);
        form.ShowDialog();
    }
}
#endif
