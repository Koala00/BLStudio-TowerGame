using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public int NUM_PLAYERS = 2;

    public GameObject marker;
    public GameObject towerSample;
    public GameObject PlayerNameLabel;
    private int CurrentPlayer = 0;

    private Color[] PlayerColors = { Color.magenta, Color.cyan, Color.red, Color.green };

    private HexPosition mouse = null;

    // Use this for initialization
    void Start()
    {
        HexPosition.setColor("Cursor", Color.blue, 1);
        HexPosition.setColor("Selectable", Color.green, 2);
        HexPosition.setColor("Selection", Color.yellow, 3);
        for (int i = 0; i < NUM_PLAYERS; i++)
          HexPosition.setColor("Player" + i , PlayerColors[i % PlayerColors.Length], 4 + i);
        HexPosition.Marker = marker;
        UpdatePlayerLabel();
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
                    //Towers.createTower(towerSample, mouse.getPosition(), CurrentPlayer);
                    Towers.createTower(towerSample, mouse.getPosition());
                    mouse.select("Player" + CurrentPlayer);
                    endTurn();
                }
            }
        }

        if (!isMouseInsideBoard)
        {
            if (mouse != null) mouse.unselect("Cursor");
            mouse = null;
        }
    }

    // endTurn
    public void endTurn ()
    {
        CurrentPlayer = (CurrentPlayer + 1) % NUM_PLAYERS;
        UpdatePlayerLabel();
        Towers.endTurn();
    }

    private void UpdatePlayerLabel()
    {
        PlayerNameLabel.GetComponent<UnityEngine.UI.Text>().text = "Player " + (CurrentPlayer + 1);
    }
}