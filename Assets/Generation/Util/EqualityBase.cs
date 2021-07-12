using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Generation.U
{
    public abstract class EqualityBase
    {
        public abstract override bool Equals(object o);
        public abstract override int GetHashCode();
        public static bool operator ==(EqualityBase lhs, EqualityBase rhs)
        {
            if (ReferenceEquals(lhs, null) && ReferenceEquals(rhs, null))
                return true;

            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
                return false;

            return lhs.Equals(rhs);
        }
        public static bool operator !=(EqualityBase lhs, EqualityBase rhs)
        {
            return !(lhs == rhs);
        }
    }
}
