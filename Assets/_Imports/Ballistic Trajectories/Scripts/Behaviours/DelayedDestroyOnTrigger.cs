// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;
using Ballistics;

// When entering a trigger, this stops the given particleEmitter immediately, and does 
// a delayed destroy of the gameobject itself. This behaviour is particularly useful
// when a particlesystems needs some time to fade out before being destoyed along with
// the gameobject itself.
public class DelayedDestroyOnTrigger : MonoBehaviour
{
    public float secondsDelay = 3.0f;

    public ParticleEmitter particleEmitterToStop;

    void OnTriggerEnter(Collider other)
    {
        if (particleEmitterToStop != null)
        {
            particleEmitterToStop.emit = false;
        }

        Destroy(gameObject, secondsDelay);
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		gameObject.active = false;
#else
		gameObject.SetActive(false);
#endif
    }
}
