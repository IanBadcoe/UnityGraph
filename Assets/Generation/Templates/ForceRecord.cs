namespace Assets.Generation.Templates
{
    public class ForceRecord
    {
        // gets the two node radii added to it, so acts as a separation
        public readonly float TargetDist;
        public readonly NodeRecord Node1;
        public readonly NodeRecord Node2;
        // if we need to add some extras that are stronger than others
        public readonly float ForceMultiplier;

        public ForceRecord(float targetDist, NodeRecord node1, NodeRecord node2, float forceMultiplier)
        {
            TargetDist = targetDist;
            Node1 = node1;
            Node2 = node2;
            ForceMultiplier = forceMultiplier;
        }
    }
}