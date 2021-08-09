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
        // only non-private for unit-testing
        [System.Diagnostics.DebuggerDisplay("Curve = {Curve}, LoopNumber = {LoopNumber}")]
        public class AnnotatedCurve : EqualityBase
        {
            public readonly Curve Curve;
            public AnnotatedCurve Next;
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

        // only non-private for unit-testing
        [System.Diagnostics.DebuggerDisplay("Loop1 = {Loop1Out}, Loop2 = {Loop2Out}")]
        public class Splice
        {
            public readonly AnnotatedCurve Loop1Out;
            public readonly AnnotatedCurve Loop2Out;

            public Splice(AnnotatedCurve l1out, AnnotatedCurve l2out)
            {
                Loop1Out = l1out;
                Loop2Out = l2out;
            }
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
                             ClRand random,
                             UnionType type = UnionType.WantPositive)
        {
#if DEBUG
            ValidateInputs(previously_merged, "previously_merged");
            ValidateInputs(to_merge, "ls2");
            //for()
#endif

            // any loops in to_merge can be struck off as they will have no effect
            // (and following code cannot handle them anyway, as they both overlap but don't intersect)
            RemoveIdenticalLoops(previously_merged, to_merge);

            // if there is no input left, we're done
            if (to_merge.Count == 0)
            {
                return previously_merged;
            }

            // needing to check +ve/-ve curve type shoots any simple early-outs in the foot
            //// simple case, also covers us being handed the same instance twice
            //if (ls1.Equals(ls2))
            //{
            //    return ls1;
            //}

            //if (ls1.Count == 0)
            //{
            //    return ls2;
            //}

            //if (ls2.Count == 0)
            //{
            //    return ls1;
            //}

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

            //Dictionary<IList<Curve>, Box2> bound_map1 = new Dictionary<IList<Curve>, Box2>();

            //foreach (IList<Curve> alc1 in working_loops1.Values)
            //{
            //    Box2 bound = alc1.Select(l => l.BoundingArea)
            //        .Aggregate(new Box2(), (a, b) => a.Union(b));

            //    bound_map1.Add(alc1, bound);
            //}

            //Dictionary<IList<Curve>, Box2> bound_map2 = new Dictionary<IList<Curve>, Box2>();

            //foreach (IList<Curve> alc2 in working_loops2.Values)
            //{
            //    Box2 bound = alc2.Select(l => l.BoundingArea)
            //        .Aggregate(new Box2(), (a, b) => a.Union(b));

            //    bound_map2.Add(alc2, bound);
            //}

            // likewise this wouldn't distinguish -ve/+ve curves, which is required for
            // UnionType switching
            //RemoveEasyLoops(working_loops1, ret, bound_map2.Values, bound_map1);
            //RemoveEasyLoops(working_loops2, ret, bound_map1.Values, bound_map2);

            HashSet<Tuple<int, int>> splittings = new HashSet<Tuple<int, int>>();

            // split all curves that intersect
            foreach (int i in working_loops1.Keys)
            {
                var alc1 = working_loops1[i];
                foreach (int j in working_loops2.Keys)
                {
                    var alc2 = working_loops2[j];

                    if (SplitCurvesAtIntersections(alc1, alc2, 1e-4f))
                    {
                        splittings.Add(new Tuple<int, int>(i, j));
                    }

                    // curves that wholly or partly overlay each other do not intersect
                    // but we still need to split them because we need to discard some overlapping parts
                    // (but not necessarily the whole curve)
                    if (SplitCurvesAtCoincidences(alc1, alc2, 1e-4f))
                    {
                        splittings.Add(new Tuple<int, int>(i, j));
                    }
#if DEBUG
                    // has a side effect of checking that the loops are still loops
                    new Loop(alc1);
                    new Loop(alc2);
#endif
                }
            }

            Dictionary<Curve, AnnotatedCurve> forward_annotations_map
                = new Dictionary<Curve, AnnotatedCurve>(
                    new ReferenceComparer<Curve>());

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

            HashSet<Curve> all_curves = new HashSet<Curve>(
                working_loops1.Values
                    .Concat(working_loops2.Values).SelectMany(x => x),
                new ReferenceComparer<Curve>());

            HashSet<AnnotatedCurve> open = new HashSet<AnnotatedCurve>(
                forward_annotations_map.Values,
                new ReferenceComparer<AnnotatedCurve>());

            HashSet<Vector2> curve_joints = new HashSet<Vector2>(all_curves.Select(c => c.StartPos));

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
                forward_annotations_map, all_curves, open, curve_joints,
                diameter,
                type))
            {
                return null;
            }

            // now find all the splices
            // did not do this in loops above, because of complexity of some of them crossing loop-ends and some of them
            // lying on existing curve boundaries
            // we do this after eliminating unwanted curves, because otherwise one curve can have two
            // successors (consider ---+===+---, where the ='s mean two lines in the same place)
            // which breaks the idea of endSpliceMap
            Dictionary<Curve, Splice> endSpliceMap = new Dictionary<Curve, Splice>(
                new ReferenceComparer<Curve>());

            if (splittings.Count > 0)
            {
                foreach (IList<Curve> alc1 in working_loops1.Values)
                {
                    foreach (IList<Curve> alc2 in working_loops2.Values)
                    {
                        FindSplices(alc1, alc2,
                              forward_annotations_map,
                              endSpliceMap,
                              open, forward_annotations_map);
                    }
                }

                // these two processes should touch the same set of loop-pairs
                HashSet<Tuple<int, int>> splicings = new HashSet<Tuple<int, int>>(endSpliceMap.Values.Select(x => new Tuple<int, int>(x.Loop1Out.LoopNumber, x.Loop2Out.LoopNumber)));
                Assertion.Assert(splittings.Union(splicings).Count() == splittings.Count);
            }

            while (open.Count > 0)
            {
                AnnotatedCurve ac_current = open.First();

                // take a loop that is part of the perimeter
                ret.Add(ExtractLoop(
                      open,
                      ac_current,
                      endSpliceMap));
            }

            // this would imply _everything_ was internal, which is impossible without
            // a dimension warp
            // we can get that, now we're considering -ve top-level curves, they can leave nothing in the
            // output if we have type == WantPositive (or the reverse for +ve and WantNegative)
            //Assertion.Assert(ret.Count > 0);

            return ret;
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

        private void ValidateInputs(LoopSet ls, string name)
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

        private void CheckLoopIntersection(Loop loop, Loop loop2, string name)
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
                    else if (GeomRepUtil.CurveCurveIntersect(c1, c2) != null)
                    {
                        throw new ArgumentException("Two loops intersects", name);
                    }
                }
            }
        }

        private void CheckSelfIntersection(Loop loop, string name)
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
                    // adjoining curves may intersect, but no others
                    else if (!c1.Adjoins(c2, 1e-4f)
                        && GeomRepUtil.CurveCurveIntersect(c1, c2) != null)
                    {
                        throw new ArgumentException("Curves in loop intersect", name);
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
            HashSet<AnnotatedCurve> seen = new HashSet<AnnotatedCurve>();
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

                    if (open.Contains(ac_intersecting))
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

        // non-private only for testing
        void RemoveEasyLoops(Dictionary<int, IList<Curve>> working_loops,
                             LoopSet ret,
                             ICollection<Box2> other_bounds,
                             Dictionary<IList<Curve>, Box2> bound_map)
        {
            List<int> keys = working_loops.Keys.ToList();

            foreach (int i in keys)
            {
                IList<Curve> alc1 = working_loops[i];

                Box2 bound = bound_map[alc1];

                bool hits = false;

                foreach (Box2 b in other_bounds)
                {
                    if (!bound.Disjoint(b))
                    {
                        hits = true;
                        break;
                    }
                }

                if (!hits)
                {
                    ret.Add(new Loop(alc1));
                    working_loops.Remove(i);
                    // won't need the bounds of this again, either
                    bound_map.Remove(alc1);
                }
            }
        }

        Loop ExtractLoop(HashSet<AnnotatedCurve> open,
                         AnnotatedCurve start_ac,
                         Dictionary<Curve, Splice> endSpliceMap)
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
                Splice splice;
                endSpliceMap.TryGetValue(c, out splice);

                // if no splice we just follow the chain of ACs
                if (splice == null)
                {
                    if (curr_ac.Next == start_ac)
                    {
                        break;
                    }

                    curr_ac = curr_ac.Next;
                }
                else
                {
                    if (splice.Loop1Out == start_ac
                          || splice.Loop2Out == start_ac)
                    {
                        break;
                    }

                    // at every splice, at least one of the two possible exits should be still open
                    Assertion.Assert(open.Contains(splice.Loop1Out) || open.Contains(splice.Loop2Out));

                    if (!open.Contains(splice.Loop1Out))
                    {
                        curr_ac = splice.Loop2Out;
                    }
                    else if (!open.Contains(splice.Loop2Out))
                    {
                        curr_ac = splice.Loop1Out;
                    }

                    // if both exit curves are still in open (happens with osculating circles)
                    // we need to take the one that puts us on a different loop
                    else if (curr_ac.LoopNumber != splice.Loop1Out.LoopNumber)
                    {
                        curr_ac = splice.Loop1Out;
                    }
                    else
                    {
                        curr_ac = splice.Loop2Out;
                    }
                }
            }

            // because input cyclic curves have a joint at 12 o'clock
            // and nothing before here removes that, we can have splits we don't need
            // between otherwise identical curves
            //
            // this merges those back together
            TidyLoop(found_curves);

            return new Loop(found_curves);
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

                Vector2 point = c.Pos(random.NextfloatRange(c.StartParam, c.EndParam));

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

        public void FindSplices(IList<Curve> working_loop1, IList<Curve> working_loop2,
                         Dictionary<Curve, AnnotatedCurve> forward_annotations_map,
                         Dictionary<Curve, Splice> endSpliceMap, HashSet<AnnotatedCurve> open,
                         Dictionary<Curve, AnnotatedCurve> forward_annotations_map1)
        {
            Curve l1prev = working_loop1.Last();

            foreach (Curve l1curr in working_loop1)
            {
                Vector2 l1_cur_start_pos = l1curr.StartPos;
                Assertion.Assert(l1prev.EndPos.Equals(l1_cur_start_pos, 1e-4f));

                Curve l2prev = working_loop2.Last();

                foreach (Curve l2curr in working_loop2)
                {
                    Vector2 l2_cur_start_pos = l2curr.StartPos;
                    Assertion.Assert(l2prev.EndPos.Equals(l2_cur_start_pos, 1e-4f));

                    Vector2 dist = l1_cur_start_pos - l2_cur_start_pos;

                    if (dist.magnitude < 1e-4f)
                    {
                        Splice s = new Splice(
                            forward_annotations_map[l1curr],
                            forward_annotations_map[l2curr]);

                        if (endSpliceMap.ContainsKey(l1prev)
                            || endSpliceMap.ContainsKey(l2prev))
                        {
                            // shouldn't happen but throwing this allows easy display of offending loops
                            throw new LoopDisplayException(new Loop(working_loop1), new Loop(working_loop2));
                        }

                        // we do not have "open" or "forward_annotations_map" in all unit tests
                        //
                        // no need to record anything for non-open curves
                        if (open == null
                            || forward_annotations_map == null
                            || open.Contains(forward_annotations_map[l1prev]))
                        {
                            endSpliceMap.Add(l1prev, s);
                        }
                        if (open == null
                            || forward_annotations_map == null
                            || open.Contains(forward_annotations_map[l2prev]))
                        {
                            endSpliceMap.Add(l2prev, s);
                        }
                    }

                    l2prev = l2curr;
                }

                l1prev = l1curr;
            }
        }

        // non-private only for unit-tests
        public bool SplitCurvesAtIntersections(
            IList<Curve> working_loop1, IList<Curve> working_loop2,
            float tol)
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
                            Tuple<float, float> split_points = ret[k];

                            float start_dist = c1.ParamCoordinateDist(c1.StartParam, split_points.Item1);
                            float end_dist = c1.ParamCoordinateDist(c1.EndParam, split_points.Item1);

                            // this is still an intersection, even if we do not have to add a split because it hits an existing one
                            intersection_count++;

                            // if we are far enough from existing splits
                            if (start_dist > tol && end_dist > tol)
                            {
                                any_splits = true;

                                Curve c1split1 = c1.CloneWithChangedParams(c1.StartParam, split_points.Item1);
                                Curve c1split2 = c1.CloneWithChangedParams(split_points.Item1, c1.EndParam);

                                working_loop1[i] = c1split1;
                                working_loop1.Insert(i + 1, c1split2);

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

                            // this is still an intersection, even if we do not have to add a split because it hits an existing one
                            intersection_count++;

                            start_dist = c2.ParamCoordinateDist(c2.StartParam, split_points.Item2);
                            end_dist = c2.ParamCoordinateDist(c2.EndParam, split_points.Item2);

                            // if we are far enough from existing splits
                            if (start_dist > tol && end_dist > tol)
                            {
                                any_splits = true;

                                Curve c2split1 = c2.CloneWithChangedParams(c2.StartParam, split_points.Item2);
                                Curve c2split2 = c2.CloneWithChangedParams(split_points.Item2, c2.EndParam);

                                working_loop2[j] = c2split1;
                                working_loop2.Insert(j + 1, c2split2);

                                // see comment in previous if-block
                                c2 = c2split1;
                            }
                        }
                    } while (any_splits);
                }
            }

            // we expect even numbers of crossings
            Assertion.Assert(intersection_count % 2 == 0);

            return intersection_count > 0;
        }

        // non-private only for unit-tests
        public bool SplitCurvesAtCoincidences(
            IList<Curve> working_loop1, IList<Curve> working_loop2,
            float tol)
        {
            bool any_found = false;

            for (int i = 0; i < working_loop1.Count; i++)
            {
                Curve c1 = working_loop1[i];
                for (int j = 0; j < working_loop2.Count; j++)
                {
                    Curve c2 = working_loop2[j];

                    var ret = c1.SplitCoincidentCurves(c2, tol);

                    if (ret == null)
                    {
                        break;
                    }

                    if (ret.Item1 != null)
                    {
                        any_found = true;

                        // once we've split once the new curves still need testing against the rest of the
                        // other loop, further splits could be in any new curve
                        //
                        // so all-in-all simplest seems to be to pretend the two earlier fragments were where we were
                        // all along and re-start this (c1, c2) pair using them

                        c1 = working_loop1[i] = ret.Item1[0];

                        for (int n_ins = 1; n_ins < ret.Item1.Count; n_ins++)
                        {
                            working_loop1.Insert(i + n_ins, ret.Item1[n_ins]);
                        }
                    }

                    if (ret.Item2 != null)
                    {
                        any_found = true;

                        // once we've split once the new curves still need testing against the rest of the
                        // other loop, further splits could be in any new curve
                        //
                        // so all-in-all simplest seems to be to pretend the two earlier fragments were where we were
                        // all along and re-start this (c1, c2) pair using them

                        c2 = working_loop2[i] = ret.Item2[0];

                        for (int n_ins = 1; n_ins < ret.Item2.Count; n_ins++)
                        {
                            working_loop2.Insert(i + n_ins, ret.Item2[n_ins]);
                        }
                    }
                }
            }

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

                if (prev != null)
                {
                    AnnotatedCurve ac_forward_prev = forward_annotations_map[prev];

                    ac_forward_prev.Next = ac_forward_curr;
                }

                forward_annotations_map.Add(curr, ac_forward_curr);

                prev = curr;
            }

            Curve first = curves[0];

            AnnotatedCurve ac_forward_first = forward_annotations_map[first];
            AnnotatedCurve ac_forward_last = forward_annotations_map[prev];

            ac_forward_last.Next = ac_forward_first;
        }

        sealed class ReferenceComparer<T> : IEqualityComparer<T>
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

    //      if (visualise)
    //      {
    //         Random r = new Random(1);
    //
    //         Main.clear(255);
    //         XY pnt = working_loops1.get(0).get(2).startPos();
    //         Area size = new Area(pnt.minus(new XY(.001, .001)),
    //               pnt.plus(new XY(.001, .001)));
    ////         engine.Area size = bounds;
    //         Main.scaleTo(size);
    //
    //         Main.fill(r.nextInt(256), r.nextInt(256), 256);
    //         for(Splice s : endSpliceMap.values())
    //         {
    //            Main.circle(s.Loop1Out.Curve.startPos().X,
    //                  s.Loop1Out.Curve.startPos().Y,
    //                  size.DX() * 0.004);
    //         }
    //
    //         for(List<Curve> alc1 : working_loops1.values())
    //         {
    //            Main.strokeWidth(size.DX() * 0.001);
    //            Main.stroke(256, r.nextInt(128), r.nextInt(128));
    //            Loop l = new Loop(alc1);
    //            Main.drawLoopPoints(l.facet(.3));
    //
    //            for(Curve c : l.getCurves())
    //            {
    //               XY end = c.endPos();
    //               Main.circle(end.X, end.Y, size.DX() * 0.002);
    //            }
    //         }
    //
    //         for (List<Curve> alc2 : working_loops2.values())
    //         {
    //            Main.stroke(r.nextInt(128), 256, r.nextInt(128));
    //            Loop l = new Loop(alc2);
    //            Main.drawLoopPoints(l.facet(.3));
    //
    //            for(Curve c : l.getCurves())
    //            {
    //               XY end = c.endPos();
    //               Main.circle(end.X, end.Y, size.DX() * 0.002);
    //            }
    //         }
    //      }

    //      // don't keep eating random numbers if we're visualising the same frame over and over
    //      if (visualise && m_visualisation_line != null)
    //      {
    //         Main.stroke(0, 0, 0);
    //         Main.line(m_visualisation_line.startPos(), m_visualisation_line.endPos());
    //
    //         return null;
    //      }

    //            if (visualise)
    //            {
    //               m_visualisation_line = lc;
    //               Main.stroke(0, 0, 0);
    //               Main.line(m_visualisation_line.startPos(), m_visualisation_line.endPos());
    //               return null;
    //            }

    // private static LineCurve m_visualisation_line;
}
