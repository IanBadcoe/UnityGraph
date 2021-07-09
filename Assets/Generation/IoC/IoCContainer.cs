﻿namespace Assets.Generation.IoC
{
    public class IoCContainer
    {
        public readonly IRelaxerFactory RelaxerFactory;
        public readonly IAllNodesExpanderFactory AllNodesExpanderFactory;
        public readonly INodeExpanderFactory NodeExpanderFactory;
        public readonly INodeTemplateExpanderFactory NodeTemplateExpanderFactory;
        public readonly IAdjusterFactory AdjusterFactory;

        public IoCContainer(IRelaxerFactory relaxerFactory,
                            IAllNodesExpanderFactory allNodesExpanderFactory,
                            INodeExpanderFactory nodeExpanderFactory,
                            INodeTemplateExpanderFactory nodeTemplateExpanderFactory,
                            IAdjusterFactory adjusterFactory)
        {
            RelaxerFactory = relaxerFactory;
            AllNodesExpanderFactory = allNodesExpanderFactory;
            NodeExpanderFactory = nodeExpanderFactory;
            NodeTemplateExpanderFactory = nodeTemplateExpanderFactory;
            AdjusterFactory = adjusterFactory;
        }
    }
}