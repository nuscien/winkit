using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;

namespace Trivial.Demo
{
    /// <summary>
    /// The item model of video.
    /// </summary>
    public class AuthorModel : BaseItemModel
    {
        /// <summary>
        /// Converts a JSON object to the model.
        /// </summary>
        /// <param name="json">The JSON source data.</param>
        /// <returns>The instance of the model.</returns>
        public static implicit operator AuthorModel(JsonObjectNode json)
        {
            if (json == null) return null;
            var model = new AuthorModel
            {
                Source = json,
                Id = json.TryGetStringValue("mid"),
                Name = json.TryGetStringValue("name")
            };
            model.SetImage(json, "face");
            return model;
        }
    }

    /// <summary>
    /// The item model of video.
    /// </summary>
    public class ItemModel : BaseItemModel
    {
        /// <summary>
        /// Gets or sets video kind.
        /// </summary>
        public int Kind
        {
            get => GetCurrentProperty<int>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// The publisher or author.
        /// </summary>
        public BaseItemModel Author
        {
            get => GetCurrentProperty<BaseItemModel>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Converts a JSON object to the model.
        /// </summary>
        /// <param name="json">The JSON source data.</param>
        /// <returns>The instance of the model.</returns>
        public static implicit operator ItemModel(JsonObjectNode json)
        {
            if (json == null) return null;
            var m = new ItemModel
            {
                Source = json,
                Id = json.TryGetStringValue("id") ?? json.TryGetStringValue("instanceID") ?? json.TryGetStringValue("aid"),
                Kind = json.TryGetInt32Value("tid") ?? 0,
                Name = json.TryGetStringValue("title"),
                PublishTime = json.TryGetDateTimeValue("pubdate", true) ?? json.TryGetDateTimeValue("lastModified"),
                Description = json.TryGetStringValue("desc") ?? json.TryGetStringValue("description") ?? json.TryGetStringValue("secondaryTitle"),
                Author = (AuthorModel)json.TryGetObjectValue("owner")
            };
            if (m.SetImage(json, "image") == null) m.SetImage(json, "pic");
            if (m.Description == "-" || m.Description == m.Name) m.Description = null;
            return m;
        }
    }
}
