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
/// The base information of device component.
/// </summary>
public abstract class BaseDeviceComponentBasicInfo
{
    /// <summary>
    /// Initializes a new instance of the BaseDeviceComponentBasicInfo class.
    /// </summary>
    /// <param name="obj">The management base object.</param>
    /// <param name="mapping">The property key mapping.</param>
    protected BaseDeviceComponentBasicInfo(ManagementBaseObject obj, IDictionary<string, string> mapping)
    {
        if (obj == null || mapping == null) return;
        var type = GetType();
        foreach (var prop in mapping)
        {
            if (string.IsNullOrWhiteSpace(prop.Key)) continue;
            var v = obj[prop.Key];
            var property = type.GetProperty(string.IsNullOrWhiteSpace(prop.Value) ? prop.Key : prop.Value);
            if (property == null || !property.CanWrite) continue;
            var vType = v?.GetType();
            try
            {
                if (property.PropertyType == typeof(string))
                {
                    if (v is null) property.SetValue(this, null);
                    else property.SetValue(this, v is string s ? s : v.ToString());
                    continue;
                }
                else if (property.PropertyType == typeof(int))
                {
                    if (v is null)
                    {
                    }
                    else if (vType.IsEnum)
                    {
                        property.SetValue(this, (int)v);
                    }
                    else if (vType.IsValueType)
                    {
                        if (v is int i) property.SetValue(this, i);
                        else if (v is bool b) property.SetValue(this, b ? 1 : 0);
                        else if (v is uint i0) property.SetValue(this, (int)i0);
                        else if (v is long i1) property.SetValue(this, (int)i1);
                        else if (v is short i3) property.SetValue(this, (int)i3);
                        else if (v is ulong i2) property.SetValue(this, (int)i2);
                        else if (v is ushort i4) property.SetValue(this, (int)i4);
                        else if (v is byte i5) property.SetValue(this, (int)i5);
                        else if (v is sbyte i6) property.SetValue(this, (int)i6);
                        else if (v is float i7) property.SetValue(this, (int)i7);
                        else if (v is double i8) property.SetValue(this, (int)i8);
                        else if (v is decimal i9) property.SetValue(this, (int)i9);
                    }
                    else if (v is string s)
                    {
                        if (int.TryParse(s, out var i)) property.SetValue(this, i);
                    }
                }
                else if (property.PropertyType == typeof(long))
                {
                    if (v is null)
                    {
                    }
                    else if (vType.IsEnum)
                    {
                        property.SetValue(this, (long)v);
                    }
                    else if (vType.IsValueType)
                    {
                        if (v is long i) property.SetValue(this, i);
                        else if (v is bool b) property.SetValue(this, b ? 1L : 0L);
                        else if (v is ulong i0) property.SetValue(this, (long)i0);
                        else if (v is int i1) property.SetValue(this, (long)i1);
                        else if (v is short i3) property.SetValue(this, (long)i3);
                        else if (v is uint i2) property.SetValue(this, (long)i2);
                        else if (v is ushort i4) property.SetValue(this, (long)i4);
                        else if (v is byte i5) property.SetValue(this, (long)i5);
                        else if (v is sbyte i6) property.SetValue(this, (long)i6);
                        else if (v is float i7) property.SetValue(this, (long)i7);
                        else if (v is double i8) property.SetValue(this, (long)i8);
                        else if (v is decimal i9) property.SetValue(this, (long)i9);
                    }
                    else if (v is string s)
                    {
                        if (long.TryParse(s, out var i)) property.SetValue(this, i);
                    }
                }
                else if (property.PropertyType == typeof(bool))
                {
                    if (v is null) continue;
                    else if (v is bool b) property.SetValue(this, b);
                    else if (v is int i) property.SetValue(this, i > 0);
                    else if (v is ushort i4) property.SetValue(this, i4 > 0);
                    else if (v is string s && JsonBooleanNode.TryParse(s, out var b2)) property.SetValue(this, b2.Value);
                }
            }
            catch (InvalidCastException)
            {
            }
            catch (OverflowException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    /// <summary>
    /// Gets all the device component information.
    /// </summary>
    /// <typeparam name="T">The type of the device component.</typeparam>
    /// <param name="query">The query string in Windows management.</param>
    /// <param name="factory">The instance factory.</param>
    /// <returns>A collection of the device component information.</returns>
    internal static IEnumerable<T> Get<T>(string query, Func<ManagementBaseObject, T> factory) where T : BaseDeviceComponentBasicInfo
    {
        var search = new ManagementObjectSearcher(query);
        foreach (var obj in search.Get())
        {
            if (obj == null) continue;
            yield return factory(obj);
        }
    }
}
