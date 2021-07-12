using Assets.Generation.G;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Generation.G
{
    public class GraphUtil
    {
        public static HashSet<IntersectionResult> FindCrossingEdges(List<DirectedEdge> edges)
        {
            HashSet<IntersectionResult> ret = new HashSet<IntersectionResult>();

            foreach (DirectedEdge e1 in edges)
            {
                foreach (DirectedEdge e2 in edges)
                {
                    if (e1 == e2)
                        break;

                    var dep = EdgeIntersect(e1, e2);

                    if (dep != null)
                    {
                        ret.Add(dep);
                    }
                }
            }

            return ret;
        }

        public static IntersectionResult EdgeIntersect(DirectedEdge edge1, DirectedEdge edge2)
        {
            Assertion.Assert(edge1 != null);
            Assertion.Assert(edge2 != null);

            Tuple <float, float> p = EdgeIntersect(edge1.Start, edge1.End, edge2.Start, edge2.End);

            if (p == null)
                return null;

            return new IntersectionResult(edge1, edge2, p.Item1, p.Item2);
        }

        public static Tuple<float, float> EdgeIntersect(INode edge1Start, INode edge1End,
                                                          INode edge2Start, INode edge2End)
        {
            Assertion.Assert(edge1Start != null);
            Assertion.Assert(edge1End != null);
            Assertion.Assert(edge2Start != null);
            Assertion.Assert(edge2End != null);

            // connecting lines not considered crossing
            if (edge1Start == edge2Start
                  || edge1Start == edge2End
                  || edge1End == edge2Start
                  || edge1End == edge2End)
            {
                return null;
            }

            return U.Util.EdgeIntersect(edge1Start.Position, edge1End.Position,
                                      edge2Start.Position, edge2End.Position);
        }
    }
}
