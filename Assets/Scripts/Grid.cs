using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public GameObject marker;
    public GameObject towerSample;

    private HexPosition mouse = null;

    // Use this for initialization
    void Start()
    {
        HexPosition.setColor("Cursor", Color.blue, 1);
        HexPosition.setColor("Selectable", Color.green, 2);
        HexPosition.setColor("Selection", Color.yellow, 3);
        HexPosition.setColor("Player1", Color.magenta, 4);
        HexPosition.setColor("Player2", Color.cyan, 5);
        HexPosition.Marker = marker;

        towerSample.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        bool isMouseInsideBoard = false;

        // Test mouse position + highlight pointed element with marker
        if (Input.mousePresent)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            if (hits.Length > 0)
            {
                isMouseInsideBoard = true;

                float minDist = float.PositiveInfinity;
                int min = 0;
                for (int i = 0; i < hits.Length; ++i)
                {
                    if (hits[i].distance < minDist)
                    {
                        minDist = hits[i].distance;
                        min = i;
                    }
                }

                // Debug.Log(pointedPosition.ToString() + "; mouse position=" + Input.mousePosition.ToString());
                Vector3 pointedPosition = hits[min].point;
                HexPosition newMouse = new HexPosition(pointedPosition);

                // update cursor position
                if (newMouse != mouse)
                {
                    if (mouse != null) { mouse.unselect("Cursor"); }
                    mouse = newMouse;
                    mouse.select("Cursor");
                }

                // add a new tower
                if (Input.GetMouseButtonDown(0))
                {
                    Towers.createTower(towerSample, mouse.getPosition());
                    mouse.select("Player1");
                }
            }
        }

        if (!isMouseInsideBoard)
        {
            if (mouse != null) mouse.unselect("Cursor");
            mouse = null;
        }
    }
}