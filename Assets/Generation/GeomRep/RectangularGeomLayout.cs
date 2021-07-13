using Assets.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Generation.GeomRep
{
    public class RectangularGeomLayout : GeomLayout
    {
        private readonly Vector2 m_start;
        private readonly Vector2 m_end;
        private readonly float m_half_width;

        public RectangularGeomLayout(Vector2 start, Vector2 end, float half_width)
        {
            m_start = start;
            m_end = end;
            m_half_width = half_width;
        }

        public override Loop MakeBaseGeometry()
        {
            Vector2 dir = m_end - m_start;
            float length = dir.magnitude;
            dir = dir / length;

            Vector2 width_dir = dir.Rot270();
            Vector2 half_width = width_dir * m_half_width;

            Vector2 start_left = m_start + half_width;
            Vector2 start_right = m_start - half_width;
            Vector2 end_left = m_end + half_width;
            Vector2 end_right = m_end - half_width;

            List<Curve> curves = new List<Curve>();
            curves.Add(new LineCurve(start_left, dir, length));
            curves.Add(new LineCurve(end_left, -width_dir, m_half_width * 2));
            curves.Add(new LineCurve(end_right, -dir, length));
            curves.Add(new LineCurve(start_right, width_dir, m_half_width * 2));

            Debug.Assert(curves[0].EndPos().Equals(curves[1].StartPos(), 1e-4f));
            Debug.Assert(curves[1].EndPos().Equals(curves[2].StartPos(), 1e-4f));
            Debug.Assert(curves[2].EndPos().Equals(curves[3].StartPos(), 1e-4f));
            Debug.Assert(curves[3].EndPos().Equals(curves[0].StartPos(), 1e-4f));

            return new Loop(curves);
        }

        public override LoopSet MakeDetailGeometry()
        {
            return null;
        }
    }
}
