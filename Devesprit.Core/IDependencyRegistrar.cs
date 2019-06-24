using Autofac;

namespace Devesprit.Core
{
    public partial interface IDependencyRegistrar
    {
        void Register(ContainerBuilder builder, ITypeFinder typeFinder);
        int Order { get; }
    }
}
