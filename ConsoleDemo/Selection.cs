using System;
using System.Collections.Generic;
using System.Text;
using Trivial.Collection;
using Trivial.Data;

namespace Trivial.CommandLine;

internal class SelectionCli : BaseCommandVerb
{
    protected override async Task OnProcessAsync(CancellationToken cancellationToken = default)
    {
        var console = CurrentConsole;
        await Task.CompletedTask;

        var selection = new SelectionData<string>();
        selection.Add('a', "Item A 测试数据一号 0123456789", "a");
        selection.Add('b', "Item B 测试数据二号 0123456789", "b");
        selection.Add('c', "Item C 测试数据三号 0123456789", "c");
        selection.Add('d', "Item D 测试数据四号 0123456789", "d");
        selection.Add('e', "Item E 测试数据五号 0123456789", "e");
        selection.Add('f', "Item F 测试数据六号 0123456789", "f");
        selection.Add('g', "Item G 测试数据七号 0123456789", "g");
        selection.Add("Item H 测试数据八号 0123456789", "h");
        selection.Add("Item I 测试数据九号 0123456789", "i");
        selection.Add("Item J 测试数据十号 0123456789", "j");
        selection.Add("Item K 测试数据11号", "k");
        selection.Add("Item L 测试数据12号", "l");
        selection.Add("Item M 测试数据13号", "m");
        selection.Add("Item N 测试数据14号", "n");
        selection.Add("Item O 测试数据15号", "o");
        selection.Add("Item P 测试数据16号", "p");
        selection.Add("Item Q 测试数据17号", "q");
        selection.Add("Item R 测试数据18号", "r");
        selection.Add("Item S 测试数据19号", "s");
        selection.Add("Item T 测试数据20号", "t");
        selection.Add("Item U 测试数据21号", "u");
        selection.Add("Item V 测试数据22号", "v");
        selection.Add("Item W 测试数据23号", "w");
        selection.Add("Item X 测试数据24号", "x");
        selection.Add("Item Y 测试数据25号", "y");
        selection.Add("Item Z 测试数据26号", "z");
        var options = new SelectionConsoleOptions
        {
            MaxRow = 5,
            Column = 4,
            Prefix = " ",
            SelectedPrefix = "→ ",
        };
        var sel = ConsoleRenderExtensions.Select(console, selection, options);
        var text = sel.Title ?? sel.Value;
        if (string.IsNullOrWhiteSpace(text)) text = sel.InputType.ToString();
        console.WriteLine(text);
    }
}
