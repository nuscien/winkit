using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Data
{
    /// <summary>
    /// The base item model.
    /// </summary>
    public class BaseItemModel : ObservableProperties
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Id
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the image URI.
        /// </summary>
        public Uri ImageUri
        {
            get => GetCurrentProperty<Uri>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the publish time.
        /// </summary>
        public DateTime? PublishTime
        {
            get => GetCurrentProperty<DateTime>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets the JSON data source.
        /// </summary>
        public JsonObjectNode Source { get; protected set; }

        /// <summary>
        /// Sets image.
        /// </summary>
        /// <param name="url">The URL of image.</param>
        /// <returns>The image URI.</returns>
        public Uri SetImage(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            try
            {
                var uri = new Uri(url);
                ImageUri = uri;
                return uri;
            }
            catch (FormatException)
            {
            }

            return null;
        }

        /// <summary>
        /// Sets image.
        /// </summary>
        /// <param name="json">The source data.</param>
        /// <param name="propertyKey">The property key of image.</param>
        /// <returns>The image URI.</returns>
        public Uri SetImage(JsonObjectNode json, string propertyKey)
            => SetImage(json?.TryGetStringValue(propertyKey));
    }
}
