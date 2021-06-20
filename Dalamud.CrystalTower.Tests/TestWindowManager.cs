using System.Linq;
using Dalamud.CrystalTower.UI;

namespace Dalamud.CrystalTower.Tests
{
    public class TestWindowManager : WindowManager
    {
        public TWindow GetWindowReference<TWindow>() where TWindow : ImmediateModeWindow
        {
            return (TWindow)Windows.First(w => w.Instance is TWindow).Instance;
        }

        public bool WillWindowDraw<TWindow>() where TWindow : ImmediateModeWindow
        {
            return Windows.First(w => w.Instance is TWindow).Visible;
        }
    }
}