using UnityEngine;
using System.Collections;
using System;

public class ExplodeWhenHit : MonoBehaviour, ICanBeHit
{

    public GameObject ExplosionPrefab;

    public void GotHit()
    {
        Instantiate(ExplosionPrefab, transform.position, transform.rotation);
    }

    void OnTriggerEnter(Collider other)
    {
        Instantiate(ExplosionPrefab, transform.position, transform.rotation);
        // Make the tower sink into the ground until only the turret shows out when it has no life.
        this.transform.Translate(new Vector3(0, -1.7f / (ConfigurationElements.tower_Life-1), 0));
    }
}
