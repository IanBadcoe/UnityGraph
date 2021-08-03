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
        readonly Dictionary<Loop, LineRenderer> RendererMap = new Dictionary<Loop, LineRenderer>();

        private void Update()
        {
            var loops = DP.GetLoops();

            if (loops != null)
            {
                foreach (Loop loop in loops)
                {
                    if (!RendererMap.ContainsKey(loop))
                    {
                        Vector3[] points = loop.Facet(10.0f);

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
        }
    }
}
