using Assets.Generation.G;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public class CircularGeomLayout : GeomLayout
    {
        public override Loop MakeBaseGeometry(INode node)
        {
            return new Loop(new CircleCurve(node.Position, node.Radius));
        }
    }
}
