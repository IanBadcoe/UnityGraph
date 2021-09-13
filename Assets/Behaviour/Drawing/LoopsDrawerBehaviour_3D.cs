using Assets.Generation.GeomRep;
using Assets.Generation.U;
using LibTessDotNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Behaviour.LayerConfigBehaviour;

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
                        var layer_data = LayerData.Default;

                        if (LCB != null)
                        {
                            LCB.LayerDict.TryGetValue(loopset.Key, out layer_data);
                        }

                        GameObject renderer = CreateRenderer(loopset.Value, layer_data);
                        CreateCollision(loopset.Value, renderer, layer_data);

                        RendererMap[loopset.Value] = renderer;
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

        private void CreateCollision(ILoopSet loopset, GameObject renderer, LayerData data)
        {
            // 50cm resolution should be OK, because every curve's start and end are included anyway
            // so nothing small should be missed, it will just be circles are a bit more polygonal
            Mesh mesh = RenderSolid(loopset, 0.5f, true, data.BaseHeight, data.TopHeight);

            MeshCollider mc = renderer.transform.GetComponent<MeshCollider>();

            mc.sharedMesh = mesh;
        }

        private GameObject CreateRenderer(ILoopSet loopset, LayerData data)
        {
            Mesh mesh = RenderSolid(loopset, 0.1f, true, data.BaseHeight, data.TopHeight);

            var renderer = Instantiate(MeshDrawTemplate, transform);

            MeshRenderer mr = renderer.transform.GetComponent<MeshRenderer>();
            MeshFilter mf = renderer.transform.GetComponent<MeshFilter>();

            mf.mesh = mesh;

            mr.materials[0].color = data.Colour;

            return renderer;
        }

        private static Mesh RenderSolid(ILoopSet loopset, float max_length, bool separate_side_normals, float bottom, float top)
        {
            Tess tess = new Tess();

            int total_verts = 0;

            List<ContourVertex[]> contours = new List<ContourVertex[]>();

            foreach (var loop in loopset)
            {
                float len = loop.ParamRange;

                var points = loop.SmartFacet(max_length);

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

            Vector3[] top_verts = tess.Vertices.Select(x => new Vector3(x.Position.X, top, x.Position.Y)).ToArray();
            Vector3[] bot_verts = tess.Vertices.Select(x => new Vector3(x.Position.X, bottom, x.Position.Y)).ToArray();

            int[] top_tris = tess.Elements;
            int[] bot_tris = new int[top_tris.Length];

            for (int i = 0; i < top_tris.Length; i += 3)
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

            for (int i = 0; i < top_verts.Length; i++)
            {
                vert_map[top_verts[i]] = i;
            }

            List<int[]> side_tris = new List<int[]>();

            Vector3[] temp_verts = tess.Vertices.Select(x => new Vector3(x.Position.X, top, x.Position.Y)).ToArray();

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

                    if (separate_side_normals)
                    {
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
                    }
                    else
                    {
                        // otherwise there's just one copy of the top and bottom verts:
                        //               0  <=  idx  <  total_verts       -->   top face
                        //     total_verts  <=  idx  <  total_verts * 2   -->   bottom face

                        tris[j * 6 + 0] = old_idx1;
                        tris[j * 6 + 2] = old_idx2 + total_verts;
                        tris[j * 6 + 1] = old_idx1 + total_verts;

                        tris[j * 6 + 3] = old_idx2;
                        tris[j * 6 + 4] = old_idx1;
                        tris[j * 6 + 5] = old_idx2 + total_verts;
                    }
                }

                side_tris.Add(tris);
            }

            IEnumerable<Vector3> all_verts = top_verts.Concat(bot_verts);

            if (separate_side_normals)
            {
                all_verts = all_verts.Concat(top_verts).Concat(bot_verts);
            }

            IEnumerable<int> all_tris = top_tris
                    .Concat(bot_tris)
                    .Concat(side_tris.SelectMany(x => x));

            var mesh = new Mesh
            {
                vertices = all_verts.ToArray(),
                triangles = all_tris.ToArray()
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
