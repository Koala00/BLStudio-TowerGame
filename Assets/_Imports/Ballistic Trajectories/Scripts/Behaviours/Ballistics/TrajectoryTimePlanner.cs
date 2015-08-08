// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using Ballistics;

// This trajectory planner that plans the flight time of a projectile from
// initialPosition to targetPosition that's equal to the 'timeToTarget' variable.
public class TrajectoryTimePlanner : TrajectoryPlannerBase
{
    public float timeToTarget = 5;

    // See the base class for an explanation of the parameters.
    public override bool PlanTimeToTarget(Projectile3D projectile3D,
                                          Vector3 initialPosition,
                                          Vector3 targetPosition,
                                          ref float timeToTarget)
    {
        // Don't override the timeToTarget if there's already another valid plan.
        if (timeToTarget > 0) return false;

        timeToTarget = this.timeToTarget;
        return true;
    }
}
