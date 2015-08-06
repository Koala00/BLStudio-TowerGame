// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using System.Collections;
using Ballistics;

// A behaviour that changes the local orientation to 'pitch' itself relative to its 
// parent (i.e. rotate around the local +X axis) towards the given direction in 
// DoUpdate(). DoUpdate() should be called manually every frame. Doing the update 
// manually makes it possible to guarantee that the pitch is modified using this 
// behaviour in the same frame (but just after) the new direction is calculated.
public class PitchMotorControl : MonoBehaviour
{
    public MechanicMotorDrive mechanicMotorDrive;

    public void DoUpdate(Vector3 direction, float deltaTime)
    {
        Vector3 localAimForwardAxis = transform.parent.InverseTransformDirection(direction);
        float pitchAngle = Mathf.Asin(-localAimForwardAxis.y) * Mathf.Rad2Deg;

        float positionMinusTarget = Mathf.DeltaAngle(pitchAngle, mechanicMotorDrive.position);
        mechanicMotorDrive.Update(deltaTime, positionMinusTarget);

        transform.localRotation = Quaternion.AngleAxis(mechanicMotorDrive.position, Vector3.right);
    }
}
