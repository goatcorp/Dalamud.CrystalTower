using Dalamud.CrystalTower.UI;

namespace Dalamud.CrystalTower.Tests.Mocks
{
    public class AltWindowMock : ImmediateModeWindow
    {
        public bool DrawCalled { get; private set; }

        public override void Draw(ref bool visible)
        {
            DrawCalled = true;
        }
    }
}