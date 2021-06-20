using System.Net.Http;
using System.Text;
using Dalamud.CrystalTower.DependencyInjection;
using Dalamud.CrystalTower.DependencyInjection.Extensions;
using Dalamud.CrystalTower.Tests.Mocks;
using Xunit;

namespace Dalamud.CrystalTower.Tests
{
    public class ServiceProviderExtensionsTests
    {
        [Fact]
        public void ServiceProviderExtensions_InjectInto_Should_Fill_Public_Properties()
        {
            var mock = new InjectIntoMock();

            using var serviceCollection = new PluginServiceCollection();
            serviceCollection.AddService(new StringBuilder());
            serviceCollection.AddService("oh.");
            serviceCollection.AddService(new HttpClient());
            serviceCollection.AddService(new PluginServiceCollection());

            serviceCollection.InjectInto(mock);

            Assert.NotNull(mock.Public);
        }

        [Fact]
        public void ServiceProviderExtensions_InjectInto_Should_Not_Fill_Protected_Properties()
        {
            var mock = new InjectIntoMock();

            using var serviceCollection = new PluginServiceCollection();
            serviceCollection.AddService(new StringBuilder());
            serviceCollection.AddService("oh.");
            serviceCollection.AddService(new HttpClient());
            serviceCollection.AddService(new PluginServiceCollection());

            serviceCollection.InjectInto(mock);

            Assert.Null(mock.GetProtected());
        }

        [Fact]
        public void ServiceProviderExtensions_InjectInto_Should_Not_Fill_Private_Properties()
        {
            var mock = new InjectIntoMock();

            using var serviceCollection = new PluginServiceCollection();
            serviceCollection.AddService(new StringBuilder());
            serviceCollection.AddService("oh.");
            serviceCollection.AddService(new HttpClient());
            serviceCollection.AddService(new PluginServiceCollection());

            serviceCollection.InjectInto(mock);

            Assert.Null(mock.GetPrivate());
        }

        [Fact]
        public void ServiceProviderExtensions_InjectInto_Should_Not_Fill_NonNull_Properties()
        {
            var mock = new InjectIntoMock();
            var existingObject = mock.NotNull;

            using var serviceCollection = new PluginServiceCollection();
            serviceCollection.AddService(new StringBuilder());
            serviceCollection.AddService("oh.");
            serviceCollection.AddService(new HttpClient());
            serviceCollection.AddService(new PluginServiceCollection());

            serviceCollection.InjectInto(mock);

            Assert.Same(existingObject, mock.NotNull);
        }
    }
}