using Assets.Generation.G;
using Assets.Generation.IoC;
using Assets.Generation.Stepping;

namespace Assets.Generation
{
    internal class RelaxerStepperFactory : IRelaxerFactory
    {
        public IStepper MakeRelaxer(IoCContainer ioc_container, Graph g, GeneratorConfig c)
        {
            throw new System.NotImplementedException();
        }
    }
}