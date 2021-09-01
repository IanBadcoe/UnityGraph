using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Generation.GeomRep
{
    public interface ILoopSet : IReadOnlyList<Loop>
    {
        LoopSet Concatenate(ILoopSet other);
        bool Equals(object o);
        Box2 GetBounds();
        LoopSet Reversed();
    }

    [System.Diagnostics.DebuggerDisplay("Loops: {Count}")]
    public class LoopSet : List<Loop>, ILoopSet
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
            foreach (var l in loops)
            {
                Add(l);
            }
        }

        public override int GetHashCode()
        {
            int ret = 0;

            foreach (Loop l in this)
            {
                ret = ret * 3 ^ l.GetHashCode();
            }

            return ret;
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(o, this))
            {
                return true;
            }

            if (!(o is ILoopSet))
            {
                return false;
            }

            return this.SequenceEqual((ILoopSet)o);
        }

        public static bool operator ==(LoopSet lhs, LoopSet rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                return ReferenceEquals(rhs, null);
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(LoopSet lhs, LoopSet rhs)
        {
            return !(lhs == rhs);
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
        public LoopSet Concatenate(ILoopSet other)
        {
            return new LoopSet(this.Concat(other));
        }
    }

    [System.Diagnostics.DebuggerDisplay("Loops: {Count}")]
    public class ReadOnlyLoopSet : ILoopSet
    {
        readonly List<Loop> Loops;

        public Loop this[int index] => Loops[index];

        public int Count => Loops.Count;

        public ReadOnlyLoopSet(IReadOnlyList<Loop> loops)
        {
            Loops = new List<Loop>(loops);
        }

        public override int GetHashCode()
        {
            int ret = 0;

            foreach (Loop l in this)
            {
                ret = ret * 3 ^ l.GetHashCode();
            }

            return ret;
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(o, this))
            {
                return true;
            }

            if (!(o is ILoopSet))
            {
                return false;
            }

            return this.SequenceEqual((ILoopSet)o);
        }

        public static bool operator ==(ReadOnlyLoopSet lhs, ReadOnlyLoopSet rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                return ReferenceEquals(rhs, null);
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(ReadOnlyLoopSet lhs, ReadOnlyLoopSet rhs)
        {
            return !(lhs == rhs);
        }

        public LoopSet Concatenate(ILoopSet other)
        {
            var ls = new LoopSet(this);

            return ls.Concatenate(other);
        }

        public Box2 GetBounds()
        {
            return Loops.Aggregate(new Box2(), (b, l) => b.Union(l.GetBounds()));
        }

        public IEnumerator<Loop> GetEnumerator() => Loops.GetEnumerator();

        public LoopSet Reversed()
        {
            var ls = new LoopSet(this);

            return ls.Reversed();
        }

        IEnumerator IEnumerable.GetEnumerator() => Loops.GetEnumerator();
    }
}