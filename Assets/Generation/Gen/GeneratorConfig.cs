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
        public float RelaxationMinimumSeparation = 5;                   // 5cm

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
        public float IntermediateRelaxationMoveTarget = 0.5f;           // 50cm
        public float FinalRelaxationMoveTarget = 0.01f;                 // 1cm

        // --------------------
        // random number source

        private ClRand RandObj;
        public int RandomSeed = 38;

        public ClRand Rand()
        {
            if (RandObj == null)
            {
                RandObj = new ClRand(RandomSeed);
            }

            return RandObj;
        }
    }
}