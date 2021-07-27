using Assets.Extensions;
using Assets.Generation.G;
using UnityEngine;

namespace Assets.Generation.Gen.Drawing
{
    public class RectangleDrawer : MonoBehaviour, IDrawer
    {
        public DirectedEdge Edge { get; set; }

        LineRenderer Renderer;

        private void Awake()
        {
            Renderer = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            if (Edge != null && Renderer != null)
            {
                Renderer.positionCount = 4;

                Vector2 half_width = (Edge.End.Position - Edge.Start.Position).Rot90();
                half_width.Normalize();
                half_width *= Edge.HalfWidth;

                Renderer.SetPosition(0, Edge.Start.Position + half_width);
                Renderer.SetPosition(1, Edge.Start.Position - half_width);
                Renderer.SetPosition(2, Edge.End.Position - half_width);
                Renderer.SetPosition(3, Edge.End.Position + half_width);
            }
        }
    }
}
