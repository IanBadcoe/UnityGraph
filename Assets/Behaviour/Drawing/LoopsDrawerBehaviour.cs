using Assets.Generation.GeomRep;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviour.Drawing
{
    public class LoopsDrawerBehaviour : MonoBehaviour
    {
        [SerializeField, SerializeReference]
        public DataProvider DP;
        public GameObject LoopDrawTemplate;
        public bool ControlCamera;
        readonly Dictionary<Loop, LineRenderer> RendererMap = new Dictionary<Loop, LineRenderer>();

        Camera Camera;

        private void Awake()
        {
            Camera = Transform.FindObjectOfType<Camera>();
        }

        private void Update()
        {
            var loops = DP.GetLoops();

            if (loops != null)
            {
                foreach (Loop loop in loops)
                {
                    if (!RendererMap.ContainsKey(loop))
                    {
                        float len = loop.ParamRange;

                        Vector3[] points = loop.Facet(1);

                        var renderer = GameObject.Instantiate(LoopDrawTemplate, transform);

                        LineRenderer lr = renderer.transform.GetComponent<LineRenderer>();
                        RendererMap[loop] = lr;

                        lr.positionCount = points.Length;
                        lr.SetPositions(points);
                    }
                }
            }

            List<Loop> to_remove = new List<Loop>();

            foreach (Loop loop in RendererMap.Keys)
            {
                if (loops == null || !loops.Contains(loop))
                {
                    GameObject.Destroy(RendererMap[loop]);
                    to_remove.Add(loop);
                }
            }

            foreach (Loop loop in to_remove)
            {
                RendererMap.Remove(loop);
            }


            if (ControlCamera && Camera != null)
            {
                Box2 bounds = RendererMap.Keys.Aggregate(new Box2(), (b, l) => b.Union(l.GetBounds()));

                Camera.transform.position = bounds.Centre() + new Vector3(0, 0, -300);

                float aspect_ratio = Screen.width / (float)Screen.height;
                float req_size = Mathf.Max(bounds.Diagonal.y, bounds.Diagonal.x / aspect_ratio);

                Camera.orthographicSize = req_size / 2;
            }
        }
    }
}
