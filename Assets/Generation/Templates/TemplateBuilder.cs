using Assets.Generation.GeomRep;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Assets.Generation.Templates
{
    public class TemplateBuilder
    {
        public string Name { get; }
        public string Codes { get; }

        private Dictionary<string, NodeRecord> m_nodes = new Dictionary<string, NodeRecord>();
        private Dictionary<string, ConnectionRecord> m_connections = new Dictionary<string, ConnectionRecord>();
        private readonly List<ForceRecord> m_extra_forces = new List<ForceRecord>();

        // just to avoid keeping counting
        private int m_num_in_nodes = 0;
        private int m_num_out_nodes = 0;
        private int m_num_internal_nodes = 0;

        public float ExtraClusterSeparation;


        public TemplateBuilder(string name, string codes)
        {
            Name = name;
            Codes = codes;

            // a dummy entry used to represent the node we are replacing in positioning rules
            m_nodes.Add("<target>", new NodeRecord(NodeRecord.NodeType.Target, "<target>",
                  false, null, null, null,
                  null, 0, 0, null));

            // m_post_expand = post_expand;
        }

        // don't want people recovering from these as just bad programming
        public abstract class TemplateException : Exception
        {
            protected TemplateException(string message)
                : base(message)
            {
            }
        }

        public class UnknownNodeException : TemplateException
        {
            public UnknownNodeException(string name, string argument)
                : base("Attempt to reference a node: '" + name + "' which does not exist.")
            {
                NodeName = name;
                Argument = argument;
            }

            public readonly string NodeName;
            public readonly string Argument;
        }

        public class DuplicateNodeException : TemplateException
        {
            public DuplicateNodeException(string name)
                : base("Attempt to add a node: '" + name + "' to a template when a node of name is already present.")
            {
                NodeName = name;
            }

            public readonly string NodeName;
        }

        // used mostly for in and out "i" and "o" nodes in templates
        // e.g. nodes which match something in the pre-graph and are thus never instantiated
        // (thus zero radius, no flags, no GeomLayout
        public void AddNode(NodeRecord.NodeType type, string name)
        {
            AddNode(type, name, false,
                    "<target>", null, null,
                    "", 0, 0, null);
        }

        // types In and Out ignore all parameters after "name"

        public void AddNode(NodeRecord.NodeType type, string name, bool nudge,
             string positionOnName, string positionTowardsName,
             string positionAwayFromName,
             string codes, float radius, float wall_thickness,
             GeomLayout layout)
        {
            if (name.Contains("->"))
            {
                throw new ArgumentException("engine.Node name: '" + name + "' cannot contain '->'.");
            }

            if (name.Contains("<target>"))
            {
                throw new ArgumentException("engine.Node name: '" + name + "' is reserved.");
            }

            if (positionOnName == null)
            {
                throw new NullReferenceException("'positionOnName' cannot be null.");
            }

            if (type == NodeRecord.NodeType.Target)
            {
                throw new ArgumentException("User cannot add a node of type 'Target'.");
            }

            if (FindNodeRecord(name) != null)
            {
                throw new DuplicateNodeException(name);
            }

            // required
            NodeRecord positionOn = null;
            // optional
            NodeRecord positionTowards = null;
            NodeRecord positionAwayFrom = null;

            if (type == NodeRecord.NodeType.Internal)
            {
                positionOn = FindNodeRecord(positionOnName);

                if (positionOn == null)
                {
                    throw new UnknownNodeException(positionOnName, "positionOnName");
                }

                if (positionTowardsName != null)
                {
                    positionTowards = FindNodeRecord(positionTowardsName);

                    if (positionTowards == null)
                    {
                        throw new UnknownNodeException(positionTowardsName, "positionTowardsName");
                    }
                }

                if (positionAwayFromName != null)
                {
                    positionAwayFrom = FindNodeRecord(positionAwayFromName);

                    if (positionAwayFrom == null)
                    {
                        throw new UnknownNodeException(positionAwayFromName, "positionAwayFromName");
                    }
                }
            }

            m_nodes.Add(name, new NodeRecord(type, name, nudge,
                  positionOn, positionTowards, positionAwayFrom,
                  codes, radius, wall_thickness,
                  layout));

            switch (type)
            {
                case NodeRecord.NodeType.In:
                    m_num_in_nodes++;
                    break;
                case NodeRecord.NodeType.Out:
                    m_num_out_nodes++;
                    break;
                case NodeRecord.NodeType.Internal:
                    m_num_internal_nodes++;
                    break;
            }
        }

        internal void ExtraForce(string node1, string node2, float targetDistance, float forceScale)
        {
            NodeRecord nr1 = m_nodes[node1];
            NodeRecord nr2 = m_nodes[node2];

            m_extra_forces.Add(new ForceRecord(targetDistance, nr1, nr2, forceScale));
        }

        public NodeRecord FindNodeRecord(string name)
        {
            if (m_nodes.TryGetValue(name, out NodeRecord ret))
            {
                return ret;
            }

            return null;
        }

        public ConnectionRecord FindConnectionRecord(string from, string to)
        {
            if (m_connections.TryGetValue(Template.MakeConnectionName(from, to), out ConnectionRecord ret))
            {
                return ret;
            }

            return null;
        }

        public void Connect(string from, string to,
                            float max_length,
                            float half_width,
                            GeomLayout layout,
                            float wall_thickness = 0)
        {
            if (from == null)
            {
                throw new NullReferenceException("Null node name: 'from'.");
            }

            if (to == null)
            {
                throw new NullReferenceException("Null node name: 'to'.");
            }

            // only one connection between nodes is permitted
            if (FindConnectionRecord(from, to) != null)
            {
                throw new ArgumentException("A connection from '" + from + "' to '" + to + "' already exists.");
            }

            // nor are we allowed forwards and backwards connections
            if (FindConnectionRecord(to, from) != null)
            {
                throw new ArgumentException("A connection from '" + to + "' from '" + to + "' already exists.");
            }

            NodeRecord nrf = FindNodeRecord(from);
            NodeRecord nrt = FindNodeRecord(to);

            if (nrf == null)
            {
                throw new UnknownNodeException(from, "from");
            }

            if (nrt == null)
            {
                throw new UnknownNodeException(to, "to");
            }

            if (nrf.Type == NodeRecord.NodeType.Target)
            {
                throw new ArgumentException("Cannot connect from node 'Target' as it is being replaced.");
            }

            if (nrt.Type == NodeRecord.NodeType.Target)
            {
                throw new ArgumentException("Cannot connect to node 'Target' as it is being replaced.");
            }

            m_connections.Add(
                  Template.MakeConnectionName(from, to),
                  new ConnectionRecord(nrf, nrt, max_length, half_width, wall_thickness, layout));
        }

        public ReadOnlyDictionary<string, NodeRecord> GetUnmodifiableNodes()
        {
            return new ReadOnlyDictionary<string, NodeRecord>(m_nodes);
        }

        public ReadOnlyDictionary<string, ConnectionRecord> GetUnmodifiableConnections()
        {
            return new ReadOnlyDictionary<string, ConnectionRecord>(m_connections);
        }

        public IReadOnlyList<ForceRecord> GetUnmodifiableExtraForces()
        {
            return m_extra_forces;
        }

        public int GetNumInNodes()
        {
            return m_num_in_nodes;
        }

        public int GetNumOutNodes()
        {
            return m_num_out_nodes;
        }

        public int GetNumInternalNodes()
        {
            return m_num_internal_nodes;
        }

        public void Clear()
        {
            m_nodes = null;
            m_connections = null;

            m_num_in_nodes = -1;
            m_num_out_nodes = -1;
            m_num_internal_nodes = -1;
        }

        public Template Build()
        {
            return new Template(this);
        }

        //Template.IPostExpand GetPostExpand()
        //{
        //    return m_post_expand;
        //}
    }
}
