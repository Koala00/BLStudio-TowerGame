// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using System.Collections;
using Ballistics;

// A simple recoil behaviour that can be used on the barrel of a turret on start a kickback
// when StartRecoil() is called. This behaviour assumes that the local origin is the 
// rest position, and the recoil should take place in the local -z direction.
public class BarrelRecoil : MonoBehaviour
{
    public float maxRecoilDistance = 0.1f;      // max recoil kickback distance
    public float relativeRecoilSpeed = 1;       // recoil speed relative to projectile
    public float counterRecoilSpeed = 8;        // speed to neutral position after recoil

    public void StartRecoil(float projectileSpeed)
    {
        _lastInitialRecoilSpeed = relativeRecoilSpeed * projectileSpeed;
        _recoilStartTime = Time.timeSinceLevelLoad;
    }

    void Update()
    {
        float recoilTime = Time.timeSinceLevelLoad - _recoilStartTime;
        float recoilUndampedPosition = _lastInitialRecoilSpeed * recoilTime;

        float boundedRecoilPosition = maxRecoilDistance * 
            (1 - Mathf.Exp(-recoilUndampedPosition));

        float dampedRecoilPosition = boundedRecoilPosition *
            Mathf.Exp(-counterRecoilSpeed * recoilTime);

        transform.localPosition = new Vector3(0, 0, -dampedRecoilPosition);
    }

    private float _lastInitialRecoilSpeed;
    private float _recoilStartTime;
}
