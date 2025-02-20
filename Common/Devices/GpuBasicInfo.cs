using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Trivial.Text;

namespace Trivial.Devices;

/// <summary>
/// The CPU information.
/// </summary>
public sealed class GpuBasicInfo : BaseDeviceComponentBasicInfo
{
    private readonly static Dictionary<string, string> mapping = new()
    {
        { "Name", null },
        { "Caption", null },
        { "Description", null },
        { "AdapterCompatibility", null },
        { "AdapterDACType", "DacType" },
        { "AdapterRAM", "MemorySize" },
        { "DriverVersion", null },
        { "VideoArchitecture", null },
        { "VideoModeDescription", null }
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
    /// Gets the name of the GPU.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the caption of the GPU.
    /// </summary>
    public string Caption { get; internal set; }

    /// <summary>
    /// Gets the description of the GPU.
    /// </summary>
    public string Description { get; internal set; }

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
