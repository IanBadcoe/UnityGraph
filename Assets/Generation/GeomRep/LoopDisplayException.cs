using System;

namespace Assets.Generation.GeomRep
{
    public class LoopDisplayException : Exception
    {
        public readonly Loop Loop1;
        public readonly Loop Loop2;

        public LoopDisplayException(Loop l1, Loop l2)
        {
            Loop1 = l1;
            Loop2 = l2;
        }
    }
}
