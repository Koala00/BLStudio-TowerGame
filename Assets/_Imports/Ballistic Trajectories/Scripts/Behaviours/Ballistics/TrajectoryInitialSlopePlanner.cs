// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using Ballistics;

// This planner plans a new plan from initialPosition to targetPosition that results in
// an initial velocity that's angled exactly at the slopeRelativeBase defined in the
// slopeBaseTransform's space. If used when a plan already exists (i.e. when 
// timeToTarget > 0), the old plan is overwritten iff the previous plan's initial 
// velocity would be angled lower then the minimum defined by slopeRelativeBase.
public class TrajectoryInitialSlopePlanner : TrajectoryPlannerBase
{
    public Transform slopeBaseTransform;
    public float slopeRelativeToBase;

    public void Start()
    {
        if (slopeBaseTransform == null)
        {
            slopeBaseTransform = transform;
        }
    }

    public override bool PlanTimeToTarget(Projectile3D projectile3D,
                                          Vector3 initialPosition,
                                          Vector3 targetPosition,
                                          ref float timeToTarget)
    {
        Vector3 targetDir = slopeBaseTransform.TransformDirection(new Vector3(0, slopeRelativeToBase, 1));

        if (timeToTarget > 0)
        {
            Vector3 initialVelocity = 
                projectile3D.GetInitialVelocityGivenRelativeTargetAndTime(
                targetPosition - initialPosition, timeToTarget);

            Vector3 validAngleDirection = Vector3.Cross(targetDir, 
                slopeBaseTransform.right);

            if (Vector2.Dot(validAngleDirection, initialVelocity) > 0)
            {
                return false;
            }
        }

        PrincipalProjectile principalProjectile;
        PrincipalSpace3D principalSpace3D = PrincipalSpace3D.Create(projectile3D,
            initialPosition, targetPosition, out principalProjectile);

        Vector2 principalTargetPosition = principalSpace3D.ToPrincipalPosition(targetPosition);

        float slope = principalSpace3D.ToPrincipalSlope(targetDir);

        if (!float.IsNaN(slope))
        {
            float newTimeToTarget = PrincipalTimePlanners.GetTimeToTargetRGivenInitialSlopeA(
                principalProjectile, principalTargetPosition, slope);

            //////////////////
            //Vector3 initialVelocity2 =
            //        projectile3D.GetInitialVelocityGivenRelativeTargetAndTime(
            //        targetPosition - initialPosition, newTimeToTarget);
            //Debug.Log(initialVelocity2.y / Mathf.Sqrt(initialVelocity2.x * initialVelocity2.x + initialVelocity2.z * initialVelocity2.z));

            if (newTimeToTarget > 0)
            {
                timeToTarget = newTimeToTarget;

                return true;
            }
        }

        return false;
    }

}
