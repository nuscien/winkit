using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Trivial.Devices;

namespace Trivial.Users;

/// <summary>
/// The user account information of operation system.
/// </summary>
public sealed class OsUserAccountBasicInfo : BaseDeviceComponentBasicInfo
{
    /// <summary>
    /// The columns mapping.
    /// </summary>
    private readonly static Dictionary<string, string> mapping = new()
    {
        { "Disabled", nameof(IsDisabled) },
        { nameof(Domain), null },
        { nameof(FullName), null },
        { "LocalAccount", nameof(IsLocalAccount) },
        { "PasswordChangeable", nameof(IsPasswordChangeable) },
        { "PasswordExpires", nameof(IsPasswordExpired) },
        { "PasswordRequired", nameof(IsPasswordRequired) },
        { "SID", nameof(SecurityID) },
        { "SIDType", nameof(SecurityIDType) },
    };

    /// <summary>
    /// Initializes a new instance of the OsUserAccountBasicInfo class.
    /// </summary>
    /// <param name="obj">The management base object.</param>
    internal OsUserAccountBasicInfo(ManagementBaseObject obj)
        : base(obj, mapping)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the account is disabled.
    /// </summary>
    public bool IsDisabled { get; internal set; }

    /// <summary>
    /// Gets the domain.
    /// </summary>
    public string Domain { get; internal set; }

    /// <summary>
    /// Gets the full name.
    /// </summary>
    public string FullName { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the account is local.
    /// </summary>
    public bool IsLocalAccount { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the password is changeable.
    /// </summary>
    public bool IsPasswordChangeable { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the password is expired.
    /// </summary>
    public bool IsPasswordExpired { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the password is required.
    /// </summary>
    public bool IsPasswordRequired { get; internal set; }

    /// <summary>
    /// Gets the security identifier (SID).
    /// </summary>
    public string SecurityID { get; internal set; }

    /// <summary>
    /// Gets the type of the security identifier (SID).
    /// </summary>
    public string SecurityIDType { get; internal set; }

    /// <summary>
    /// Gets all the device component information.
    /// </summary>
    /// <returns>A collection of the device component information.</returns>
    public static IEnumerable<OsUserAccountBasicInfo> Get()
        => Get<OsUserAccountBasicInfo>("SELECT * FROM Win32_UserAccount", obj => new(obj));
}
