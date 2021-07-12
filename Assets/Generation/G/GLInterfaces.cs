using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Generation.G
{
    namespace GLInterfaces
    {
        // blind interface so graph doesn't need any geometry implementation details
        public interface IGeomLayout
        {

        }

        public interface IGeomLayoutFactory
        {
            IGeomLayout Create(INode n);
            IGeomLayout Create(DirectedEdge de);
        }
    }
}
