using Assets.Generation.G;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Generation.G
{
    public class GraphUtil
    {
        public static HashSet<Tuple<DirectedEdge, DirectedEdge>> FindCrossingEdges(List<DirectedEdge> edges)
        {
            HashSet<Tuple<DirectedEdge, DirectedEdge>> ret = new HashSet<Tuple<DirectedEdge, DirectedEdge>>();

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

        public static Tuple<DirectedEdge, DirectedEdge> EdgeIntersect(DirectedEdge edge1, DirectedEdge edge2)
        {
            Debug.Assert(edge1 != null);
            Debug.Assert(edge2 != null);

            Tuple <float, float> p = EdgeIntersect(edge1.Start, edge1.End, edge2.Start, edge2.End);

            if (p == null)
                return null;

            return new Tuple<DirectedEdge, DirectedEdge>(edge1, edge2);
        }

        public static Tuple<float, float> EdgeIntersect(INode edge1Start, INode edge1End,
                                                          INode edge2Start, INode edge2End)
        {
            Debug.Assert(edge1Start != null);
            Debug.Assert(edge1End != null);
            Debug.Assert(edge2Start != null);
            Debug.Assert(edge2End != null);

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
