using Assets.Generation.G;
using Assets.Generation.GeomRep;
using System.Collections.Generic;
using UnityEngine;

// if the interface for generation can be factored out of the current implementation
// then this might be drived wholly off the interface and not need to depend on the implementation

namespace Assets.Behaviour.Drawing
{
    public class InProgressDrawerUpdaterBehaviour : MonoBehaviour
    {
        public GameObject CircleDrawerTemplate;
        public GameObject RectangleDrawerTemplate;
        public bool ControlCamera = true;

        Dictionary<object, GameObject> AllDrawers = new Dictionary<object, GameObject>();

        Camera Camera;
        // ideally we would abstract drawing away into some base class that Generator would implement
        // but all the nodes and edges would need to be appropriately based-classed as well (nodes have Node but that
        // is for the reverse purpose, of abstracting the non-positional properties, and probably has drifted away from that
        // by now, anyway...)
        public DataProvider DP;

        private void Awake()
        {
            Camera = Transform.FindObjectOfType<Camera>();
        }

        private void Update()
        {
            Graph graph = DP != null ? DP.GetGraph() : null;

            if (graph != null)
            {
                UpdateGeometry(graph);
            }
        }

        public void UpdateGeometry(Graph graph)
        {
            if (graph == null)
            {
                return;
            }

            Dictionary<object, GameObject> n_dict = new Dictionary<object, GameObject>();

            foreach (Node node in graph.GetAllNodes())
            {

                if (AllDrawers.TryGetValue(node, out GameObject drawer))
                {
                    n_dict[node] = drawer;
                }
                else
                {
                    CircleDrawerBehaviour cd = GameObject.Instantiate(CircleDrawerTemplate, transform).GetComponent<CircleDrawerBehaviour>();
                    cd.Node = node;
                    cd.gameObject.tag = "WIP";
                    n_dict[node] = cd.gameObject;
                }
            }

            foreach (DirectedEdge de in graph.GetAllEdges())
            {

                if (AllDrawers.TryGetValue(de, out GameObject drawer))
                {
                    n_dict[de] = drawer;
                }
                else
                {
                    RectangleDrawerBehaviour rd = GameObject.Instantiate(RectangleDrawerTemplate, transform).GetComponent<RectangleDrawerBehaviour>();
                    rd.Edge = de;
                    rd.gameObject.tag = "WIP";
                    n_dict[de] = rd.gameObject;
                }
            }

            foreach (var key in AllDrawers.Keys)
            {
                if (!n_dict.ContainsKey(key))
                {
                    GameObject.Destroy(AllDrawers[key]);
                }
            }

            AllDrawers = n_dict;

            if (ControlCamera)
            {
                Box2 bounds = graph.Bounds();
                Camera.transform.position = bounds.Centre() + new Vector3(0, 0, -300);

                float aspect_ratio = Screen.width / (float)Screen.height;
                float req_size = Mathf.Max(bounds.Diagonal.y, bounds.Diagonal.x / aspect_ratio);

                Camera.orthographicSize = req_size / 2;
            }
        }
    }
}