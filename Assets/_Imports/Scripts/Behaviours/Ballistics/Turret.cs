// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using System.Collections;
using Ballistics;
using System.Collections.Generic;

// An example implementation of an automatic turret with targetting, pitch and yaw 
// control and recoil. This classes indirectly uses most of the classes in this sample.
public class Turret : MonoBehaviour
{
    public Transform launchTransform;           // the muzzle child gameobject
    public Transform targetTransform;           // the gameobject to target
    public float remainingReloadTime;           // seconds to the next launch
    public float totalReloadTime;               // seconds between launches
    public GameObject launchProjectile;         // The projectile to spawn on launch
    public bool drawDebugTrajectoryPlan;        // show the 3D trajectory widget

    // If true, projectiles are launched in the exact current barrel direction with
    // the launch speed determined by the plan
    public bool forceProjectileBarrelAlignment; // align projectiles with barrel, even
                                                // if the barrel is misaligned with the
                                                // latest plan.

    void Start()
    {
        _projectileKinematics = launchProjectile.GetComponent<ProjectileKinematics>();
        _planners = TurretHelper.SortPlanners(
            GetComponents<TrajectoryPlannerBase>());

        _pitchMotorControl = GetComponentInChildren<PitchMotorControl>();
        _yawMotorControl = GetComponentInChildren<YawMotorControl>();
        _barrelRecoil = GetComponentInChildren<BarrelRecoil>();

        _aimForwardAxis = launchTransform.forward;
    }

    void FixedUpdate()
    {
        remainingReloadTime = Mathf.Max(remainingReloadTime - Time.deltaTime, 0.0f);

        UpdateTrackedPositionsAndVelocities();

        if (targetTransform == null)
            return;

        // get the latest plan for the latest data
        bool planIsReadyForLaunch;
        Vector3 absoluteProjectileVelocity = TurretHelper.UpdatePlan(
            _planners, _projectileKinematics, launchTransform.position, _velocity, 
            _targetPosition, _targetVelocity, remainingReloadTime, ref _plannedFlightTime,
            out planIsReadyForLaunch);

        // output debug widgets if requested
        if (drawDebugTrajectoryPlan)
        {
            Trajectory3D trajectory3D = new Trajectory3D(
                _projectileKinematics.Projectile3D, launchTransform.position +
                _velocity * remainingReloadTime, absoluteProjectileVelocity);

            TurretHelper.DebugDrawLastTrajectory(trajectory3D, 
                _plannedFlightTime);
        }

        // if a plan has been found, update the turret's aim
        bool hasPlan = absoluteProjectileVelocity.sqrMagnitude > 0;
        Vector3 relativeProjectileVelocity = absoluteProjectileVelocity -
            _velocity;

        if (hasPlan)
        {
            UpdateTurretAim(relativeProjectileVelocity);

            // if the plan and aim is good enough, and there's no more remaining
            // weapon reload time, then launch.
            bool stillImprovingAimAccuracy = UpdateTurretOrientation();
            if (planIsReadyForLaunch && !stillImprovingAimAccuracy &&
                Vector3.Dot(relativeProjectileVelocity, launchTransform.forward) > 0)
            {
                Launch(relativeProjectileVelocity);
            }
        }
    }

    // Launch the 'launchProjectile' from the barrel given the initial velocity
    private void Launch(Vector3 relativeProjectileVelocity)
    {
        float relativeProjectileSpeed = relativeProjectileVelocity.magnitude;

        if (forceProjectileBarrelAlignment)
        {
            relativeProjectileVelocity = launchTransform.forward *
                                         relativeProjectileSpeed;
        }

        Vector3 absoluteLaunchVelocity = relativeProjectileVelocity + _velocity;

        GameObject spawnedProjectile = (GameObject)GameObject.Instantiate(
            launchProjectile, launchTransform.position, Quaternion.identity);

        var projectileKinematics = spawnedProjectile.GetComponent<ProjectileKinematics>();
        projectileKinematics.velocity = absoluteLaunchVelocity;
        projectileKinematics.rotationTransform.rotation = launchTransform.rotation;

        remainingReloadTime = totalReloadTime;

        if (_barrelRecoil != null)
        {
            _barrelRecoil.StartRecoil(relativeProjectileSpeed);
        }
        targetTransform = null; // Forget target so we don't shoot again in in this round.
    }

    // Track movement of the turret and of the target to be able to make
    // future predictions on where to shoot from/to when the reloadtime is zero again.
    private void UpdateTrackedPositionsAndVelocities()
    {
        _velocity = (transform.position - _position) / Time.deltaTime;
        _position = transform.position;

        if (targetTransform != null)
        {
            _targetVelocity = (targetTransform.position - _targetPosition) / Time.deltaTime;
            _targetPosition = targetTransform.position;
        }
    }

    private void UpdateTurretAim(Vector3 relativeProjectileVelocity)
    {
        float relativeProjectileSpeed = relativeProjectileVelocity.magnitude;
        if (relativeProjectileSpeed > 0)
        {
            _aimForwardAxis = relativeProjectileVelocity / relativeProjectileSpeed;
        }
    }

    // given the current aim, update the gameobjects controlling the yaw (local +y axis)
    // and pitch (local +x) direction, respectively.
    private bool UpdateTurretOrientation()
    {
        if (_yawMotorControl != null)
        {
            _yawMotorControl.DoUpdate(_aimForwardAxis, Time.deltaTime);
        }

        if (_pitchMotorControl != null)
        {
            _pitchMotorControl.DoUpdate(_aimForwardAxis, Time.deltaTime);
        }

        float aimAccuracy = Vector3.Dot(launchTransform.forward, _aimForwardAxis);
        bool stillImprovingAimAccuracy = aimAccuracy > _previousAimAccuracy + Mathf.Epsilon;
        _previousAimAccuracy = aimAccuracy;
        return stillImprovingAimAccuracy;
    }

    // the list of planners also attached to this gameobject, sorted by planNumber
    private ICollection<TrajectoryPlannerBase> _planners;  

    // the kinematics settings of the projectile to launch
    private ProjectileKinematics _projectileKinematics; 

    // the behaviour of a child gameobject that's responsible for controlling the pitch
    // (that is, local +x axis rotation) of the turret.
    private PitchMotorControl _pitchMotorControl;

    // the behaviour of a child gameobject that's responsible for controlling the yaw
    // (that is, local +x yaw rotation) of the turret.
    private YawMotorControl _yawMotorControl; 

    // the behaviour that controls the recoil of the barrel, starting this animation
    // on command from this turret behaviour
    private BarrelRecoil _barrelRecoil;

    // a measure of the quality of the previous frame's aim
    private float _previousAimAccuracy;    

    private Vector3 _position;              // the current turret's position
    private Vector3 _velocity;              // the current turret's velocity
    private Vector3 _targetPosition;        // the current target's position
    private Vector3 _targetVelocity;        // the current target's velocity
    private Vector3 _aimForwardAxis;        // the latest direction to aim for
    private float _plannedFlightTime;       // the latest projectile's time-to-target
}
