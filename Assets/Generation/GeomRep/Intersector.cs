﻿using Assets.Extensions;
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
        // only non-private for unit-testing
        [System.Diagnostics.DebuggerDisplay("Curve = {Curve}, LoopNumber = {LoopNumber}")]
        public class AnnotatedCurve : EqualityBase
        {
            public readonly Curve Curve;
            public readonly int LoopNumber;

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

        public class Splice
        {
            public List<Curve> ForwardLinks = new List<Curve>();
            public List<Curve> BackwardLinks = new List<Curve>();
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
            var endSpliceMap = MakeEndSpliceMap();

            foreach(var loop in working_loops1.Values.Concat(working_loops2.Values))
            {
                SetupInitialSplices(loop, endSpliceMap);
            }

            // split all curves that intersect
            foreach (int i in working_loops1.Keys)
            {
                var alc1 = working_loops1[i];
                foreach (int j in working_loops2.Keys)
                {
                    var alc2 = working_loops2[j];

                    SplitCurvesAtIntersections(alc1, alc2, 1e-4f, endSpliceMap);

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
                    SplitCurvesAtCoincidences(alc1, alc2, 1e-4f, endSpliceMap);
#if DEBUG
                    // has a side effect of checking that the loops are still loops
                    new Loop("", alc1);
                    new Loop("", alc2);
#endif
                }
            }

            Dictionary<Curve, AnnotatedCurve> forward_annotations_map
                = MakeForwardAnnotationsMap();

            // build forward and reverse chains of annotation-curves around both loops
            foreach (int i in working_loops1.Keys)
            {
                IList<Curve> alc1 = working_loops1[i];

                BuildAnnotationChains(alc1, i, forward_annotations_map);
            }

            foreach (int i in working_loops2.Keys)
            {
                IList<Curve> alc1 = working_loops2[i];

                BuildAnnotationChains(alc1, i, forward_annotations_map);
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

            HashSet<AnnotatedCurve> open = MakeOpenSet(forward_annotations_map);

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
                forward_annotations_map, all_curves, open, clustered_joints,
                diameter,
                type))
            {
                return null;
            }

            while (open.Count > 0)
            {
                AnnotatedCurve ac_current = open.First();

                // take a loop that is part of the perimeter
                ret.Add(ExtractLoop(
                      open,
                      ac_current,
                      endSpliceMap,
                      layer));
            }

#if DEBUG
            ValidatePreviouslyMerged(ret, "ret");
#endif

            return ret;
        }

        public void SetupInitialSplices(IList<Curve> loop, Dictionary<Curve, Splice> endSpliceMap)
        {
            var prev = loop.Last();

            foreach(var curr in loop)
            {
                Splice splice;
                
                splice = new Splice();

                splice.ForwardLinks.Add(curr);
                splice.BackwardLinks.Add(prev);

                endSpliceMap[prev] = splice;

                prev = curr;
            }
        }

        public static Dictionary<Curve, AnnotatedCurve> MakeForwardAnnotationsMap()
        {
            return new Dictionary<Curve, AnnotatedCurve>(
                                new ReferenceComparer<Curve>());
        }

        public static Dictionary<Curve, Splice> MakeEndSpliceMap()
        {
            return new Dictionary<Curve, Splice>(new ReferenceComparer<Curve>());
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

        public static HashSet<AnnotatedCurve> MakeOpenSet(Dictionary<Curve, AnnotatedCurve> forward_annotations_map)
        {
            return new HashSet<AnnotatedCurve>(
                            forward_annotations_map.Values,
                            new ReferenceComparer<AnnotatedCurve>());
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
            Dictionary<Curve, AnnotatedCurve> forward_annotations_map, HashSet<Curve> all_curves,
            HashSet<AnnotatedCurve> open, HashSet<Vector2> curve_joints,
            float diameter,
            UnionType type)
        {
            // if we find a curve is non-internal while inspecting another, need not look at it again
            HashSet<AnnotatedCurve> seen = new HashSet<AnnotatedCurve>(
                new ReferenceComparer<AnnotatedCurve>());

            int non_zero_value = type == UnionType.WantPositive ? 1 : -1;

            foreach (Curve c in all_curves)
            {
                AnnotatedCurve ac_c = forward_annotations_map[c];

                if (!open.Contains(ac_c) || seen.Contains(ac_c))
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

                EliminateCancellingLines(intervals, open, tol, forward_annotations_map);

                // now use the intervals to decide what to do with the AnnotationEdges
                int prev_crossings = 0;

                foreach (var intersection in intervals)
                {
                    int crossings = intersection.CrossingNumber;

                    AnnotatedCurve ac_intersecting = forward_annotations_map[intersection.Curve];

                    if (open.Contains(ac_intersecting) && !seen.Contains(ac_intersecting))
                    {
                        // three cases, 0 -> 1, 1 -> 0 and anything else
                        if ((prev_crossings != 0 || crossings != non_zero_value)
                              && (prev_crossings != non_zero_value || crossings != 0))
                        {
                            open.Remove(ac_intersecting);
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

        // where N lines all appear with successive separations of < tol
        // (e.g. approximating zero separation)
        // we can cancel any pairs that lie in exactly opposite directions
        // (because we have already snipped curves to separate coincident subsections)
        public void EliminateCancellingLines(List<Interval> intervals,
            HashSet<AnnotatedCurve> open, float tol,
            Dictionary<Curve, AnnotatedCurve> forward_annotations_map)
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
                                if (forward_annotations_map != null)
                                {
                                    var ac1 = forward_annotations_map[int1.Curve];
                                    var ac2 = forward_annotations_map[int2.Curve];
                                    open.Remove(ac1);
                                    open.Remove(ac2);
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

        Loop ExtractLoop(HashSet<AnnotatedCurve> open,
                         AnnotatedCurve start_ac,
                         Dictionary<Curve, Splice> endSpliceMap,
                         string layer)
        {
            AnnotatedCurve curr_ac = start_ac;

            List<Curve> found_curves = new List<Curve>();

            while (true)
            {
                Assertion.Assert(open.Contains(curr_ac));

                Curve c = curr_ac.Curve;
                found_curves.Add(c);
                open.Remove(curr_ac);

                // look for a splice that ends this curve
                endSpliceMap.TryGetValue(c, out Splice splice);

                Assertion.Assert(splice != null);

                // at every splice, at least one of the possible exits should be still open
                // or we should have just come full circle
                Assertion.Assert(splice.ForwardLinks.Contains(start_ac.Curve)
                    || open.Where(x => splice.ForwardLinks.Contains(x.Curve)).Any());

                List<AnnotatedCurve> still_open =
                    open.Where(x => splice.ForwardLinks.Contains(x.Curve)).ToList();
                List<AnnotatedCurve> different_loop =
                    still_open.Where(x => x.LoopNumber != curr_ac.LoopNumber).ToList();

                if (still_open.Count == 1)
                {
                    curr_ac = still_open[0];
                }
                else if (different_loop.Count == 1)
                {
                    curr_ac = different_loop[0];
                }
                else if (different_loop.Count != 0)
                {
                    // if there is more than one option on a different loop, take the sharpest clockwise corner
                    // first
                    float found_ang = float.MaxValue;
                    AnnotatedCurve found_ac = null;

                    foreach (var ac in different_loop)
                    {
                        var cur_normal = curr_ac.Curve.Normal(curr_ac.Curve.EndParam);
                        var try_normal = -ac.Curve.Normal(ac.Curve.EndParam);

                        float ang = AngleRange.FixupAngle(Util.SignedAngleDifference(cur_normal, try_normal));

                        if (ang < found_ang)
                        {
                            found_ang = ang;
                            found_ac = ac;
                        }
                    }

                    curr_ac = found_ac;
                }
                else
                {
                    // otherwise we expect to have arrived back at our starting curve
                    Assertion.Assert(splice.ForwardLinks.Contains(start_ac.Curve));

                    curr_ac = start_ac;
                }

                if (curr_ac == start_ac)
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

            return new Loop(layer, found_curves);
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
            float tol, Dictionary<Curve, Splice> endSpliceMap)
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
                        List<Tuple<float, float>> ret = GeomRepUtil.CurveCurveIntersect(c1, c2, 0.01f);

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
                            // these can change when the size of the loop changes, so they need
                            // recalculating more often than i and j...
                            int i_prev = (i + working_loop1.Count - 1) % working_loop1.Count;
                            int j_prev = (j + working_loop2.Count - 1) % working_loop2.Count;

                            Curve c1_prev = working_loop1[i_prev];
                            Curve c2_prev = working_loop2[j_prev];

                            Splice c1_from = endSpliceMap[c1_prev];
                            Assertion.Assert(c1_from.ForwardLinks.Contains(c1));
                            Splice c1_to = endSpliceMap[c1];
                            Assertion.Assert(c1_to.BackwardLinks.Contains(c1));

                            Splice c2_from = endSpliceMap[c2_prev];
                            Assertion.Assert(c2_from.ForwardLinks.Contains(c2));
                            Splice c2_to = endSpliceMap[c2];
                            Assertion.Assert(c2_to.BackwardLinks.Contains(c2));

                            Splice joint;

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

                                // c1 is replaced now
                                endSpliceMap.Remove(c1);                // move c1_to onto the second new curve in the splice map
                                endSpliceMap[c1split2] = c1_to;         //      "

                                c1_to.BackwardLinks.Remove(c1);         // swap its backward link to match
                                c1_to.BackwardLinks.Add(c1split2);      //      "

                                c1_from.ForwardLinks.Remove(c1);        // swap the preceding splice's forward link
                                c1_from.ForwardLinks.Add(c1split1);     // to point to the first new curve

                                joint = new Splice();                   // create the new joint
                                endSpliceMap[c1split1] = joint;         // add it to the map at the end of first new curve
                                joint.ForwardLinks.Add(c1split2);       // fix its links
                                joint.BackwardLinks.Add(c1split1);      //      "

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
                            }
                            else if (start_dist > tol)
                            {
                                joint = c1_to;
                            }
                            else
                            {
                                joint = c1_from;
                            }

#if DEBUG
                            ValidateEndSpliceMap(endSpliceMap, working_loop2.Concat(working_loop2).ToList());
#endif

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

                                // c2 is replaced now
                                endSpliceMap.Remove(c2);                // move c1_to onto the second new curve in the splice map
                                endSpliceMap[c2split2] = c2_to;         //      "

                                c2_to.BackwardLinks.Remove(c2);         // swap its backward link to match
                                c2_to.BackwardLinks.Add(c2split2);      //      "

                                c2_from.ForwardLinks.Remove(c2);        // swap the preceding splice's forward link
                                c2_from.ForwardLinks.Add(c2split1);     // to point to the first new curve

                                endSpliceMap[c2split1] = joint;         // add it to the map at the end of first new curve
                                joint.ForwardLinks.Add(c2split2);       // fix its links
                                joint.BackwardLinks.Add(c2split1);      //      "

                                // see comment in previous if-block
                                c2 = c2split1;
                            }
                            else if (start_dist > tol && any_splits)
                            {
                                // we're not adding a splice, but our end-splice is now merged with joint
                                joint.ForwardLinks.AddRange(c2_to.ForwardLinks);
                                joint.BackwardLinks.AddRange(c2_to.BackwardLinks);
                                endSpliceMap[c2] = joint;
                            }
                            else if (any_splits)
                            {
                                // we're not adding a splice, but our start-splice is now merged with joint
                                joint.ForwardLinks.AddRange(c2_from.ForwardLinks);
                                joint.BackwardLinks.AddRange(c2_from.BackwardLinks);
                                endSpliceMap[c2_prev] = joint;
                            }

#if DEBUG
                            ValidateEndSpliceMap(endSpliceMap, working_loop2.Concat(working_loop2).ToList());
#endif
                        }
                    } while (any_splits);
                }
            }

            // we expect even numbers of crossings
            Assertion.Assert(intersection_count % 2 == 0);

            return intersection_count > 0;
        }

        private void ValidateEndSpliceMap(Dictionary<Curve, Splice> endSpliceMap, IList<Curve> allCurves)
        {
            Dictionary<Curve, int> forward_counts = new Dictionary<Curve, int>(
                new ReferenceComparer<Curve>());
            Dictionary<Curve, int> backward_counts = new Dictionary<Curve, int>(
                new ReferenceComparer<Curve>());

            foreach (var c in endSpliceMap.Values.SelectMany(x => x.ForwardLinks))
            {
                forward_counts[c] = 0;
            }

            foreach (var c in endSpliceMap.Values.SelectMany(x => x.BackwardLinks))
            {
                backward_counts[c] = 0;
            }

            var hfk = new HashSet<Curve>(forward_counts.Keys);
            var hbk = new HashSet<Curve>(backward_counts.Keys);
            var hk = new HashSet<Curve>(endSpliceMap.Keys);
            var hc = new HashSet<Curve>(allCurves);

            // we expect every curve to have an entry,
            // and every entry in the map to appear as a forwards and a backwards link
            Assertion.Assert(hk.IsSupersetOf(hc));
            Assertion.Assert(hk.SetEquals(hfk));
            Assertion.Assert(hk.SetEquals(hbk));

            foreach (var splice in endSpliceMap.Values.Distinct())
            {
                Assertion.Assert(splice.ForwardLinks.Count == splice.BackwardLinks.Count);

                foreach (var c in splice.ForwardLinks)
                {
                    Assertion.Assert(endSpliceMap.ContainsKey(c));

                    forward_counts[c]++;
                }

                foreach (var c in splice.BackwardLinks)
                {
                    Assertion.Assert(endSpliceMap.ContainsKey(c));

                    backward_counts[c]++;
                }
            }

            foreach (var c in forward_counts.Keys)
            {
                Assertion.Assert(forward_counts[c] == 1);
                Assertion.Assert(backward_counts[c] == 1);
            }
        }

        // non-private only for unit-tests
        public bool SplitCurvesAtCoincidences(
            IList<Curve> working_loop1, IList<Curve> working_loop2,
            float tol, Dictionary<Curve, Splice> endSpliceMap)
        {
            bool any_found = false;

            for (int i = 0; i < working_loop1.Count; i++)
            {
                Curve c1 = working_loop1[i];
                for (int j = 0; j < working_loop2.Count; j++)
                {
                    Curve c2 = working_loop2[j];

                    // these can change when the size of the loop changes, so they need
                    // recalculating more often than i and j...
                    int i_prev = (i + working_loop1.Count - 1) % working_loop1.Count;
                    int j_prev = (j + working_loop2.Count - 1) % working_loop2.Count;

                    Curve c1_prev = working_loop1[i_prev];
                    Curve c2_prev = working_loop2[j_prev];

                    Splice c1_from = endSpliceMap[c1_prev];
                    Assertion.Assert(c1_from.ForwardLinks.Contains(c1));
                    Splice c1_to = endSpliceMap[c1];
                    Assertion.Assert(c1_to.BackwardLinks.Contains(c1));

                    Splice c2_from = endSpliceMap[c2_prev];
                    Assertion.Assert(c2_from.ForwardLinks.Contains(c2));
                    Splice c2_to = endSpliceMap[c2];
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

                        // c1 is replaced now
                        endSpliceMap.Remove(c1);                    // move c1_to onto the second new curve in the splice map

                        c1_to.BackwardLinks.Remove(c1);             // swap its backward link to match
                        c1_from.ForwardLinks.Remove(c1);            // swap the preceding splice's forward link

                        Splice prev_splice = c1_from;

                        Splice[] inserted_splices = ret.Item1.Select(x => new Splice()).ToArray();
                        inserted_splices[inserted_splices.Length - 1] = c1_to;

                        for (int n_ins = 0; n_ins < ret.Item1.Count; n_ins++)
                        {
                            Curve hc = ret.Item1[n_ins];
                            working_loop1.Insert(i + n_ins, hc);

                            var curr_splice = inserted_splices[n_ins];
                            curr_splice.BackwardLinks.Add(hc);
                            endSpliceMap[hc] = curr_splice;

                            prev_splice.ForwardLinks.Add(hc);
                            prev_splice = curr_splice;
                        }

                        c1 = ret.Item1[0];
                    }

#if DEBUG
                    ValidateEndSpliceMap(endSpliceMap, working_loop2.Concat(working_loop2).ToList());
#endif

                    if (ret.Item2 != null)
                    {
                        any_found = true;

                        // once we've split once the new curves still need testing against the rest of the
                        // other loop, further splits could be in any new curve
                        //
                        // so all-in-all simplest seems to be to pretend the two earlier fragments were where we were
                        // all along and re-start this (c1, c2) pair using them

                        working_loop2.RemoveAt(j);

                        // c1 is replaced now
                        endSpliceMap.Remove(c2);                    // move c1_to onto the second new curve in the splice map

                        c2_to.BackwardLinks.Remove(c2);             // swap its backward link to match
                        c2_from.ForwardLinks.Remove(c2);            // swap the preceding splice's forward link

                        Splice prev_splice = c2_from;

                        Splice[] inserted_splices = ret.Item2.Select(x => new Splice()).ToArray();
                        inserted_splices[inserted_splices.Length - 1] = c2_to;

                        for (int n_ins = 0; n_ins < ret.Item2.Count; n_ins++)
                        {
                            Curve hc = ret.Item2[n_ins];
                            working_loop2.Insert(j + n_ins, hc);

                            var curr_splice = inserted_splices[n_ins];
                            curr_splice.BackwardLinks.Add(hc);
                            endSpliceMap[hc] = curr_splice;

                            prev_splice.ForwardLinks.Add(hc);
                            prev_splice = curr_splice;
                        }

                        c2 = ret.Item2[0];
                    }

#if DEBUG
                    ValidateEndSpliceMap(endSpliceMap, working_loop2.Concat(working_loop2).ToList());
#endif
                }
            }

            // I don't really like this way of merging splices, as there is a tolerance involved
            // HOWEVER matching up the new and old splices from the two loops in the iteration above is difficult
            // I DID CONSIDER returning List<Tuple<Curve, Curve>> from SplitConcidentCurves, however
            // that is also a bit tricky, so trying this for the moment
            foreach (var c1 in working_loop1)
            {
                var spl1 = endSpliceMap[c1];
                var p1 = c1.EndPos;

                foreach (var c2 in working_loop2)
                {
                    var spl2 = endSpliceMap[c2];
                    var p2 = c2.EndPos;

                    // following SplitCurvesAtIntersections, the two curves can already share splices
                    // so don't try to merge anything that is already the same item :-)
                    if (ReferenceEquals(spl1, spl2))
                        continue;

                    if ((p1 - p2).magnitude < 1e-4f)
                    {
                        spl1.ForwardLinks.AddRange(spl2.ForwardLinks);
                        spl1.BackwardLinks.AddRange(spl2.BackwardLinks);

                        endSpliceMap[c2] = spl1;
                    }
                }
            }

#if DEBUG
            ValidateEndSpliceMap(endSpliceMap, working_loop2.Concat(working_loop2).ToList());
#endif

            return any_found;
        }

        // only non-private for unit-testing
        public void BuildAnnotationChains(IList<Curve> curves, int loop_number,
                                          Dictionary<Curve, AnnotatedCurve> forward_annotations_map)
        {
            Curve prev = null;

            foreach (Curve curr in curves)
            {
                AnnotatedCurve ac_forward_curr = new AnnotatedCurve(curr, loop_number);

                forward_annotations_map.Add(curr, ac_forward_curr);

                prev = curr;
            }

            Curve first = curves[0];

            AnnotatedCurve ac_forward_first = forward_annotations_map[first];
            AnnotatedCurve ac_forward_last = forward_annotations_map[prev];
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
