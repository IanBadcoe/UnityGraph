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
        }

        public Dictionary<string, Color> ColourDict;

        public LayerMaterial[] Colours;

        private void Start()
        {
            ColourDict = new Dictionary<string, Color>();

            foreach (var ent in Colours)
            {
                ColourDict[ent.Name] = ent.Colour;
            }
        }
    }
}