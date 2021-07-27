using System.Collections.Generic;

namespace Assets.Generation.GeomRep
{
    public class LoopSet : List<Loop>
    {
        public LoopSet()
        {
        }

        // convenience ctor
        public LoopSet(Loop loop)
        {
            Add(loop);
        }

        public override int GetHashCode()
        {
            int ret = 0;

            foreach (Loop l in this)
            {
                ret = ret * 3
                      ^ l.GetHashCode();
            }

            return ret;
        }

        public override bool Equals(object o)
        {
            if (o == this)
            {
                return true;
            }

            if (!(o is LoopSet))
            {
                return false;
            }

            LoopSet lso = (LoopSet)o;

            if (Count != lso.Count)
            {
                return false;
            }

            for (int i = 0; i < Count; i++)
            {
                if (!this[i].Equals(lso[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
