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
public sealed class CpuBasicInfo : BaseDeviceComponentBasicInfo
{
    private readonly static Dictionary<string, string> mapping = new()
    {
        { "ProcessorId", "Id" },
        { "Name", null },
        { "Caption", null },
        { "Description", null },
        { "Family", null },
        { "Manufacturer", null },
        { "AddressWidth", null },
        { "DataWidth", null },
        { "NumberOfCores", "CoreCount" },
        { "NumberOfEnabledCore", "EnabledCoreCount" },
        { "NumberOfLogicalProcessors", "LogicalProcessorCount" },
        { "ThreadCount", null },
        { "VirtualizationFirmwareEnabled", null },
        { "MaxClockSpeed", null },
        { "SerialNumber", null },
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
    /// Gets the name of the CPU.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the caption of the CPU.
    /// </summary>
    public string Caption { get; internal set; }

    /// <summary>
    /// Gets the description of the CPU.
    /// </summary>
    public string Description { get; internal set; }

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
