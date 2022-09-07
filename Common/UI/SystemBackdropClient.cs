using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.System;
using WinRT;

namespace Trivial.UI;

internal class SystemBackdropClient : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct DispatcherQueueOptions
    {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }

    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

    private object dispatcherQueueController = null;
    private SystemBackdropConfiguration configurationSource;
    private ISystemBackdropControllerWithTargets backdropController;
    private bool disposedValue;

    public Func<ElementTheme, ISystemBackdropControllerWithTargets> BackdropControllerMaker { get; set; }

    public bool IsInputActive
    {
        get
        {
            return configurationSource?.IsInputActive ?? false;
        }

        set
        {
            var s = configurationSource;
            if (s == null) return;
            s.IsInputActive = value;
        }
    }

    public void EnsureWindowsSystemDispatcherQueueController()
    {
        if (DispatcherQueue.GetForCurrentThread() != null || dispatcherQueueController != null) return;
        DispatcherQueueOptions options;
        options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
        options.threadType = 2;     // DQTYPE_THREAD_CURRENT
        options.apartmentType = 2;  // DQTAT_COM_STA
        _ = CreateDispatcherQueueController(options, ref dispatcherQueueController);
    }

    public void SetConfigurationSourceTheme(ElementTheme theme)
    {
        if (configurationSource == null) return;
        switch (theme)
        {
            case ElementTheme.Dark:
                configurationSource.Theme = SystemBackdropTheme.Dark;
                break;
            case ElementTheme.Light:
                configurationSource.Theme = SystemBackdropTheme.Light;
                break;
            case ElementTheme.Default:
                configurationSource.Theme = SystemBackdropTheme.Default;
                break;
        }
    }

    public bool UpdateWindowBackground(Window window, ElementTheme theme)
    {
        var oldBc = backdropController;
        var h = BackdropControllerMaker;
        if (h != null)
        {
            var isFailed = true;
            try
            {
                backdropController = h(theme);
                isFailed = false;
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (NotImplementedException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (ExternalException)
            {
            }

            if (isFailed) backdropController = null;
        }
        else
        {
            backdropController = VisualUtility.TryCreateMicaBackdrop();
            backdropController ??= VisualUtility.TryCreateAcrylicBackdrop();
        }

        if (backdropController == null) return false;
        try
        {
            EnsureWindowsSystemDispatcherQueueController();

            // Hooking up the policy object
            if (configurationSource == null) configurationSource = new();

            // Initial configuration state.
            configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme(theme);

            // Enable the system backdrop.
            backdropController.AddSystemBackdropTarget(window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            backdropController.SetSystemBackdropConfiguration(configurationSource);
            try
            {
                if (oldBc != null) oldBc.Dispose();
            }
            catch (InvalidOperationException)
            {
            }
            catch (ExternalException)
            {
            }

            return true;
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NotImplementedException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (ExternalException)
        {
        }

        return false;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (backdropController != null) backdropController.Dispose();
            }

            backdropController = null;
            configurationSource = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
