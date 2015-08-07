using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Tower behavior (shooting on command, taking damage, dying).
/// </summary>
class TowerControl: MonoBehaviour
{
    public int playerNumber;
    public int life = ConfigurationElements.tower_Life;
    public GameObject ExplosionPrefab;

    public IEnumerable<TowerControl> GetEnemyTowersInReach()
    {
        Transform towerContainer = transform.parent;
        var towersInReach = new List<TowerControl>();

        var allTtowers = towerContainer.GetComponentsInChildren<TowerControl>();
        foreach (TowerControl targetTower in allTtowers)
        {   // count towers in reach = not same position and not same player number ; and distance < x)
            if (transform.position != targetTower.transform.position
                && playerNumber != targetTower.playerNumber
                && calculateDistance(transform.position, targetTower.transform.position) <= ConfigurationElements.towers_reachDistance)
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
    }

    public void SetColor(int player)
    {
        var renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        renderer.material.color = ConfigurationElements.players_lerpedColor[player];
    }

    // Tower got hit?
    void OnTriggerEnter(Collider other)
    {
        Explode();
        ReduceTowerHeight();
        ReduceLifeAndDestroyIfZero();
    }

    private void ReduceLifeAndDestroyIfZero()
    {
        life -= 1;
        if (life <= 0)
            Destroy();
    }

    private void ReduceTowerHeight()
    {
        // Make the tower sink into the ground until only the turret shows out when it has no life.
        this.transform.Translate(new Vector3(0, -1.7f / (ConfigurationElements.tower_Life - 1), 0));
    }

    private void Explode()
    {
        Instantiate(ExplosionPrefab, transform.position, transform.rotation);
    }

    private void Destroy()
    {
        // Destroy tower with delay.
        Destroy(gameObject, .5f);
        HexPosition position = new HexPosition(transform.position);
        var towerControl = GetComponent<TowerControl>();
        position.unselect("Player" + towerControl.playerNumber);
        GridPositionElements.DecreasePositionControl(transform.position, towerControl.playerNumber);
    }

    private static int calculateDistance(Vector3 position1, Vector3 position2)
    {
        var hexPos1 = new HexPosition(position1);
        var hexPos2 = new HexPosition(position2);
        return hexPos1.dist(hexPos2);
    }

}
