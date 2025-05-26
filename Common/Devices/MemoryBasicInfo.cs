using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Trivial.Devices;

/// <summary>
/// The physical memory information.
/// </summary>
public sealed class MemoryBasicInfo : BaseDeviceComponentBasicInfo
{
    [StructLayout(LayoutKind.Sequential)]
    private struct UnmanagedInfo
    {
        /// <summary>
        /// Current structure size.
        /// </summary>
        public uint dwLength;

        /// <summary>
        /// Current memory utilization.
        /// </summary>
        public uint dwMemoryLoad;

        /// <summary>
        /// Total physical memory size.
        /// </summary>
        public ulong ullTotalPhys;

        /// <summary>
        /// Available physical memory size
        /// </summary>
        public ulong ullAvailPhys;

        /// <summary>
        /// Total exchange file size.
        /// </summary>
        public ulong ullTotalPageFile;

        /// <summary>
        /// Available exchange file size.
        /// </summary>
        public ulong ullAvailPageFile;

        /// <summary>
        /// Total virtual memory size.
        /// </summary>
        public ulong ullTotalVirtual;

        /// <summary>
        /// Available virtual memory size.
        /// </summary>
        public ulong ullAvailVirtual;

        /// <summary>
        /// Keep this value always zero.
        /// </summary>
        public ulong ullAvailExtendedVirtual;
    }

    private static class UnmanagedServices
    {
        /// <summary>
        /// Gets the current memory information and status.
        /// </summary>
        /// <param name="mi">The struct to transfer data.</param>
        /// <returns>true if get succeeded; otherwise, false.</returns>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GlobalMemoryStatusEx(ref UnmanagedInfo mi);

        /// <summary>
        /// Gets the current memory information and status.
        /// </summary>
        /// <returns>The information and status of memory.</returns>
        internal static UnmanagedInfo GetMemoryStatus()
        {
            var memoryInfo = new UnmanagedInfo();
            memoryInfo.dwLength = (uint)Marshal.SizeOf(memoryInfo);
            GlobalMemoryStatusEx(ref memoryInfo);
            return memoryInfo;
        }
    }

    /// <summary>
    /// The columns mapping.
    /// </summary>
    private readonly static Dictionary<string, string> mapping = new()
    {
        { nameof(BankLabel), null },
        { nameof(Capacity), null },
        { nameof(ConfiguredClockSpeed), null },
        { nameof(Speed), null },
        { nameof(ConfiguredVoltage), null },
        { nameof(Manufacturer), null },
        { nameof(PartNumber), null },
        { nameof(DeviceLocator), null },
        { nameof(SerialNumber), null },
        { "SKU", nameof(Sku) },
        { nameof(DataWidth), null },
        { nameof(TotalWidth), null },
        { nameof(Tag), null },
    };

    /// <summary>
    /// Initializes a new instance of the MemoryBasicInfo class.
    /// </summary>
    /// <param name="obj">The management base object.</param>
    internal MemoryBasicInfo(ManagementBaseObject obj)
        : base(obj, mapping)
    {
    }

    /// <summary>
    /// Gets the bank label.
    /// </summary>
    public string BankLabel { get; internal set; }

    /// <summary>
    /// Gets the capacity.
    /// </summary>
    public long Capacity { get; internal set; }

    /// <summary>
    /// Gets the clock speed configured.
    /// </summary>
    public int ConfiguredClockSpeed { get; internal set; }

    /// <summary>
    /// Gets the speed.
    /// </summary>
    public int Speed { get; internal set; }

    /// <summary>
    /// Gets the voltage configured.
    /// </summary>
    public int ConfiguredVoltage { get; internal set; }

    /// <summary>
    /// Gets the manufacturer.
    /// </summary>
    public string Manufacturer { get; internal set; }

    /// <summary>
    /// Gets the part number.
    /// </summary>
    public string PartNumber { get; internal set; }

    /// <summary>
    /// Gets the device locator.
    /// </summary>
    public string DeviceLocator { get; internal set; }

    /// <summary>
    /// Gets the serial number.
    /// </summary>
    public string SerialNumber { get; internal set; }

    /// <summary>
    /// Gets the SKU.
    /// </summary>
    public string Sku { get; internal set; }

    /// <summary>
    /// Gets the data width.
    /// </summary>
    public int DataWidth { get; internal set; }

    /// <summary>
    /// Gets the total width.
    /// </summary>
    public int TotalWidth { get; internal set; }

    /// <summary>
    /// Gets the tag.
    /// </summary>
    public string Tag { get; internal set; }

    /// <summary>
    /// Gets all the device component information.
    /// </summary>
    /// <returns>A collection of the device component information.</returns>
    public static IEnumerable<MemoryBasicInfo> Get()
        => Get<MemoryBasicInfo>("SELECT * FROM Win32_PhysicalMemory", obj => new(obj));
}
