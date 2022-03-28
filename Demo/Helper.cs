using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trivial.UI;

namespace Trivial.Demo;

internal static class Helper
{
    /// <summary>
    /// Creates a tile collection control with the specific data.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="col">The collection of item model.</param>
    /// <returns>The control of tile collection.</returns>
    public static TileCollection CreateTileCollection(string title, IEnumerable<ItemModel> col, Action<TileItem, ItemModel, RoutedEventArgs> onClick)
    {
        if (col == null) return null;
        var c = new TileCollection
        {
            Title = title,
            Style = VisualUtility.GetResource<Style>("CommonTilesList"),
            ItemStyle = VisualUtility.GetResource<Style>("DefaultTileItem")
        };
        foreach (var item in col)
        {
            if (string.IsNullOrWhiteSpace(item?.Name) || string.IsNullOrWhiteSpace(item.Id)) continue;
            c.AddItem(item.Name, item.ImageUri, true, item.Description, (sender, e) =>
            {
                onClick?.Invoke(sender as TileItem, item, e);
            });
        }

        return c;
    }
}
