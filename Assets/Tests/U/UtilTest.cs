using Assets.Generation.U;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class UtilTest
{
    [Test]
    public void TestClockAwareRangeOverlap()
    {
        // neither range crosses midnight, no overlap
        Assert.IsNull(Util.ClockAwareRangeOverlap(1, 2, 3, 4, 1e-4f));
        Assert.IsNull(Util.ClockAwareRangeOverlap(3, 4, 1, 2, 1e-4f));

        // same but with an overlap at the end
        {
            var res = Util.ClockAwareRangeOverlap(1, 3, 2, 4, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(2, 3).Equals(res[0], 1e-4f));
        }

        {
            var res = Util.ClockAwareRangeOverlap(2, 4, 1, 3, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(2, 3).Equals(res[0], 1e-4f));
        }

        // same but with an overlap inside one range
        {
            var res = Util.ClockAwareRangeOverlap(1, 4, 2, 3, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(2, 3).Equals(res[0], 1e-4f));
        }

        {
            var res = Util.ClockAwareRangeOverlap(2, 3, 1, 4, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(2, 3).Equals(res[0], 1e-4f));
        }

        // one range crosses midnight, no overlap
        Assert.IsNull(Util.ClockAwareRangeOverlap(6, 2, 3, 4, 1e-4f));
        Assert.IsNull(Util.ClockAwareRangeOverlap(3, 4, 6, 2, 1e-4f));

        {
            var res = Util.ClockAwareRangeOverlap(6, 3, 2, 4, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(2, 3).Equals(res[0], 1e-4f));
        }

        {
            var res = Util.ClockAwareRangeOverlap(2, 4, 6, 3, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(2, 3).Equals(res[0], 1e-4f));
        }


        // we have an overlap anti-clockwise of midnight, not crossing it
        {
            var res = Util.ClockAwareRangeOverlap(5, 3, 4, 6, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(5, 6).Equals(res[0], 1e-4f));
        }

        {
            var res = Util.ClockAwareRangeOverlap(4, 6, 5, 3, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(5, 6).Equals(res[0], 1e-4f));
        }

        // we have an overlap clockwise of midnight, not crossing it
        {
            var res = Util.ClockAwareRangeOverlap(5, 3, 1, 2, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(1, 2).Equals(res[0], 1e-4f));
        }

        {
            var res = Util.ClockAwareRangeOverlap(1, 2, 5, 3, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(1, 2).Equals(res[0], 1e-4f));
        }

        // overlap crosses midnight
        {
            var res = Util.ClockAwareRangeOverlap(5, 3, 6, 2, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(6, 2).Equals(res[0], 1e-4f));
        }

        {
            var res = Util.ClockAwareRangeOverlap(6, 2, 5, 3, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(6, 2).Equals(res[0], 1e-4f));
        }

        // ranges meet at both ends, so two overlaps
        {
            var res = Util.ClockAwareRangeOverlap(4, 3, 1, 0, 1e-4f);

            Assert.AreEqual(2, res.Count);
            Assert.IsTrue(new Util.AngleRange(1, 3).Equals(res[0], 1e-4f)
                || new Util.AngleRange(4, 0).Equals(res[0], 1e-4f));
            Assert.IsTrue(new Util.AngleRange(1, 3).Equals(res[1], 1e-4f)
                || new Util.AngleRange(4, 0).Equals(res[1], 1e-4f));
        }

        {
            var res = Util.ClockAwareRangeOverlap(1, 0, 4, 3, 1e-4f);

            Assert.AreEqual(2, res.Count);
            Assert.IsTrue(new Util.AngleRange(1, 3).Equals(res[0], 1e-4f)
                || new Util.AngleRange(4, 0).Equals(res[0], 1e-4f));
            Assert.IsTrue(new Util.AngleRange(1, 3).Equals(res[1], 1e-4f)
                || new Util.AngleRange(4, 0).Equals(res[1], 1e-4f));
        }

        // if one of the ranges is a full circle
        {
            var res = Util.ClockAwareRangeOverlap(0, Mathf.PI * 2, 0, 1, 1e-4f); 

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(0, 1).Equals(res[0], 1e-4f));
        }

        {
            var res = Util.ClockAwareRangeOverlap(0, 1, 0, Mathf.PI * 2, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(0, 1).Equals(res[0], 1e-4f));
        }

        // or both are
        {
            var res = Util.ClockAwareRangeOverlap(0, Mathf.PI * 2, 0, Mathf.PI * 2, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(0, Mathf.PI * 2).Equals(res[0], 1e-4f));
        }

        // even if the second range crosses midnight
        {
            var res = Util.ClockAwareRangeOverlap(0, Mathf.PI * 2, 5, 1, 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new Util.AngleRange(5, 1).Equals(res[0], 1e-4f));
        }
    }
}
