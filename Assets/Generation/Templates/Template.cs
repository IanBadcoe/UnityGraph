using Assets.Generation.G;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Assets.Generation.Templates
{
    [System.Diagnostics.DebuggerDisplay("Name = {Name}, Codes = {Codes}")]
    public class Template
    {
        private readonly int m_num_in_nodes;
        private readonly int m_num_out_nodes;
        private readonly int m_num_internal_nodes;

        private readonly ReadOnlyDictionary<string, NodeRecord> m_nodes;
        private readonly ReadOnlyDictionary<string, ConnectionRecord> m_connections;
        private readonly IReadOnlyList<ForceRecord> m_extra_forces;

        public string Name { get; private set; }

        public string Codes { get; private set; }
        public readonly float ExtraClusterSeparation;

        public Template(TemplateBuilder builder)
        {
            Name = builder.Name;
            Codes = builder.Codes;

            m_nodes = builder.GetUnmodifiableNodes();
            m_connections = builder.GetUnmodifiableConnections();
            m_extra_forces = builder.GetUnmodifiableExtraForces();

            m_num_in_nodes = builder.GetNumInNodes();
            m_num_out_nodes = builder.GetNumOutNodes();
            m_num_internal_nodes = builder.GetNumInternalNodes();

            ExtraClusterSeparation = builder.ExtraClusterSeparation;

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

        public bool Expand(Graph graph, Node target, ClRand random)
        {
            IReadOnlyList<DirectedEdge> target_in_connections = target.GetInConnections();
            IReadOnlyList<DirectedEdge> target_out_connections = target.GetOutConnections();

            HierarchyMetadata hm = new HierarchyMetadata(target.Parent, this);

            if (m_num_in_nodes != target_in_connections.Count)
            {
                return false;
            }

            if (m_num_out_nodes != target_out_connections.Count)
            {
                return false;
            }

            // here we might check codes, if we haven't already

            Dictionary<NodeRecord, Node> template_to_graph = new Dictionary<NodeRecord, Node>
            {
                { FindNodeRecord("<target>"), target }
            };

            HashSet<float> existing_corridor_widths = new HashSet<float>();
            HashSet<float> existing_wall_thicknesses = new HashSet<float>();

            // find nodes for in-connections and map to their NodeRecords
            {
                IEnumerator<DirectedEdge> g_it = target_in_connections.GetEnumerator();

                foreach (NodeRecord nr in m_nodes.Values)
                {
                    if (nr.Type == NodeRecord.NodeType.In)
                    {
                        g_it.MoveNext();

                        Node g_conn = g_it.Current.Start;

                        template_to_graph.Add(nr, g_conn);

                        existing_corridor_widths.Add(g_it.Current.HalfWidth);
                        existing_wall_thicknesses.Add(g_it.Current.WallThickness);
                    }
                }
            }

            // find nodes for out-connections and map to their NodeRecords
            {
                IEnumerator<DirectedEdge> g_it = target_out_connections.GetEnumerator();

                foreach (NodeRecord nr in m_nodes.Values)
                {
                    if (nr.Type == NodeRecord.NodeType.Out)
                    {
                        g_it.MoveNext();

                        Node g_conn = g_it.Current.End;

                        template_to_graph.Add(nr, g_conn);

                        existing_corridor_widths.Add(g_it.Current.HalfWidth);
                        existing_wall_thicknesses.Add(g_it.Current.WallThickness);
                    }
                }
            }

            if (existing_corridor_widths.Count == 0)
            {
                existing_corridor_widths.Add(1);
            }

            if (existing_wall_thicknesses.Count == 0)
            {
                existing_wall_thicknesses.Add(0);
            }

            float chosen_existing_wall_thickness = Util.RemoveRandom(random, existing_wall_thicknesses.ToList());

            // create nodes for each we are adding and map to their NodeRecords
            foreach (NodeRecord nr in m_nodes.Values)
            {
                if (nr.Type == NodeRecord.NodeType.Internal)
                {
                    float wall_thickness = nr.WallThickness;

                    if (wall_thickness == -1)
                    {
                        wall_thickness = chosen_existing_wall_thickness;
                    }

                    Node n = graph.AddNode(nr.Name, nr.Codes, nr.Radius, wall_thickness, nr.Layout);
                    template_to_graph.Add(nr, n);
                }
            }

            ApplyConnections(target, template_to_graph, graph,
                Util.RemoveRandom(random, existing_corridor_widths.ToList()),
                chosen_existing_wall_thickness
            );

            // make three attempts to position the nodes
            // no point if no random components, but pretty cheap to do...
            for (int i = 0; i < 3; i++)
            {
                if (TryPositions(graph, template_to_graph, random))
                {
                    // we needed target for use in position calculations
                    // but now we're done with it
                    graph.RemoveNode(target);

                    ApplyExtraForces(hm, template_to_graph);
                    // ApplyPostExpand(template_to_graph);

                    return true;
                }
            }

            return false;
        }

        private void ApplyExtraForces(HierarchyMetadata hm, Dictionary<NodeRecord, Node> template_to_graph)
        {
            foreach (var fr in m_extra_forces)
            {
                Node n1 = template_to_graph[fr.Node1];
                Node n2 = template_to_graph[fr.Node2];

                hm.AddExtraForce(n1, n2, fr.TargetDist, fr.ForceMultiplier);
            }
        }

        private void ApplyConnections(Node node_replacing, Dictionary<NodeRecord, Node> template_to_graph,
                                      Graph graph, float existing_width, float existing_wall_thickness)
        {
            foreach (DirectedEdge e in node_replacing.GetConnections())
            {
                graph.Disconnect(e.Start, e.End);
            }

            // apply new connections
            foreach (ConnectionRecord cr in m_connections.Values)
            {
                Node nf = template_to_graph[cr.From];
                Node nt = template_to_graph[cr.To];

                float half_width = cr.HalfWidth;

                if (half_width == -1)
                {
                    half_width = existing_width;
                }

                float wall_thickness = cr.WallThickness;

                if (wall_thickness == -1)
                {
                    wall_thickness = existing_wall_thickness;
                }

                DirectedEdge de = graph.Connect(nf, nt, cr.MaxLength, half_width, cr.Layout, wall_thickness);
            }
        }

        private bool TryPositions(Graph graph,
                             Dictionary<NodeRecord, Node> template_to_graph,
                             ClRand rand)
        {
            // position new nodes relative to known nodes
            foreach (NodeRecord nr in m_nodes.Values)
            {
                if (nr.Type == NodeRecord.NodeType.Internal)
                {
                    Node positionOn = template_to_graph[nr.PositionOn];

                    Vector2 pos = positionOn.Position;
                    Vector2 towards_step = new Vector2();
                    Vector2 away_step = new Vector2();

                    if (nr.PositionTowards != null)
                    {
                        Node positionTowards = template_to_graph[nr.PositionTowards];

                        Vector2 d = positionTowards.Position - pos;

                        towards_step = d * 0.1f;
                    }

                    if (nr.PositionAwayFrom != null)
                    {
                        Node positionAwayFrom = template_to_graph[nr.PositionAwayFrom];

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

                    Node n = template_to_graph[nr];

                    n.Position = pos;
                }
            }

            return !GraphUtil.AnyCrossingEdges(graph.GetAllEdges());
        }

        public static String MakeConnectionName(String from, String to)
        {
            return from + "->" + to;
        }

        //private void ApplyPostExpand(Dictionary<NodeRecord, Node> template_to_graph)
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