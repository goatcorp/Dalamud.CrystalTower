using Dalamud.CrystalTower.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dalamud.CrystalTower.UI
{
    public class WindowManager : ImmediateModeWindow, IDisposable
    {
        private readonly IList<WindowInfo> _windows;
        private readonly PluginServiceCollection _serviceCollection;

        public WindowManager()
        {
            _windows = new List<WindowInfo>();
        }

        public WindowManager(PluginServiceCollection serviceCollection) : this()
        {
            _serviceCollection = serviceCollection;
        }

        /// <summary>
        /// Draws all visible windows assigned to this <see cref="WindowManager"/>.
        /// </summary>
        public void Draw()
        {
            foreach (var windowInfo in _windows)
            {
                var window = windowInfo.Instance;
                var visible = windowInfo.Visible;

                if (!visible)
                {
                    continue;
                }

                window.Draw(ref visible);

                if (windowInfo.Visible != visible)
                {
                    windowInfo.Visible = visible;
                }
            }
        }

        /// <summary>
        /// Draws all visible windows assigned to this <see cref="WindowManager"/> if it is visible.
        /// </summary>
        /// <param name="visible">Whether or not this <see cref="WindowManager"/> is visible.</param>
        public override void Draw(ref bool visible)
        {
            if (visible)
            {
                Draw();
            }
        }

        /// <summary>
        /// Shows the <see cref="ImmediateModeWindow"/> specified by the type parameter. Throws an exception if the
        /// window has not been installed into this instance.
        /// </summary>
        /// <typeparam name="TWindow">The window type.</typeparam>
        public void ShowWindow<TWindow>() where TWindow : ImmediateModeWindow
        {
            var windowInfo = _windows.First(w => w.Instance is TWindow);
            windowInfo.Visible = true;
        }

        /// <summary>
        /// Toggles the <see cref="ImmediateModeWindow"/> specified by the type parameter. Throws an exception if the
        /// window has not been installed into this instance.
        /// </summary>
        /// <typeparam name="TWindow">The window type.</typeparam>
        public void ToggleWindow<TWindow>() where TWindow : ImmediateModeWindow
        {
            var windowInfo = _windows.First(w => w.Instance is TWindow);
            windowInfo.Visible = !windowInfo.Visible;
        }

        /// <summary>
        /// Installs an <see cref="ImmediateModeWindow"/> into this instance and hydrates it with any applicable service implementations.
        /// </summary>
        /// <typeparam name="TWindow">The window type.</typeparam>
        /// <param name="initiallyVisible">Whether or not the window should begin visible.</param>
        public void AddWindow<TWindow>(bool initiallyVisible) where TWindow : ImmediateModeWindow
        {
            var instance = (ImmediateModeWindow)Activator.CreateInstance<TWindow>();
            _serviceCollection?.InjectInto(instance);

            instance.ForeignWindowOpenRequested += OnWindowOpenRequested;
            instance.ForeignWindowCloseRequested += OnWindowCloseRequested;

            instance.ForeignWindowReferenceRequested += OnWindowReferenceRequested;

            _windows.Add(new WindowInfo
            {
                Instance = instance,
                Visible = initiallyVisible,
            });
        }

        /// <summary>
        /// Callback method called when an installed <see cref="ImmediateModeWindow"/> requests that another window be opened.
        /// </summary>
        /// <param name="windowType">The type of the window to be opened.</param>
        private void OnWindowOpenRequested(Type windowType)
        {
            var windowInfo = _windows.First(w => windowType.IsInstanceOfType(w.Instance));
            windowInfo.Visible = true;
        }

        /// <summary>
        /// Callback method called when an installed <see cref="ImmediateModeWindow"/> requests that another window be closed.
        /// </summary>
        /// <param name="windowType">The type of the window to be closed.</param>
        private void OnWindowCloseRequested(Type windowType)
        {
            var windowInfo = _windows.First(w => windowType.IsInstanceOfType(w.Instance));
            windowInfo.Visible = false;
        }

        /// <summary>
        /// Callback method called when an installed <see cref="ImmediateModeWindow"/> requests that another window be returned.
        /// </summary>
        /// <param name="windowType">The type of the window to be returned.</param>
        public object OnWindowReferenceRequested(Type windowType)
        {
            return _windows.First(w => windowType.IsInstanceOfType(w.Instance)).Instance;
        }

        /// <summary>
        /// Frees any <see cref="ImmediateModeWindow"/> instances stored in this manager that implement <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose()
        {
            foreach (var windowsInfo in _windows)
            {
                if (windowsInfo.Instance is IDisposable disposableInstance)
                {
                    disposableInstance.Dispose();
                }
            }
        }

        private class WindowInfo
        {
            public ImmediateModeWindow Instance { get; set; }

            public bool Visible { get; set; }
        }
    }
}
