﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Trivial.Data;
using Trivial.Net;
using Trivial.Tasks;
using Windows.UI.Text;

namespace Trivial.UI;

/// <summary>
/// The static proxy for dependency object.
/// </summary>
public static class DependencyObjectProxy<TControl> where TControl : DependencyObject
{
    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <typeparam name="TPropertyValue">The type of property value.</typeparam>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <param name="converter">The property value converter.</param>
    /// <param name="stillUpdateEvenIfSame">true if still raise the event handler even on same; otherwise, false.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterProperty<TPropertyValue>(string name, Action<TControl, ChangeEventArgs<TPropertyValue>, DependencyProperty> callback, TPropertyValue defaultValue = default, Func<object, TPropertyValue> converter = null, bool stillUpdateEvenIfSame = false)
        => DependencyProperty.Register(name, typeof(TPropertyValue), typeof(TControl), new PropertyMetadata(defaultValue, callback == null ? null : (d, e) =>
        {
            var c = d as TControl;
            var changeMethod = ChangeMethods.Update;
            if (converter != null)
            {
                var newValue = converter(e.NewValue);
                var oldValue = converter(e.OldValue);
                OnChange(c, name, callback, newValue, oldValue, changeMethod, stillUpdateEvenIfSame, e.Property);
            }
            else
            {
                if (e.NewValue is not TPropertyValue newValue)
                {
                    if (e.NewValue is not null) return;
                    newValue = defaultValue;
                    changeMethod = ChangeMethods.Remove;
                }

                if (e.OldValue is not TPropertyValue oldValue)
                {
                    if (e.OldValue is not null) return;
                    oldValue = defaultValue;
                    changeMethod = ChangeMethods.Add;
                }

                OnChange(c, name, callback, newValue, oldValue, changeMethod, stillUpdateEvenIfSame, e.Property);
            }
        }));

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <typeparam name="TPropertyValue">The type of property value.</typeparam>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterProperty<TPropertyValue>(string name, PropertyChangedCallback callback, TPropertyValue defaultValue = default)
        => DependencyProperty.Register(name, typeof(TPropertyValue), typeof(TControl), new PropertyMetadata(defaultValue, callback));

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <typeparam name="TPropertyValue">The type of property value.</typeparam>
    /// <param name="name">The property name.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterProperty<TPropertyValue>(string name, TPropertyValue defaultValue = default)
        => DependencyProperty.Register(name, typeof(TPropertyValue), typeof(TControl), new PropertyMetadata(defaultValue));

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <param name="stillUpdateEvenIfSame">true if still raise the event handler even on same; otherwise, false.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterInt32Property(string name, Action<TControl, ChangeEventArgs<int>, DependencyProperty> callback, int defaultValue = 0, bool stillUpdateEvenIfSame = false)
        => RegisterProperty(name, callback, defaultValue, v => {
            if (v is null) return defaultValue;
            if (v is int i) return i;
            if (v is long l) return (int)l;
            if (v is float f) return (int)Math.Round(f);
            if (v is double d) return (int)Math.Round(d);
            if (v is decimal d2) return (int)Math.Round(d2);
            if (v is string s) return Maths.Numbers.ParseToInt32(s, 10);
            return (int)v;
        }, stillUpdateEvenIfSame);

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <param name="stillUpdateEvenIfSame">true if still raise the event handler even on same; otherwise, false.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterInt64Property(string name, Action<TControl, ChangeEventArgs<long>, DependencyProperty> callback, long defaultValue = 0L, bool stillUpdateEvenIfSame = false)
        => RegisterProperty(name, callback, defaultValue, v => {
            if (v is null) return defaultValue;
            if (v is long l) return l;
            if (v is int i) return i;
            if (v is float f) return (long)Math.Round(f);
            if (v is double d) return (long)Math.Round(d);
            if (v is decimal d2) return (long)Math.Round(d2);
            if (v is string s) return Maths.Numbers.ParseToInt64(s, 10);
            return (long)v;
        }, stillUpdateEvenIfSame);

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterSingleProperty(string name, Action<TControl, ChangeEventArgs<float>, DependencyProperty> callback = null, float defaultValue = float.NaN)
        => RegisterProperty(name, callback, defaultValue);

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterDoubleProperty(string name, Action<TControl, ChangeEventArgs<double>, DependencyProperty> callback = null, double defaultValue = double.NaN)
        => RegisterProperty(name, callback, defaultValue);

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterFontWeightProperty(string name, Action<TControl, ChangeEventArgs<FontWeight>, DependencyProperty> callback = null, FontWeight? defaultValue = null)
        => RegisterProperty(name, callback, defaultValue ?? FontWeights.Normal);

    /// <summary>
    /// Registers a dependency property for a control.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="callback">The event handler on property value changed.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <param name="stillUpdateEvenIfSame">true if still raise the event handler even on same; otherwise, false.</param>
    /// <returns>A dependency property.</returns>
    public static DependencyProperty RegisterUriProperty(string name, Action<TControl, ChangeEventArgs<Uri>, DependencyProperty> callback, Uri defaultValue = null, bool stillUpdateEvenIfSame = false)
        => RegisterProperty(name, callback, defaultValue, v => {
            if (v is null) return defaultValue;
            if (v is Uri u) return u;
            if (v is string s) return new Uri(s, UriKind.RelativeOrAbsolute);
            return (Uri)v;
        }, stillUpdateEvenIfSame);

    /// <summary>
    /// Finds a visual child of the control type.
    /// </summary>
    /// <param name="obj">The parent control.</param>
    /// <returns>The first child found.</returns>
    public static TControl FindVisualChild(DependencyObject obj)
        => FindVisualChild(obj, short.MaxValue);

    /// <summary>
    /// Finds a visual child of the control type.
    /// </summary>
    /// <param name="obj">The parent control.</param>
    /// <param name="deepLevel">The child levels to find recurrence.</param>
    /// <returns>The first child found.</returns>
    public static TControl FindVisualChild(DependencyObject obj, int deepLevel)
    {
        if (obj == null || deepLevel <= 0) return null;
        deepLevel--;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child != null && child is TControl c)
                return c;
            c = FindVisualChild(child, deepLevel);
            if (c != null)
                return c;
        }

        return null;
    }

    private static void OnChange<TPropertyValue>(TControl c, string name, Action<TControl, ChangeEventArgs<TPropertyValue>, DependencyProperty> callback, TPropertyValue newValue, TPropertyValue oldValue, ChangeMethods changeMethod, bool stillUpdateEvenIfSame, DependencyProperty p)
    {
        if (newValue is null)
        {
            if (oldValue is null)
            {
                if (stillUpdateEvenIfSame) changeMethod = ChangeMethods.Same;
                else return;
            }
            else
            {
                changeMethod = ChangeMethods.Remove;
            }
        }
        else
        {
            if (oldValue is null) changeMethod = ChangeMethods.Add;
            if (newValue.Equals(oldValue))
            {
                if (stillUpdateEvenIfSame) changeMethod = ChangeMethods.Same;
                else return;
            }
        }

        callback(c, new ChangeEventArgs<TPropertyValue>(oldValue, newValue, changeMethod, name), p);
    }
}

/// <summary>
/// The callback parameter for page navigation.
/// </summary>
/// <typeparam name="T">The type of the page.</typeparam>
public interface IPageNavigationCallbackParameter<T> where T : Page
{
    /// <summary>
    /// Processes.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event argments.</param>
    public void OnNavigate(T sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e);
}
