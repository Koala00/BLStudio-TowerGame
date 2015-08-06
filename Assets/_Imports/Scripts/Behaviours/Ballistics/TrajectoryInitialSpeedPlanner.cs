// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using Ballistics;

// A planner that tries to solve for the flight time from initialPosition to the 
// targetPosition given the exact initial launch speed. If initialSpeed is great enough 
// to even hit the target then two solutions will be available: one for a low arc, and 
// one for a high arc. For which the time-to-target will be returned is determined by 
// the highArc variable.
public class TrajectoryInitialSpeedPlanner : TrajectoryPlannerBase
{
    public float initialSpeed = 25;
    public bool highArc = false;

    public override bool PlanTimeToTarget(Projectile3D projectile3D,
                                          Vector3 initialPosition,
                                          Vector3 targetPosition,
                                          ref float timeToTarget)
    {
        // Don't override the timeToTarget if there's already another valid plan.
        if (timeToTarget > 0) return false;

        PrincipalProjectile principalProjectile;
        PrincipalSpace3D principalSpace3D = PrincipalSpace3D.Create(projectile3D,
            initialPosition, targetPosition, out principalProjectile);

        Vector2 principalTargetPosition = principalSpace3D.ToPrincipalPosition(targetPosition);

        float newTimeToTarget = PrincipalTimePlanners.GetTimeToTargetRGivenInitialSpeedS(
            principalProjectile, principalTargetPosition, initialSpeed, highArc);

        if (newTimeToTarget > 0)
        {
            timeToTarget = newTimeToTarget;

            return true;
        }

        return false;
    }

}
