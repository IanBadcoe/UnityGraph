using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Behaviour
{
    public class LayerConfigBehaviour : MonoBehaviour
    {
        [Serializable]
        public struct LayerData
        {
            // just for reasonable defaults
            public readonly static LayerData Default = new LayerData
            {
                Name = "default",
                Colour = new Color(1.0f, 0.5f, 1.0f),
                DrawPriority = 1,
                BaseHeight = -2,
                TopHeight = 0
            };

            public string Name;
            public Color Colour;
            public int DrawPriority;
            public float BaseHeight;
            public float TopHeight;
        }

        public Dictionary<string, LayerData> LayerDict;

        [FormerlySerializedAs("Colours")]
        public LayerData[] Layers;

        [Serializable]
        public struct LayerCut
        {
            public string Cut;
            public string CutBy;
        }

        // we just define a sequence of cuts
        // this allows some pragmatism, such as walls are cut by floors, but floors are cut by water
        // however, if we cut the floors with the water and then cut the walls with the floors,
        // then cut the walls with the water, we'd potentially be carefully removing some area from
        // the floor, hence not removing it from the wall, then removing it from the wall in a separate
        // operation..  which might have precision problems and leave little slivers of wrong stuff
        //
        // e.g. mathematically:
        // floor =cut=> water
        // walls =cut=> floor
        // walls =cut=> water
        //
        // is the same as:
        //
        // walls =cut=> floor
        // floor =cut=> water
        // walls =cut=> water
        // 
        // but the latter cuts some areas twice (not a problem) where the former relies on cutting
        // right up to the edge of the water, then separately cutting the water to precisely leave nothing

        public LayerCut[] CutSequence;

        private void Start()
        {
            LayerDict = new Dictionary<string, LayerData>();

            foreach (var ent in Layers)
            {
                LayerDict[ent.Name] = ent;
            }
        }
    }
}