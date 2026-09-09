using System;
using System.Collections.Generic;
using System.Text;

namespace Trivial.CommandLine;

internal class ProgressCli : BaseCommandVerb
{
    public static string Description => "Test progress component";

    protected override async Task OnProcessAsync(CancellationToken cancellationToken = default)
    {
        await Test(new(), cancellationToken);
        await Task.Delay(100, cancellationToken);
        await Test(new()
        {
            Kind = ConsoleProgressStyle.Kinds.AngleBracket,
            Size = ConsoleProgressStyle.Sizes.Wide,
        }, cancellationToken);
    }

    private async Task Test(ConsoleProgressStyle style, CancellationToken cancellationToken = default)
    {
        var console = CurrentConsole;
        var progress = console.WriteLine(style);
        for (var i = 0d; i < 1; i += 0.04)
        {
            progress.Report(i);
            await Task.Delay(100, cancellationToken);
        }

        progress.Succeed();
    }
}
