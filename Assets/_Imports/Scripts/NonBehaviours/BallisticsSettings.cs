// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

[System.Serializable]
public class BallisticsSettings
{
    // gravity's strength in m/s^2 (default: earth's 9.81 m/s^2)
    public Vector3 gravity = new Vector3(0, -9.81f, 0);
    
    // velocity towards gravitational pull to approach during a windless free fall               
    public float terminalVelocity = 35;

    // the constant wind velocity during a projectile's flight
    public Vector3 windVelocity = Vector3.zero;

    // Stiffness of the virtual spring rotating the current heading towards the velocity's direction
    public float rotationStiffness = 3.0f;

    // Strength at which to damping any rotating rotate. 1 means critically dampened.
    public float rotationDamping = 0.3f;
}
