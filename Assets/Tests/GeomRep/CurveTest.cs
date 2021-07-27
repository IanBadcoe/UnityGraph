//using Assets.Generation.GeomRep;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace Assets.Tests.GeomRep
//{
//    class TestCurve : Curve
//    {
//        TestCurve(float start_param, float end_param)
//            : base(start_param, end_param)
//        {
//        }

//        protected override Vector2 ComputePos_Inner(float param)
//        {
//            throw new NotImplementedException();
//        }

//        protected override float FindParamForPoint_Inner(Vector2 pnt)
//        {
//            throw new NotImplementedException();
//        }

//        public override Curve CloneWithChangedParams(float start, float end)
//        {
//            return null;
//        }

//        public Area BoundingBox()
//        {
//            return null;
//        }

//        public override Vector2 Tangent(float param)
//        {
//            throw new NotImplementedException();
//        }

//        public Curve Merge(Curve c_after)
//        {
//            return null;
//        }

//        public float length()
//        {
//            return 0;
//        }

//        public override Vector2 ComputeNormal(float p)
//        {
//            throw new NotImplementedException();
//        }

//        public override int GetHashCode()
//        {
//            return 0;
//        }

//        public override bool Equals(object o)
//        {
//            return false;
//        }
//    }


//    [Test]
//    public void TestCtor()
//    {
//        bool thrown = false;

//        try
//        {
//            new TestCurve(0, -1);
//        }
//        catch (UnsupportedOperationException e)
//        {
//            thrown = true;
//        }

//        Assert.IsTrue(thrown);
//    }
//}
