using Assets.Generation.IoC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Generation
{
    public class Generator : MonoBehaviour
    {
        [SerializeField]
        private GeneratorConfig Config;

        private IoCContainer ioc;

        private void Start()
        {
            ioc = new IoCContainer(new RelaxerStepperFactory(),
                                   new TryAllNodesExpandStepperFactory(),
                                   new TryAllTemplatesOnOneNodeStepperFactory(),
                                   new TryTemplateExpandStepperFactory(),
                                   new EdgeAdjusterStepperFactory());
        }
    }
}