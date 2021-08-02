using Assets.Generation.G;
using UnityEngine;

namespace Assets.Behaviour.Drawing
{
    public class CircleDrawerBehaviour : MonoBehaviour
    {
        public INode Node { get; set; }

        LineRenderer Renderer;

        const int NumFacets = 20;

        private void Awake()
        {
            Renderer = GetComponent<LineRenderer>();
        }

        private void Update()
        {
            if (Node != null && Renderer != null)
            {
                Renderer.positionCount = NumFacets;

                for (int i = 0; i < NumFacets; i++)
                {
                    float ang = (float)i / NumFacets * Mathf.PI * 2;

                    Vector3 position = new Vector3(
                        Mathf.Sin(ang) * Node.Radius + Node.Position.x,
                        Mathf.Cos(ang) * Node.Radius + Node.Position.y,
                        0);

                    Renderer.SetPosition(i, position);
                }
            }
        }
    }
}
