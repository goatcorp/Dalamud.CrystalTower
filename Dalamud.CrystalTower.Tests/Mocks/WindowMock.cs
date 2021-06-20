using Dalamud.CrystalTower.UI;

namespace Dalamud.CrystalTower.Tests.Mocks
{
    public class WindowMock : ImmediateModeWindow
    {
        public bool DrawCalled { get; private set; }

        public override void Draw(ref bool visible)
        {
            DrawCalled = true;
        }

        public void RequestWindowOpen()
        {
            OpenWindow<AltWindowMock>();
        }

        public void RequestWindowClose()
        {
            CloseWindow<AltWindowMock>();
        }
    }
}