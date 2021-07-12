using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Generation.G
{
    abstract public class GeomLayout
    {
        //// one +ve loop that cuts the outer envelope of the space the node will occupy
        //public abstract Loop makeBaseGeometry();

        //// one or more -ve loops that put things like pillars back inside
        //// the base geometry
        //public abstract LoopSet makeDetailGeometry();

        //public static GeomLayout makeDefaultCorridor(DirectedEdge de)
        //{
        //    // scale the corridor rectangle's width down slightly
        //    // so that it doesn't precisely hit at a tangent to any adjoining junction-node's circle
        //    // -- that causes awkward numerical precision problems in the curve-curve intersection routines
        //    // which can throw out the union operation
        //    return new RectangularGeomLayout(de.Start.getPos(),
        //          de.End.getPos(), de.HalfWidth * 0.99);
        //}
    }
}
