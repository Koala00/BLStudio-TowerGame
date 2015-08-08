// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using Ballistics;

// A planner that plans the flight time from initialPosition to targetPosition with 
// 'curviness' as its additional parameter. Higher curviness numbers lead to higher 
// arcs. See PrincipalTimePlanners.GetTimeToTargetRGivenCurvinessH for more details.
public class TrajectoryCurvinessPlanner : TrajectoryPlannerBase
{
    public float curviness = .5f; // The ratio of the arcHeight of the trajectory
    // relative to the distance from initial to target position

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

        timeToTarget = PrincipalTimePlanners.GetTimeToTargetRGivenCurvinessH(
            principalProjectile, principalTargetPosition, curviness);
        return true;
    }
}
