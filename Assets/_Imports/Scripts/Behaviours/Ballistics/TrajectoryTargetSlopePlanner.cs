// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using Ballistics;

// This trajectory planner solves for the initial velocity that leads to going from 
// initialPosition to targetPosition, hitting the targetPosition at the exact given 
// world-space targetSlope. targetSlope must be more negative that the slope from 
// initial to target position for a solution to exist.
public class TrajectoryTargetSlopePlanner : TrajectoryPlannerBase
{
    public float targetSlope;

    public override bool PlanTimeToTarget(Projectile3D projectile3D,
                                          Vector3 initialPosition,
                                          Vector3 targetPosition,
                                          ref float timeToTarget)
    {
        if (timeToTarget > 0) return false;
		
        PrincipalProjectile principalProjectile;
        PrincipalSpace3D principalSpace3D = PrincipalSpace3D.Create(projectile3D,
            initialPosition, targetPosition, out principalProjectile);

        Vector2 principalTargetPosition = principalSpace3D.ToPrincipalPosition(targetPosition);

        float slope = principalSpace3D.ToPrincipalSlope(targetSlope);
		
	    if (!float.IsNaN(slope))
		{
	        float newTimeToTarget = PrincipalTimePlanners.GetTimeToTargetRGivenTargetSlopeA(
	            principalProjectile, principalTargetPosition, slope);
		
	        if (newTimeToTarget > 0)
	        {
	            timeToTarget = newTimeToTarget;
	            return true;
	        }
		}

        return false;
    }

}
