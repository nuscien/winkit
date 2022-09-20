using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Trivial.CommandLine;

namespace Trivial.Web;

class Program
{
    static Task Main(string[] args)
    {
        Console.WriteLine("CLI of Local Web App - Trivial");
        var dispatcher = new CommandDispatcher();
        dispatcher.Register<BuildVerb>("build");
        return dispatcher.ProcessAsync();
    }
}
