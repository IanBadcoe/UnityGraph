using Assets.Generation.G;
using System;
using System.Collections.Generic;

namespace Assets.Generation.Stepping
{
    public class StepperController
    {
        private readonly Stack<Tuple<IStepper, IGraphRestore>>
              m_stack = new Stack<Tuple<IStepper, IGraphRestore>>();

        private Graph Graph { get; set; }
        private Status m_last_step_status;

        public enum Status
        {
            Iterate,          // current stepper requires more steps
            StepIn,           // move down into a child stepper
            StepOutSuccess,   // current stepper successfully completed, move back to parent
            StepOutFailure    // current stepper failed, revert graph and move back to parent
        }

        public class StatusReportInner
        {
            public readonly StepperController.Status Status;
            public readonly IStepper ChildStepper;
            public readonly string Log;

            public StatusReportInner(StepperController.Status status,
                                     IStepper childStepper,
                                     string log)
            {
                Status = status;
                ChildStepper = childStepper;
                Log = log;
            }
        }

        public class StatusReport
        {
            public readonly StepperController.Status Status;
            public readonly string Log;
            public readonly bool Complete;

            public StatusReport(StatusReportInner eri,
                                bool complete)
            {
                Status = eri.Status;
                Log = eri.Log;

                Complete = complete;
            }

            public StatusReport(StepperController.Status status,
                                string log,
                                bool complete)
            {
                Status = status;
                Log = log;

                Complete = complete;
            }
        }

        public StepperController(IStepper initial_stepper)
        {
            PushStepper(initial_stepper);
            // we start with a (conceptual) step in from the invoking code
            m_last_step_status = Status.StepIn;
        }

        public StatusReport Step()
        {
            IStepper stepper = CurrentStepper();

            if (stepper == null)
            {
                throw new NullReferenceException("Attempt to step without an initial stepper.  Either you failed to supply one, or this engine.StepperController has completed.");
            }

            if (Graph == null)
            {
                Graph = stepper.Graph;
            }

            StatusReportInner eri = stepper.Step(m_last_step_status);

            m_last_step_status = eri.Status;

            switch (m_last_step_status)
            {
                case Status.StepIn:
                    PushStepper(eri.ChildStepper);
                    break;

                case Status.StepOutFailure:
                    PopStepper(false);
                    break;

                case Status.StepOutSuccess:
                    PopStepper(true);
                    break;
            }

            return new StatusReport(eri, CurrentStepper() == null);
        }

        private void PushStepper(IStepper stepper)
        {
            m_stack.Push(
                  new Tuple<IStepper, IGraphRestore>(stepper, Graph != null ? Graph.CreateRestorePoint() : null));
        }

        private IStepper CurrentStepper()
        {
            if (m_stack.Count == 0)
            {
                return null;
            }

            return m_stack.Peek().Item1;
        }

        private void PopStepper(bool success)
        {
            IGraphRestore igr = m_stack.Pop().Item2;

            if (!success && igr != null)
            {
                igr.Restore();
            }
        }

    }
}