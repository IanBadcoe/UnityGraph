using Assets.Generation.G;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Generation.Gen.Drawing
{
    public class RectangleDrawer : MonoBehaviour, IDrawer
    {
        public DirectedEdge Edge { get; set; }
    }
}
