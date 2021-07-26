using Assets.Generation.Gen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Generation.GeomRep.Drawing
{
    public class LoopsDrawer : MonoBehaviour
    {
        public Generator Generator;
        public GameObject LoopDrawTemplate;

        Dictionary<Loop, LineRenderer> RendererMap = new Dictionary<Loop, LineRenderer>();

        private void Update()
        {
            if (Generator != null && Generator.UnionHelper != null)
            {
                foreach (Loop loop in Generator.UnionHelper.MergedLoops)
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
                if (Generator == null
                    || Generator.UnionHelper == null
                    || Generator.UnionHelper.MergedLoops == null
                    || !Generator.UnionHelper.MergedLoops.Contains(loop))
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
