// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

namespace Ballistics
{
    // Represents the mapping/transform between principal space and world space. Use this
    // to convert position, velocity and orientation data from one space into the other.
    // The easiest way to create a principalSpace3D is to use Create().
    public class PrincipalSpace3D
    {
        public Vector3 p0;       // position at t = 0 in world space
        public Vector3 xAxis;    // direction perpendicular to vInfinity in world space
        public Vector3 yAxis;    // direction opposite to vInfinity in world space

        // When the principal space's axes and origin is already known, create a 
        // principalSpace3D from that.
        public PrincipalSpace3D(Vector3 p0, Vector3 xAxis, Vector3 yAxis)
        {
            this.p0 = p0;
            this.xAxis = xAxis;
            this.yAxis = yAxis;
        }

        // Create a principalSpace3D and PrincipalTrajectory from a given Trajectory3D
        public static PrincipalSpace3D Create(
            Trajectory3D trajectory3D,                      // trajectory in world space
            out PrincipalTrajectory principalTrajectory)    // in principal space
        {
            PrincipalProjectile principalProjectile;

            // The following position should always lie on the principal plane as well
            Vector3 worldP1 = trajectory3D.p0 + trajectory3D.v0; 

            PrincipalSpace3D principalSpace3D = Create(trajectory3D, trajectory3D.p0, 
                worldP1, out principalProjectile);

            Vector2 principalVelocity = principalSpace3D.ToPrincipalVelocity(trajectory3D.v0);
            principalTrajectory = new PrincipalTrajectory(principalProjectile, principalVelocity);

            return principalSpace3D;
        }

        // Create a principalSpace3D and PrincipalTrajectory from a given Projectile3D, 
        // the launch position and another point on the plane (e.g. a 3d target).
		// Implements Equation 10 from the paper.		
        public static PrincipalSpace3D Create(
            Projectile3D projectile3D,      // projectile in world space
            Vector3 worldP0,  	            // the launch position
            Vector3 worldP1,                // any other position on the principal plane
            out PrincipalProjectile principalProjectile) // projectile in principal space
        {

			principalProjectile = new PrincipalProjectile(
                projectile3D.k, projectile3D.vInfinity.magnitude);
            Vector3 yAxis = projectile3D.vInfinity / -principalProjectile.vInfinity;

            Vector3 deltaPos = worldP1 - worldP0;
            Vector3 xAxis = deltaPos - Vector3.Dot(yAxis, deltaPos) * yAxis;
            float squaredXScale = xAxis.sqrMagnitude;
            if (squaredXScale == 0)
            {
                xAxis = Misc.GetAnyVector3PerpendicularTo(yAxis);
                squaredXScale = xAxis.sqrMagnitude;
            }
            xAxis /= Mathf.Sqrt(squaredXScale);

            return new PrincipalSpace3D(worldP0, xAxis, yAxis);
        }

        // Convert a velocity from world space to principal space
        // Implements Equation 13 from the paper.		
        public Vector2 ToPrincipalVelocity(Vector3 world3DVelocity)
        {
            return new Vector2(Vector3.Dot(world3DVelocity, xAxis), 
                               Vector3.Dot(world3DVelocity, yAxis));
        }

        // Convert a position from world space to principal space.
        // Implements Equation 11 from the paper.		
        public Vector2 ToPrincipalPosition(Vector3 world3DPosition)
        {
            return ToPrincipalVelocity(world3DPosition - p0);
        }

        // Convert a position from principal space to world space
        // Implements Equation 12 from the paper.		
        public Vector3 ToWorld3DPosition(Vector2 principalPosition)
        {
            return ToWorld3DVelocity(principalPosition) + p0;
        }

        // Convert a velocity from principal space to world space
        // Implements Equation 14 from the paper.		
        public Vector3 ToWorld3DVelocity(Vector2 principalVelocity)
        {
            return principalVelocity.x * xAxis + principalVelocity.y * yAxis;
        }

        // Convert a world space slope to a slope in principal space. Note that without
        // wind (and thus when yAxis is vertical), these two slopes are identical. If the slope
        // can't be map to a direction with x > 0 in principal space, return NaN.
        public float ToPrincipalSlope(float worldSlope)
        {
            float x = xAxis.y, y = yAxis.y;
            float w = worldSlope, s = 1 + worldSlope * worldSlope;   
            float denominator = s * y * y - w * w;
			float sxx = s * x * x;
            float squared = sxx + denominator;
			// Fail if the worldSlope 'cone' doesn't intersect the principal plane
			// (apart from through the origin), or if the slope has a negative x value 
			// in principal space, respectively.			
            if (squared <= 0 || (w * x < 0 && sxx > 1))
            {
                return float.NaN;
            }

            return (Mathf.Sign(y) * w * Mathf.Sqrt(squared) - s * x * y) / denominator;
        }

        // Convert a world space normal to a slope in principal space. Note that without
        // wind (and thus when yAxis is vertical), the two 'slopes' are equivalent. If the slope
        // can't be map to a direction with x > 0 in principal space, return NaN.
        public float ToPrincipalSlope(Vector3 direction)
        {
            return ToPrincipalSlope(direction.y /
                Mathf.Sqrt(direction.x * direction.x + direction.z * direction.z));
        }

        // Find the intersection line between the given plane and the principal plane. 
        // The resulting line is outputted here as the 2D vector of (a, b), which can be
        // used with PrincipalTimePlanners.GetTimeToTargetRGivenLineToTouch().
        public Vector2 ToPrincipalIntersectionLine(Plane plane)
        {
            float denom = Vector3.Dot(yAxis, plane.normal);
            float a = -Vector3.Dot(xAxis, plane.normal) / denom;
            float b = (plane.distance - Vector3.Dot(p0, plane.normal)) / denom;
            return new Vector2(a, b);
        }
    }
}