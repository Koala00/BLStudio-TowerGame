// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

// A simple user-input control behaviour allowing the gameobject it's attached to
// to move using the keys W,A,S,D,Q and E when NOT pressing the left or right mouse button.
public class TargetMoveControl : MonoBehaviour
{
    public float positionInputSpeed = 10.0f;

    void FixedUpdate()
    {
        if (!Input.GetButton("MouseRight") && !Input.GetButton("MouseLeft"))
        {
            Vector3 inputVector = new Vector3(Input.GetAxis("KeyXAxis"), Input.GetAxis("KeyYAxis"), Input.GetAxis("KeyZAxis"));
            inputVector /= inputVector.magnitude + Mathf.Epsilon;

            // Make the movement relative to the camera's orientation, so that when key A is pressed, for example,
            // it always causes movement to the right as seen from the current camera.
            Quaternion filteredCameraRotation = Quaternion.Euler(0.0f, Camera.main.transform.rotation.eulerAngles.y, 0.0f);
            Vector3 newPosition = transform.position + filteredCameraRotation * (Time.deltaTime * positionInputSpeed * inputVector);

            // Always stay above ground
            newPosition.y = Mathf.Max(newPosition.y, 0.0f);
            transform.position = newPosition;
        }
    }
}
