using System;

namespace Dalamud.CrystalTower.Tests.Mocks
{
    public class MockDisposable : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}