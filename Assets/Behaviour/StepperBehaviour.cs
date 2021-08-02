using Assets.Generation.G;
using Assets.Generation.GeomRep;
using Assets.Generation.IoC;
using Assets.Generation.Stepping;
using Assets.Generation.Templates;
using UnityEngine;


namespace Assets.Behaviour
{
    public class StepperBehaviour : MonoBehaviour
    {
        public StepperController Controller;

        bool m_complete;

        public bool Pause { get; set; }

        private void Update()
        {
            if (Pause)
            {
                return;
            }

            if (!m_complete)
            {
                StepperController.StatusReport ret;

                ret = Controller.Step();

                if (ret == null || ret.Complete)
                {
                    //if (ret.Status != StepperController.Status.StepOutSuccess)
                    //{
                    //     failure handling?
                    //}

                    m_complete = true;
                }
            }
        }
    }
}