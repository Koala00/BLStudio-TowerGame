using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class Grid : MonoBehaviour
{
    public GameObject sampleCellEmptyMarker;
    public GameObject sampleCellFullMarker;
    public GameObject canvas;

    private HexPosition mouse = null;

    void OnEnable()
    {
        HexPosition.clearAll();
        GridPositionElements.Clear();
        Player.Reset();
    }

    // Use this for initialization
    void Start()
    {
        HexPosition.setColor("Cursor", Color.blue, 1);
        GridPositionElements.sampleCellColored = sampleCellFullMarker;        
        HexPosition.Marker = sampleCellEmptyMarker;
        UpdateUi();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameEnd.GameEnded)
            return;
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
                    var gridTowers = GetComponent<GridTowers>();
                    if (gridTowers.createTower(mouse.getPosition(), Player.Current))
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
        var gridTowers = GetComponent<GridTowers>();
        gridTowers.endTurn();
        Player.NextPlayer();
        GridPositionElements.updateColors();
        UpdateUi();
        ExecuteEvents.Execute<IHandleEndTurn>(canvas, null, (msg, data) => msg.EndTurn());
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