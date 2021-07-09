using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation.G
{
    public interface INode
    {
        Vector2 Pos { get; set; }
        string Name { get; }
        uint Colour { get; set; }
        Vector2 Position { get; set; }
        Vector2 Force { get; set; }
        float Radius { get; }

        bool Connects(INode n);
        bool ConnectsForwards(INode to);
        bool ConnectsBackwards(INode from);
        public float Step(float t);

        DirectedEdge GetConnectionTo(INode node);
        DirectedEdge GetConnectionFrom(INode from);
        List<DirectedEdge> GetConnections();
        List<DirectedEdge> GetInConnections();
        List<DirectedEdge> GetOutConnections();
    }
}