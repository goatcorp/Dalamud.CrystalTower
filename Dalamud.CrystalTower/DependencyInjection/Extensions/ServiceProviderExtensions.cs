using System;

namespace Dalamud.CrystalTower.DependencyInjection.Extensions
{
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Injects services into an instance's public properties.
        /// Injected services are identified with public properties on the instance type; however, a missing implementation
        /// will not assume that a service is required, and will instead leave it <c>null</c>. Likewise, if the constructor of the instance type
        /// assigns to a public property, this will be detected and no services will be injected into those populated properties.
        /// </summary>
        /// <param name="serviceProvider">The service provider to read from.</param>
        /// <param name="instance">The instance.</param>
        public static void InjectInto(this IServiceProvider serviceProvider, object instance)
        {
            foreach (var property in instance.GetType().GetProperties())
            {
                if (property.GetValue(instance) != null)
                {
                    continue;
                }

                var fulfillingService = serviceProvider.GetService(property.PropertyType);
                property.SetValue(instance, fulfillingService);
            }
        }
    }
}