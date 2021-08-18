using Assets.Generation.U;
using NUnit.Framework;
using UnityEngine;

class UtilTest
{
    [Test]
    public void SignedAngleTest()
    {
        var v1 = new Vector2(1, 0);
        var v2 = new Vector2(0, 1);
        var v3 = new Vector2(-1, 0);
        var v4 = new Vector2(0, -1);
        var v5 = new Vector2(10, 10);

        Assert.AreEqual(0, Util.SignedAngleDifference(v1, v1));
        Assert.AreEqual(0, Util.SignedAngleDifference(v2, v2));

        Assert.AreEqual(Mathf.PI / 2, Util.SignedAngleDifference(v2, v1));
        Assert.AreEqual(-Mathf.PI / 2, Util.SignedAngleDifference(v1, v2));

        Assert.AreEqual(Mathf.PI, Util.SignedAngleDifference(v3, v1));
        Assert.AreEqual(Mathf.PI, Util.SignedAngleDifference(v1, v3));
        Assert.AreEqual(Mathf.PI, Util.SignedAngleDifference(v2, v4));
        Assert.AreEqual(Mathf.PI, Util.SignedAngleDifference(v4, v2));

        Assert.AreEqual(-Mathf.PI / 2, Util.SignedAngleDifference(v4, v1));
        Assert.AreEqual(Mathf.PI / 2, Util.SignedAngleDifference(v1, v4));

        Assert.AreEqual(Mathf.PI / 4, Util.SignedAngleDifference(v5, v1));
        Assert.AreEqual(-Mathf.PI / 4, Util.SignedAngleDifference(v1, v5));

        Assert.AreEqual(-3 * Mathf.PI / 4, Util.SignedAngleDifference(v5, v3));
        Assert.AreEqual(3 * Mathf.PI / 4, Util.SignedAngleDifference(v3, v5));
    }
}
