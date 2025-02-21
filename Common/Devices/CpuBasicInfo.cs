using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Devices;

/// <summary>
/// The CPU information.
/// </summary>
public sealed class CpuBasicInfo : BaseDeviceComponentBasicInfo
{
    /// <summary>
    /// The columns mapping.
    /// </summary>
    private readonly static Dictionary<string, string> mapping = new()
    {
        { "ProcessorId", nameof(Id) },
        { nameof(Family), null },
        { nameof(Manufacturer), null },
        { nameof(AddressWidth), null },
        { nameof(DataWidth), null },
        { "NumberOfCores", nameof(CoreCount) },
        { "NumberOfEnabledCore", nameof(EnabledCoreCount) },
        { "NumberOfLogicalProcessors", nameof(LogicalProcessorCount) },
        { nameof(ThreadCount), null },
        { nameof(VirtualizationFirmwareEnabled), null },
        { nameof(MaxClockSpeed), null },
        { nameof(SerialNumber), null },
    };

    /// <summary>
    /// Initializes a new instance of the CpuBasicInfo class.
    /// </summary>
    /// <param name="obj">The management base object.</param>
    internal CpuBasicInfo(ManagementBaseObject obj)
        : base(obj, mapping)
    {
    }

    /// <summary>
    /// Gets the processor identifier.
    /// </summary>
    public string Id { get; internal set; }

    /// <summary>
    /// Gets the family name of the CPU.
    /// </summary>
    public string Family { get; internal set; }

    /// <summary>
    /// Gets the manufacturer name of the CPU.
    /// </summary>
    public string Manufacturer { get; internal set; }

    /// <summary>
    /// Gets the width of address.
    /// </summary>
    public int AddressWidth { get; internal set; }

    /// <summary>
    /// Gets the width of data.
    /// </summary>
    public int DataWidth { get; internal set; }

    /// <summary>
    /// Gets the count of CPU core.
    /// </summary>
    public int CoreCount { get; internal set; }

    /// <summary>
    /// Gets the count of CPU core enabled.
    /// </summary>
    public int EnabledCoreCount { get; internal set; }

    /// <summary>
    /// Gets the count of logical processor.
    /// </summary>
    public int LogicalProcessorCount { get; internal set; }

    /// <summary>
    /// Gets the count of the thread.
    /// </summary>
    public int ThreadCount { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether enables the virtualization.
    /// </summary>
    public bool VirtualizationFirmwareEnabled { get; internal set; }

    /// <summary>
    /// Gets the maximum clock speed.
    /// </summary>
    public int MaxClockSpeed { get; internal set; }

    /// <summary>
    /// Gets the serial number.
    /// </summary>
    public string SerialNumber { get; internal set; }

    /// <summary>
    /// Gets all the device component information.
    /// </summary>
    /// <returns>A collection of the device component information.</returns>
    public static IEnumerable<CpuBasicInfo> Get()
        => Get<CpuBasicInfo>("SELECT * FROM Win32_Processor", obj => new(obj));
}
