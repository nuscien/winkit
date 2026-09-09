using System;
using System.Collections.Generic;
using System.Text;
using Trivial.Collection;
using Trivial.Data;

namespace Trivial.CommandLine;

internal class SelectionCli : BaseCommandVerb
{
    public static string Description => "Test advanced selection";

    protected override async Task OnProcessAsync(CancellationToken cancellationToken = default)
    {
        var console = GetConsole();
        await Task.CompletedTask;

        var selection = new SelectionData<string>();
        selection.Add(new('a', "Item A 测试数据一号 0123456789", "a"));
        selection.Add(new('b', "Item B 测试数据二号 0123456789", "b"));
        selection.Add(new('c', "Item C 测试数据三号 0123456789", "c"));
        selection.Add(new('d', "Item D 测试数据四号 0123456789", "d"));
        selection.Add(new('e', "Item E 测试数据五号 0123456789", "e"));
        selection.Add(new('f', "Item F 测试数据六号 0123456789", "f"));
        selection.Add(new('g', "Item G 测试数据七号 0123456789", "g"));
        var options = new SelectionConsoleOptions
        {
            MaxRow = 5,
            Column = 4,
            Prefix = "· ",
            SelectedPrefix = "→ ",
        };
        var sel = ConsoleRenderExtensions.Select(console, selection, options);
        console.WriteLine(sel.ToString());
    }
}
