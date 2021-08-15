using Assets.Generation.Gen;
using Assets.Generation.GeomRep;
using Assets.Generation.Templates;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation.G
{
    public interface INode : IHMChild, IRelaxationParamSource
    {
        string Name { get; }
        string Codes { get; }
        Vector2 Position { get; set; }
        Vector2 Force { get; set; }
        float Radius { get; }

        public GeomLayout Layout { get; }

        bool Connects(INode n);
        bool ConnectsForwards(INode to);
        bool ConnectsBackwards(INode from);
        public float Step(float t);

        DirectedEdge GetConnectionTo(INode node);
        DirectedEdge GetConnectionFrom(INode from);
        IReadOnlyList<DirectedEdge> GetConnections();
        IReadOnlyList<DirectedEdge> GetInConnections();
        IReadOnlyList<DirectedEdge> GetOutConnections();
    }
}