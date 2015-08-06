// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

namespace Ballistics
{
    // Defines the trajectory in 3D world space given Projectile3D data and
    // the initial velocity v0. 
    [System.Serializable]
    public class Trajectory3D : Projectile3D
    {
        public Vector3 v0;                      // projectile3D velocity at t = 0
        public Vector3 p0;                      // projectile3D position at t = 0

        public Trajectory3D(float k, Vector3 vInfinity) :
            base(k, vInfinity)
        {
        }

        public Trajectory3D(float k, Vector3 vInfinity, Vector3 p0) :
            base(k, vInfinity)
        {
            this.p0 = p0;
        }

        public Trajectory3D(float k, Vector3 vInfinity, Vector3 p0, Vector3 v0) :
            base(k, vInfinity)
        {
            this.p0 = p0;
            this.v0 = v0;
        }

        public Trajectory3D(Projectile3D projectile3D) :
            base(projectile3D)
        {
        }

        public Trajectory3D(Projectile3D projectile3D, Vector3 p0) :
            base(projectile3D)
        {
            this.p0 = p0;
        }

        public Trajectory3D(Projectile3D projectile3D, Vector3 p0, Vector3 v0) :
            base(projectile3D)
        {
            this.p0 = p0;
            this.v0 = v0;
        }

        // The formula all the other math is based on: get the parameteric 
        // position p in world space at time t. Implements Equation 4 from the paper.
        public Vector3 PositionAtTime(float t)
        {
            float kt = k * t;
            return (v0 + vInfinity * kt) * t / (1 + kt) + p0;
        }

        // Use the analytical derivative of PositionAtTime at t to calculate v(t)
        // Implements Equation 5 from the paper.
        public Vector3 VelocityAtTime(float t)
        {
            float kt = k * t;
            float h = 1 + kt;
            return (v0 + kt * (2 + kt) * vInfinity) / (h * h);
        }

        // Get the maximum 'height' of the trajectory in the direction n, where n is 
        // normalized and upwards.   public float GetTimeAtMaximumInDirectionN(Vector3 normal)
        // Implements Equation 6 from the paper.
        public float GetTimeAtMaximumInDirectionN(Vector3 normal)
        {
            float ratio = Vector3.Dot(normal, v0) / Vector3.Dot(normal, vInfinity);
            if (ratio >= 0.0f) return 0.0f;
            else return (Mathf.Sqrt(1.0f - ratio) - 1) / k;
        }

        // Equivalent to GetTimeAtMaximumInDirectionN(Vector2.up), but more optimized.
        // Implements Equation 7 from the paper.
        public float GetTimeAtMaximumHeight()
        {
            float ratio = v0.y / vInfinity.y;
            if (ratio >= 0.0f) return 0.0f;
            else return (Mathf.Sqrt(1.0f - ratio) - 1) / k;
        }
    }
}