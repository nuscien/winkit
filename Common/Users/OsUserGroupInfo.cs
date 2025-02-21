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
/// The user group information of operation system.
/// </summary>
public sealed class OsUserGroupBasicInfo : BaseDeviceComponentBasicInfo
{
    /// <summary>
    /// The columns mapping.
    /// </summary>
    private readonly static Dictionary<string, string> mapping = new()
    {
        { nameof(Domain), null },
        { "LocalAccount", nameof(IsLocalAccount) },
        { nameof(SID), null },
        { nameof(SIDType), null },
    };

    /// <summary>
    /// Initializes a new instance of the OsUserGroupBasicInfo class.
    /// </summary>
    /// <param name="obj">The management base object.</param>
    internal OsUserGroupBasicInfo(ManagementBaseObject obj)
        : base(obj, mapping)
    {
    }

    /// <summary>
    /// Gets the domain.
    /// </summary>
    public string Domain { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the account is local.
    /// </summary>
    public bool IsLocalAccount { get; internal set; }

    /// <summary>
    /// Gets the SID.
    /// </summary>
    public string SID { get; internal set; }

    /// <summary>
    /// Gets the SID type.
    /// </summary>
    public string SIDType { get; internal set; }

    /// <summary>
    /// Gets all the device component information.
    /// </summary>
    /// <returns>A collection of the device component information.</returns>
    public static IEnumerable<OsUserGroupBasicInfo> Get()
        => Get<OsUserGroupBasicInfo>("SELECT * FROM Win32_Group", obj => new(obj));
}
