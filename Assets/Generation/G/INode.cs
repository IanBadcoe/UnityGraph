using UnityEngine;

namespace Generation.G
{
    public interface INode
    {
        Vector2 Pos { get; set; }
        string Name { get; }

        bool Connects(INode n);
        bool ConnectsForwards(INode to);
        bool ConnectsBackwards(INode from);
        DirectedEdge GetConnectionTo(INode node);
        DirectedEdge GetConnectionFrom(INode from);
    }
}