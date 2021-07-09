namespace Assets.Generation.Stepping
{
    public class StepperController
    {
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
    }
}