using Assets.Generation.G;
using System;
using System.Collections.Generic;
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
        Dictionary<Tuple<Node, Node>, float> dists;

        void SetDist(Node a, Node b, float dist)
        {
            dists[new Tuple<Node, Node>(a, b)] = dist;
            dists[new Tuple<Node, Node>(b, a)] = dist;
        }

        public float GetDist(Node a, Node b)
        {
            var key = new Tuple<Node, Node>(a, b);

            if (dists.ContainsKey(key))
            {
                return dists[key];
            }

            return float.MaxValue;
        }

        public Dictionary<Tuple<Node, Node>, float> FindPathLengths(Graph g, Func<DirectedEdge, float> get_edge_length)
        {
            dists = new Dictionary<Tuple<Node, Node>, float>();

            // could fill the whole matrix with summed radii (or zero for the diagonal)
            // except that we'd need to add in the minimum separation and if we ever get >1 value for that
            // it would be a pain, so ATM easier to consider only paths here and min those with radii + minimum separation
            // at point of use
            foreach (Node n in g.GetAllNodes())
            {
                SetDist(n, n, 0);
            }

            foreach (DirectedEdge de in g.GetAllEdges())
            {
                float len = get_edge_length(de);

                SetDist(de.Start, de.End, len);
            }

            foreach (Node nk in g.GetAllNodes())
            {
                foreach (Node ni in g.GetAllNodes())
                {
                    foreach (Node nj in g.GetAllNodes())
                    {
                        SetDist(ni, nj, Mathf.Min(GetDist(ni, nj), GetDist(ni, nk) + GetDist(nk, nj)));
                    }
                }
            }

            return dists;
        }
    }
}
