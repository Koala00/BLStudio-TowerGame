// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

// A position controller that, given the difference between the current position and the 
// desired position, applies a maxAccelaration until it hits the maxVelocity as fast as
// possible, and then breaks with maxAcceleration as late as possible to come to a halt at 
// the desired position without overshoot. In a way, it acts like a critically damped spring, 
// but it feels more robotic and mechanical, as it only ever applies an acceleration of 
// -maxAcceleration, 0, or maxAcceleration, and nothing in between. When trying to use this 
// class to drive rotation towards some myTargetAngle, use Mathf.DeltaAngle(this.position, 
// myTargetAngle) instead of this.position-myTargetAngle for positionMinusTarget, as that will 
// make the rotation always go around the shortest arc.

[System.Serializable]
public class MechanicMotorDrive
{
    [HideInInspector]
    public float position = 0;              // Current position (input & output for Update())
    [HideInInspector]
    public float velocity = 0;              // Current velocity (input & output for Update())

    public float maxVelocity = 10;          // Clamp velocity between -maxVelocity and maxVelocity

    public float maxAcceleration = 10;      // Use either -maxAcceleration, 0 or maxAcceleration as acceleration

    public void Update(float deltaTime, float positionMinusTarget)
    {
        float timeAtZeroVelocity = Mathf.Abs(velocity / maxAcceleration);
        float errorAtZeroVelocity = positionMinusTarget + velocity * timeAtZeroVelocity;
        float deltaVelocity = -Mathf.Sign(errorAtZeroVelocity) * maxAcceleration * deltaTime;
        velocity = Mathf.Clamp(velocity + deltaVelocity, -maxVelocity, maxVelocity);
        float postError = positionMinusTarget + velocity * deltaTime;
        if (positionMinusTarget * postError < Mathf.Epsilon) // snap to target on potential overshoot
        {
            position -= positionMinusTarget;
            velocity = 0;
        }
        position += velocity * deltaTime;
    }
}

