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
        Assert.IsNull(new AngleRange(-1, 2).ClockAwareRangeOverlap(new AngleRange(3, 4), 1e-4f));
        Assert.IsNull(new AngleRange(3, 4).ClockAwareRangeOverlap(new AngleRange(-1, 2), 1e-4f));

        {
            var res = new AngleRange(-1, 3).ClockAwareRangeOverlap(new AngleRange(2, 4), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(2, 3).Equals(res[0], 1e-4f));
        }

        {
            var res = new AngleRange(2, 4).ClockAwareRangeOverlap(new AngleRange(-1, 3), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(2, 3).Equals(res[0], 1e-4f));
        }


        // we have an overlap anti-clockwise of midnight, not crossing it
        {
            var res = new AngleRange(-2, 3).ClockAwareRangeOverlap(new AngleRange(-3, -1), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(-2, -1).Equals(res[0], 1e-4f));
        }

        {
            var res = new AngleRange(-3, -1).ClockAwareRangeOverlap(new AngleRange(-2, 3), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(-2, -1).Equals(res[0], 1e-4f));
        }

        // we have an overlap clockwise of midnight, not crossing it
        {
            var res = new AngleRange(-2, 3).ClockAwareRangeOverlap(new AngleRange(1, 2), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(1, 2).Equals(res[0], 1e-4f));
        }

        {
            var res = new AngleRange(1, 2).ClockAwareRangeOverlap(new AngleRange(-2, 3), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(1, 2).Equals(res[0], 1e-4f));
        }

        // overlap crosses midnight
        {
            var res = new AngleRange(-2, 3).ClockAwareRangeOverlap(new AngleRange(-1, 2), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(-1, 2).Equals(res[0], 1e-4f));
        }

        {
            var res = new AngleRange(-1, 2).ClockAwareRangeOverlap(new AngleRange(-1, 3), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(-1, 2).Equals(res[0], 1e-4f));
        }

        // ranges meet at both ends, so two overlaps
        {
            var res = new AngleRange(-2, 3).ClockAwareRangeOverlap(new AngleRange(1, 6), 1e-4f);

            Assert.AreEqual(2, res.Count);
            Assert.IsTrue(new AngleRange(1, 3).Equals(res[0], 1e-4f)
                || new AngleRange(-2, 6 - Mathf.PI * 2).Equals(res[0], 1e-4f));
            Assert.IsTrue(new AngleRange(1, 3).Equals(res[1], 1e-4f)
                || new AngleRange(-2, 6 - Mathf.PI * 2).Equals(res[1], 1e-4f));
            Assert.IsFalse(res[0].Equals(res[1], 1e-4f));
        }

        {
            var res = new AngleRange(1, 6).ClockAwareRangeOverlap(new AngleRange(-2, 3), 1e-4f);

            Assert.AreEqual(2, res.Count);
            Assert.IsTrue(new AngleRange(1, 3).Equals(res[0], 1e-4f)
                || new AngleRange(-2, 6 - Mathf.PI * 2).Equals(res[0], 1e-4f));
            Assert.IsTrue(new AngleRange(1, 3).Equals(res[1], 1e-4f)
                || new AngleRange(-2, 6 - Mathf.PI * 2).Equals(res[1], 1e-4f));
            Assert.IsFalse(res[0].Equals(res[1], 1e-4f));
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

        // even if the second range crosses midnight
        {
            var res = new AngleRange(0, Mathf.PI * 2).ClockAwareRangeOverlap(new AngleRange(-2, 1), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(-2, 1).Equals(res[0], 1e-4f));
        }

        // or both are
        {
            var res = new AngleRange(0, Mathf.PI * 2).ClockAwareRangeOverlap(new AngleRange(0, Mathf.PI * 2), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(0, Mathf.PI * 2).Equals(res[0], 1e-4f));
        }

        // --
        // spot-check a few with reverse ranges, which should make no difference...
        // --

        {
            var res = new AngleRange(1, 3).ClockAwareRangeOverlap(new AngleRange(3, 1), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(1, 3).Equals(res[0], 1e-4f));
        }

        // one range a truncation of the other
        {
            var res = new AngleRange(1, 2).ClockAwareRangeOverlap(new AngleRange(3, 1), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(1, 2).Equals(res[0], 1e-4f));
        }

        // one range a truncation of the other
        {
            var res = new AngleRange(2, 1).ClockAwareRangeOverlap(new AngleRange(3, 1), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(1, 2).Equals(res[0], 1e-4f));
        }

        // even if the second range crosses midnight
        {
            var res = new AngleRange(Mathf.PI * 2, 0).ClockAwareRangeOverlap(new AngleRange(-2, 1), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(-2, 1).Equals(res[0], 1e-4f));
        }

        // even if the second range crosses midnight
        {
            var res = new AngleRange(0, Mathf.PI * 2).ClockAwareRangeOverlap(new AngleRange(1, -2), 1e-4f);

            Assert.AreEqual(1, res.Count);
            Assert.IsTrue(new AngleRange(-2, 1).Equals(res[0], 1e-4f));
        }
    }

    [Test]
    public void TestFixAngleForRange()
    {
        AngleRange r = new AngleRange(1, 2);
        AngleRange rr = new AngleRange(2, 1);

        Assert.AreEqual(1, r.FixAngleForRange(1), 1e-5f);
        Assert.AreEqual(1, r.FixAngleForRange(1 + 2 * Mathf.PI), 1e-5f);
        Assert.AreEqual(1, r.FixAngleForRange(1 - 2 * Mathf.PI), 1e-5f);

        Assert.AreEqual(2, rr.FixAngleForRange(2), 1e-5f);
        Assert.AreEqual(2, rr.FixAngleForRange(2 + 2 * Mathf.PI), 1e-5f);
        Assert.AreEqual(2, rr.FixAngleForRange(2 - 2 * Mathf.PI), 1e-5f);

        Assert.AreEqual(1.5f, r.FixAngleForRange(1.5f), 1e-5f);
        Assert.AreEqual(1.5f, r.FixAngleForRange(1.5f + 2 * Mathf.PI), 1e-5f);
        Assert.AreEqual(1.5f, r.FixAngleForRange(1.5f - 2 * Mathf.PI), 1e-5f);

        Assert.AreEqual(1.5f, rr.FixAngleForRange(1.5f), 1e-5f);
        Assert.AreEqual(1.5f, rr.FixAngleForRange(1.5f + 2 * Mathf.PI), 1e-5f);
        Assert.AreEqual(1.5f, rr.FixAngleForRange(1.5f - 2 * Mathf.PI), 1e-5f);

        Assert.AreEqual(1.5f, r.FixAngleForRange(1.5f), 1e-5f);
        Assert.AreEqual(1.5f, r.FixAngleForRange(1.5f + 2 * Mathf.PI), 1e-5f);
        Assert.AreEqual(1.5f, r.FixAngleForRange(1.5f - 2 * Mathf.PI), 1e-5f);

        Assert.AreEqual(1.5f, rr.FixAngleForRange(1.5f), 1e-5f);
        Assert.AreEqual(1.5f, rr.FixAngleForRange(1.5f + 2 * Mathf.PI), 1e-5f);
        Assert.AreEqual(1.5f, rr.FixAngleForRange(1.5f - 2 * Mathf.PI), 1e-5f);

        // out of range, but within tolerance, should end up on the same whole rotation as the range
        Assert.AreEqual(0.9f, r.FixAngleForRange(0.9f, 0.2f), 1e-5f);
        Assert.AreEqual(0.9f, r.FixAngleForRange(0.9f + Mathf.PI * 2, 0.2f), 1e-5f);
        Assert.AreEqual(0.9f, r.FixAngleForRange(0.9f - Mathf.PI * 2, 0.2f), 1e-5f);

        Assert.AreEqual(0.9f, rr.FixAngleForRange(0.9f, 0.2f), 1e-5f);
        Assert.AreEqual(0.9f, rr.FixAngleForRange(0.9f + Mathf.PI * 2, 0.2f), 1e-5f);
        Assert.AreEqual(0.9f, rr.FixAngleForRange(0.9f - Mathf.PI * 2, 0.2f), 1e-5f);

        // out of range, but within tolerance, should end up on the same whole rotation as the range
        Assert.AreEqual(2.1f, r.FixAngleForRange(2.1f, 0.2f), 1e-5f);
        Assert.AreEqual(2.1f, r.FixAngleForRange(2.1f + Mathf.PI * 2, 0.2f), 1e-5f);
        Assert.AreEqual(2.1f, r.FixAngleForRange(2.1f - Mathf.PI * 2, 0.2f), 1e-5f);

        Assert.AreEqual(2.1f, rr.FixAngleForRange(2.1f, 0.2f), 1e-5f);
        Assert.AreEqual(2.1f, rr.FixAngleForRange(2.1f + Mathf.PI * 2, 0.2f), 1e-5f);
        Assert.AreEqual(2.1f, rr.FixAngleForRange(2.1f - Mathf.PI * 2, 0.2f), 1e-5f);

        // but outside the tolerance should not end up within it
        Assert.IsFalse(r.FixAngleForRange(2.3f, 0.2f) > 0.8f && r.FixAngleForRange(2.3f, 0.2f) < 1.2f);
        Assert.IsFalse(r.FixAngleForRange(0.7f, 0.2f) > 0.8f && r.FixAngleForRange(0.7f, 0.2f) < 1.2f);

        Assert.IsFalse(rr.FixAngleForRange(2.3f, 0.2f) > 0.8f && rr.FixAngleForRange(2.3f, 0.2f) < 1.2f);
        Assert.IsFalse(rr.FixAngleForRange(0.7f, 0.2f) > 0.8f && rr.FixAngleForRange(0.7f, 0.2f) < 1.2f);
    }

    [Test]
    public void TestInRange()
    {
        AngleRange r = new AngleRange(1, 2);
        AngleRange rr = new AngleRange(2, 1);

        Assert.IsTrue(r.InRange(1));
        Assert.IsTrue(r.InRange(2));
        Assert.IsTrue(r.InRange(1.5f));
        Assert.IsFalse(r.InRange(0.9f));
        Assert.IsFalse(r.InRange(2.1f));

        Assert.IsTrue(rr.InRange(1));
        Assert.IsTrue(rr.InRange(2));
        Assert.IsTrue(rr.InRange(1.5f));
        Assert.IsFalse(rr.InRange(0.9f));
        Assert.IsFalse(rr.InRange(2.1f));

        Assert.IsTrue(r.InRange(1 + Mathf.PI * 2));
        Assert.IsTrue(r.InRange(2 + Mathf.PI * 2));
        Assert.IsTrue(r.InRange(1.5f + Mathf.PI * 2));
        Assert.IsFalse(r.InRange(0.9f + Mathf.PI * 2));
        Assert.IsFalse(r.InRange(2.1f + Mathf.PI * 2));
        Assert.IsFalse(r.InRange(1 + Mathf.PI * 2, false));
        Assert.IsFalse(r.InRange(2 + Mathf.PI * 2, false));
        Assert.IsFalse(r.InRange(1.5f + Mathf.PI * 2, false));

        Assert.IsTrue(rr.InRange(1 + Mathf.PI * 2));
        Assert.IsTrue(rr.InRange(2 + Mathf.PI * 2));
        Assert.IsTrue(rr.InRange(1.5f + Mathf.PI * 2));
        Assert.IsFalse(rr.InRange(0.9f + Mathf.PI * 2));
        Assert.IsFalse(rr.InRange(2.1f + Mathf.PI * 2));
        Assert.IsFalse(rr.InRange(1 + Mathf.PI * 2, false));
        Assert.IsFalse(rr.InRange(2 + Mathf.PI * 2, false));
        Assert.IsFalse(rr.InRange(1.5f + Mathf.PI * 2, false));

        Assert.IsTrue(r.InRange(1 - Mathf.PI * 2));
        Assert.IsTrue(r.InRange(2 - Mathf.PI * 2));
        Assert.IsTrue(r.InRange(1.5f - Mathf.PI * 2));
        Assert.IsFalse(r.InRange(0.9f - Mathf.PI * 2));
        Assert.IsFalse(r.InRange(2.1f - Mathf.PI * 2));

        Assert.IsTrue(rr.InRange(1 - Mathf.PI * 2));
        Assert.IsTrue(rr.InRange(2 - Mathf.PI * 2));
        Assert.IsTrue(rr.InRange(1.5f - Mathf.PI * 2));
        Assert.IsFalse(rr.InRange(0.9f - Mathf.PI * 2));
        Assert.IsFalse(rr.InRange(2.1f - Mathf.PI * 2));

        AngleRange c = new AngleRange(0, Mathf.PI * 2);
        AngleRange rc = new AngleRange(Mathf.PI * 2, 0);

        for (int i = -3; i < 10; i++)
        {
            Assert.IsTrue(c.InRange(i));
            Assert.IsTrue(rc.InRange(i));
        }

        Assert.IsFalse(c.InRange(-1, false));
        Assert.IsFalse(c.InRange(7, false));

        Assert.IsFalse(rc.InRange(-1, false));
        Assert.IsFalse(rc.InRange(7, false));
    }
}
