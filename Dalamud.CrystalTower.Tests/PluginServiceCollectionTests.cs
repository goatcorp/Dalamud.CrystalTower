using Dalamud.CrystalTower.DependencyInjection;
using Dalamud.CrystalTower.Tests.Mocks;
using System.Text;
using Xunit;

namespace Dalamud.CrystalTower.Tests
{
    public class PluginServiceCollectionTests
    {
        [Fact]
        public void PluginServiceCollection_GetService_Should_Return_Added_Service()
        {
            using var serviceCollection = new PluginServiceCollection();
            serviceCollection.AddService(new StringBuilder());

            Assert.IsType<StringBuilder>(serviceCollection.GetService<StringBuilder>());
        }

        [Fact]
        public void PluginServiceCollection_GetService_Should_Respect_Lifetime()
        {
            var mockDisposable = new MockDisposable();

            var serviceCollection = new PluginServiceCollection();
            serviceCollection.AddService(mockDisposable, shouldDispose: false);
            serviceCollection.Dispose();

            Assert.False(mockDisposable.Disposed);

            var nextServiceCollection = new PluginServiceCollection();
            nextServiceCollection.AddService(mockDisposable);
            nextServiceCollection.Dispose();

            Assert.True(mockDisposable.Disposed);
        }
    }
}
