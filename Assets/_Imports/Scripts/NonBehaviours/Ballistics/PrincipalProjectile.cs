// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

namespace Ballistics
{
    // Contains the parameters specifying the aerodynamic, wind, and gravity effects in 
    // a convenient format in principal space. It's similar to Projectile3D, but works 
    // in a differently defined space. The easiest way to create a PrincipalProjectile 
    // from world space parameters is to use principalSpace3D.Create().
    public class PrincipalProjectile
    {
        public float k;                 // drag constant of projectile through air
        public float vInfinity;			// velocity of projectile falling indefinitely

        public PrincipalProjectile(PrincipalProjectile principalProjectile)
        {
            k = principalProjectile.k;
            vInfinity = principalProjectile.vInfinity;
        }

        public PrincipalProjectile(float k, float vInfinity)
        {
            this.k = k;
            this.vInfinity = vInfinity;
        }

        // Calculate the initial velocity for this PrincipalProjectile that results in
        // hitting the given target position (in principal space) at the given time.
        // Implements Equation 21 and 22 from the paper.
        public Vector2 GetInitialVelocityGivenRelativeTargetAndTime(
            Vector2 relativeTargetPosition, float timeToTarget)
        {
            Vector2 v0 = relativeTargetPosition * (k + 1 / timeToTarget);
            v0.y += k * vInfinity * timeToTarget;
            return v0;
        }
    }
}
