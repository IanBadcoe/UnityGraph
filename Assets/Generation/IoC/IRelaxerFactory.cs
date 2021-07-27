using Assets.Generation.G;
using Assets.Generation.Gen;
using Assets.Generation.Stepping;

namespace Assets.Generation.IoC
{
    public interface IRelaxerFactory
    {
        IStepper MakeRelaxer(IoCContainer ioc_container,
              Graph g, GeneratorConfig c);
    }
}