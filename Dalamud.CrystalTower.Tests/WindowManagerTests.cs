using Dalamud.CrystalTower.Tests.Mocks;
using Xunit;

namespace Dalamud.CrystalTower.Tests
{
    public class WindowManagerTests
    {
        [Fact]
        public void WindowManager_CallsDraw()
        {
            using var windowManager = new TestWindowManager();
            windowManager.AddWindow<AltWindowMock>(initiallyVisible: false);
            windowManager.AddWindow<WindowMock>(initiallyVisible: true);

            windowManager.Draw();

            Assert.True(windowManager.GetWindowReference<WindowMock>().DrawCalled);
        }

        [Fact]
        public void WindowManager_Window_Open_Request_Works()
        {
            using var windowManager = new TestWindowManager();
            windowManager.AddWindow<AltWindowMock>(initiallyVisible: false);
            windowManager.AddWindow<WindowMock>(initiallyVisible: true);

            var altWindowMock = windowManager.GetWindowReference<AltWindowMock>();

            Assert.False(altWindowMock.DrawCalled);

            var windowMock = windowManager.GetWindowReference<WindowMock>();
            windowMock.RequestWindowOpen();
            windowManager.Draw();

            Assert.True(altWindowMock.DrawCalled);
        }

        [Fact]
        public void WindowManager_Window_Close_Request_Works()
        {
            using var windowManager = new TestWindowManager();
            windowManager.AddWindow<AltWindowMock>(initiallyVisible: true);
            windowManager.AddWindow<WindowMock>(initiallyVisible: true);
            windowManager.Draw();

            var altWindowMock = windowManager.GetWindowReference<AltWindowMock>();

            Assert.True(altWindowMock.DrawCalled);

            var windowMock = windowManager.GetWindowReference<WindowMock>();
            windowMock.RequestWindowClose();
            windowManager.Draw();

            Assert.False(windowManager.WillWindowDraw<AltWindowMock>());
        }

        [Fact]
        public void WindowManager_ShowWindow_Works()
        {
            using var windowManager = new TestWindowManager();
            windowManager.AddWindow<WindowMock>(initiallyVisible: false);

            Assert.False(windowManager.WillWindowDraw<WindowMock>());

            windowManager.ShowWindow<WindowMock>();

            Assert.True(windowManager.WillWindowDraw<WindowMock>());
        }

        [Fact]
        public void WindowManager_HideWindow_Works()
        {
            using var windowManager = new TestWindowManager();
            windowManager.AddWindow<WindowMock>(initiallyVisible: true);

            Assert.True(windowManager.WillWindowDraw<WindowMock>());

            windowManager.HideWindow<WindowMock>();

            Assert.False(windowManager.WillWindowDraw<WindowMock>());
        }

        [Fact]
        public void WindowManager_ToggleWindow_Works()
        {
            using var windowManager = new TestWindowManager();
            windowManager.AddWindow<WindowMock>(initiallyVisible: false);

            Assert.False(windowManager.WillWindowDraw<WindowMock>());

            windowManager.ToggleWindow<WindowMock>();

            Assert.True(windowManager.WillWindowDraw<WindowMock>());
        }
    }
}