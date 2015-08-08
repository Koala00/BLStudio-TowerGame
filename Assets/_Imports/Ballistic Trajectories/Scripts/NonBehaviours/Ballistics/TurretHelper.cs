// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using System.Collections;
using Ballistics;
using System.Collections.Generic;

namespace Ballistics
{
    // Controls the planning and launching of the given projectile. It uses all 
    // TrajectoryXXXPlanner behaviours that are attached to this gameobject to do the 
    // actual trajectory solving/planning.
    public abstract class TurretHelper
    {

        // Based on the incoming positions, velocities and timing information, this function
        // uses dead reckoning to work out the TrajectoryXXXPlanner inputs to plan plan the
        // next projectile's trajectory with. The planners are called from lo to high 
        // PlanNumber. If any (future) plan is available, it returns the initial velocity for
        // that plan. Otherwise, it returns Vector3.Zero. Furthermore, if a future plan is
        // found AND it allows launching AND the target is within reach AND 
        // minimumTimeToNextLaunch <= 0, then shouldLaunch = true.
        public static Vector3 UpdatePlan(ICollection<TrajectoryPlannerBase> planners,
                                         ProjectileKinematics projectileKinematics,
                                         Vector3 launcherPosition,
                                         Vector3 launcherVelocity,
                                         Vector3 targetPosition,
                                         Vector3 targetVelocity,
                                         float minimumTimeToNextLaunch,
                                         ref float lastFlightTime,
                                         out bool shouldLaunch)
        {
            // Work out where the launcher and the target are most likely to be when
            // minimumTimeToNextLaunch becomes zero again.
            minimumTimeToNextLaunch = Mathf.Max(0.0f, minimumTimeToNextLaunch);
            float timeToTarget = minimumTimeToNextLaunch + lastFlightTime;
            Vector3 predictedLauncherPosition = launcherPosition + launcherVelocity *
                                        minimumTimeToNextLaunch;
            Vector3 predictedTargetPosition = targetPosition + targetVelocity * timeToTarget;

            // Run the planners
            bool mayLaunch = RunPlanners(planners, projectileKinematics,
                predictedLauncherPosition, predictedTargetPosition, out lastFlightTime);

            // Calculate the projectile spawn velocity if a flight time estimate is available
            Vector3 projectileVelocity = Vector3.zero;
            if (lastFlightTime > 0)
            {
                Projectile3D projectile3D = projectileKinematics.Projectile3D;

                projectileVelocity =
                    projectile3D.GetInitialVelocityGivenRelativeTargetAndTime(
                    predictedTargetPosition - predictedLauncherPosition, lastFlightTime);
            }

            // Set shouldLaunch to true iff the planners presented a plan and 
            // the time is right.
            shouldLaunch = mayLaunch && minimumTimeToNextLaunch <= 0;
            return projectileVelocity;
        }

        // Get all the TrajectoryXXXPlanners, and sort them according to their PlanNumber
        public static ICollection<TrajectoryPlannerBase> SortPlanners(
            ICollection<TrajectoryPlannerBase> unsortedPlanners)
        {
            var result = new TrajectoryPlannerBase[unsortedPlanners.Count];
            if (unsortedPlanners.Count == 0)
            {
                throw new MissingComponentException("Tried to use a " +
                "TurretHelper on a gameobject without any TrajectoryPlanner");
            }

            foreach (var planner in unsortedPlanners)
            {
                if (planner.planNumber < 0 || planner.planNumber >= result.Length)
                {
                    throw new UnityException(
                        "Tried to use an out-of-bounds PlanNumber in a TrajectoryPlanner");
                }
                if (result[planner.planNumber] != null)
                {
                    throw new UnityException("Tried to use the same TrajectoryPlanner " +
                        "planNumber multiple times in a gameobject");
                }
                result[planner.planNumber] = planner;
            }

            return result;
        }

        // Call all planners in order and return the flightTime if at least one planner
        // found a solution. The return value is only true if the last planner that found a
        // plan had mayLaunch = true.
        private static bool RunPlanners(ICollection<TrajectoryPlannerBase> planners,
                                        ProjectileKinematics projectileKinematics,
                                        Vector3 predictedLauncherPosition,
                                        Vector3 predictedTargetPosition,
                                        out float flightTime)
        {
            Projectile3D projectile3D = projectileKinematics.Projectile3D;
            flightTime = 0;
            bool mayLaunch = false;
            foreach (TrajectoryPlannerBase planner in planners)
            {
                if (planner.PlanTimeToTarget(projectile3D, predictedLauncherPosition,
                    predictedTargetPosition, ref flightTime))
                {
                    mayLaunch = planner.mayLaunch;
                }
            }

            return mayLaunch;
        }

        // Draw an animated trajectory with the given parameters using Debug.DrawLines.
        public static void DebugDrawLastTrajectory(Trajectory3D trajectory3D,
                                                   float timeToTarget)
        {
            int numSegments = 20;
            int numSamples = numSegments * 2 + 4;
            float dt = timeToTarget / (numSamples - 4);
            float t = Time.timeSinceLevelLoad / (dt * 4);
            t = (t - Mathf.Floor(t)) * dt * 4 - dt * 4;
            Color black = new Color(0, 0, 0, 0.75f), white = new Color(1, 1, 1, 0.75f);

            for (int i = 0; i < numSamples; ++i)
            {
                float fromTime = Mathf.Clamp(t, 0.0f, timeToTarget);
                float toTime = Mathf.Clamp(t + dt, 0.0f, timeToTarget);
                Debug.DrawLine(trajectory3D.PositionAtTime(fromTime),
                               trajectory3D.PositionAtTime(toTime),
                               i % 4 < 2 ? black : white);
                t += dt;
            }
        }
    }
}
