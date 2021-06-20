using Dalamud.CrystalTower.DependencyInjection;
using System.Net.Http;
using System.Text;

namespace Dalamud.CrystalTower.Tests.Mocks
{
    public class InjectIntoMock
    {
        public StringBuilder Public { get; set; }

        public string NotNull { get; set; } = "Something";

        protected HttpClient Protected { get; set; }

        private PluginServiceCollection Private { get; set; }

        public HttpClient GetProtected() => Protected;

        public PluginServiceCollection GetPrivate() => Private;
    }
}