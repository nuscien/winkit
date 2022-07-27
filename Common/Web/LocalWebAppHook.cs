using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trivial.Web;

namespace Trivial.Web;

/// <summary>
/// The global settings of the local web app.
/// </summary>
public static class LocalWebAppHook
{
    /// <summary>
    /// The locale strings.
    /// </summary>
    public static class CustomizedLocaleStrings
    {
        /// <summary>
        /// Gets or sets the title of error.
        /// </summary>
        public static string ErrorTitle { get; set; }

        /// <summary>
        /// Gets or sets the description of invalid file signature.
        /// </summary>
        public static string InvalidFileSignature { get; set; }

        /// <summary>
        /// Gets or sets the description of loading.
        /// </summary>
        public static string Loading { get; set; }
    }

    private static string hostId;

    /// <summary>
    /// Gets or sets the identifier of the host app.
    /// </summary>
    public static string HostId
    {
        get
        {
            if (!string.IsNullOrEmpty(hostId)) return hostId;
            try
            {
                var assembly = Assembly ?? System.Reflection.Assembly.GetEntryAssembly();
                if (assembly != null) hostId = assembly.GetName()?.Name;
            }
            catch (InvalidOperationException)
            {
            }
            catch (MemberAccessException)
            {
            }
            catch (TypeLoadException)
            {
            }
            catch (NotSupportedException)
            {
            }

            if (string.IsNullOrEmpty(hostId)) hostId = "x-private-wasdk-app";
            return hostId;
        }

        set
        {
            hostId = value;
        }
    }

    /// <summary>
    /// Gets or sets the additional string of host.
    /// </summary>
    public static string HostAdditionalString { get; set; }

    /// <summary>
    /// Gets or sets the handler to generate the default virtual host.
    /// </summary>
    public static Func<LocalWebAppManifest, LocalWebAppOptions, string> VirtualHostGenerator { get; set; }

    /// <summary>
    /// Gets or sets the handler to generate the error message for signature error..
    /// </summary>
    public static Func<LocalWebAppSignatureException, string> SignErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the handler to modify the HTTP client of update service.
    /// </summary>
    public static Action<Net.JsonHttpClient<Text.JsonObjectNode>> UpdateServiceClientHandler { get; set; }

    /// <summary>
    /// Gets or sets the handler to build dev package.
    /// </summary>
    public static Action<LocalWebAppPackageResult> BuildDevPackage { get; set; }

    /// <summary>
    /// Gets or sets the hanlder after updated.
    /// </summary>
    public static Action<LocalWebAppHost> OnUpdate { get; set; }

    /// <summary>
    /// Gets or sets the main assembly of the app.
    /// </summary>
    public static System.Reflection.Assembly Assembly { get; set; }
}
