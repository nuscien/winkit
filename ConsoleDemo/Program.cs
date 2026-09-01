using System.Drawing;
using Trivial.CommandLine;

var console = StyleConsole.Default;
console.WriteLine(new LinearGradientConsoleStyle(null, Color.FromArgb(15, 250, 250), Color.FromArgb(85, 168, 255))
{
    Bold = true
}, "Demo of Trivial CommandLine");
console.WriteLine();
var dispatcher = new CommandDispatcher();
dispatcher.Register<SelectionCli>("select");
await dispatcher.ProcessAsync();
