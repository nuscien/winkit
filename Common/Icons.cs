using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.UI;

/// <summary>
/// The utilities of visual element.
/// </summary>
public static partial class VisualUtilities
{
    /// <summary>
    /// The icon set.
    /// Based on font Segoe Fluent Icons.
    /// </summary>
    public static class IconSet
    {
        /// <summary>
        /// The glyph for folder.
        /// </summary>
        internal const string FolderGlyph = "\xE8B7";

        /// <summary>
        /// The glyph for file.
        /// </summary>
        internal const string FileGlyph = "\xE130";

        /// <summary>
        /// The glyph for compressed file.
        /// </summary>
        internal const string ZipGlyph = "\xF012";

        /// <summary>
        /// The glyph for application.
        /// </summary>
        internal const string AppGlyph = "\xE737";

        /// <summary>
        /// The glyph for text file.
        /// </summary>
        internal const string TextGlyph = "\xF000";

        /// <summary>
        /// The glyph for CD.
        /// </summary>
        internal const string CompactDiskGlyph = "\xE958";

        /// <summary>
        /// The glyph for PDF.
        /// </summary>
        internal const string PdfGlyph = "\xEA90";

        /// <summary>
        /// Gets the icon of the specific file.
        /// </summary>
        /// <param name="name">The file name.</param>
        /// <returns>The file icon.</returns>
        public static string GetFileGlyph(string name)
        {
            name = name?.Trim();
            if (string.IsNullOrEmpty(name)) return null;
            if (name.EndsWith('/') || name.Replace(".", string.Empty).Length < 1) return FolderGlyph;
            var extPos = name.LastIndexOf('.');
            if (extPos < 0) return FileGlyph;
            var ext = (extPos > 0 ? name[extPos..]?.Trim() : name).ToLowerInvariant();
            return ext switch
            {
                ".txt" or ".json" or ".xml" or ".log" or ".ini" or ".yaml" or ".config" or ".md" or ".sgml" or ".rtf" or ".def" or ".diff" or ".patch" or ".rtx" or ".docx" or "doc" or ".dotx" or ".dot" or ".odt" or ".odm" => TextGlyph,
                ".csv" or ".tsv" => TextGlyph,
                ".jpg" or ".jpeg" or ".jpe" or ".webp" or ".png" or ".apng" or ".bmp" or ".dib" or ".tif" or ".tiff" or ".svg" or ".psd" or ".jpg" or ".jpeg" or ".jpe" or ".jfif" or ".cdr" or ".xbm" or ".emf" or ".wmf" or ".art" or ".heif" or ".hif" or ".heic" or ".rgb" or ".xbm" or ".xpm" or ".ief" or ".cmx" or ".cod" or ".ppm" => "\xE91B",
                ".gif" => "\xF4A9",
                ".mp4" or ".mp4v" or ".mpg4" or ".webm" or ".wmv" or ".avi" or ".h268" or ".h267" or ".h265" or ".h264" or ".av2" or ".av1" or ".mpeg" or ".mpg" or ".mpe" or ".3gp" or ".3gpp" or ".jpgv" or ".jpm" or ".jpgm" or ".ogv" or ".mov" or ".qt" or ".qtl" or ".dvd" or ".m4v" or ".mkv" or ".mk3d" or ".mks" or ".wm" or ".wmx" or ".wmp" or ".mpa" or ".flv" or ".rm" or ".rmvb" => "\xE173",
                ".flac" or ".mp3" or ".wma" or ".weba" or ".mpga" or ".mp2" or ".mp2a" or ".m2a" or ".m3a" or ".au" or ".snd" or ".smd" or ".smx" or ".smz" or ".wav" or ".wave" or ".aac" or ".m4a" or ".mp4a" or ".ogg" or ".oga" or ".spx" or ".pya" or ".aif" or ".aiff" or ".aifc" or ".mka" or ".m3u" or ".wax" or ".cda" => "\xE189",
                ".mid" or ".midi" or ".kar" or ".rmi" => "\xE189",
                ".bat" or ".ps1" or ".sh" => "\xE756",
                ".iso" => CompactDiskGlyph,
                ".msix" or ".msi" or ".msu" or ".msixbundle" or ".appxbundle" or ".appinstaller" => "\xE178",
                ".exe" or ".app" or ".appx" or ".wasm" or ".xap" or ".application" => AppGlyph,
                ".apk" or ".aab" => "\xE1C9",
                ".pdf" => PdfGlyph,
                ".epub" or ".xps" or ".oxps" => "\xE82D",
                ".zip" or ".7z" or ".rar" or ".z" or ".tar" or ".br" or ".gz" or ".gzip" or ".cab" or ".tgz" => ZipGlyph,
                ".pptx" or ".ppt" or ".potx" or ".pot" or ".ppsx" or ".pps" or ".odp" => "\xEB05",
                ".xlsx" or ".xls" or ".xltx" or ".xlt" or ".ods" => "\xE9F9",
                ".htm" or ".html" or ".hta" or ".shtml" or ".htt" or ".hxt" or ".dtd" or ".rss" or ".atom" or ".mht" => "\xE12B",
                ".ink" or ".inkml" => "\xE929",
                ".ics" or ".ifb" => "\xE163",
                ".eml" => "\xE119",
                ".nws" => "\xE12A",
                ".vcf" => "\xE136",
                ".chm" or ".hlp" => "\xE11B",
                ".lnk" or ".url" or ".uri" or ".uris" or ".urls" => "\xE71B",
                ".msg" => "\xE206",
                ".mml" => "\xF6BA",
                ".3ds" or ".cad" => "\xF158",
                ".cer" or ".cat" or ".pfx" or ".p12" or ".p10" or ".p8" => "\xEB95",
                ".pem" or ".pki" or ".secret" => "\xE8D7",
                ".font" or ".ttf" or ".ttc" or ".eot" or ".pcf" or ".snf" or ".otf" or ".woff" or ".woff2" => "\xE185",
                _ => FileGlyph
            };
        }

        /// <summary>
        /// Gets the icon of the specific file.
        /// </summary>
        /// <param name="name">The file name.</param>
        /// <param name="style">The optional style of the icon.</param>
        /// <returns>The file icon.</returns>
        public static FontIcon GetFileIcon(string name, Style style = null)
        {
            var glyph = GetFileGlyph(name);
            if (glyph == null) return null;
            return new FontIcon
            {
                Glyph = glyph,
                Style = style
            };
        }

        /// <summary>
        /// Gets the icon of directory.
        /// </summary>
        /// <param name="style">The optional style of the icon.</param>
        /// <returns>The directory icon.</returns>
        public static FontIcon GetDirectoryIcon(Style style = null)
            => new()
            {
                Glyph = FolderGlyph,
                Style = style
            };
    }
}
