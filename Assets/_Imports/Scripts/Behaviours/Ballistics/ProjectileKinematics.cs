// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using Ballistics;

// Behaviour that moves a projectile over an analytic trajectory from Ballistics.
// Additionally, it uses a stable and efficient custom rotation simulation
// that is only approximate but is still physically plausible.
public class ProjectileKinematics : MonoBehaviour
{
    // The ballistics settings that are used when useGlobalSettings = false
    public BallisticsSettings ballisticsSettings;

    // Determine to use either GlobalSettings.Instance.ballisticsSettings or the 
    // ballisticsSettings member.
    public bool useGlobalSettings = false;

    // If non-null, use the GameObject's own transform to set the position, and use
    // this rotationTransform to set the orientation. If null before Start() is called,
    // the GameObject's own transform is used to set both the position and the rotation.
    public Transform rotationTransform = null;

    // The current velocity. This is also used as the initial velocity before Start() is
    // called.
    public Vector3 velocity;

    // Return the Projectile3D settings that is used for this projectile.
    public Projectile3D Projectile3D
    {
        get
        {
            BallisticsSettings ballistics = UsedBallisticsSettings;
            return Projectile3D.Create(ballistics.gravity,
                                       ballistics.windVelocity,
                                       ballistics.terminalVelocity);
        }
    }

    // Start moving the projectile over the trajectory using UsedBallisticsSettings,
    // the current GameObject's position, the rotationTransform's orientation, and 
    // the velocity vector.
    public void Start()
    {
        BallisticsSettings ballistics = UsedBallisticsSettings;
        _time = 0.0f;
        _isActive = true;
        _trajectory3D = new Trajectory3D(Projectile3D, transform.position, velocity);
        _headingFrequency = Mathf.Sqrt(_trajectory3D.k) * ballistics.rotationStiffness;
        _headingDamping = ballistics.rotationDamping;
        _windVelocity = ballistics.windVelocity;

        if (rotationTransform == null) rotationTransform = transform;
        AdvanceTime(0.0f);
    }

    // Update the position, rotation and velocity for the current frame
    void FixedUpdate()
    {
        AdvanceTime(Time.deltaTime);
    }

    // Advance the position, rotation and velocity by the given amount of time
    private void AdvanceTime(float deltaTime)
    {
        if (_isActive)
        {
            _time += deltaTime;
            UpdatePosition();
            UpdateVelocity();
            UpdateRotation(deltaTime);
        }
    }

    // Update the position for the (new) current time
    private void UpdatePosition()
    {
        transform.position = _trajectory3D.PositionAtTime(_time);
    }

    // Update the velocity for the (new) current time
    private void UpdateVelocity()
    {
        velocity = _trajectory3D.VelocityAtTime(_time);
    }

    // Advance an efficient but approximate rotation simulation by deltaTime seconds
    private void UpdateRotation(float deltaTime)
    {
        Vector3 forward = rotationTransform.forward;
        Vector3 relativeVelocity = velocity - _windVelocity;
        Vector3 sin = Vector3.Cross(relativeVelocity, forward);
        float cos = Vector3.Dot(relativeVelocity, forward);

        // Convert the difference between velocity and heading in terms of sin and cos to an
        // approximation of this as an angle-times-axis rotational acceleration. This approximation 
        // is strictly conservative, steadily climbs up to 120 degrees, and then drops off 
        // to 0 again. The approximation is a bit like sin itself, but has an extremum not at
        // +/- 90 degrees but at 120 degrees. Consequently, it will force the headings of
        // of projectiles that are even more than 90 degrees off to self correct, while 
        // still avoiding any discontinuity (and thus unstability) at +180 and -180.
        Vector3 angularError = 3 * sin / (2 * relativeVelocity.magnitude + cos + Mathf.Epsilon);

        // Use an inherently stable implicit Euler integration to update _angularVelocity
        // and angularDelta. Note that the angular velocity and angularDelta are in world-space
        // (which is physically incorrect but faster to computer, and close enough for our 
        // purposes as there's virtually no need to consider spin here.
        float wt = _headingFrequency * deltaTime;
        _angularVelocity = (_angularVelocity - _headingFrequency * wt * angularError) /
                           (1 + wt * (2 * _headingDamping + wt));
        Vector3 angularDelta = _angularVelocity * deltaTime;

        // Conservatively and efficiently approximate the delta quaternion q = [
        // angularDelta.normalized() * sin(angularDelta.magnitude()/2), cos(angularDelta.magnitude()/2] 
        // by approximating tan(x/4) with x/4 and using the trigonometric identities 
        // cos(2x)=(1-tan(x)^2)/(1+tan(x)^2) and sin(2x)=(2*tan(x)^2)/(1+tan(x)^2). Note
        // that the result isn't normalized, but that's handled by Unity automatically.
        Quaternion q = new Quaternion(angularDelta.x, angularDelta.y, angularDelta.z,
                                      2.0f - 0.125f * angularDelta.sqrMagnitude);

        // Apply the rotation delta. Again, this is done (physically incorrectly) in world space.
        rotationTransform.rotation = q * rotationTransform.rotation;
    }

    // Return the ballisticsSettings used before/when calling Start()
    private BallisticsSettings UsedBallisticsSettings
    {
        get
        {
            return useGlobalSettings ? GlobalSettings.Instance.ballisticsSettings :
                                       ballisticsSettings;
        }
    }

    private Trajectory3D _trajectory3D; // the trajectory to following
    private Vector3 _angularVelocity;   // the current world-space angular velocity
    private Vector3 _windVelocity;      // the constaint wind velocity
    private float _headingFrequency;    // similar to a rotational 'spring' strength
    private float _headingDamping;      // the amount of rotation velocity damping
    private bool _isActive;             // true when updating the transform each frame
    private float _time;                // time elapsed since _isActive became true
}
