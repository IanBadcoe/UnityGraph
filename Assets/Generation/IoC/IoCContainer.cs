using Assets.Generation.G.GLInterfaces;

namespace Assets.Generation.IoC
{
    public class IoCContainer
    {
        public readonly IRelaxerFactory RelaxerFactory;
        public readonly IAllNodesExpanderFactory AllNodesExpanderFactory;
        public readonly INodeExpanderFactory NodeExpanderFactory;
        public readonly INodeTemplateExpanderFactory NodeTemplateExpanderFactory;
        public readonly IAdjusterFactory AdjusterFactory;
        public readonly IGeomLayoutFactory LayoutFactory;

        public IoCContainer(IRelaxerFactory relaxerFactory,
                            IAllNodesExpanderFactory allNodesExpanderFactory,
                            INodeExpanderFactory nodeExpanderFactory,
                            INodeTemplateExpanderFactory nodeTemplateExpanderFactory,
                            IAdjusterFactory adjusterFactory,
                            IGeomLayoutFactory layoutFactory)
        {
            RelaxerFactory = relaxerFactory;
            AllNodesExpanderFactory = allNodesExpanderFactory;
            NodeExpanderFactory = nodeExpanderFactory;
            NodeTemplateExpanderFactory = nodeTemplateExpanderFactory;
            AdjusterFactory = adjusterFactory;
            LayoutFactory = layoutFactory;
        }
    }
}