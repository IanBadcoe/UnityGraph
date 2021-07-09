using Generation.G;
using Generation.IoC;
using Generation.Stepping;
using Generation.Templates;

namespace Generation
{
    internal class TryTemplateExpandStepperFactory : INodeTemplateExpanderFactory
    {
        public IStepper makeNodeTemplateExpander(IoCContainer ioc_container, Graph g, INode n, Template t, GeneratorConfig c)
        {
            throw new System.NotImplementedException();
        }
    }
}