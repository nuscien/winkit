using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Devices;

/// <summary>
/// The GPU information.
/// </summary>
public sealed class GpuBasicInfo : BaseDeviceComponentBasicInfo
{
    /// <summary>
    /// The columns mapping.
    /// </summary>
    private readonly static Dictionary<string, string> mapping = new()
    {
        { nameof(AdapterCompatibility), null },
        { "AdapterDACType", nameof(DacType) },
        { "AdapterRAM", nameof(MemorySize) },
        { nameof(DriverVersion), null },
        { nameof(VideoArchitecture), null },
        { nameof(VideoModeDescription), null }
    };

    /// <summary>
    /// Initializes a new instance of the GpuBasicInfo class.
    /// </summary>
    /// <param name="obj">The management base object.</param>
    internal GpuBasicInfo(ManagementBaseObject obj)
        : base(obj, mapping)
    {
    }

    /// <summary>
    /// Gets the adapter compatibility.
    /// </summary>
    public string AdapterCompatibility { get; internal set; }

    /// <summary>
    /// Gets the adapter DAC type.
    /// </summary>
    public string DacType { get; internal set; }

    /// <summary>
    /// Gets the count of RAM in byte.
    /// </summary>
    public long MemorySize { get; internal set; }

    /// <summary>
    /// Gets the driver version.
    /// </summary>
    public string DriverVersion { get; internal set; }

    /// <summary>
    /// Gets the kind code of video architecture.
    /// </summary>
    public int VideoArchitecture { get; internal set; }

    /// <summary>
    /// Gets the description of video mode including resolution and color bits.
    /// </summary>
    public string VideoModeDescription { get; internal set; }

    /// <summary>
    /// Gets all the device component information.
    /// </summary>
    /// <returns>A collection of the device component information.</returns>
    public static IEnumerable<GpuBasicInfo> Get()
        => Get<GpuBasicInfo>("SELECT * FROM Win32_VideoController", obj => new(obj));
}
