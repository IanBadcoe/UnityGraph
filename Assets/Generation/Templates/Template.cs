using Assets.Generation.Templates;
using Assets.Generation.G;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using Assets.Generation.U;

namespace Assets.Generation.Templates
{
    public class Template
    {
        private readonly int m_num_in_nodes;
        private readonly int m_num_out_nodes;
        private readonly int m_num_internal_nodes;

        private readonly ReadOnlyDictionary<string, NodeRecord> m_nodes;
        private readonly ReadOnlyDictionary<string, ConnectionRecord> m_connections;

        public string Name { get; private set; }

        public string Codes { get; private set; }

        public Template(TemplateBuilder builder)
        {
            Name = builder.Name;
            Codes = builder.Codes;

            m_nodes = builder.GetUnmodifiableNodes();
            m_connections = builder.GetUnmodifiableConnections();

            m_num_in_nodes = builder.GetNumInNodes();
            m_num_out_nodes = builder.GetNumOutNodes();
            m_num_internal_nodes = builder.GetNumInternalNodes();

            //m_post_expand = builder.GetPostExpand();

            // cannot use this again
            builder.Clear();
        }

        public int NodesAdded()
        {
            // we add a node for each internal node but we
            // remove the one we are replacing
            return m_num_internal_nodes - 1;
        }

        private NodeRecord FindNodeRecord(string name)
        {
            return m_nodes[name];
        }

        public bool Expand(Graph graph, INode target, ClRand random)
        {
            IReadOnlyList<DirectedEdge> target_in_connections = target.GetInConnections();
            IReadOnlyList<DirectedEdge> target_out_connections = target.GetOutConnections();

            if (m_num_in_nodes != target_in_connections.Count)
            {
                return false;
            }

            if (m_num_out_nodes != target_out_connections.Count)
            {
                return false;
            }

            // here we might check codes, if we haven't already

            Dictionary<NodeRecord, INode> template_to_graph = new Dictionary<NodeRecord, INode>();

            template_to_graph.Add(FindNodeRecord("<target>"), target);

            // create nodes for each we are adding and map to their NodeRecords
            foreach (NodeRecord nr in m_nodes.Values)
            {
                if (nr.Type == NodeRecord.NodeType.Internal)
                {
                    INode n = graph.AddNode(nr.Name, nr.Codes, Name, nr.Radius, nr.LayoutCreator);
                    template_to_graph.Add(nr, n);
                    n.Colour = nr.Colour;
                }
            }

            // find nodes for in connections and map to their NodeRecords
            {
                IEnumerator<DirectedEdge> g_it = target_in_connections.GetEnumerator();

                foreach (NodeRecord nr in m_nodes.Values)
                {
                    if (nr.Type == NodeRecord.NodeType.In)
                    {
                        g_it.MoveNext();

                        INode g_conn = g_it.Current.Start;

                        template_to_graph.Add(nr, g_conn);
                    }
                }
            }

            // find nodes for out connections and map to their NodeRecords
            {
                IEnumerator<DirectedEdge> g_it = target_out_connections.GetEnumerator();

                foreach (NodeRecord nr in m_nodes.Values)
                {
                    if (nr.Type == NodeRecord.NodeType.Out)
                    {
                        g_it.MoveNext();

                        INode g_conn = g_it.Current.End;

                        template_to_graph.Add(nr, g_conn);
                    }
                }
            }

            ApplyConnections(target, template_to_graph, graph);

            // make three attempts to position the nodes
            // no point if no random components, but pretty cheap to do...
            for (int i = 0; i < 3; i++)
            {
                if (TryPositions(graph, template_to_graph, random))
                {
                    // we needed target for use in position calculations
                    // but now we're done with it
                    graph.RemoveNode(target);

                    // ApplyPostExpand(template_to_graph);

                    return true;
                }
            }

            return false;
        }

        private void ApplyConnections(INode node_replacing, Dictionary<NodeRecord, INode> template_to_graph,
                                      Graph graph)
        {
            foreach (DirectedEdge e in node_replacing.GetConnections())
            {
                graph.Disconnect(e.Start, e.End);
            }

            // apply new connections
            foreach (ConnectionRecord cr in m_connections.Values)
            {
                INode nf = template_to_graph[cr.From];
                INode nt = template_to_graph[cr.To];

                DirectedEdge de = graph.Connect(nf, nt, cr.MinLength, cr.MaxLength, cr.HalfWidth);
                de.Colour = cr.Colour;
            }
        }

        private bool TryPositions(Graph graph,
                             Dictionary<NodeRecord, INode> template_to_graph,
                             ClRand rand)
        {
            // position new nodes relative to known nodes
            foreach (NodeRecord nr in m_nodes.Values)
            {
                if (nr.Type == NodeRecord.NodeType.Internal)
                {
                    INode positionOn = template_to_graph[nr.PositionOn];

                    Vector2 pos = positionOn.Position;
                    Vector2 towards_step = new Vector2();
                    Vector2 away_step = new Vector2();

                    if (nr.PositionTowards != null)
                    {
                        INode positionTowards = template_to_graph[nr.PositionTowards];

                        Vector2 d = positionTowards.Position - pos;

                        towards_step = d * 0.1f;
                    }

                    if (nr.PositionAwayFrom != null)
                    {
                        INode positionAwayFrom = template_to_graph[nr.PositionAwayFrom];

                        Vector2 d = positionAwayFrom.Position - pos;

                        away_step = d * 0.1f;
                    }

                    pos = pos + towards_step - away_step;

                    if (nr.Nudge)
                    {
                        // we make the typical scale of edges and node radii on the order of
                        // 100, so a displacement of 5 should be enough to separate things enough to avoid
                        // stupid forces, while being nothing like as far as the nearest existing neighbours
                        float angle = (float)(rand.Nextfloat() * (2 * Mathf.PI));
                        pos = pos + new Vector2(Mathf.Sin(angle) * 5, Mathf.Cos(angle) * 5);
                    }

                    INode n = template_to_graph[nr];

                    n.Position = pos;
                }
            }

            return GraphUtil.AnyCrossingEdges(graph.GetAllEdges());
        }

        public static String MakeConnectionName(String from, String to)
        {
            return from + "->" + to;
        }

        //private void ApplyPostExpand(Dictionary<NodeRecord, INode> template_to_graph)
        //{
        //    if (m_post_expand == null)
        //        return;

        //    foreach (NodeRecord nr in m_nodes.Values)
        //    {
        //        // could have chance to modify existing (e.g. In/Out nodes?)
        //        if (nr.Type == NodeType.Internal)
        //        {
        //            m_post_expand.AfterExpand(template_to_graph.get(nr));
        //        }
        //    }

        //    m_post_expand.Done();
        //}

    }
}