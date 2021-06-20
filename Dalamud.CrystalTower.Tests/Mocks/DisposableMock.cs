using System;

namespace Dalamud.CrystalTower.Tests.Mocks
{
    public class DisposableMock : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}