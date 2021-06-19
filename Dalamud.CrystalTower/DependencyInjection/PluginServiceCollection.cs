using System;
using System.Collections.Generic;
using System.Linq;

namespace Dalamud.CrystalTower.DependencyInjection
{
    public class PluginServiceCollection : IDisposable
    {
        private readonly IList<object> _services;

        public PluginServiceCollection()
        {
            _services = new List<object>();
        }

        /// <summary>
        /// Installs a service implementation into the service collection of this <see cref="PluginServiceCollection"/>.
        /// This instance will become responsible for disposing the service.
        /// </summary>
        /// <typeparam name="TServiceImplementation">The service type.</typeparam>
        /// <param name="instance">The service instance.</param>
        public void AddService<TServiceImplementation>(TServiceImplementation instance)
        {
            _services.Add(instance);
        }

        /// <summary>
        /// Retrieves a service instance, or <c>null</c> if none has been installed in this collection.
        /// </summary>
        /// <param name="serviceType">The service type to retrieve an instance of.</param>
        /// <returns>The service instance, or <c>null</c> if none has been installed in this collection.</returns>
        public object GetService(Type serviceType)
        {
            return _services.FirstOrDefault(serviceType.IsInstanceOfType);
        }

        /// <summary>
        /// Retrieves a service instance, or <c>null</c> if none has been installed in this collection.
        /// </summary>
        /// <typeparam name="TService">The service type to retrieve an instance of.</typeparam>
        /// <returns>The service instance, or <c>null</c> if none has been installed in this collection.</returns>
        public TService GetService<TService>()
            => (TService)GetService(typeof(TService));

        /// <summary>
        /// Injects services into an instance's public properties.
        /// Injected services are identified with public properties on the instance type; however, a missing implementation
        /// will not assume that a service is required, and will instead leave it <c>null</c>. Likewise, if the constructor of the instance type
        /// assigns to a public property, this will be detected and no services will be injected into those populated properties.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void InjectInto(object instance)
        {
            foreach (var property in instance.GetType().GetProperties())
            {
                if (property.GetValue(instance) != null)
                {
                    continue;
                }

                var fulfillingService = GetService(property.PropertyType);
                property.SetValue(instance, fulfillingService);
            }
        }

        /// <summary>
        /// Calls <see cref="IDisposable.Dispose"/> on any services installed into this instance that implement <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose()
        {
            foreach (var service in _services)
            {
                if (service is IDisposable disposableService)
                {
                    disposableService.Dispose();
                }
            }
        }
    }
}