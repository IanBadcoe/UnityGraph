using Assets.Generation.U;


namespace Assets.Generation.Gen
{
    [System.Serializable]
    public class GeneratorConfig
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

        private ClRand RandObj;
        public int RandomSeed = 38;

        // ---------------------------------------------------------
        // steps to run at once during expansion or final relaxation
        // (just makes fewer calls to engine.LevelGenerator.step as relaxation takes thousands of steps to complete)
        // CG relaxation uses far fewer steps
        //public int ExpandStepsToRun = 1;

        // ------------------------------
        // settings for the created level

        public float CellSize = 20;
        public float WallFacetLength = 10;

        public ClRand Rand()
        {
            if (RandObj == null)
            {
                RandObj = new ClRand(RandomSeed);
            }

            return RandObj;
        }

        public static GeneratorConfig ShallowCopy(GeneratorConfig old)
        {
            // does Unity even allow this?
            GeneratorConfig lcg = new GeneratorConfig();

            lcg.RandomSeed = old.RandomSeed;
            lcg.RandObj = old.RandObj != null ? new ClRand(old.RandObj) : null;

            lcg.RelaxationMinimumSeparation = old.RelaxationMinimumSeparation;

            lcg.EdgeToNodeForceScale = old.EdgeToNodeForceScale;
            lcg.EdgeLengthForceScale = old.EdgeLengthForceScale;
            lcg.NodeToNodeForceScale = old.NodeToNodeForceScale;

            lcg.RelaxationMaxMove = old.RelaxationMaxMove;

            lcg.RelaxationForceTarget = old.RelaxationForceTarget;
            lcg.RelaxationMoveTarget = old.RelaxationMoveTarget;

            lcg.CellSize = old.CellSize;
            lcg.WallFacetLength = old.WallFacetLength;

            return lcg;
        }

    }
}