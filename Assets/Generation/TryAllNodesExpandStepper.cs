using Generation.G;
using Generation.IoC;
using Generation.Stepping;
using Generation.Templates;

namespace Generation
{
    internal class TryAllNodesExpandStepperFactory : IAllNodesExpanderFactory
    {
        public IStepper MakeAllNodesExpander(IoCContainer ioc_container, Graph g, TemplateStore ts, GeneratorConfig c)
        {
            throw new System.NotImplementedException();
        }
    }
}