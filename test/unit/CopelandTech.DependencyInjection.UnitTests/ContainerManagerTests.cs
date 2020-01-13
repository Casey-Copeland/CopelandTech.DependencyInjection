using CopelandTech.DependencyInjection.Interfaces;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;

namespace CopelandTech.DependencyInjection.UnitTests
{
    public class ContainerManagerTests
    {
        [Fact]
        public void ContainerRegistersSuccessfully()
        {
            var containerManager = new ContainerManager();
            containerManager.AutoRegisterServices();
            Assert.Throws<ActivationException>(() => containerManager.GetInstance<ITest>());
        }

        [Fact]
        public void ContainerRegistersScopedSuccessfully()
        {
            var container = new Container();
            var containerManager = new ContainerManager(container);
            containerManager.AutoRegisterServices();
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                var test = containerManager.GetInstance<IScopedTest>();
                Assert.IsAssignableFrom<IScopedService>(test);
            }
        }

        [Fact]
        public void ContainerRegistersSingletonSuccessfully()
        {
            var containerManager = new ContainerManager();
            containerManager.AutoRegisterServices();
            var test = containerManager.GetInstance<ISingletonTest>();
            Assert.IsAssignableFrom<ISingletonService>(test);
        }

        [Fact]
        public void ContainerRegistersTransientSuccessfully()
        {
            var containerManager = new ContainerManager();
            containerManager.AutoRegisterServices();
            var test = containerManager.GetInstance<ITransientTest>();
            Assert.IsAssignableFrom<ITransientService>(test);
        }
    }

    public interface ITest : IService
    {

    }

    public class Test : ITest
    {

    }

    public interface IScopedTest : IScopedService
    {

    }

    public class ScopedTest : IScopedTest
    {

    }

    public interface ISingletonTest : ISingletonService
    {

    }

    public class SingletonTest : ISingletonTest
    {

    }

    public interface ITransientTest : ITransientService
    {

    }

    public class TransientTest : ITransientTest
    {

    }

    public interface IGenericTest<T> : IOpenGenericService, ITransientService
    {

    }

    public class GenericTest<T> : IGenericTest<T>, IOpenGenericDefaultService
    {

    }
}
