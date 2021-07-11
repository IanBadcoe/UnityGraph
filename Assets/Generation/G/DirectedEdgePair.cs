using Assets.Generation.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Generation.G
{
    public class DirectedEdgePair : EqualityBase
    {
        readonly DirectedEdge m_e1;
        readonly DirectedEdge m_e2;

        public DirectedEdgePair(DirectedEdge e1, DirectedEdge e2)
        {
            m_e1 = e1;
            m_e2 = e2;
        }

        public override int GetHashCode()
        {
            int x = m_e1.GetHashCode();
            int y = m_e2.GetHashCode();

            // we want this symmetric as in this case which edge is which is irrelevant
            return x ^ y;
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;

            DirectedEdgePair dep = o as DirectedEdgePair;

            return (m_e1 == dep.m_e1 && m_e2 == dep.m_e2)
                || (m_e1 == dep.m_e2 && m_e2 == dep.m_e1);
        }
    }
}
