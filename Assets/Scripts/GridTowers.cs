using UnityEngine;
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
        public int life = 10;
    }

    // create a new tower at selected position
    public static void createTower (GameObject towerSample, Vector3 position, int playerNumber)
    {
        // check borders
        if ((position.x >= ConfigurationElements.board_size_x) || (position.z >= ConfigurationElements.board_size_z))
            return;

        // check if tower does not already exists, before creating new tower
        if (getTower(position) == null)
        {
            Debug.Log("New Tower instance; position=" + position);
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
        }
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
                    if (GridPositionElements.calculateDistance(sourceTower.coordinates, targetTower.coordinates) <= ConfigurationElements.towers_reachDistance)
                        towersInReach.Add(targetTower);
                }
            }

            if (towersInReach.Count > 0)
            {
                int selectedTower = Random.Range(0, towersInReach.Count - 1);
                towersInReach[selectedTower].life -= 1;
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
}
