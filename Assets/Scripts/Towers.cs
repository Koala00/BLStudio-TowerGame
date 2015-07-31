using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Towers : MonoBehaviour {

    private static List<TowerElement> towersList = new List<TowerElement>();

    void Start() { }
    void Update() { }

    public class TowerElement
    {
        public GameObject towerObject;
        public Vector3 coordinates;
    }

    // create a new tower at selected position
    public static void createTower (GameObject towerSample, Vector3 position)
    {
        // check if tower does not already exists, before creating new tower
        if (getTower(position) == null)
        {
            // Debug.Log("New Tower instance; position=" + position);
            GameObject newTower = (GameObject)GameObject.Instantiate(towerSample, position, towerSample.transform.rotation);
            newTower.SetActive(true);

            TowerElement tower = new TowerElement();
            tower.towerObject = newTower;
            tower.coordinates = position;

            towersList.Add(tower);
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
}
