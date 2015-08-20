using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Tower behavior (shooting on command, taking damage, dying).
/// </summary>
class TowerControl: MonoBehaviour, IHandleMissleLaunched
{
    public int playerNumber;
    public int life;
    public GameObject ExplosionHit;
    public GameObject ExplosionDestroyed;
    private GameObject Board;
    private HexCoord Position
    {
        get
        {
            if (!_Position.HasValue)
                _Position = HexCoord.AtPosition3d(transform.localPosition / HexBoard.TileScale);
            return _Position.Value;
        }
    }
    private HexCoord? _Position;



    void Start()
    {
        Board = transform.parent.gameObject;
        life = GameRuleSettings.Instance.Tower.Hitpoints;
    }

    public IEnumerable<TowerControl> GetEnemyTowersInReach()
    {
        Transform towerContainer = transform.parent;
        var towersInReach = new List<TowerControl>();
        var settings = GameRuleSettings.Instance.Tower;
        var allTtowers = towerContainer.GetComponentsInChildren<TowerControl>();
        foreach (TowerControl targetTower in allTtowers)
        {
            // count towers in reach = not same position and not same player number ; and distance < x)
            if (transform.position != targetTower.transform.position
                && playerNumber != targetTower.playerNumber
                && CalculateDistance(Position, targetTower.Position) <= settings.ShootingDistance)
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

    /// <summary>
    /// Colors the tower base in the player's color.
    /// </summary>
    /// <param name="player"></param>
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
        InformBoardAboutHit();
    }

    private void InformBoardAboutHit()
    {
        ExecuteEvents.ExecuteHierarchy<IHandleMissleHit>(Board, null, (msg, data) => msg.HitByMissle());
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
        var gridPositionElements = Board.GetComponent<GridPositionElements>();
        gridPositionElements.DecreasePositionControl(Position, towerControl.playerNumber);
        gridPositionElements.UpdateColors();
    }

    private static int CalculateDistance(HexCoord position1, HexCoord position2)
    {
        return HexCoord.Distance(position1, position2);
    }

    public void LaunchedMissle()
    {
        MakeShootingSound();
        InformBoardAboutLaunch();
    }

    private void InformBoardAboutLaunch()
    {
        ExecuteEvents.ExecuteHierarchy<IHandleMissleLaunched>(Board, null, (msg, data) => msg.LaunchedMissle());
    }

    private void MakeShootingSound()
    {
        var detonator = transform.GetComponentInChildren<Detonator>();
        detonator.Explode();
    }
}
