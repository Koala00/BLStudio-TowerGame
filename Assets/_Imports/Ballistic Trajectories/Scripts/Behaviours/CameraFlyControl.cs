// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

// Directly controls a 'first person' camera using the W,A,S,D,Q,E keys or 
// mouse movement when the left or right mouse button is being pressed.
public class CameraFlyControl : MonoBehaviour
{
    public float positionInputSpeed = 20;   // The rotation mouse sensitivity
    public float angleInputSpeed = 200;     // The translation key sensitivity

    void Update()
    {
        if (Input.GetButton("MouseRight") || (Input.GetButton("MouseLeft") && 
            (Input.mousePosition.x > 210 || Input.mousePosition.y < Screen.height - 320)))
   {
            UpdateRotation();
            UpdatePosition();
        }
    }

    private void UpdateRotation()
    {
        float x = -Input.GetAxis("MouseZAxis");
        float y = Input.GetAxis("MouseXAxis");
        Vector3 eulerDelta = angleInputSpeed * Time.deltaTime * new Vector3(x, y, 0.0f);

        if (eulerDelta.sqrMagnitude > Mathf.Epsilon)
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler += eulerDelta;
            if (euler.x > 180)
            {
                if (euler.x < 271) euler.x = 271;
            }
            else
            {
                if (euler.x > 89) euler.x = 89;
            }
            euler.z = 0.0f;
            transform.rotation = Quaternion.Euler(euler);
        }
    }

    private void UpdatePosition()
    {
        float x = Input.GetAxis("KeyXAxis");
        float y = Input.GetAxis("KeyYAxis");
        float z = Input.GetAxis("KeyZAxis");

        Vector3 localPositionDelta = positionInputSpeed * Time.deltaTime * 
            new Vector3(x, y, z);
                
        Vector3 position = transform.position + transform.rotation * localPositionDelta;
        position.y = Mathf.Max(position.y, 1.8f);
                
        transform.position = position;
    }
}
