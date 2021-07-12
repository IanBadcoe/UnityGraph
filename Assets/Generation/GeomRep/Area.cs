using Assets.Extensions;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public class Area : EqualityBase
    {
        public readonly Vector2 BL;
        public readonly Vector2 TR;

        static public Area Empty { get; } = new Area();

        public Area()
        {
            BL = new Vector2(0, 0);
            TR = new Vector2(-1, -1);
        }

        public Area(Vector2 bl, Vector2 tr)
        {
            BL = bl;
            TR = tr;
        }

        public bool IsEmpty()
        {
            // if either high edge is on the wrong side of the low edge, we are empty
            // (empty is different from zero-sized, edges are in the same place for that)
            return BL.x > TR.x || BL.y > TR.y;
        }

        public Area Union(Area rhs)
        {
            // empty areas add nothing to other areas
            if (IsEmpty())
            {
                return rhs;
            }
            else if (rhs.IsEmpty())
            {
                return this;
            }

            return new Area(BL.Min(rhs.BL), TR.Max(rhs.TR));
        }

        public override int GetHashCode()
        {
            if (IsEmpty())
                return 0x48e5083f;

            return BL.GetHashCode() ^ TR.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj) || obj.GetType() != GetType())
                return false;

            Area a_obj = obj as Area;

            return BL == a_obj.BL && TR == a_obj.TR;
        }
    }
}
