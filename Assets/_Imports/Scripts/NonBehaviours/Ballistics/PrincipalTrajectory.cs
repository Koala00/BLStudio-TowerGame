// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

namespace Ballistics
{
    // Defines the trajectory in principal space given the PrincipalProjectile data and
    // the initial velocity v0. Use principalSpace3D.Create() to create a 
    // PrincipalTrajectory from a World3DTrajectory.
    public class PrincipalTrajectory : PrincipalProjectile
    {
        public Vector2 v0;                      // projectile velocity at t = 0

        public PrincipalTrajectory(PrincipalProjectile principalProjectile) :
            base(principalProjectile)
        {
        }

        public PrincipalTrajectory(PrincipalProjectile principalProjectile, Vector2 v0) :
            base(principalProjectile)
        {
            this.v0 = v0;
        }

        // Get the position in principal space at time t.
 		// Implements Equation 9, 15 and 16 from the paper.
        public Vector2 PositionAtTime(float t)
        {
            float kt = k * t;
            return new Vector2(v0.x, v0.y - vInfinity * kt) * t / (1 + kt);
        }

        // get the (vertical) y value given the (horizontal) x value, where 
        // 0 <= x < v0.x/k. Implements Equation 18 from the paper. 
        public float PositionYAtX(float x)
        {
            float kx = k * x;
            return (kx * vInfinity / (kx - v0.x) + v0.y) * x / v0.x;
        }

        // Get the velocity in principal space at time t.
        // Implements parts of Equation 19 from the paper.
        public Vector2 VelocityAtTime(float t)
        {
            float kt = k * t;
            float h = 1 + kt;
            return new Vector2(v0.x, v0.y - kt * (2 + kt) * vInfinity) / (h * h);
        }

        // Get the maximum 'height' of the trajectory in the direction n, where n is 
        // normalized and upwards. Implements parts of Equation 19 from the paper.
        public float GetTimeAtMaximumInDirectionN(Vector2 n)
        {
            float ratio = Vector3.Dot(n, v0) / (n.y * vInfinity);
            if (ratio <= 0.0f) return 0.0f;
            else return (Mathf.Sqrt(1.0f + ratio) - 1) / k;
        }

        // Equivalent to GetTimeAtMaximumInDirectionN(Vector2.up), but more optimized.
        // Implements parts of Equation 19 from the paper.
        public float GetTimeAtMaximumHeight()
        {
            float ratio = v0.y / vInfinity;
            if (ratio <= 0.0f) return 0.0f;
            else return (Mathf.Sqrt(1.0f + ratio) - 1) / k;
        }

    }
}