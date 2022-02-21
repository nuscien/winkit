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
        /// Initializes a new instance of the BaseItemModel class.
        /// </summary>
        public BaseItemModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BaseItemModel class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="image">The image URI.</param>
        /// <param name="source">The source.</param>
        public BaseItemModel(string name, Uri image, JsonObjectNode source = null)
        {
            Name = name;
            ImageUri = image;
            Source = source;
        }

        /// <summary>
        /// Initializes a new instance of the BaseItemModel class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="image">The image URI.</param>
        /// <param name="description">The description.</param>
        /// <param name="source">The source.</param>
        public BaseItemModel(string id, string name, Uri image, string description, JsonObjectNode source = null)
            : this(name, image, source)
        {
            Id = id;
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the BaseItemModel class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="image">The image URI.</param>
        /// <param name="description">The description.</param>
        /// <param name="publish">The publish time.</param>
        /// <param name="source">The source.</param>
        public BaseItemModel(string id, string name, Uri image, string description, DateTime publish, JsonObjectNode source = null)
            : this(id, name, image, description, source)
        {
            PublishTime = publish;
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Id
        {
            get => GetCurrentProperty<string>();
            protected set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Tests if the name is not null, empty nor consists only of white-space characters.
        /// </summary>
        public bool HasId => !string.IsNullOrWhiteSpace(Id);

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Tests if the name is not null, empty nor consists only of white-space characters.
        /// </summary>
        public bool HasName => !string.IsNullOrWhiteSpace(Name);

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
        public JsonObjectNode Source
        {
            get
            {
                return GetCurrentProperty<JsonObjectNode>();
            }

            protected set
            {
                SetCurrentProperty(value);
                OnSourceChanged();
            }
        }

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

        /// <summary>
        /// Occurs on source has changed.
        /// </summary>
        protected virtual void OnSourceChanged()
        {
        }
    }

    /// <summary>
    /// The base active item model.
    /// </summary>
    public abstract class BaseActiveItemModel : BaseItemModel
    {
        /// <summary>
        /// Occurs on action request.
        /// </summary>
        public abstract void Process();

        /// <summary>
        /// Occurs on action request.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        internal void ProcessRouted(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
            => Process();
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

        /// <summary>
        /// Gets or sets the additional tag.
        /// </summary>
        public object Tag { get; set; }
    }
}
