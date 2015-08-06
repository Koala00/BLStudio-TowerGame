﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridTowers {

    private static List<TowerElement> towersList = new List<TowerElement>();

    void Start() { }
    void Update() { }

    public class TowerElement
    {
        public GameObject towerObject;
        public Vector3 coordinates;
        public int playerNumber;
        public int life = ConfigurationElements.tower_Life;
    }

    // create a new tower at selected position
    public static bool createTower (GameObject towerSample, Vector3 position, int playerNumber)
    {
        // check borders
        if (!Grid.checkElementInsideGrid(position))
            return false;

        // check if tower does not already exists, before creating new tower
        if (getTower(position) == null)
        {
            // Debug.Log("New Tower instance; position=" + position);
            GameObject newTower = (GameObject)GameObject.Instantiate(towerSample, position, towerSample.transform.rotation);
            newTower.SetActive(true);

            TowerElement tower = new TowerElement();
            tower.towerObject = newTower;
            tower.coordinates = position;
            tower.playerNumber = playerNumber;

            towersList.Add(tower);
            GridPositionElements.IncreasePositionControl(position, playerNumber);

            // put color on tile
            (new HexPosition(position)).select("Player" + playerNumber);

        } else
        {
            return false;
        }

        return true;
    }

    public static TowerElement getTower (Vector3 position)
    {
        foreach (TowerElement tower in towersList)
        {
            if (tower.coordinates.Equals(position))
            {
                return tower;
            }
        }

        return null;
    }

    public static void endTurn()
    {
        foreach (TowerElement sourceTower in towersList)
        {
            List<TowerElement> towersInReach = new List<TowerElement>();

            foreach (TowerElement targetTower in towersList)
            {   // count towers in reach = not same position and not same player number ; and distance < x)
                if (! ((sourceTower.coordinates.Equals(targetTower.coordinates)) || (sourceTower.playerNumber.Equals(targetTower.playerNumber))))
                {
                    if (calculateDistance(sourceTower.coordinates, targetTower.coordinates) <= ConfigurationElements.towers_reachDistance)
                        towersInReach.Add(targetTower);
                }
            }

            if (towersInReach.Count > 0)
            {
                int selectedTower = Random.Range(0, towersInReach.Count - 1);
                var targetTower = towersInReach[selectedTower];
                targetTower.life -= 1;
                var shootingTurret = sourceTower.towerObject.GetComponent<Turret>();
                shootingTurret.targetTransform = targetTower.towerObject.transform;
            }
        }

        // remove towers with no life left
        List<TowerElement> toRemove = new List<TowerElement>();

        foreach (TowerElement tower in towersList)
        {
            if (tower.life <= 0)
            {
                toRemove.Add(tower);
            }
        }

        foreach (TowerElement tower in toRemove)
        {
            Object.Destroy(tower.towerObject);

            HexPosition position = new HexPosition(tower.coordinates);
            position.unselect("Player" + tower.playerNumber);
            towersList.Remove(tower);
            GridPositionElements.DecreasePositionControl(tower.coordinates, tower.playerNumber);
        }
    }

    public static int calculateDistance(Vector3 position1, Vector3 position2)
    {
        var hexPos1 = new HexPosition(position1);
        var hexPos2 = new HexPosition(position2);
        return hexPos1.dist(hexPos2);
    }
}
