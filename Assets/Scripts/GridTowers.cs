using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Grid behavior that controls the towers on the grid.
/// </summary>
/// createTower: Allows tower creation,
/// endTurn: Lets all towers shoot at the end of each turn.
public class GridTowers : MonoBehaviour
{
    public GameObject TowerPrefab;
    
    // create a new tower at selected position
    public bool createTower(Vector3 position, int playerNumber)
    {
        // check borders
        if (!Grid.checkElementInsideGrid(position))
            return false;
        // check if tower does not already exists, before creating new tower
        if (IsTowerAt(position))
            return false;

        GameObject newTower = (GameObject)Instantiate(TowerPrefab, position, TowerPrefab.transform.rotation);
        newTower.transform.SetParent(transform);
        var towerControl = newTower.GetComponent<TowerControl>();
        towerControl.playerNumber = playerNumber;

        GridPositionElements.IncreasePositionControl(position, playerNumber);

        // put color on tile
        (new HexPosition(position)).select("Player" + playerNumber);

        return true;
    }
    
    public void endTurn()
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
                int selectedTower = Random.Range(0, towersInReach.Count() - 1);
                var targetTower = towersInReach.ElementAt(selectedTower);
                sourceTower.Shoot(targetTower);
            }
        }
    }

    private bool IsTowerAt(Vector3 position)
    {
        return GetTowerAt(position) != null;
    }

    private TowerControl GetTowerAt(Vector3 position)
    {
        return GetComponentsInChildren<TowerControl>()
               .Where(tower => tower.transform.position == position)
               .FirstOrDefault();
    }
}
