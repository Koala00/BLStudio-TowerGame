// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using System.Collections;

// When put on a GameObject with a ParticleAnimator, it accelerates the particles
// in the direction of the wind as specified in the current BallisticsSettings settings.
public class WindDrivenParticleVelocity : MonoBehaviour
{
    // the trail's own velocity on top of the global wind
    public Vector3 buoyancyVelocity = new Vector3(0, .33f, 0); 

    private Vector3 _position;
    private Vector3 _velocity;
    private Vector3 _terminalVelocity;
    private Vector3 _localPosition;
    private ParticleRenderer _particleRenderer;

    void Start()
    {
        BallisticsSettings globalBallisticsSettings = GlobalSettings.Instance.ballisticsSettings;
        _terminalVelocity = globalBallisticsSettings.windVelocity + buoyancyVelocity;

        _position = transform.position;
        _localPosition = transform.localPosition;

        var particleAnimator = GetComponent<ParticleAnimator>();
        particleAnimator.force = 0.5f * (1 / particleAnimator.damping - 1) * _terminalVelocity;

        _particleRenderer = GetComponent<ParticleRenderer>();
    }

    void FixedUpdate()
    {
        _velocity = (transform.parent.position - _position) / Time.deltaTime;
        _position = transform.parent.position;
    }

    void Update()
    {
        Vector3 emitVelocity = Vector3.Lerp(_velocity, _terminalVelocity, 0.7f);
        var particleEmitter = GetComponent<ParticleEmitter>();
        particleEmitter.worldVelocity = emitVelocity;
        particleEmitter.transform.position = transform.parent.TransformPoint(_localPosition) +
             0.5f * emitVelocity * _particleRenderer.velocityScale;
    }

}
