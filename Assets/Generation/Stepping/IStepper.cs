﻿namespace Generation.Stepping
{
    public interface IStepper
    {
        // some steppers will consider themselves failed if a child fails
        // others will carry on and try something else
        // status will be:
        // StepIn : on first invokation from parent step
        // Iterate : on iteration
        // StepOutSuccess : on a child step exiting successfully
        // StepOutFailure : on a child step exiting with failure
        //
        // some steps will themselves fail when a child fails
        // others will go on to try other stuff
        StepperController.StatusReportInner Step(StepperController.Status status);
    }
}