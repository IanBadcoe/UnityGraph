namespace Assets.Generation.U
{
    public abstract class EqualityBase
    {
        public abstract override bool Equals(object o);
        public abstract override int GetHashCode();
        public static bool operator ==(EqualityBase lhs, EqualityBase rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                return ReferenceEquals(rhs, null);
            }

            return lhs.Equals(rhs);
        }
        public static bool operator !=(EqualityBase lhs, EqualityBase rhs)
        {
            return !(lhs == rhs);
        }
    }
}
