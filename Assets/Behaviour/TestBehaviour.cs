namespace Assets.Behaviour
{
    class TestBehaviour : DataProvider
    {
        //    List<Loop> m_loops;

        //    public override IReadOnlyDictionary<string, LoopSet> GetLoops()
        //    {
        //        return m_loops;
        //    }

        //    private void Start()
        //    {
        //        try
        //        {
        //            const int NumShapes = 5;

        //            int i = 996;

        //            Intersector intersector = new Intersector();

        //            // let us jump straight to a given test
        //            ClRand test_rand = new ClRand(i);

        //            LoopSet merged = new LoopSet();

        //            for (int j = 0; j < NumShapes; j++)
        //            {
        //                LoopSet ls2 = RandShapeLoop(test_rand);

        //                if (j == 0 || j == 4)
        //                {
        //                    // point here is to run all the Unions internal logic/asserts
        //                    merged = intersector.Union(merged, ls2, 1e-5f, new ClRand(1));
        //                }
        //            }

        //            m_loops = merged;
        //        }
        //        catch (LoopDisplayException lde)
        //        {
        //            m_loops = new List<Loop>()
        //            {
        //                lde.Loop1, lde.Loop2
        //            };
        //        }
        //    }

        //    private LoopSet RandShapeLoop(ClRand test_rand)
        //    {
        //        LoopSet ret = new LoopSet();

        //        if (test_rand.Nextfloat() > 0.5f)
        //        {
        //            ret.Add(new Loop("", new CircleCurve(
        //                test_rand.Nextpos(0, 10),
        //                test_rand.Nextfloat() * 2 + 0.1f,
        //                test_rand.Nextfloat() > 0.5f ? RotationDirection.Forwards : RotationDirection.Reverse)));
        //        }
        //        else
        //        {
        //            Vector2 p1 = test_rand.Nextpos(0, 10);
        //            Vector2 p2 = test_rand.Nextpos(0, 10);
        //            Vector2 p3 = test_rand.Nextpos(0, 10);

        //            // triangles cannot be self-intersecting
        //            Loop loop = new Loop("", new List<Curve>{
        //                LineCurve.MakeFromPoints(p1, p2),
        //                LineCurve.MakeFromPoints(p2, p3),
        //                LineCurve.MakeFromPoints(p3, p1),
        //            });

        //            float dist = GeomRepUtil.DistFromLine(p1, p2, p3);

        //            ret.Add(loop);
        //        }

        //        return ret;
        //    }
    }
}
