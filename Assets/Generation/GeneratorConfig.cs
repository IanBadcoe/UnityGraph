using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Generation
{
    public class GeneratorConfig : MonoBehaviour
    {
        // ---------------------
        // relaxation parameters

        // The node sizes and edge widths define the space that rooms and corridors will occupy internally
        // this is added on to give an additional minimum wall thickness between them
        public float RelaxationMinimumSeparation = 5;

        // these scale the strength of the three fundamental forces
        // current theory is to set EdgeLength force significantly weaker so that edge stretch can be used to detect
        // when they need splitting
        public float EdgeToNodeForceScale = 1.0f;
        public float EdgeLengthForceScale = 0.01f;
        public float NodeToNodeForceScale = 1.0f;

        // time steps are scaled down if they would lead to any node moving further than this
        public float RelaxationMaxMove = 1.0f;

        // relaxation is considered complete when the max force or max move seen on a node
        // drops below both of these
        public float RelaxationForceTarget = 0.001f;
        public float RelaxationMoveTarget = 0.01f;

        // --------------------
        // random number source

        public System.Random Rand { get; private set; }
        public int RandomSeed = 38;

        // ---------------------------------------------------------
        // steps to run at once during expansion or final relaxation
        // (just makes fewer calls to engine.LevelGenerator.step as relaxation takes thousands of steps to complete)

        public int ExpandStepsToRun = 1000;

        // ------------------------------
        // settings for the created level

        public float CellSize = 20;
        public float WallFacetLength = 10;

        void Start()
        {
            Rand = new System.Random(RandomSeed);
        }
    }
}