// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using System.Collections;
using Ballistics;

// Goes back and forth between the localPosition and -localPosition relative to its parent.
// The MechanicsMotorDrive controls the maximum speed and acceleration/deceleration to get
// to the next extreme as fast as possible.
public class BackAndForthKinematics : MonoBehaviour
{
    public MechanicMotorDrive mechanicMotorDrive;

    void Start()
    {
        mechanicMotorDrive.position = 1;
        _extremeLocalPosition = transform.localPosition;
        _extremeLocalRotation = transform.localRotation;
        _isMovingForth = false;
    }

    void FixedUpdate()
    {
        float targetPosition = _isMovingForth ? 1.0f : -1.0f;
        float positionMinusTarget = mechanicMotorDrive.position - targetPosition;
        mechanicMotorDrive.Update(Time.deltaTime, positionMinusTarget);

        transform.localPosition = _extremeLocalPosition * mechanicMotorDrive.position;

        transform.localRotation = Quaternion.Slerp(
            Quaternion.Inverse(_extremeLocalRotation), 
            _extremeLocalRotation, 
            0.5f + 0.5f * mechanicMotorDrive.position);

        if (targetPosition * mechanicMotorDrive.position == 1)
        {
            _isMovingForth = !_isMovingForth;
        }
    }

    private Vector3 _extremeLocalPosition;
    private Quaternion _extremeLocalRotation;
    private bool _isMovingForth;
}
