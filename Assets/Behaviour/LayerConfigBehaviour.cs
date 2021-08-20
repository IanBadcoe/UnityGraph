using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Behaviour
{
    public class LayerConfigBehaviour : MonoBehaviour
    {
        [Serializable]
        public struct LayerMaterial
        {
            public string Name;
            public Color Colour;
            public int DrawPriority;
        }

        public Dictionary<string, Color> ColourDict;
        public Dictionary<string, int> PriorityDict;

        public LayerMaterial[] Colours;

        private void Start()
        {
            ColourDict = new Dictionary<string, Color>();
            PriorityDict = new Dictionary<string, int>();

            foreach (var ent in Colours)
            {
                ColourDict[ent.Name] = ent.Colour;
                PriorityDict[ent.Name] = ent.DrawPriority;
            }
        }
    }
}