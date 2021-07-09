using Generation.Stepping;
using Generation.G;
using Generation;

namespace Generation.IoC
{
    public interface IRelaxerFactory
    {
        IStepper MakeRelaxer(IoCContainer ioc_container,
              Graph g, GeneratorConfig c);
    }
}