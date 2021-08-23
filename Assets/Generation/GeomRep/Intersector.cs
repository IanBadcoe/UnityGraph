using Assets.Extensions;
using Assets.Generation.U;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public class Intersector
    {

        [System.Diagnostics.DebuggerDisplay("Forward({ForwardLinks.Count}) Backward({BackwardLinks.Count})")]
        public class Splice
        {
            public List<Curve> ForwardLinks = new List<Curve>();
            public List<Curve> BackwardLinks = new List<Curve>();
        }

        // only non-private for unit-testing
        [System.Diagnostics.DebuggerDisplay("Curve = {Curve}, LoopNumber = {LoopNumber}")]
        public class AnnotatedCurve : EqualityBase
        {
            public Curve Curve;
            public readonly int LoopNumber;
            public Splice ForwardSplice = new Splice();
            public Splice BackwardSplice = new Splice();

            public AnnotatedCurve(Curve curve, int loop_number)
            {
                Curve = curve;
                LoopNumber = loop_number;
            }

            public override bool Equals(object o)
            {
                if (!(o is AnnotatedCurve))
                {
                    return false;
                }

                AnnotatedCurve aco = (AnnotatedCurve)o;

                return Curve == aco.Curve;
            }

            public override int GetHashCode()
            {
                return Curve.GetHashCode();
            }
        }

        public LoopSet Cut(LoopSet to_cut, LoopSet cut_by, float tol, ClRand random, string layer)
        {
            // assuming cut_by has no outer -ve curves (which it shouldn't have if it is the output of a previous union)
            // removing cut_by from cut is the same as unioning with the inverse

            return Union(to_cut, cut_by.Reversed(), tol, random, layer);
        }

        // union operation cannot return a mix of positive and negative top-level curves
        // e.g. if you think of a negative curve as something subtracted from a positive curve
        // if we have inputs like this:
        //
        //    +--+ <- -ve curve (in one loopset)
        //    |  |
        // +--+--+--+ <- +ve curve (in the other)
        // |  |  |  |
        // +--+--+--+
        //    |  |
        //    +--+
        //
        // then the output must be either:
        //
        // +--+  +--+
        // |  |  |  |
        // +--+  +--+
        //
        // or
        //    +--+
        //    |  |
        //    +--+
        //
        //    +--+
        //    |  |
        //    +--+
        //
        // according to whether we selected WantPositive or WantNegative
        //
        // the output can obviously contain nested curves of the opposite polarity:
        //
        // +--------+
        // |        | <- +ve curve with a negative curved cutting a hole in it
        // |  +--+  |
        // |  |  |  |
        // |  +--+  |
        // |        |
        // +--------+
        //
        // But we cannot represent a mix of opposite polarity curves at the top level
        // (because that is essentially not completely unioned)

        public enum UnionType
        {
            WantPositive,
            WantNegative
        }

        public LoopSet Union(LoopSet previously_merged, LoopSet to_merge, float tol,
                             ClRand random, string layer)
        {
            return Union(previously_merged, to_merge, tol,
                random, UnionType.WantPositive, layer);
        }

        public LoopSet Union(LoopSet previously_merged, LoopSet to_merge, float tol,
                             ClRand random,
                             UnionType type = UnionType.WantPositive, string layer = "")
        {
            ValidatePreviouslyMerged(previously_merged, "previously_merged");
            //            ValidateInputs(to_merge, "ls2");

            // any loops in to_merge can be struck off as they will have no effect
            // (and following code cannot handle them anyway, as they both overlap but don't intersect)
            RemoveIdenticalLoops(previously_merged, to_merge);

            // if there is no input left, we're done
            if (to_merge.Count == 0)
            {
                return previously_merged;
            }

            // needing to check +ve/-ve curve type shoots any simple early-outs in the foot
            // ...

            // used later as an id for which loop an AnnotationCurve comes from
            int loop_count = 0;

            Dictionary<int, IList<Curve>> working_loops1 = new Dictionary<int, IList<Curve>>();

            foreach (Loop l in previously_merged)
            {
                working_loops1.Add(loop_count, new List<Curve>(l.Curves));
                loop_count++;
            }

            Dictionary<int, IList<Curve>> working_loops2 = new Dictionary<int, IList<Curve>>();

            foreach (Loop l in to_merge)
            {
                working_loops2.Add(loop_count, new List<Curve>(l.Curves));
                loop_count++;
            }

            // build forward and reverse chains of annotation-curves around both loops
            Dictionary<Curve, AnnotatedCurve> ann_map
                = MakeAnnotationsMap();

            foreach (int i in working_loops1.Keys)
            {
                IList<Curve> alc1 = working_loops1[i];

                BuildAnnotationChains(alc1, i, ann_map);
            }

            foreach (int i in working_loops2.Keys)
            {
                IList<Curve> alc1 = working_loops2[i];

                BuildAnnotationChains(alc1, i, ann_map);
            }


            LoopSet ret = new LoopSet();

            // first, an easy bit, any loops from either set whose bounding boxes are disjunct from all loops in the
            // other set, they have no influence on any other loops and can be simply copied unchanged into
            // the output
            //
            // could still do this, but would have to select only the +ve or -ve loops according to UnionType

            // now find all the splices
            // (this is _all_ the places where movement around the loop passes the end of a curve
            //  _including_ 12 o'clock on a closed circle)
            // 
            // we do this using the clustered joints, because otherwise very small lines can
            // cause problems where the line is too small to be considered, and that leaves
            // a gap that we would somehow have to detect and compensate for...

            // split all curves that intersect
            foreach (int i in working_loops1.Keys)
            {
                var alc1 = working_loops1[i];
                foreach (int j in working_loops2.Keys)
                {
                    var alc2 = working_loops2[j];

                    SplitCurvesAtIntersections(alc1, alc2, 1e-4f, ann_map);
#if DEBUG
                    // has a side effect of checking that the loops are still loops
                    new Loop("", alc1);
                    new Loop("", alc2);
#endif

                    // curves that wholly or partly overlay each other do not intersect
                    // but we still need to split them because we need to discard some overlapping parts
                    // (but not necessarily the whole curve)
                    //
                    // mostly SplitCurvesAtIntersections catches these but if we had something like
                    //
                    //        |                 |
                    //        +-----+-----+-----+
                    // +-------------------------------+
                    // |                               |
                    //
                    // (where there is no actual gap between the horizontal lines)
                    // then this catches the need to split the long line at the central section
                    SplitCurvesAtCoincidences(alc1, alc2, 1e-4f, ann_map);
#if DEBUG
                    // has a side effect of checking that the loops are still loops
                    new Loop("", alc1);
                    new Loop("", alc2);
#endif
                }
            }

            // build a set of all curves and another of all AnnotatedCurves (Open)
            // 1) pick a line that crosses at least one open AnnotationCurve
            // 2) find all curve intersections along the line (taking care to ignore tangent touchings and not duplicate
            //    an intersection if it occurs at a curve-curve joint)
            // 3) calculate a count of crossings so that when we cross a curve inwards (crosses us from the right
            //    as we look down our line) we increase the count and when we cross a curve outwards we decrease it
            // 4) label the intervals on the line between the intersections with the count in that interval
            // 5) only counts from zero -> 1 or from 1 -> zero are interesting
            // 6) for 0 -> 1 or 1 -> 0 crossings we are entering the output shape
            // 6a) if the forward AnnoationCurve is open
            // 6b) follow the curve forwards, removing AnnotationCurves from open
            // 6c) when we get to a splice, find the sharpest left turn (another forwards AnnotationCurve)
            // 6d) until we reach our start curve
            // 6e) add all these curves as a loop in the output
            // 6f (this will be a
            // 7) for +ve -> +ve or -ve -> -ve crossings we walk backwards around the loop just removing
            //    the annotation edges from open
            // 8) until there are no open AnnotationEdges

            HashSet<Curve> all_curves = MakeAllCurvesSet(
                working_loops1.Values
                    .Concat(working_loops2.Values).SelectMany(x => x));

            var open = MakeOpenSet(ann_map);

            // this is used only for finding if stabbing lines are sufficiently clear of curve end-points
            // when removing unwanted curves
            HashSet<Vector2> clustered_joints = ClusterJoints(new HashSet<Vector2>(all_curves.SelectMany(c => new List<Vector2> { c.StartPos, c.EndPos })), 1e-4f);

            // bounding box allows us to create cutting lines that definitely exceed all loop boundaries
            Box2 bounds = all_curves.Select(c => c.BoundingArea).Aggregate(new Box2(), (a, b) => a.Union(b));

            // but all we need from that is the max length in the box
            float diameter = bounds.Diagonal.magnitude;

            // "wanted" means +ve or -ve according to "type"
            //
            // if an outermost curve has the wrong polarity then it is directly not "wanted" in the output
            //
            // if a nested curve has the same polarity as the curve it is nested in, then it is more subtly "not wanted"
            // as it surrounds a "wanted" volume but is not an overall border:
            //
            // e.g two +ve curves:
            //
            // +------+     +------+
            // | +--+ |     |      |
            // | |  | | ==> |      |
            // | +--+ |     |      |
            // +------+     +------+
            //
            // so the inner curve is not in the output, similarly but a little more complex:
            //
            // +------+       +------+
            // |    +-|-+     |      +-+
            // |    | | | ==> |        |
            // |    +-|-+     |      +-+
            // +------+       +------+
            //
            // so the inner parts of the overlapped shape are not in the output
            //
            // there are some further complexities with concident lines
            //
            //     +------+     +------+    +------+
            //     +--+   |     +      |    +--+   |
            // x-> |  |   | ==> |      | or    |   | (depending on direction of smaller polygon)
            //     +--+   |     +      |    +--+   |
            //     +------+     +------+    +------+
            // 
            // if the two lines at x are in the same direction, then we only want one of them...
            // if they are in opposite directions, then they both disappear, stacking >2 lines makes this more complex,
            // but follows the same rule (this only works if we follow the intended usage, 

            // try moving this before annotation chains and splices after we have it 100% working
            if (!RemoveUnwantedCurves(tol,
                random,
                ann_map,
                all_curves, open, clustered_joints,
                diameter,
                type))
            {
                return null;
            }

            foreach(var c in ann_map.Keys.ToList())
            {
                if (!open.Contains(c))
                {
                    ann_map.Remove(c);
                }
                else
                {
                    var spl = ann_map[c].ForwardSplice;

                    spl.ForwardLinks = spl.ForwardLinks.Where(x => open.Contains(x)).ToList();
                    spl.BackwardLinks = spl.BackwardLinks.Where(x => open.Contains(x)).ToList();
                }
            }

#if DEBUG
            // double check we didn't remove anything we needed :-)
            ValidateAnnotations(ann_map, open.ToList());
#endif

            while (open.Count > 0)
            {
                Curve ac_current = open.First();

                // take a loop that is part of the perimeter
                Loop loop = ExtractLoop(
                                      open,
                                      ac_current,
                                      ann_map,
                                      layer);

                if (loop != null)
                {
                    ret.Add(loop);
                }
            }

#if DEBUG
            ValidatePreviouslyMerged(ret, "ret");
#endif

            return ret;
        }

        public static Dictionary<Curve, AnnotatedCurve> MakeAnnotationsMap()
        {
            return new Dictionary<Curve, AnnotatedCurve>(
                                new ReferenceComparer<Curve>());
        }

        public static HashSet<Curve> MakeAllCurvesSet(IEnumerable<Curve> curves)
        {
            return new HashSet<Curve>(
                            curves,
                            new ReferenceComparer<Curve>());
        }

        public static HashSet<Curve> MakeAllCurvesSet(Curve curve)
        {
            return new HashSet<Curve>(
                            new List<Curve> { curve },
                            new ReferenceComparer<Curve>());
        }

        public static HashSet<Curve> MakeOpenSet(Dictionary<Curve, AnnotatedCurve> ann_map)
        {
            return new HashSet<Curve>(
                            ann_map.Values.Select(x => x.Curve),
                            new ReferenceComparer<Curve>());
        }

        // public for unit-testing
        public HashSet<Vector2> ClusterJoints(HashSet<Vector2> curve_joints, float cluster_limit)
        {
            List<Vector2> centroids = curve_joints.ToList();
            List<List<Vector2>> groups = curve_joints.Select(x => new List<Vector2> { x }).ToList();

            float last_dist_2 = 0;

            float limit_2 = cluster_limit * cluster_limit;

            while (last_dist_2 < limit_2)
            {
                last_dist_2 = float.MaxValue;
                int found_i = -1;
                int found_j = -1;

                for (int i = 0; i < centroids.Count - 1; i++)
                {
                    for (int j = i + 1; j < centroids.Count; j++)
                    {
                        float d2 = (centroids[i] - centroids[j]).sqrMagnitude;

                        if (d2 < limit_2 && d2 < last_dist_2)
                        {
                            last_dist_2 = d2;
                            found_i = i;
                            found_j = j;
                        }
                    }
                }

                if (found_i != -1)
                {
                    groups[found_i].AddRange(groups[found_j]);
                    centroids[found_i] = groups[found_i].Aggregate(new Vector2(), (x, y) => x + y) / groups[found_i].Count;
                    groups.RemoveAt(found_j);
                    centroids.RemoveAt(found_j);
                }
            }

            return new HashSet<Vector2>(centroids);
        }

        private static void RemoveIdenticalLoops(LoopSet previously_merged, LoopSet ls2)
        {
            List<Loop> to_remove = new List<Loop>();

            foreach (var loop1 in previously_merged)
            {
                foreach (var loop2 in ls2)
                {
                    if (loop1 == loop2)
                    {
                        to_remove.Add(loop2);
                    }
                }
            }

            foreach (var loop in to_remove)
            {
                ls2.Remove(loop);
            }
        }

        private void ValidatePreviouslyMerged(LoopSet ls, string name)
        {
            for (int i = 0; i < ls.Count; i++)
            {
                var loop = ls[i];
                CheckSelfIntersection(loop, name);

                for (int j = i + 1; j < ls.Count; j++)
                {
                    var loop2 = ls[j];

                    CheckLoopIntersection(loop, loop2, name);
                }
            }
        }

        private static void CheckLoopIntersection(Loop loop, Loop loop2, string name)
        {
            for (int i = 0; i < loop.NumCurves - 1; i++)
            {
                var c1 = loop.Curves[i];

                for (int j = i + 1; j < loop2.NumCurves; j++)
                {
                    var c2 = loop2.Curves[j];
                    if (ReferenceEquals(loop, loop2))
                    {
                        throw new ArgumentException("Same curve object present twice in loop", name);
                    }
                    else if (loop == loop2)
                    {
                        throw new ArgumentException("Two copies of the same loop", name);
                    }
                    else
                    {
                        var intrs = GeomRepUtil.CurveCurveIntersect(c1, c2);

                        if (intrs != null)
                        {
                            foreach (var intr in intrs)
                            {
                                // "false" as we expect these already in range
                                float pd1 = Mathf.Min((c1.Pos(intr.Item1, false) - c1.StartPos).magnitude,
                                                      (c1.Pos(intr.Item1, false) - c1.EndPos).magnitude);
                                float pd2 = Mathf.Min((c2.Pos(intr.Item2, false) - c2.StartPos).magnitude,
                                                      (c2.Pos(intr.Item2, false) - c2.EndPos).magnitude);

                                // intersections at the ends of curves are permitted
                                if (pd1 > 1e-4f || pd2 > 1e-4f)
                                {
                                    throw new ArgumentException("Curves in loop intersect", name);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void CheckSelfIntersection(Loop loop, string name)
        {
            for (int i = 0; i < loop.NumCurves - 1; i++)
            {
                var c1 = loop.Curves[i];

                for (int j = i + 1; j < loop.NumCurves; j++)
                {
                    var c2 = loop.Curves[j];

                    if (ReferenceEquals(c1, c2))
                    {
                        throw new ArgumentException("Same curve object present twice in loop", name);
                    }
                    else if (c1 == c2)
                    {
                        throw new ArgumentException("Same curve present twice in loop", name);
                    }
                    else
                    {
                        var intrs = GeomRepUtil.CurveCurveIntersect(c1, c2);

                        if (intrs != null)
                        {
                            foreach (var intr in intrs)
                            {
                                // "false" as we expect these already in range
                                float pd1 = Mathf.Min((c1.Pos(intr.Item1, false) - c1.StartPos).magnitude,
                                                      (c1.Pos(intr.Item1, false) - c1.EndPos).magnitude);
                                float pd2 = Mathf.Min((c2.Pos(intr.Item2, false) - c2.StartPos).magnitude,
                                                      (c2.Pos(intr.Item2, false) - c2.EndPos).magnitude);

                                // intersections at the ends of curves are permitted
                                if (pd1 > 1e-4f || pd2 > 1e-4f)
                                {
                                    throw new ArgumentException("Curves in loop intersect", name);
                                }
                            }
                        }
                    }
                }
            }
        }

        // public and virtual only for unit-tests
        virtual public bool RemoveUnwantedCurves(
            float tol,
            ClRand random,
            Dictionary<Curve, AnnotatedCurve> ann_map,
            HashSet<Curve> all_curves,
            HashSet<Curve> open,
            HashSet<Vector2> curve_joints,
            float diameter,
            UnionType type)
        {
            // we analyze via "chains" a chain is a set of connected curves with no internal branches, e.g:
            //        [ ====== this is a chain ============ ]
            // ---> + ---> + ---> + ---> + ---> + ---> + ---> + --->
            //      ^                                         |
            //      |                                         |
            //      |                                         v
            //
            // each chain is either wholly wanted or wholly unwanted

            IList<HashSet<Curve>> chains = FindChains(ann_map, open);

            // if we find a curve is non-internal while inspecting another, need not look at it again
            HashSet<AnnotatedCurve> seen = new HashSet<AnnotatedCurve>(
                new ReferenceComparer<AnnotatedCurve>());

            int non_zero_value = type == UnionType.WantPositive ? 1 : -1;

            foreach (Curve c in all_curves)
            {
                AnnotatedCurve ac_c = ann_map[c];

                if (!open.Contains(c) || seen.Contains(ac_c))
                {
                    continue;
                }

                var intervals = TryFindIntersections(c, all_curves, curve_joints,
                                                     diameter, tol, random);

                // failure, don't really expect this as have had multiple tries and it
                // shouldn't be so hard to find a good cutting line
                if (intervals == null)
                {
                    throw new AnalysisFailedException("Could not find suitable intersection line for a curve.");
                }

                EliminateCancellingLines(intervals, open, tol, ann_map);

                // now use the intervals to decide what to do with the AnnotationEdges
                int prev_crossings = 0;

                foreach (var intersection in intervals)
                {
                    int crossings = intersection.CrossingNumber;

                    AnnotatedCurve ac_intersecting = ann_map[intersection.Curve];

                    if (open.Contains(intersection.Curve) && !seen.Contains(ac_intersecting))
                    {
                        // three cases, 0 -> 1, 1 -> 0 and anything else
                        if ((prev_crossings != 0 || crossings != non_zero_value)
                              && (prev_crossings != non_zero_value || crossings != 0))
                        {
                            open.Remove(intersection.Curve);
                        }
                        else
                        {
                            seen.Add(ac_intersecting);
                        }
                    }

                    prev_crossings = crossings;
                }
            }

            return true;
        }

        private IList<HashSet<Curve>> FindChains(Dictionary<Curve, AnnotatedCurve> ann_map, HashSet<Curve> open)
        {
            var temp_open = new HashSet<Curve>(open, new ReferenceComparer<Curve>());

            var ret = new List<HashSet<Curve>>();

            while (temp_open.Count > 0)
            {
                var chain = new HashSet<Curve>(new ReferenceComparer<Curve>());

                var seed = temp_open.First();
                chain.Add(seed);
                temp_open.Remove(seed);

                var curr = seed;
                bool full_circle = false;

                while (true)
                {
                    var spl = ann_map[curr].ForwardSplice;

                    if (spl.ForwardLinks.Count > 1)
                    {
                        break;
                    }

                    curr = spl.ForwardLinks[0];

                    if (!temp_open.Contains(curr))
                    {
                        // should only be possible if we came full circle
                        Assertion.Assert(ReferenceEquals(curr, seed));
                        full_circle = true;
                        break;
                    }

                    chain.Add(curr);
                    temp_open.Remove(curr);
                }

                if (!full_circle)
                {
                    curr = seed;

                    while (true)
                    {
                        var spl = ann_map[curr].BackwardSplice;

                        if (spl.BackwardLinks.Count > 1)
                        {
                            break;
                        }

                        curr = spl.BackwardLinks[0];

                        if (!temp_open.Contains(curr))
                        {
                            // cannot assert the same as in first loop as first loop may have taken
                            break;
                        }

                        chain.Add(curr);
                        temp_open.Remove(curr);
                    }
                }

                ret.Add(chain);
            }

            return ret;
        }

        // where N lines all appear with successive separations of < tol
        // (e.g. approximating zero separation)
        // we can cancel any pairs that lie in exactly opposite directions
        // (because we have already snipped curves to separate coincident subsections)
        public void EliminateCancellingLines(List<Interval> intervals,
            HashSet<Curve> open, float tol,
            Dictionary<Curve, AnnotatedCurve> ann_map)
        {
            int start = 0;

            while (start < intervals.Count)
            {
                int end = 0;
                for (int i = start + 1; i < intervals.Count; i++)
                {
                    Interval int1 = intervals[i - 1];
                    Interval int2 = intervals[i];

                    if (int2.Distance - int1.Distance >= tol)
                    {
                        break;
                    }

                    end = i + 1;
                }

                if (end != 0)
                {
                    bool any_removed;

                    do
                    {
                        any_removed = false;

                        for (int i = start; i < end - 1; i++)
                        {
                            Interval int1 = intervals[i];
                            Interval int2 = intervals[i + 1];

                            if (int1.Curve.SameSupercurve(int2.Curve, tol)
                                && int1.DotProduct * int2.DotProduct < 0)
                            {
                                // we skip these params for unit-tests
                                if (ann_map != null)
                                {
                                    open.Remove(int1.Curve);
                                    open.Remove(int2.Curve);
                                }

                                intervals.RemoveAt(i + 1);
                                intervals.RemoveAt(i);

                                end -= 2;

                                // now we removed something, must restart this loop
                                any_removed = true;
                                break;
                            }
                        }
                        // we run as long as we are removing things
                        // _and_ re've not removed so much that there's no comparison left to make
                    } while (any_removed && start < intervals.Count - 1);

                    // if anything is left, we step over it, otherwise start stays at the same number and we scan again
                    // forwards from the new stuff at that index
                    start = end;
                }
                else
                {
                    // no block of touching curves, move on to the next possible start
                    start++;
                }
            }
        }

        Loop ExtractLoop(HashSet<Curve> open,
                         Curve start_c,
                         Dictionary<Curve, AnnotatedCurve> ann_map,
                         string layer)
        {
            Curve c = start_c;

            List<Curve> found_curves = new List<Curve>();

            while (true)
            {
                Assertion.Assert(open.Contains(c));

                found_curves.Add(c);
                open.Remove(c);

                // look for a splice that ends this curve
                var curr_ac = ann_map[c];

                Assertion.Assert(curr_ac != null);

                var splice = curr_ac.ForwardSplice;

                // at every splice, at least one of the possible exits should be still open
                // or we should have just come full circle
                Assertion.Assert(splice.ForwardLinks.Contains(start_c)
                    || open.Where(x => splice.ForwardLinks.Contains(x)).Any());

                List<AnnotatedCurve> still_open =
                    open.Where(x => splice.ForwardLinks.Contains(x)).Select(x => ann_map[x]).ToList();
                List<AnnotatedCurve> different_loop =
                    still_open.Where(x => x.LoopNumber != curr_ac.LoopNumber).ToList();

                if (still_open.Count == 1)
                {
                    c = still_open[0].Curve;
                }
                else if (different_loop.Count == 1)
                {
                    c = different_loop[0].Curve;
                }
                else if (different_loop.Count != 0)
                {
                    // if there is more than one option on a different loop, take the sharpest clockwise corner
                    // first
                    float found_ang = float.MaxValue;
                    AnnotatedCurve found_ac = null;

                    foreach (var ac in different_loop)
                    {
                        var cur_normal = c.Normal(c.EndParam);
                        var try_normal = -ac.Curve.Normal(ac.Curve.EndParam);

                        float ang = AngleRange.FixupAngle(Util.SignedAngleDifference(cur_normal, try_normal));

                        if (ang < found_ang)
                        {
                            found_ang = ang;
                            found_ac = ac;
                        }
                    }

                    c = found_ac.Curve;
                }
                else
                {
                    // otherwise we expect to have arrived back at our starting curve
                    Assertion.Assert(splice.ForwardLinks.Contains(start_c));

                    c = start_c;
                }

                if (c == start_c)
                {
                    break;
                }
            }

            // because input cyclic curves have a joint at 12 o'clock
            // and nothing before here removes that, we can have splits we don't need
            // between otherwise identical curves
            //
            // this merges those back together
            TidyLoop(found_curves);

            // this cannot impact any previous algorithms with precision problems
            // so free to strip curves down to quite a broad limit, and given we're drawing stuff
            // on a scale metres (maybe down to 10cm) feel happy stripping 1cm and smaller features
            IList<Curve> reduced_curves = RemoveTinyCurves(found_curves, 1e-2f);

            if (reduced_curves == null)
            {
                return null;
            }

            return new Loop(layer, reduced_curves);
        }

        private void TidyLoop(List<Curve> curves)
        {
            int prev = curves.Count - 1;
            Curve c_prev = curves[prev];

            for (int i = 0; i < curves.Count;)
            {
                Curve c_here = curves[i];

                // if we get down to (or start with) only one curve, don't try to merge it with itself
                if (c_here == c_prev)
                {
                    break;
                }

                Curve merged = c_prev.Merge(c_here);

                if (merged != null)
                {
                    curves[prev] = merged;
                    curves.RemoveAt(i);
                    c_prev = merged;

                    // if we've taken one out and prev is still at the end
                    // then it must be decremented...
                    if (prev > i)
                    {
                        prev--;
                    }

                    // if we've removed curves[i] then we'll look at the new
                    // curves[i] next pass and prev remains the same
                }
                else
                {
                    // move everything on one
                    prev = i;
                    i++;
                    c_prev = c_here;
                }
            }
        }


        // strip-out adjoining sequences of curves where
        // length of each curve < lim
        // total dist from start to end of stripped section < lim
        public IList<Curve> RemoveTinyCurves(List<Curve> curves, float lim)
        {
            IList<Curve> ret = curves.ToList();

            bool all_tiny = true;

            if (curves.Aggregate(0.0f, (x, y) => x + y.Length) < lim)
            {
                return null;
            }

            if (curves.Count == 1)
            {
                return curves;
            }

            // run this three times:
            // forwards and backwards on even and odd passes
            //
            // Passes 0 - 1: we look for non-strippable curves to merge adjoining curves into
            //
            // Pass 2: then we take arbitrary start points and hope to merge curves up to lim
            // (no need to reverse that as the choice of start point is "arbitrary"
            //
            // If literally everything is minute we'll end up with one zero-length curve (technically a "loop" :-))
            // and return false
            //
            // if we end up with a line (two LineCurves), we can also return false as that is useless

            HashSet<Curve> inserted = new HashSet<Curve>();

            for (int q = 0; q <= 2; q++)
            {
                for (int i = 0; i < ret.Count; i++)
                {
                    Curve c_start = ret[i];

                    // on first two passes (forward and backwards), look for a non-strippable LineCurves to merge
                    // any following tiny curves into
                    //
                    // (have not worked out if there is a way to do that with CircleCurves, maybe
                    //  by fitting a new circle to the orig start, changed end and orig mid-point
                    //  but that would move the centre and I am concerned the new circle would no-longer overlay
                    //  other circles it is supposed to, of course, that could be a problem with lines too...)
                    if ((!(c_start is LineCurve) || c_start.Length < lim) && q != 2)
                    {
                        continue;
                    }

                    // third time through look for any length of linecurve or tiny
                    // circlecurves (previous merge products will still be ignored)
                    if (!(c_start is LineCurve) && c_start.Length > lim && q != 2)
                    {
                        continue;
                    }

                    // we don't want to keep adding to a line we already extended
                    if (inserted.Contains(c_start))
                    {
                        continue;
                    }

                    all_tiny = false;

                    List<Curve> found = new List<Curve>();

                    for (int j = 1; j < ret.Count; j++)
                    {
                        Curve c_end = ret[(i + j) % ret.Count];

                        if (c_end.Length >= lim)
                        {
                            break;
                        }
                        else if ((c_start.EndPos - c_end.EndPos).sqrMagnitude >= lim * lim)
                        {
                            break;
                        }

                        found.Add(c_end);
                    }

                    if (found.Count > 0)
                    {
                        // very hard to use a index-based loop here as range to delete may cross end of "ret"
                        foreach (var c in found)
                        {
                            ret.Remove(c);
                        }

                        // because of cyclic permutation, we may have removed curves before i
                        // so fix that up
                        i = ret.IndexOf(c_start);
                        ret.RemoveAt(i);

                        var replacement_c = LineCurve.MakeFromPoints(c_start.StartPos, found.Last().EndPos);

                        ret.Insert(i, replacement_c);

                        inserted.Add(replacement_c);
                        inserted.Add(replacement_c.Reversed());
                    }
                }

                if (q < 2)
                {
                    if (all_tiny && q == 0)
                    {
                        // no point running the reverse pass if the forward pass found no big curves
                        q = 1;
                    }
                    else
                    {
                        ret = new Loop("", ret).Reversed().Curves.ToList();
                    }
                }
            }

            if (ret.Count > 1
                || ret.Cast<CircleCurve>().Any()
                || Mathf.Abs(GeomRepUtil.SignedPolygonArea(ret)) > 1e-5f)
            {
                return ret;
            }

            return null;
        }

        // this is an interval in the sense that CrossingNumber describes the conditions on the
        // stabbling line *after* we intersect Curve and Distance at an angle of DotProduct
        // so it is more like "LowerBoundaryOfIntervalOnStabbingLine" OK?
        [System.Diagnostics.DebuggerDisplay("Curve = {Curve}, Crossings = {CrossingNumber}, Dot = {DotProduct}, Dist = {Distance}")]
        public struct Interval
        {
            public readonly Curve Curve;
            public readonly int CrossingNumber;
            public readonly float DotProduct;
            public readonly float Distance;

            public Interval(Curve curve, int crossingNumber, float dotProduct, float distance)
            {
                Curve = curve;
                CrossingNumber = crossingNumber;
                DotProduct = dotProduct;
                Distance = distance;
            }
        }

        // virtual and non-private for unit-testing only
        // return is a list of which curve, crossing number after curve, dot-product with stabbing line
        virtual public List<Interval> TryFindIntersections(
            Curve c,
            HashSet<Curve> all_curves,
            HashSet<Vector2> curve_joints,
            float diameter, float tol,
            ClRand random)
        {
            for (int i = 0; i < 25; i++)
            {
                float rand_ang = random.Nextfloat() * Mathf.PI * 2;
                float dx = Mathf.Sin(rand_ang);
                float dy = Mathf.Cos(rand_ang);

                Vector2 direction = new Vector2(dx, dy);

                Vector2 point = c.Pos(random.NextfloatRange(c.StartParam, c.EndParam), false);

                Vector2 start = point - direction * diameter;

                LineCurve lc = new LineCurve(start, direction, 2 * diameter);

                // must use a smaller tolerance here as our curve splitting can
                // give us curves < 2 * tol long, and if we are on the midpoint of one of those
                // we can't clear both ends by tol...
                if (!LineClearsPoints(lc, curve_joints, tol / 10))
                {
                    continue;
                }

                var ret = TryFindCurveIntersections(lc, all_curves);

                if (ret != null)
                {
                    return ret;
                }
            }

            return null;
        }

        // virtual and public for testing
        virtual public bool LineClearsPoints(LineCurve lc, HashSet<Vector2> curve_joints, float tol)
        {
            foreach (Vector2 pnt in curve_joints)
            {
                if (Mathf.Abs((pnt - lc.Position).Dot(lc.Direction.Rot90())) < tol)
                {
                    return false;
                }
            }

            return true;
        }

        // returns a set of <engine.brep.Curve, int> pairs sorted by distance down the line
        // at which the intersection occurs
        //
        // the curve is the curve intersecting and the integer is the
        // crossing number after we have passed that intersection
        //
        // the crossing number is implicitly zero before the first intersection
        //
        // non-private only for unit-testing
        public List<Interval> TryFindCurveIntersections(
            LineCurve lc,
            HashSet<Curve> all_curves)
        {
            List<Tuple<Curve, float, float>> intersecting_curves = new List<Tuple<Curve, float, float>>();

            foreach (Curve c in all_curves)
            {
                List<Tuple<float, float>> intersections = GeomRepUtil.CurveCurveIntersect(lc, c);

                if (intersections == null)
                {
                    continue;
                }

                foreach (Tuple<float, float> intersection in intersections)
                {
                    float dot = c.Tangent(intersection.Item2).Dot(lc.Direction.Rot270());

                    // chicken out and scrap any line that has a glancing contact with a curve
                    // a bit more than 1 degree
                    if (Mathf.Abs(dot) < 0.01)
                    {
                        return null;
                    }

                    intersecting_curves.Add(new Tuple<Curve, float, float>(c, intersection.Item1, dot));
                }
            }

            if (intersecting_curves.Count == 0)
            {
                return null;
            }

            // sort by distance down the line
            var ordered = intersecting_curves.OrderBy(a => a.Item2);

            int crossings = 0;

            var ret = new List<Interval>();

            foreach (Tuple<Curve, float, float> entry in ordered)
            {
                if (entry.Item3 > 0)
                {
                    crossings++;
                }
                else
                {
                    crossings--;
                }

                ret.Add(new Interval(entry.Item1, crossings, entry.Item3, entry.Item2));
            }

            if (ret.Count % 2 != 0)
            {
                // topologically, (e.g. if we have eliminated brushing-contacts and any weirdness with a line stabbing the
                // joint between two curves) we should only get even numbers of intersections

                // we try to avoid this happening, but if it does it's just an invalid stab and we can abort it
                // but put a breakpoint here if we're getting too many failed stabs
                return null;
            }

            return ret;
        }

        // non-private only for unit-tests
        public bool SplitCurvesAtIntersections(
            IList<Curve> working_loop1, IList<Curve> working_loop2,
            float tol, Dictionary<Curve, AnnotatedCurve> ann_map)
        {
            int intersection_count = 0;

            for (int i = 0; i < working_loop1.Count; i++)
            {
                Curve c1 = working_loop1[i];

                for (int j = 0; j < working_loop2.Count; j++)
                {
                    Curve c2 = working_loop2[j];

                    bool any_splits;

                    do
                    {
                        any_splits = false;

                        // we intersect with a broad tolerance, because if we split the occasional curve that is off the end
                        // of another one, it should not be a problem, but not splitting a curve we should will be a problem
                        //
                        // PREVIOUS NO LONGER TRUE as we now rely on the detection of intersections to know when
                        // two curves should be added to the same Splice, and adding the wrong curves to the same splice
                        // might be bad??
                        List<Tuple<float, float>> ret = GeomRepUtil.CurveCurveIntersect(c1, c2, 1e-5f);

                        if (ret == null)
                        {
                            break;
                        }

                        // we only count up in case the earlier entries fall close to existing splits and
                        // are ignored, otherwise if the first intersection causes a split
                        // we exit this loop immediately and look at the first pair from the newly inserted curve(s)
                        // instead
                        for (int k = 0; k < ret.Count && !any_splits; k++)
                        {
                            AnnotatedCurve c1_ac = ann_map[c1];
                            Splice c1_from = c1_ac.BackwardSplice;
                            Assertion.Assert(c1_from.ForwardLinks.Contains(c1));
                            Splice c1_to = c1_ac.ForwardSplice;
                            Assertion.Assert(c1_to.BackwardLinks.Contains(c1));

                            AnnotatedCurve c2_ac = ann_map[c2];
                            Splice c2_from = c2_ac.BackwardSplice;
                            Assertion.Assert(c2_from.ForwardLinks.Contains(c2));
                            Splice c2_to = c2_ac.ForwardSplice;
                            Assertion.Assert(c2_to.BackwardLinks.Contains(c2));

                            Splice joint = new Splice();

                            Tuple<float, float> split_points = ret[k];

                            float start_dist = c1.ParamCoordinateDist(c1.StartParam, split_points.Item1);
                            float end_dist = c1.ParamCoordinateDist(c1.EndParam, split_points.Item1);

                            // this is still an intersection, even if we do not have to add a split because it hits an existing one
                            intersection_count++;

                            // if we are far enough from existing splits
                            if (start_dist > tol && end_dist > tol)
                            {
                                any_splits = true;

                                Curve c1split1 = c1.CloneWithChangedExtents(c1.StartParam, split_points.Item1);
                                Curve c1split2 = c1.CloneWithChangedExtents(split_points.Item1, c1.EndParam);

                                working_loop1[i] = c1split1;
                                working_loop1.Insert(i + 1, c1split2);

                                // fix annotations and splices to swap of c1 to c1split1 and c1split2
                                joint = FixAnnotationsForCurveSplit(ann_map, c1, c1split1, c1split2, joint);

                                // once we've split once any second split could be in either new curve
                                // and also any further comparisons of the original c1 now need to be done separately on the two
                                // fragments
                                //
                                // so all-in-all simplest seems to be to pretend the two earlier fragments were where we were
                                // all along and re-start this (c1, c2) pair using them
                                //
                                // this will lead to a little repetition, as c1split2 will be checked against working_list2 items
                                // at indices < j, but hardly seems worth worrying about for small-ish curve numbers with few splits
                                c1 = c1split1;

#if DEBUG
                                ValidateAnnotations(ann_map, working_loop2.Concat(working_loop2).ToList());
#endif
                            }
                            else if (start_dist > tol)
                            {
                                joint = c1_to;
                            }
                            else
                            {
                                joint = c1_from;
                            }

                            // this is still an intersection, even if we do not have to add a split because it hits an existing one
                            intersection_count++;

                            start_dist = c2.ParamCoordinateDist(c2.StartParam, split_points.Item2);
                            end_dist = c2.ParamCoordinateDist(c2.EndParam, split_points.Item2);

                            // if we are far enough from existing splits
                            if (start_dist > tol && end_dist > tol)
                            {
                                any_splits = true;

                                Curve c2split1 = c2.CloneWithChangedExtents(c2.StartParam, split_points.Item2);
                                Curve c2split2 = c2.CloneWithChangedExtents(split_points.Item2, c2.EndParam);

                                working_loop2[j] = c2split1;
                                working_loop2.Insert(j + 1, c2split2);

                                // fix annotations and splices to swap of c2 to c2split2 and c2split2
                                joint = FixAnnotationsForCurveSplit(ann_map, c2, c2split1, c2split2, joint);

                                // see comment in previous if-block
                                c2 = c2split1;
                            }
                            else if (start_dist > tol && any_splits)
                            {
                                // we're not adding a splice, but our end-splice is now merged with joint
                                MergeSplices(ann_map, c2_to, joint);
                            }
                            else if (any_splits)
                            {
                                // we're not adding a splice, but our start-splice is now merged with joint
                                MergeSplices(ann_map, c2_from, joint);
                            }

#if DEBUG
                            if (any_splits)
                            {
                                ValidateAnnotations(ann_map, working_loop2.Concat(working_loop2).ToList());
                            }
#endif
                        }
                    } while (any_splits);
                }
            }

            // we expect even numbers of crossings
            Assertion.Assert(intersection_count % 2 == 0);

            return intersection_count > 0;
        }

        // order of merge_from and merge_to should be irrelevant
        private static void MergeSplices(Dictionary<Curve, AnnotatedCurve> ann_map, Splice merge_from, Splice merge_to)
        {
            merge_to.ForwardLinks.AddRange(merge_from.ForwardLinks);
            merge_to.BackwardLinks.AddRange(merge_from.BackwardLinks);

            // there can be other curves using our old splice, and they all need swapping to the new one
            foreach (var c in merge_from.BackwardLinks)
            {
                ann_map[c].ForwardSplice = merge_to;
            }

            foreach (var c in merge_from.ForwardLinks)
            {
                ann_map[c].BackwardSplice = merge_to;
            }
        }

        private static Splice FixAnnotationsForCurveSplit(Dictionary<Curve, AnnotatedCurve> ann_map,
            Curve c, Curve csplit1, Curve csplit2, Splice joint)
        {
            AnnotatedCurve c_ac = ann_map[c];
            // make two new ACs for the new curves
            var new_ac1 = new AnnotatedCurve(csplit1, c_ac.LoopNumber);
            var new_ac2 = new AnnotatedCurve(csplit2, c_ac.LoopNumber);

            ann_map.Remove(c);                     // fix up the map
            ann_map[csplit1] = new_ac1;            //      "
            ann_map[csplit2] = new_ac2;            //      "

            // now we have two ACs and three splices
            // get the splices on the right places on the ACs

            new_ac1.ForwardSplice = new_ac2.BackwardSplice = joint;
            new_ac1.BackwardSplice = c_ac.BackwardSplice;
            new_ac2.ForwardSplice = c_ac.ForwardSplice;

            // fix the splice contents

            new_ac1.BackwardSplice.ForwardLinks.Remove(c);
            new_ac1.BackwardSplice.ForwardLinks.Add(csplit1);

            new_ac2.ForwardSplice.BackwardLinks.Remove(c);
            new_ac2.ForwardSplice.BackwardLinks.Add(csplit2);

            joint.ForwardLinks.Add(csplit2);
            joint.BackwardLinks.Add(csplit1);

            return joint;
        }

        private static void FixAnnotationsForCurveSplit(Dictionary<Curve, AnnotatedCurve> ann_map,
            Curve c, IList<Curve> splits)
        {
            AnnotatedCurve c_ac = ann_map[c];

            // clear references to the old curve from existing splices
            c_ac.BackwardSplice.ForwardLinks.Remove(c);
            c_ac.ForwardSplice.BackwardLinks.Remove(c);

            // make new ACs for the curves
            IList<AnnotatedCurve> new_acs = splits.Select(x => new AnnotatedCurve(x, c_ac.LoopNumber)).ToList();

            // fix up the map
            ann_map.Remove(c);
            foreach(var ac in new_acs)
            {
                ann_map[ac.Curve] = ac;            //      "

            }

            // now we have N ACs and N+1 splices
            // get the splices on the right places on the ACs
            var back_splice = c_ac.BackwardSplice;

            foreach(var ac in new_acs)
            {
                ac.BackwardSplice = back_splice;
                back_splice = ac.ForwardSplice;
            }

            new_acs.Last().ForwardSplice = c_ac.ForwardSplice;

            // now add the links to all splices
            foreach (var ac in new_acs)
            {
                ac.BackwardSplice.ForwardLinks.Add(ac.Curve);
                ac.ForwardSplice.BackwardLinks.Add(ac.Curve);
            }
        }

        private void ValidateAnnotations(Dictionary<Curve, AnnotatedCurve> ann_map, IList<Curve> allCurves)
        {
            Dictionary<Curve, int> forward_counts = new Dictionary<Curve, int>(
                new ReferenceComparer<Curve>());
            Dictionary<Curve, int> backward_counts = new Dictionary<Curve, int>(
                new ReferenceComparer<Curve>());

            // do a load of checking on the ForwardSplices

            foreach (var pair in ann_map)
            {
                Assertion.Assert(pair.Key == pair.Value.Curve);

                Assertion.Assert(pair.Value.ForwardSplice.BackwardLinks.Contains(pair.Key));
                Assertion.Assert(pair.Value.BackwardSplice.ForwardLinks.Contains(pair.Key));

                foreach(var c in pair.Value.ForwardSplice.ForwardLinks)
                {
                    Assertion.Assert(ann_map[c].BackwardSplice == pair.Value.ForwardSplice);
                }

                foreach (var c in pair.Value.BackwardSplice.BackwardLinks)
                {
                    Assertion.Assert(ann_map[c].ForwardSplice == pair.Value.BackwardSplice);
                }
            }

            foreach (var c in ann_map.Values.SelectMany(x => x.ForwardSplice.ForwardLinks))
            {
                forward_counts[c] = 0;
            }

            foreach (var c in ann_map.Values.SelectMany(x => x.ForwardSplice.BackwardLinks))
            {
                backward_counts[c] = 0;
            }

            var hfk = new HashSet<Curve>(forward_counts.Keys);
            var hbk = new HashSet<Curve>(backward_counts.Keys);
            var hk = new HashSet<Curve>(ann_map.Keys);
            var hc = new HashSet<Curve>(allCurves);

            // we expect every curve to have an entry,
            // and every entry in the map to appear as a forwards and a backwards link
            Assertion.Assert(hk.IsSupersetOf(hc));
            Assertion.Assert(hk.SetEquals(hfk));
            Assertion.Assert(hk.SetEquals(hbk));

            foreach (var splice in ann_map.Values.Select(x => x.ForwardSplice).Distinct())
            {
                Assertion.Assert(splice.ForwardLinks.Count == splice.BackwardLinks.Count);

                foreach (var c in splice.ForwardLinks)
                {
                    Assertion.Assert(ann_map.ContainsKey(c));

                    forward_counts[c]++;
                }

                foreach (var c in splice.BackwardLinks)
                {
                    Assertion.Assert(ann_map.ContainsKey(c));

                    backward_counts[c]++;
                }
            }

            foreach (var c in forward_counts.Keys)
            {
                Assertion.Assert(forward_counts[c] == 1);
                Assertion.Assert(backward_counts[c] == 1);
            }

            // confirm BackwardSplices refer to the same objects

            foreach(var ac in ann_map.Values)
            {
                foreach (var c in ac.ForwardSplice.ForwardLinks)
                {
                    Assertion.Assert(ann_map[c].BackwardSplice == ac.ForwardSplice);
                }
            }

            HashSet<Curve> open = new HashSet<Curve>(ann_map.Keys, new ReferenceComparer<Curve>());

            // check we can take the curves by loops
            while(open.Count > 0)
            {
                var c = open.First();
                var start = c;
                bool done = false;

                do
                {
                    open.Remove(c);

                    var splice = ann_map[c].ForwardSplice;

                    Curve next = null;

                    foreach(var f in splice.ForwardLinks)
                    {
                        // end as soon as we can
                        if (f == start)
                        {
                            done = true;
                            next = start;
                            break;
                        }
                        if (open.Contains(f))
                        {
                            next = f;
                        }
                    }

                    Assertion.Assert(next != null || done);

                    c = next;

                }
                while (!done);
            }
        }

        // non-private only for unit-tests
        public bool SplitCurvesAtCoincidences(
            IList<Curve> working_loop1, IList<Curve> working_loop2,
            float tol, Dictionary<Curve, AnnotatedCurve> ann_map)
        {
            bool any_found = false;

            for (int i = 0; i < working_loop1.Count; i++)
            {
                Curve c1 = working_loop1[i];
                for (int j = 0; j < working_loop2.Count; j++)
                {
                    Curve c2 = working_loop2[j];

                    AnnotatedCurve c1_ac = ann_map[c1];
                    Splice c1_from = c1_ac.BackwardSplice;
                    Assertion.Assert(c1_from.ForwardLinks.Contains(c1));
                    Splice c1_to = c1_ac.ForwardSplice;
                    Assertion.Assert(c1_to.BackwardLinks.Contains(c1));

                    AnnotatedCurve c2_ac = ann_map[c2];
                    Splice c2_from = c2_ac.BackwardSplice;
                    Assertion.Assert(c2_from.ForwardLinks.Contains(c2));
                    Splice c2_to = c2_ac.ForwardSplice;
                    Assertion.Assert(c2_to.BackwardLinks.Contains(c2));

                    var ret = c1.SplitCoincidentCurves(c2, tol);

                    if (ret == null)
                    {
                        continue;
                    }

                    if (ret.Item1 != null)
                    {
                        any_found = true;

                        // once we've split once the new curves still need testing against the rest of the
                        // other loop, further splits could be in any new curve
                        //
                        // so all-in-all simplest seems to be to pretend the two earlier fragments were where we were
                        // all along and re-start this (c1, c2) pair using them

                        working_loop1.RemoveAt(i);
                        for (int k = 0; k < ret.Item1.Count; k++) {
                            working_loop1.Insert(i + k, ret.Item1[k]);
                        }

                        // c1 is replaced now
                        FixAnnotationsForCurveSplit(ann_map, c1, ret.Item1);

                        c1 = ret.Item1[0];

#if DEBUG
                        ValidateAnnotations(ann_map, working_loop2.Concat(working_loop2).ToList());
#endif
                    }

                    if (ret.Item2 != null)
                    {
                        any_found = true;

                        // once we've split once the new curves still need testing against the rest of the
                        // other loop, further splits could be in any new curve
                        //
                        // so all-in-all simplest seems to be to pretend the two earlier fragments were where we were
                        // all along and re-start this (c1, c2) pair using them

                        working_loop2.RemoveAt(j);
                        for (int k = 0; k < ret.Item2.Count; k++)
                        {
                            working_loop2.Insert(i + k, ret.Item2[k]);
                        }

                        // c2 is replaced now
                        FixAnnotationsForCurveSplit(ann_map, c2, ret.Item2);

                        c2 = ret.Item2[0];

#if DEBUG
                        ValidateAnnotations(ann_map, working_loop2.Concat(working_loop2).ToList());
#endif
                    }
                }
            }

            bool any_merges = false;

            // I don't really like this way of merging splices, as there is a tolerance involved
            // HOWEVER matching up the new and old splices from the two loops in the iteration above is difficult
            // I DID CONSIDER returning List<Tuple<Curve, Curve>> from SplitConcidentCurves, however
            // that is also a bit tricky, so trying this for the moment
            foreach (var c1 in working_loop1)
            {
                var spl1 = ann_map[c1].ForwardSplice;
                var p1 = c1.EndPos;

                foreach (var c2 in working_loop2)
                {
                    var spl2 = ann_map[c2].ForwardSplice;
                    var p2 = c2.EndPos;

                    // don't try to merge anything that is already the same item :-)
                    if (ReferenceEquals(spl1, spl2))
                        continue;

                    if ((p1 - p2).magnitude < 1e-4f)
                    {
                        any_merges = true;

                        spl1.ForwardLinks.AddRange(spl2.ForwardLinks);
                        spl1.BackwardLinks.AddRange(spl2.BackwardLinks);

                        // there can be other curves using our old splice, and they all need swapping to the new one
                        foreach (var c in spl2.BackwardLinks)
                        {
                            ann_map[c].ForwardSplice = spl1;
                        }

                        // there can be other curves using our old splice, and they all need swapping to the new one
                        foreach (var c in spl2.ForwardLinks)
                        {
                            ann_map[c].BackwardSplice = spl1;
                        }
                    }
                }
            }

#if DEBUG
            if (any_merges)
            {
                ValidateAnnotations(ann_map, working_loop2.Concat(working_loop2).ToList());
            }
#endif

            return any_found;
        }

        // only non-private for unit-testing
        public void BuildAnnotationChains(IList<Curve> curves, int loop_number,
                                          Dictionary<Curve, AnnotatedCurve> ann_map)
        {
            new Loop("", curves);

            Curve prev = curves.Last();

            foreach (Curve curr in curves)
            {
                AnnotatedCurve ac = new AnnotatedCurve(prev, loop_number);
                ac.ForwardSplice.BackwardLinks.Add(prev);
                ac.ForwardSplice.ForwardLinks.Add(curr);

                ann_map.Add(prev, ac);

                prev = curr;
            }

            prev = curves.First();

            foreach(Curve curr in curves.Reverse())
            {
                ann_map[prev].BackwardSplice = ann_map[curr].ForwardSplice;

                prev = curr;
            }

            ValidateAnnotations(ann_map, new HashSet<Curve>(ann_map.Keys.Concat(curves)).ToList());
        }


        public sealed class ReferenceComparer<T> : IEqualityComparer<T>
            where T : class
        {
            public int GetHashCode(T value)
            {
                return RuntimeHelpers.GetHashCode(value);
            }

            public bool Equals(T left, T right)
            {
                return left == right; // Reference identity comparison
            }
        }
    }
}
