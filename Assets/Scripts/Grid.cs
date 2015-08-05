using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class Grid : MonoBehaviour
{
    public GameObject sampleCellEmptyMarker;
    public GameObject sampleCellFullMarker;
    public GameObject sampleTower;
    public GameObject canvas;

    private HexPosition mouse = null;

    // Use this for initialization
    void Start()
    {
        HexPosition.setColor("Cursor", Color.blue, 1);
        GridPositionElements.sampleCellColored = sampleCellFullMarker;

        /* - not wanted / color set inside instead of borders
        HexPosition.setColor("Selectable", Color.green, 2);
        HexPosition.setColor("Selection", Color.yellow, 3);
        for (int i = 0; i < Player.Count; i++)
          HexPosition.setColor("Player" + i , Player.GetColor(i), 4 + i);
        */

        HexPosition.Marker = sampleCellEmptyMarker;
        UpdateUi();
        sampleTower.SetActive(false);
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
                    if (GridTowers.createTower(sampleTower, mouse.getPosition(), Player.Current))
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
        GridTowers.endTurn();
        Player.NextPlayer();
        GridPositionElements.updateColors();
        UpdateUi();
    }

    private void UpdateUi()
    {
        ExecuteEvents.Execute<IUpdateUi>(canvas, null, (msg, data) => msg.SetCurrentPlayer());
        int[] scores = GridPositionElements.GetNumberOfControlledPositionsPerPlayer();
        ExecuteEvents.Execute<IUpdateUi>(canvas, null, (msg, data) => msg.SetScores(scores));
    }

    public static bool checkElementInsideGrid (Vector3 position)
    {
        if ((position.x >= ConfigurationElements.board_size_x) || (position.z >= ConfigurationElements.board_size_z))
            return false;
        if ((position.x <= -ConfigurationElements.board_size_x) || (position.z >= ConfigurationElements.board_size_z))
            return false;
        if ((position.x >= ConfigurationElements.board_size_x) || (position.z <= -ConfigurationElements.board_size_z))
            return false;
        if ((position.x <= -ConfigurationElements.board_size_x) || (position.z <= -ConfigurationElements.board_size_z))
            return false;

        return true;
    }
}