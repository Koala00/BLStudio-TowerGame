// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

namespace Ballistics
{
    // Contains the parameters specifying the aerodynamic, wind, and gravity effects 
    // in a convenient format in world space. It's similar to PrincipalProjectile3D, 
    // but assumes a differently defined space. The easiest way to create a 
    // Projectile3D from more easy-to-use parameters is to use Projectile3D.Create().
    [System.Serializable]
    public class Projectile3D
    {
        public float k;                 // drag constant of projectile through air
        public Vector3 vInfinity;		// velocity of projectile falling indefinitely

        public Projectile3D(float k, Vector3 vInfinity)
        {
            this.k = k;
            this.vInfinity = vInfinity;
        }

        public Projectile3D(Projectile3D projectile3D)
        {
            this.k = projectile3D.k;
            this.vInfinity = projectile3D.vInfinity;
        }

        // Create a Projectile3D from convenient world space parameters
        // Implements Equation 1 and 2 from the paper.
        public static Projectile3D Create(Vector3 gravity,
                                          Vector3 windVelocity,
                                          float terminalVelocity)
        {
            float gravityLength = gravity.magnitude;
			float k = 0.5f * gravityLength / terminalVelocity;
			Vector3 vInfinity = gravity * (terminalVelocity / gravityLength) + windVelocity;
            return new Projectile3D(k, vInfinity);
        }

        // Calculate the initial velocity for this Projectile3D that results in
        // hitting the given target relative to the launch position at the given time.
        // Implements Equation 8 from the paper.
        public Vector3 GetInitialVelocityGivenRelativeTargetAndTime(
            Vector3 relativeTargetPosition, float timeToTarget)
        {
            return k * (relativeTargetPosition - vInfinity * timeToTarget) + 
                   relativeTargetPosition / timeToTarget;
        }
    }
}