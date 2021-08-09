using System;

namespace Assets.Generation.GeomRep
{
    public class AnalysisFailedException : Exception
    {
        public AnalysisFailedException(string message)
            : base(message)
        {
        }
    }
}
