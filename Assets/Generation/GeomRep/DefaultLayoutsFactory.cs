using Assets.Generation.G;
using Assets.Generation.G.GLInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Generation.GeomRep
{
    class DefaultLayoutsFactory : IGeomLayoutFactory
    {
        public IGeomLayout Create(INode n)
        {
            throw new NotImplementedException();
        }

        public IGeomLayout Create(DirectedEdge de)
        {
            throw new NotImplementedException();
        }
    }
}
