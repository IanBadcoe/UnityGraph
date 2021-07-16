using Assets.Generation.G;
using Assets.Generation.GeomRep;
using System;
using System.Collections.Generic;
using UnityEngine;

// if the interface for generation can be factored out of the current implementation
// then this might be drived wholly off the interface and not need to depend on the implementation

namespace Assets.Generation.Gen.Drawing
{
    public class InProgressDrawerUpdater : MonoBehaviour
    {
        public GameObject CircleDrawerTemplate;
        public GameObject RectangleDrawerTemplate;

        Dictionary<object, GameObject> AllDrawers = new Dictionary<object, GameObject>();

        Camera Camera;

        private void Awake()
        {
            Camera = Transform.FindObjectOfType<Camera>();
        }

        internal void UpdateGeometry(Generator generator)
        {
            Dictionary<object, GameObject> n_dict = new Dictionary<object, GameObject>();

            foreach(INode node in generator.Graph.GetAllNodes())
            {
                GameObject drawer;

                if (AllDrawers.TryGetValue(node, out drawer))
                {
                    n_dict[node] = drawer;
                }
                else
                {
                    CircleDrawer cd = GameObject.Instantiate(CircleDrawerTemplate).GetComponent<CircleDrawer>();
                    cd.Node = node;
                    n_dict[node] = cd.gameObject;
                }
            }

            foreach (DirectedEdge de in generator.Graph.GetAllEdges())
            {
                GameObject drawer;

                if (AllDrawers.TryGetValue(de, out drawer))
                {
                    n_dict[de] = drawer;
                }
                else
                {
                    RectangleDrawer rd = GameObject.Instantiate(RectangleDrawerTemplate).GetComponent<RectangleDrawer>();
                    rd.Edge = de;
                    n_dict[de] = rd.gameObject;
                }
            }

            foreach(var key in AllDrawers.Keys)
            {
                if (!n_dict.ContainsKey(key))
                {
                    GameObject.Destroy(AllDrawers[key]);
                }
            }

            Area bounds = generator.Graph.Bounds();
            Camera.transform.position = bounds.Centre() + new Vector3(0, 0, -200);

            AllDrawers = n_dict;
        }
    }
}