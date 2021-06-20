using System;
using System.Collections.Generic;
using System.Linq;

namespace Dalamud.CrystalTower.DependencyInjection
{
    public class PluginServiceCollection : IDisposable, IServiceProvider
    {
        protected readonly IList<ServiceWrapper> Services;

        public PluginServiceCollection()
        {
            Services = new List<ServiceWrapper>();

            AddService(this, shouldDispose: false);
        }

        /// <summary>
        /// Installs a service implementation into the service collection of this <see cref="PluginServiceCollection"/>.
        /// This instance will become responsible for disposing the service unless the <paramref name="shouldDispose"/>
        /// parameter is <c>false</c>.
        /// </summary>
        /// <typeparam name="TServiceImplementation">The service type.</typeparam>
        /// <param name="instance">The service instance.</param>
        /// <param name="shouldDispose">Whether or not this collection class should dispose the service.</param>
        public void AddService<TServiceImplementation>(TServiceImplementation instance, bool shouldDispose = true) where TServiceImplementation : class
        {
            Services.Add(new ServiceWrapper
            {
                Instance = instance,
                ShouldDispose = shouldDispose,
            });
        }

        /// <summary>
        /// Retrieves a service instance, or <c>null</c> if none has been installed in this collection.
        /// </summary>
        /// <param name="serviceType">The service type to retrieve an instance of.</param>
        /// <returns>The service instance, or <c>null</c> if none has been installed in this collection.</returns>
        public object GetService(Type serviceType)
        {
            return Services.FirstOrDefault(serviceType.IsInstanceOfType);
        }

        /// <summary>
        /// Retrieves a service instance, or <c>null</c> if none has been installed in this collection.
        /// </summary>
        /// <typeparam name="TService">The service type to retrieve an instance of.</typeparam>
        /// <returns>The service instance, or <c>null</c> if none has been installed in this collection.</returns>
        public TService GetService<TService>() where TService : class
            => (TService)GetService(typeof(TService));

        /// <summary>
        /// Calls <see cref="IDisposable.Dispose"/> on any services installed into this instance that implement
        /// <see cref="IDisposable"/> and were configured to be disposed by this collection.
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var service in Services)
            {
                if (service.Instance is IDisposable disposableInstance)
                {
                    disposableInstance.Dispose();
                }
            }
        }

        protected class ServiceWrapper
        {
            public object Instance { get; set; }

            public bool ShouldDispose { get; set; }
        }
    }
}