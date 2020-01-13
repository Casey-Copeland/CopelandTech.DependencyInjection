using CopelandTech.DependencyInjection.Interfaces;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CopelandTech.DependencyInjection
{
    public interface IContainerManager
    {
        T GetInstance<T>() where T : class, IService;
        void RegisterService<TInterface, TImplementation>() where TInterface : class, IService where TImplementation : class, TInterface;
        void AutoRegisterServices();
    }

    public class ContainerManager : IContainerManager
    {
        private readonly Container _container;

        public ContainerManager(Container container = null)
        {
            if (container == null)
                _container = new Container();
            else
                _container = container;

            if (_container.Options.DefaultScopedLifestyle == null)
                _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle(); //TODO: Make sure this is right? Should this be moved into a method?
        }

        public void AutoRegisterServices()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.Contains("CopelandTech")); //TODO: Figure out how to assign this name dynamically.

            foreach (var file in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CopelandTech.*.dll")) //TODO: Figure out how to assign this name dynamically.
            {
                var loadedAssembly = loadedAssemblies.SingleOrDefault(a => file.Contains(a.GetName().Name + ".dll"));

                if (loadedAssembly == null)
                    Assembly.LoadFrom(file);
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.Contains("CopelandTech"))) //TODO: Figure out how to assign this name dynamically.
            {
                var types = assembly.GetExportedTypes().Where(t => t.IsClass && typeof(IService).IsAssignableFrom(t));

                foreach (var type in types)
                {
                    var services = type.GetInterfaces().Where(i => i != typeof(IService) && i != typeof(ISingletonService) && i != typeof(IScopedService) && i != typeof(ITransientService) && i != typeof(IOpenGenericService) && i != typeof(IOpenGenericDefaultService) && typeof(IService).IsAssignableFrom(i));

                    foreach (var service in services)
                    {
                        if (type.GetInterfaces().Contains(typeof(IOpenGenericDefaultService)) && service.GetInterfaces().Contains(typeof(ISingletonService))) //TODO: Write test to verify this order is kept. Multi layer interfaces can override the lifetime based on the implementation
                            _container.RegisterConditional(service.GetGenericTypeDefinition(), type.GetGenericTypeDefinition(), Lifestyle.Singleton, c => !c.Handled);
                        else if (type.GetInterfaces().Contains(typeof(IOpenGenericDefaultService)) && service.GetInterfaces().Contains(typeof(IScopedService)))
                            _container.RegisterConditional(service.GetGenericTypeDefinition(), type.GetGenericTypeDefinition(), Lifestyle.Scoped, c => !c.Handled);
                        else if (type.GetInterfaces().Contains(typeof(IOpenGenericDefaultService)) && service.GetInterfaces().Contains(typeof(ITransientService)))
                            _container.RegisterConditional(service.GetGenericTypeDefinition(), type.GetGenericTypeDefinition(), Lifestyle.Transient, c => !c.Handled);
                        else if (service.GetInterfaces().Contains(typeof(ISingletonService))) //TODO: Write test to verify this order is kept. Multi layer interfaces can override the lifetime based on the implementation
                            _container.Register(service, type, Lifestyle.Singleton);
                        else if (service.GetInterfaces().Contains(typeof(IScopedService)))
                            _container.Register(service, type, Lifestyle.Scoped);
                        else if (service.GetInterfaces().Contains(typeof(ITransientService)))
                            _container.Register(service, type, Lifestyle.Transient);
                    }
                }
            }
        }

        public T GetInstance<T>() where T : class, IService
        {
            return _container.GetInstance<T>();
        }

        public void RegisterService<TInterface, TImplementation>() where TInterface : class, IService where TImplementation : class, TInterface
        {
            _container.Register<TInterface, TImplementation>(Lifestyle.Singleton);
        }
    }
}
