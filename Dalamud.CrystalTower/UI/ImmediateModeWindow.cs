using System;

namespace Dalamud.CrystalTower.UI
{
    public abstract class ImmediateModeWindow
    {
        public Action<Type> ForeignWindowOpenRequested { get; set; }
        public Action<Type> ForeignWindowCloseRequested { get; set; }

        public Func<Type, object> ForeignWindowReferenceRequested { get; set; }

        public abstract void Draw(ref bool visible);

        protected void OpenWindow<TWindow>() where TWindow : ImmediateModeWindow
        {
            ForeignWindowOpenRequested?.Invoke(typeof(TWindow));
        }

        protected void CloseWindow<TWindow>() where TWindow : ImmediateModeWindow
        {
            ForeignWindowCloseRequested?.Invoke(typeof(TWindow));
        }

        protected TWindow GetWindow<TWindow>() where TWindow : ImmediateModeWindow
        {
            return (TWindow)ForeignWindowReferenceRequested?.Invoke(typeof(TWindow));
        }
    }
}