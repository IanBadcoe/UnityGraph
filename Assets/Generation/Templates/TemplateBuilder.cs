using Assets.Generation.G.GLInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Generation.Templates
{
    public class TemplateBuilder
    {
        public string Name { get; }
        public string Codes { get; }

        private Dictionary<string, NodeRecord> m_nodes = new Dictionary<string, NodeRecord>();
        private Dictionary<string, ConnectionRecord> m_connections = new Dictionary<string, ConnectionRecord>();

        // just to avoid keeping counting
        private int m_num_in_nodes = 0;
        private int m_num_out_nodes = 0;
        private int m_num_internal_nodes = 0;

        public readonly IGeomLayoutFactory DefaultLayoutCreator;

        // private final Template.IPostExpand m_post_expand;

        //public TemplateBuilder(string name, string codes)
        //    : this(name, codes, null)
        //{
        //}

        public TemplateBuilder(string name, string codes,
            IGeomLayoutFactory defaultLayoutCreator/*, Template.IPostExpand post_expand*/)
        {
            Name = name;
            Codes = codes;
            DefaultLayoutCreator = defaultLayoutCreator;

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

        public void AddNode(NodeRecord.NodeType type, string name)
        {
            AddNode(type, name, false,
                    "<target>", null, null,
                    "", 0f);
        }

        // types In and Out ignore all parameters after "name"
        public void AddNode(NodeRecord.NodeType type, string name, bool nudge,
              string positionOnName, string positionTowardsName,
              string positionAwayFromName,
              string codes, float radius)
        {
            AddNode(type, name, nudge,
                    positionOnName, positionTowardsName, positionAwayFromName,
                    codes, radius,
                    0xff8c8c8c,
                    DefaultLayoutCreator);
        }

        public void AddNode(NodeRecord.NodeType type, string name, bool nudge,
              string positionOnName, string positionTowardsName,
              string positionAwayFromName,
              string codes, float radius,
                uint colour)
        {
            AddNode(type, name, nudge,
                    positionOnName, positionTowardsName, positionAwayFromName,
                    codes, radius,
                    colour,
                    DefaultLayoutCreator);
        }

        public void AddNode(NodeRecord.NodeType type, string name, bool nudge,
             string positionOnName, string positionTowardsName,
             string positionAwayFromName,
             string codes, float radius,
             uint colour,
             IGeomLayoutFactory layoutCreator)
        {
            if (name.Contains("->"))
                throw new ArgumentException("engine.Node name: '" + name + "' cannot contain '->'.");

            if (name.Contains("<target>"))
                throw new ArgumentException("engine.Node name: '" + name + "' is reserved.");

            if (positionOnName == null)
                throw new NullReferenceException("'positionOnName' cannot be null.");

            if (type == NodeRecord.NodeType.Target)
                throw new ArgumentException("User cannot add a node of type 'Target'.");

            if (FindNodeRecord(name) != null)
                throw new DuplicateNodeException(name);

            // required
            NodeRecord positionOn = null;
            // optional
            NodeRecord positionTowards = null;
            NodeRecord positionAwayFrom = null;

            if (type == NodeRecord.NodeType.Internal)
            {
                positionOn = FindNodeRecord(positionOnName);

                if (positionOn == null)
                    throw new UnknownNodeException(positionOnName, "positionOnName");

                if (positionTowardsName != null)
                {
                    positionTowards = FindNodeRecord(positionTowardsName);

                    if (positionTowards == null)
                        throw new UnknownNodeException(positionTowardsName, "positionTowardsName");
                }

                if (positionAwayFromName != null)
                {
                    positionAwayFrom = FindNodeRecord(positionAwayFromName);

                    if (positionAwayFrom == null)
                        throw new UnknownNodeException(positionAwayFromName, "positionAwayFromName");
                }
            }

            m_nodes.Add(name, new NodeRecord(type, name, nudge,
                  positionOn, positionTowards, positionAwayFrom,
                  codes, radius,
                  colour,
                  layoutCreator));

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

        public NodeRecord FindNodeRecord(string name)
        {
            NodeRecord ret;
            if (m_nodes.TryGetValue(name, out ret)) {
                return ret;
            }

            return null;
        }

        public ConnectionRecord FindConnectionRecord(string from, string to)
        {
            ConnectionRecord ret;
            if (m_connections.TryGetValue(Template.MakeConnectionName(from, to), out ret))
            {
                return ret;
            }

            return null;
        }

        public void Connect(string from, string to,
                            float min_length, float max_length,
                            float half_width)
        {
            Connect(from, to,
                    min_length, max_length,
                    half_width,
                    0xffb4b4b4);
        }

       public void Connect(string from, string to,
                           float min_length, float max_length,
                           float half_width,
                           uint colour)
        {
            if (from == null)
                throw new NullReferenceException("Null node name: 'from'.");

            if (to == null)
                throw new NullReferenceException("Null node name: 'to'.");

            // only one connection between nodes is permitted
            if (FindConnectionRecord(from, to) != null)
                throw new ArgumentException("A connection from '" + from + "' to '" + to + "' already exists.");

            // nor are we allowed forwards and backwards connections
            if (FindConnectionRecord(to, from) != null)
                throw new ArgumentException("A connection from '" + to + "' from '" + to + "' already exists.");

            NodeRecord nrf = FindNodeRecord(from);
            NodeRecord nrt = FindNodeRecord(to);

            if (nrf == null)
                throw new UnknownNodeException(from, "from");

            if (nrt == null)
                throw new UnknownNodeException(to, "to");

            if (nrf.Type == NodeRecord.NodeType.Target)
                throw new ArgumentException("Cannot connect from node 'Target' as it is being replaced.");

            if (nrt.Type == NodeRecord.NodeType.Target)
                throw new ArgumentException("Cannot connect to node 'Target' as it is being replaced.");

            m_connections.Add(
                  Template.MakeConnectionName(from, to),
                  new ConnectionRecord(nrf, nrt, min_length, max_length, half_width, colour));
       }

        public ReadOnlyDictionary<string, NodeRecord> GetUnmodifiableNodes()
        {
            return new ReadOnlyDictionary<string, NodeRecord>(m_nodes);
        }

        public ReadOnlyDictionary<string, ConnectionRecord> GetUnmodifiableConnections()
        {
            return new ReadOnlyDictionary<string, ConnectionRecord>(m_connections);
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
