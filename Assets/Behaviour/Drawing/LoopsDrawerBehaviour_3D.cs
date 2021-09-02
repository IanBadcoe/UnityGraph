using Assets.Generation.GeomRep;
using Assets.Generation.U;
using LibTessDotNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviour.Drawing
{
    public class LoopsDrawerBehaviour_3D : MonoBehaviour
    {
        public DataProvider DP;
        public GameObject MeshDrawTemplate;
        public bool ControlCamera;

        readonly Dictionary<ILoopSet, GameObject> RendererMap =
            new Dictionary<ILoopSet, GameObject>(
                new Intersector.ReferenceComparer<ILoopSet>());

        private LayerConfigBehaviour LCB;

        Camera Camera;

        private void Awake()
        {
            Camera = Transform.FindObjectOfType<Camera>();
            LCB = Transform.FindObjectOfType<LayerConfigBehaviour>();
        }

        private void Update()
        {
            var loopsets = DP.GetLoops();

            if (loopsets != null)
            {
                foreach (var cd in GameObject.FindGameObjectsWithTag("WIP"))
                {
                    Destroy(cd);
                }

                foreach (var loopset in loopsets)
                {
                    if (!RendererMap.ContainsKey(loopset.Value))
                    {
                        string layer = loopset.Key;

                        Color col = new Color(1, 0.5f, 0.5f);

                        if (LCB != null)
                        {
                            LCB.ColourDict.TryGetValue(layer, out col);
                        }

                        Tess tess = new Tess();

                        int total_verts = 0;

                        List<ContourVertex[]> contours = new List<ContourVertex[]>();

                        foreach (var loop in loopset.Value)
                        {
                            float len = loop.ParamRange;

                            var points = loop.SmartFacet(0.1f);

                            var contour = points.Select(x => new ContourVertex()
                            {
                                Position = new Vec3(x.x, x.y, 0)
                            }).ToArray();

                            tess.AddContour(contour);

                            contours.Add(contour);
                            total_verts += points.Length;
                        }

                        tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3);

                        // we do not expect tesselation to add any verts
                        Assertion.Assert(total_verts == tess.Vertices.Length);

                        Vector3[] top_verts = tess.Vertices.Select(x => new Vector3(x.Position.X, x.Position.Z, x.Position.Y)).ToArray();
                        Vector3[] bot_verts = tess.Vertices.Select(x => new Vector3(x.Position.X, x.Position.Z - 1, x.Position.Y)).ToArray();

                        int[] top_tris = tess.Elements;
                        int[] bot_tris = new int[top_tris.Length];

                        for(int i = 0; i < top_tris.Length; i += 3)
                        {
                            // bottom tris need the rotation direction flipping
                            bot_tris[i + 0] = top_tris[i + 0] + top_verts.Length;
                            bot_tris[i + 1] = top_tris[i + 2] + top_verts.Length;
                            bot_tris[i + 2] = top_tris[i + 1] + top_verts.Length;
                        }

                        foreach (var contour in contours)
                        {
                            tess.AddContour(contour);
                        }

                        tess.Tessellate(WindingRule.Positive, ElementType.BoundaryContours, 3);

                        Dictionary<Vector3, int> vert_map = new Dictionary<Vector3, int>();

                        for(int i = 0; i < top_verts.Length; i++)
                        {
                            vert_map[top_verts[i]] = i;
                        }

                        List<int[]> side_tris = new List<int[]>();

                        Vector3[] temp_verts = tess.Vertices.Select(x => new Vector3(x.Position.X, x.Position.Z, x.Position.Y)).ToArray();

                        for (int i = 0; i < tess.Elements.Length; i += 2)
                        {
                            int start_idx = tess.Elements[i];
                            int length = tess.Elements[i + 1];

                            int[] tris = new int[length * 6];

                            for (int j = 0; j < length; j++)
                            {
                                int new_idx1 = start_idx + j;
                                int new_idx2 = start_idx + (j + 1) % length;

                                var v1 = temp_verts[new_idx1];
                                var v2 = temp_verts[new_idx2];

                                int old_idx1 = vert_map[v1];
                                int old_idx2 = vert_map[v2];

                                // we need to give the sides their own normals, so we need to duplicate the verts
                                // thus the arrangement of verts is:
                                //               0  <=  idx  <  total_verts       -->   top face
                                //     total_verts  <=  idx  <  total_verts * 2   -->   bottom face
                                // total_verts * 2  <=  idx  <  total_verts * 3   -->   top face verts for edges
                                // total_verts * 3  <=  idx  <  total_verts * 4   -->   bottom face verts for edges
 
                                tris[j * 6 + 0] = old_idx1 + total_verts * 2;
                                tris[j * 6 + 2] = old_idx2 + total_verts * 3;
                                tris[j * 6 + 1] = old_idx1 + total_verts * 3;

                                tris[j * 6 + 3] = old_idx2 + total_verts * 2;
                                tris[j * 6 + 4] = old_idx1 + total_verts * 2;
                                tris[j * 6 + 5] = old_idx2 + total_verts * 3;

                                side_tris.Add(tris);
                            }
                        }

                        var renderer = Instantiate(MeshDrawTemplate, transform);

                        MeshRenderer mr = renderer.transform.GetComponent<MeshRenderer>();
                        MeshFilter mf = renderer.transform.GetComponent<MeshFilter>();
                        var mesh = new Mesh
                        {
                            vertices =
                                top_verts.Concat(bot_verts)
                                .Concat(top_verts).Concat(bot_verts)
                                .ToArray(),
                            triangles = top_tris
                                .Concat(bot_tris)
                                .Concat(side_tris.SelectMany(x => x))
                                .ToArray()
                        };

                        mesh.RecalculateNormals();

                        mf.mesh = mesh;

                        mr.materials[0].color = col;

                        RendererMap[loopset.Value] = renderer;

                        //lr.positionCount = points.Length;
                        //lr.SetPositions(points);
                        //lr.startColor = lr.endColor = col;
                        //lr.sortingOrder = draw_priority;
                    }
                }
            }

            var to_remove = new List<ILoopSet>();

            foreach (var ls in RendererMap.Keys)
            {
                if (loopsets == null || !loopsets.Values.Contains(ls))
                {
                    Destroy(RendererMap[ls]);
                    to_remove.Add(ls);
                }
            }

            foreach (var ls in to_remove)
            {
                RendererMap.Remove(ls);
            }


            //if (ControlCamera && Camera != null)
            //{
            //    Box2 bounds = RendererMap.Keys.Aggregate(new Box2(), (b, l) => b.Union(l.GetBounds()));

            //    Camera.transform.position = bounds.Centre() + new Vector3(0, 0, -300);

            //    float aspect_ratio = Screen.width / (float)Screen.height;
            //    float req_size = Mathf.Max(bounds.Diagonal.y, bounds.Diagonal.x / aspect_ratio);

            //    Camera.orthographicSize = req_size / 2;
            //}
        }
    }
}
