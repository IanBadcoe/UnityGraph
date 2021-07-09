using Assets.Generation.Stepping;
using Assets.Generation.G;
using Assets.Generation;

namespace Assets.Generation.IoC
{
    public interface IRelaxerFactory
    {
        IStepper MakeRelaxer(IoCContainer ioc_container,
              Graph g, GeneratorConfig c);
    }
}