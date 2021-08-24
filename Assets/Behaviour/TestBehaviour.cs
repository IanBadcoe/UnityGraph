using Assets.Generation.GeomRep;
using Assets.Generation.U;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Behaviour
{
    class TestBehaviour : DataProvider
    {
        Dictionary<string, LoopSet> m_loops = new Dictionary<string, LoopSet>();
        public int TestNum = 62;
        public int ShapeNum = 0;
        public int StepControl = -1;
        public List<int> SkipShapes = new List<int>();
        public bool NeedSetup = true;

        ClRand test_rand;
        Intersector intersector = new Intersector();
        LoopSet merged = new LoopSet();
        LoopSet[] lss = new LoopSet[5];

        public override IReadOnlyDictionary<string, LoopSet> GetLoops()
        {
            return m_loops;
        }

        private void Update()
        {
            switch (StepControl) {
                case -1:
                    test_rand = new ClRand(TestNum);

                    int j = 0;

                    for(int i = 0; i < 5; i++) {
                        var ls = RandShapeLoop(test_rand);

                        if (SkipShapes == null || !SkipShapes.Contains(i))
                        {
                            lss[j] = ls;
                            m_loops[$"T{j}"] = ls;
                            j++;
                        }
                    }

                    StepControl++;

                    break;

                case 0:
                    break;

                case 1:
                    StepControl++;

                    try
                    {
                        // point here is to run all the Unions internal logic/asserts
                        intersector.Union(lss[ShapeNum], 1e-5f, new ClRand(1));
                        merged = intersector.GetMerged();
                        ShapeNum++;

                        m_loops["Merged"] = merged;
                    }
                    catch (LoopDisplayException lde)
                    {
                        m_loops.Clear();
                        m_loops["T0"] = new LoopSet { lde.Loop1 };
                        m_loops["T1"] = new LoopSet { lde.Loop2 };
                    }

                    break;

                default:
                    StepControl++;

                    if (StepControl == 30)
                    {
                        StepControl = 0;
                    }

                    break;
            }
        }

        private LoopSet RandShapeLoop(ClRand test_rand)
        {
            LoopSet ret = new LoopSet();

            if (test_rand.Nextfloat() > 0.5f)
            {
                ret.Add(new Loop("", new CircleCurve(
                    test_rand.Nextpos(0, 10),
                    test_rand.Nextfloat() * 2 + 0.1f,
                    test_rand.Nextfloat() > 0.5f ? RotationDirection.Forwards : RotationDirection.Reverse)));
            }
            else
            {
                Vector2 p1 = test_rand.Nextpos(0, 10);
                Vector2 p2 = test_rand.Nextpos(0, 10);
                Vector2 p3 = test_rand.Nextpos(0, 10);

                // triangles cannot be self-intersecting
                Loop loop = new Loop("", new List<Curve>{
                        LineCurve.MakeFromPoints(p1, p2),
                        LineCurve.MakeFromPoints(p2, p3),
                        LineCurve.MakeFromPoints(p3, p1),
                    });

                float dist = GeomRepUtil.DistFromLine(p1, p2, p3);

                ret.Add(loop);
            }

            return ret;
        }
    }
}
