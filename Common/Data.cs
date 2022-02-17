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

    /// <summary>
    /// The menu item header information.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class BasicMenuItemInfo<T> : NameValueObservableProperties<T>
    {
        /// <summary>
        /// Initializes a new instance of the BasicMenuItemInfo class.
        /// </summary>
        public BasicMenuItemInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BasicMenuItemInfo class.
        /// </summary>
        /// <param name="name">The name.</param>
        public BasicMenuItemInfo(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the BasicMenuItemInfo class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public BasicMenuItemInfo(string name, T value) : this(name)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the BasicMenuItemInfo class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="icon">The icon URI.</param>
        public BasicMenuItemInfo(string id, string name, T value, Uri icon = null) : this(name, value)
        {
            Id = id;
            Icon = icon;
        }

        /// <summary>
        /// Gets or sets the additional identifier.
        /// </summary>
        public string Id
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
        /// Gets or sets the icon URI.
        /// </summary>
        public Uri Icon
        {
            get => GetCurrentProperty<Uri>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        public BasicMenuItemInfo<T> Group
        {
            get => GetCurrentProperty<BasicMenuItemInfo<T>>();
            set => SetCurrentProperty(value);
        }
    }
}
