// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using Ballistics;

// This trajectory planner solves the flightTime from initialPosition to targetPosition
// with the minimal absolute initial speed. This minimal effort approach results in a 
// naturally looking arc for targets at all distances and heights.
public class TrajectoryMinimalInitialSpeedPlanner : TrajectoryPlannerBase
{
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

        timeToTarget = PrincipalTimePlanners.GetTimeToTargetRWithMinimalInitialSpeed(
            principalProjectile, principalTargetPosition);

        return true;
    }
}
