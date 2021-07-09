using Generation.G;
using Generation.IoC;
using Generation.Stepping;

namespace Generation
{
    internal class RelaxerStepperFactory : IRelaxerFactory
    {
        public IStepper MakeRelaxer(IoCContainer ioc_container, Graph g, GeneratorConfig c)
        {
            throw new System.NotImplementedException();
        }
    }
}