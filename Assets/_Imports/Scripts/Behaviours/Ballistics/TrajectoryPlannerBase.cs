// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using Ballistics;

// The class all trajectory planners are derived from, providing an ordering hint,
// a launch flag, and a time-to-target plan interface.
public abstract class TrajectoryPlannerBase : MonoBehaviour
{
    // Helper flag determining whether or not the planner is used for aiming only, or 
    // also for firing.
    public bool mayLaunch = true;

    // The planNumber determines the relative order of planning when multiple planners 
    // are executed in sorted order by a TurretHelper. Lower numbers result 
    // in earlier execution. The planNumbers attached to a gameobject should start at  
    // zero and form a sequence of subsequent numbers.
    public int planNumber = 0;

    // Plan a trajectory in terms of flight time from initialPosition and targetPosition 
    // using the settings in projectile3D. It's the responsibility of each implementated 
    // subclass to decide whether or not to override provided timeToTarget, making it 
    // possible to use the same interface for simple planners, backup planning and plan 
    // modifiers. The function returns true if it modified the timeToTarget. To convert 
    // the (new) timeToTarget to a 3D initial velocity use the projectile3D's
    // GetInitialVelocityGivenRelativeTargetAndTime() function. 
    public abstract bool PlanTimeToTarget(Projectile3D projectile3D,
                                          Vector3 initialPosition,
                                          Vector3 targetPosition,
                                          ref float timeToTarget);
}
