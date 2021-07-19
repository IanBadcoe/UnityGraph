using Assets.Generation.G;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Generation.U
{
    //
    // uses the Floyd-Warshall algorithm to find all shortest path lengths in a graph
    //
    // at the moment, we ultimately want the minimum of the shortest path through the graph
    // between two nodes, and the two nodes summed radii, so we could seed this with the summed radii
    // instead of 1e30.  However writing this as an un-mucked-about "shortest path through graph edges"
    // algorithm feels more likely to have other uses later...
    //
    class ShortestPathFinder
    {
        Dictionary<Tuple<INode, INode>, float> dists;

        void SetDist(INode a, INode b, float dist)
        {
            dists[new Tuple<INode, INode>(a, b)] = dist;
            dists[new Tuple<INode, INode>(b, a)] = dist;
        }

        public float GetDist(INode a, INode b)
        {
            var key = new Tuple<INode, INode>(a, b);

            if (dists.ContainsKey(key))
            {
                return dists[key];
            }

            return float.MaxValue;
        }

        public Dictionary<Tuple<INode, INode>, float> FindPathLengths(Graph g, Func<DirectedEdge, float> get_edge_length)
        {
            dists = new Dictionary<Tuple<INode, INode>, float>();
            
            foreach (INode n in g.GetAllNodes())
            {
                SetDist(n, n, 0);
            }

            foreach (DirectedEdge de in g.GetAllEdges())
            {
                float len = get_edge_length(de);

                SetDist(de.Start, de.End, len);
            }

            foreach (INode nk in g.GetAllNodes())
            {
                foreach (INode ni in g.GetAllNodes())
                {
                    foreach (INode nj in g.GetAllNodes())
                    {
                        SetDist(ni, nj, Mathf.Min(GetDist(ni, nj), GetDist(ni, nk) + GetDist(nk, nj)));
                    }
                }
            }

            return dists;
        }
    }
}
