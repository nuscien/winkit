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
        var console = CurrentConsole;
        await Task.CompletedTask;

        var selection = new SelectionData<string>
        {
            { 'a', "Item A 测试数据一号 0123456789", "a" },
            { 'b', "Item B 测试数据二号 0123456789", "b" },
            { 'c', "Item C 测试数据三号 0123456789", "c" },
            { 'd', "Item D 测试数据四号 0123456789", "d" },
            { 'e', "Item E 测试数据五号 0123456789", "e" },
            { 'f', "Item F 测试数据六号 0123456789", "f" },
            { 'g', "Item G 测试数据七号 0123456789", "g" },
            { "Item H 测试数据八号 0123456789", "h" },
            { "Item I 测试数据九号 0123456789", "i" },
            { "Item J 测试数据十号 0123456789", "j" },
            { "Item K 测试数据11号", "k" },
            { "Item L 测试数据12号", "l" },
            { "Item M 测试数据13号", "m" },
            { "Item N 测试数据14号", "n" },
            { "Item O 测试数据15号", "o" },
            { "Item P 测试数据16号", "p" },
            { "Item Q 测试数据17号", "q" },
            { "Item R 测试数据18号", "r" },
            { "Item S 测试数据19号", "s" },
            { "Item T 测试数据20号", "t" },
            { "Item U 测试数据21号", "u" },
            { "Item V 测试数据22号", "v" },
            { "Item W 测试数据23号", "w" },
            { "Item X 测试数据24号", "x" },
            { "Item Y 测试数据25号", "y" },
            { "Item Z 测试数据26号", "z" }
        };
        var options = new SelectionConsoleOptions
        {
            MaxRow = 5,
            Column = 4,
            Prefix = " ",
            SelectedPrefix = "→ ",
        };
        var sel = ConsoleRenderExtensions.Select(console, selection, options);
        console.WriteLine(sel.ToString());
    }
}
