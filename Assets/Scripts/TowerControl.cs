using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Tower behavior (shooting on command, taking damage, dying).
/// </summary>
class TowerControl: MonoBehaviour, IHandleMissleLaunched
{
    public int playerNumber;
    public int life = GameRuleSettings.Instance.Tower.Hitpoints;
    public GameObject ExplosionHit;
    public GameObject ExplosionDestroyed;

    public IEnumerable<TowerControl> GetEnemyTowersInReach()
    {
        Transform towerContainer = transform.parent;
        var towersInReach = new List<TowerControl>();

        var allTtowers = towerContainer.GetComponentsInChildren<TowerControl>();
        foreach (TowerControl targetTower in allTtowers)
        {   // count towers in reach = not same position and not same player number ; and distance < x)
            if (transform.position != targetTower.transform.position
                && playerNumber != targetTower.playerNumber
                && calculateDistance(transform.position, targetTower.transform.position) <= GameRuleSettings.Instance.Tower.ShootingDistance)
            {
                towersInReach.Add(targetTower);
            }
        }
        return towersInReach;
    }

    public void Shoot(TowerControl targetTower)
    {
        var shootingTurret = GetComponent<Turret>();
        shootingTurret.targetTransform = targetTower.transform;
        // Note that it takes time to turn the turret before shooting. Therefore we do not play the shooting sound until the missle was launched.
        // For that we get an event IHandleMissleLaunched from the Turret script.
    }

    public void SetColor(int player)
    {
        var renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        renderer.material.color = GameRuleSettings.Instance.Player.lerpedColors[player];
    }

    // Tower got hit?
    void OnTriggerEnter(Collider other)
    {
        ReduceTowerHeight();
        ReduceLifeAndDestroyIfZero();
    }

    private void ReduceLifeAndDestroyIfZero()
    {
        life -= 1;
        if (life <= 0)
        {
            ExplodeDestroyed();
            Destroy();
        }
        else
            ExplodeHit();
    }

    private void ReduceTowerHeight()
    {
        // Make the tower sink into the ground until only the turret shows out when it has no life.
        this.transform.Translate(new Vector3(0, -1.7f / (GameRuleSettings.Instance.Tower.Hitpoints - 1), 0));
    }

    private void ExplodeHit()
    {
        var turret = transform.GetChild(1);
        Instantiate(ExplosionHit, turret.position, turret.rotation);
    }

    private void ExplodeDestroyed()
    {
        var turret = transform.GetChild(1);
        Instantiate(ExplosionDestroyed, turret.position, transform.rotation);
    }

    private void Destroy()
    {
        // Destroy tower with delay.
        Destroy(gameObject, .5f);
        var towerControl = GetComponent<TowerControl>();
        //HexPosition position = new HexPosition(transform.position);
        //position.unselect("Player" + towerControl.playerNumber);
        GridPositionElements.DecreasePositionControl(transform.position, towerControl.playerNumber);
    }

    private static int calculateDistance(Vector3 position1, Vector3 position2)
    {
        var hexPos1 = new HexPosition(position1);
        var hexPos2 = new HexPosition(position2);
        return hexPos1.dist(hexPos2);
    }

    public void Launched()
    {
        MakeShootingSound();
    }

    private void MakeShootingSound()
    {
        var detonator = transform.GetComponentInChildren<Detonator>();
        detonator.Explode();
    }
}
