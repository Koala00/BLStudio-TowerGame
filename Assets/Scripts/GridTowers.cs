using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

/// <summary>
/// Grid behavior that controls the towers on the grid.
/// </summary>
/// createTower: Allows tower creation,
/// endTurn: Lets all towers shoot at the end of each turn.
[RequireComponent(typeof(HexBoard))]
[RequireComponent(typeof(GridPositionElements))]
public class GridTowers : MonoBehaviour, IHandleMissleHit
{
    public GameObject TowerPrefab;
    private GridPositionElements Positions;
    private HexBoard HexBoard;
    private int NumStartedShootings = 0;

    public void Start()
    {
        HexBoard = GetComponent<HexBoard>();
        Positions = GetComponent<GridPositionElements>();
    }

    // create a new tower at selected position
    public bool CreateTower(HexCoord position, int playerNumber)
    {
        if (!HexBoard.IsPositionOnBoard(position) // check borders
            || IsTowerAt(position) // check if tower does not already exists, before creating new tower
            || NumStartedShootings > 0)
            return false;

        GameObject newTower = (GameObject)Instantiate(TowerPrefab);
        newTower.transform.SetParent(transform);
        newTower.transform.localPosition = position.Position3d() * HexBoard.TileScale;
        newTower.transform.rotation = Quaternion.Euler(0, 90, 0); // point the turret to the right
        var towerControl = newTower.GetComponent<TowerControl>();
        towerControl.playerNumber = playerNumber;
        towerControl.SetColor(playerNumber);
        Positions.IncreasePositionControl(position, playerNumber);
        return true;
    }

    public void EndTurn()
    {
        AllTowersShoot();
    }

    private void AllTowersShoot()
    {
        foreach (TowerControl sourceTower in GetComponentsInChildren<TowerControl>())
        {
            var towersInReach = sourceTower.GetEnemyTowersInReach();
            if (towersInReach.Count() > 0)
            {
                NumStartedShootings++;
                int selectedTower = UnityEngine.Random.Range(0, towersInReach.Count() - 1);
                var targetTower = towersInReach.ElementAt(selectedTower);
                sourceTower.Shoot(targetTower);
            }
        }
    }

    private bool IsTowerAt(HexCoord position)
    {
        return Positions.IsTowerAt(position);
    }

    #region IHandleMissleHit

    public void HitByMissle()
    {
        NumStartedShootings--;
    }

    #endregion

    /*private TowerControl GetTowerAt(HexCoord position)
    {
        var position3d = position.Position3d();
        return GetComponentsInChildren<TowerControl>()
               .Where(tower => tower.transform.position == position3d)
               .FirstOrDefault();
    }*/
}
