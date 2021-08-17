using Assets.Generation.U;
using NUnit.Framework;
using UnityEngine;

class AngleRangeTest
{
    [Test]
    public void TestClockAwareRangeOverlap()
    {
        // neither range crosses midnight, no overlap
        Assert.IsNull(new AngleRange(1, 2).ClockAwareRangeOverlap(new AngleRange(3, 4), 1e-4f));
        Assert.IsNull(new AngleRange(3, 4).ClockAwareRangeOverlap(new AngleRange(1, 2), 1e-4f));

        // identities
        {
            var res = new AngleRange(1, 3).ClockAwareRangeOverlap(new AngleRange(1, 3), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(1, 3).Equals(res[0], 1e-4f));
        }

        // one range a truncation of the other
        {
            var res = new AngleRange(1, 2).ClockAwareRangeOverlap(new AngleRange(1, 3), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(1, 2).Equals(res[0], 1e-4f));
        }

        // one range a truncation of the other
        {
            var res = new AngleRange(1, 3).ClockAwareRangeOverlap(new AngleRange(1, 2), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(1, 2).Equals(res[0], 1e-4f));
        }

        // same but with an overlap at the end
        {
            var res = new AngleRange(1, 3).ClockAwareRangeOverlap(new AngleRange(2, 4), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(2, 3).Equals(res[0], 1e-4f));
        }

        {
            var res = new AngleRange(2, 4).ClockAwareRangeOverlap(new AngleRange(1, 3), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(2, 3).Equals(res[0], 1e-4f));
        }

        // same but with an overlap inside one range
        {
            var res = new AngleRange(1, 4).ClockAwareRangeOverlap(new AngleRange(2, 3), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(2, 3).Equals(res[0], 1e-4f));
        }

        {
            var res = new AngleRange(2, 3).ClockAwareRangeOverlap(new AngleRange(1, 4), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(2, 3).Equals(res[0], 1e-4f));
        }

        // one range crosses midnight, no overlap
        Assert.IsNull(new AngleRange(6, 2).ClockAwareRangeOverlap(new AngleRange(3, 4), 1e-4f));
        Assert.IsNull(new AngleRange(3, 4).ClockAwareRangeOverlap(new AngleRange(6, 2), 1e-4f));

        {
            var res = new AngleRange(6, 3).ClockAwareRangeOverlap(new AngleRange(2, 4), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(2, 3).Equals(res[0], 1e-4f));
        }

        {
            var res = new AngleRange(2, 4).ClockAwareRangeOverlap(new AngleRange(6, 3), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(2, 3).Equals(res[0], 1e-4f));
        }


        // we have an overlap anti-clockwise of midnight, not crossing it
        {
            var res = new AngleRange(5, 3).ClockAwareRangeOverlap(new AngleRange(4, 6), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(5, 6).Equals(res[0], 1e-4f));
        }

        {
            var res = new AngleRange(4, 6).ClockAwareRangeOverlap(new AngleRange(5, 3), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(5, 6).Equals(res[0], 1e-4f));
        }

        // we have an overlap clockwise of midnight, not crossing it
        {
            var res = new AngleRange(5, 3).ClockAwareRangeOverlap(new AngleRange(1, 2), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(1, 2).Equals(res[0], 1e-4f));
        }

        {
            var res = new AngleRange(1, 2).ClockAwareRangeOverlap(new AngleRange(5, 3), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(1, 2).Equals(res[0], 1e-4f));
        }

        // overlap crosses midnight
        {
            var res = new AngleRange(5, 3).ClockAwareRangeOverlap(new AngleRange(6, 2), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(6, 2).Equals(res[0], 1e-4f));
        }

        {
            var res = new AngleRange(6, 2).ClockAwareRangeOverlap(new AngleRange(5, 3), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(6, 2).Equals(res[0], 1e-4f));
        }

        // ranges meet at both ends, so two overlaps
        {
            var res = new AngleRange(4, 3).ClockAwareRangeOverlap(new AngleRange(1, 0), 1e-4f);

            Assert.AreEqual(2, res.Count);
            Assert.IsTrue(new AngleRange(1, 3).Equals(res[0], 1e-4f)
                || new AngleRange(4, 0).Equals(res[0], 1e-4f));
            Assert.IsTrue(new AngleRange(1, 3).Equals(res[1], 1e-4f)
                || new AngleRange(4, 0).Equals(res[1], 1e-4f));
        }

        {
            var res = new AngleRange(1, 0).ClockAwareRangeOverlap(new AngleRange(4, 3), 1e-4f);

            Assert.AreEqual(2, res.Count);
            Assert.IsTrue(new AngleRange(1, 3).Equals(res[0], 1e-4f)
                || new AngleRange(4, 0).Equals(res[0], 1e-4f));
            Assert.IsTrue(new AngleRange(1, 3).Equals(res[1], 1e-4f)
                || new AngleRange(4, 0).Equals(res[1], 1e-4f));
        }

        // if one of the ranges is a full circle
        {
            var res = new AngleRange(0, Mathf.PI * 2).ClockAwareRangeOverlap(new AngleRange(0, 1), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(0, 1).Equals(res[0], 1e-4f));
        }

        {
            var res = new AngleRange(0, 1).ClockAwareRangeOverlap(new AngleRange(0, Mathf.PI * 2), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(0, 1).Equals(res[0], 1e-4f));
        }

        // or both are
        {
            var res = new AngleRange(0, Mathf.PI * 2).ClockAwareRangeOverlap(new AngleRange(0, Mathf.PI * 2), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(0, Mathf.PI * 2).Equals(res[0], 1e-4f));
        }

        // even if the second range crosses midnight
        {
            var res = new AngleRange(0, Mathf.PI * 2).ClockAwareRangeOverlap(new AngleRange(5, 1), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(5, 1).Equals(res[0], 1e-4f));
        }
    }
}
