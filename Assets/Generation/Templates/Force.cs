﻿using Assets.Generation.G;
using Assets.Generation.Gen;
using System;

namespace Assets.Generation.Templates
{
    public class Force
    {
        public readonly INode N1;
        public readonly INode N2;
        public readonly float TargetDist;
        public readonly float ForceMultiplier;

        public Force(INode n1, INode n2, float targetDist, float forceMultiplier)
        {
            N1 = n1;
            N2 = n2;
            TargetDist = targetDist;
            ForceMultiplier = forceMultiplier;
        }

        public double CalcEnergy(double dist)
        {
            float effective_radius = TargetDist + N1.Radius + N2.Radius;

            return RelaxerStepper_CG.EdgeEnergy(dist, effective_radius, effective_radius) * ForceMultiplier;
        }
    }
}