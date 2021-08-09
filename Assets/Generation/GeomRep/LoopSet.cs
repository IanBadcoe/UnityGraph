using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Generation.GeomRep
{
    [System.Diagnostics.DebuggerDisplay("Loops = {Count}")]
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

        public LoopSet(IEnumerable<Loop> loops)
        {
            foreach(var l in loops)
            {
                Add(l);
            }
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

        public Box2 GetBounds()
        {
            return this.Aggregate(new Box2(), (b, l) => b.Union(l.GetBounds()));
        }

        public LoopSet Reversed()
        {
            // whether to reverse the order of the loops of not, in theory order in a loopset doesn't matter???
            return new LoopSet(this.Select(l => l.Reversed()));
        }

        // just does dumb concatenation
        public LoopSet Concatenate(LoopSet other)
        {
            return new LoopSet(this.Concat(other));
        }
    }
}
