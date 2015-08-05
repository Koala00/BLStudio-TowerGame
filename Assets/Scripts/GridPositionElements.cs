﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

class GridPositionElements
{
    private static Dictionary<HexPosition, PositionControl> PositionControls = new Dictionary<HexPosition, PositionControl>();

    /// <summary>
    /// Keeps track of which player controls the position.
    /// </summary>
    /// When a tower is placed that affects the position, its control of the position is increased.
    public class PositionControl
    {
        private int[] ControlOfPlayer = new int[Player.Count];
        public int TowerOfPlayer = Player.NoPlayer; // If occupied by a tower of a player, this contains the player number.

        public void IncreaseControl(int player)
        {
            Assert.IsTrue(player >= 0 && player < Player.Count);
            ControlOfPlayer[player]++;
        }

        public void DecreaseControl(int player)
        {
            Assert.IsTrue(player >= 0 && player < Player.Count);
            if (ControlOfPlayer[player] > 0)
              ControlOfPlayer[player]--;
        }

        /// <summary>
        /// Returns the player in control or Player.NoPlayer no player has control.
        /// </summary>
        public int GetPlayerInControl()
        {
            int playerInControl = Player.NoPlayer;
            int strongestControl = -1;
            for (int p = 0; p < Player.Count; p++)
            {
                int control = ControlOfPlayer[p];
                if (control > strongestControl)
                {
                    strongestControl = control;
                    playerInControl = p;
                }
                else if (control == strongestControl)
                {  
                    // Another player has as much control as the currently strongest player => No one has control.
                    playerInControl = Player.NoPlayer;
                }
            }
            return playerInControl;
        }
    }

    public static int[] GetNumberOfControlledPositionsPerPlayer()
    {
        int[] playerControl = new int[Player.Count];
        foreach(var positionControl in PositionControls.Values)
        {
            int player = positionControl.GetPlayerInControl();
            bool onePlayerHasControl = player != Player.NoPlayer;
            bool positionIsEmpty = positionControl.TowerOfPlayer == Player.NoPlayer;
            if (onePlayerHasControl && positionIsEmpty)
                playerControl[player]++;
        }
        return playerControl;
    }

    public static void IncreasePositionControl(Vector3 position, int player)
    {
        foreach (var neighbor in ControlledPositionsAround(position, ConfigurationElements.ControlDistance))
            neighbor.IncreaseControl(player);
        // Also add a PositionControl for the now occupied position, if missing.
        PositionControl control;
        var hexPosition = new HexPosition(position);
        if (!PositionControls.TryGetValue(hexPosition, out control))
            PositionControls.Add(hexPosition, control = new PositionControl());
        control.TowerOfPlayer = player;
    }

    public static void DecreasePositionControl(Vector3 position, int player)
    {
        foreach (var neighbor in ControlledPositionsAround(position, ConfigurationElements.ControlDistance))
            neighbor.DecreaseControl(player);
        // Remove the mark of the current player from the position.
        PositionControl control;
        var hexPosition = new HexPosition(position);
        if (PositionControls.TryGetValue(hexPosition, out control))
            control.TowerOfPlayer = Player.NoPlayer;
    }
    
    /// <summary>
    /// Gets infos about who controls the positions around a given position.
    /// </summary>
    public static IEnumerable<PositionControl> ControlledPositionsAround(Vector3 position, int distance)
    {
        foreach(HexPosition neighbor in NeighborhoodRespectingBorders(position, distance))
        {
            PositionControl control;
            if (!PositionControls.TryGetValue(neighbor, out control))
            {
                control = new PositionControl();
                PositionControls.Add(neighbor, control);
            }
            yield return control;
        }
    }

    private static IEnumerable<HexPosition> NeighborhoodRespectingBorders(Vector3 position, int distance)
    {
        foreach (HexPosition neighbor in Neighborhood(position, distance))
            if (Grid.checkElementInsideGrid(neighbor.getPosition()))
                yield return neighbor;
    }


    private static IEnumerable<HexPosition> Neighborhood(Vector3 position, int distance)
    {
        return Neighborhood(new HexPosition(position), distance);
    }

    /// <summary>
    /// Returns all valid positions around the given center within a given distance.
    /// </summary>
    private static IEnumerable<HexPosition> Neighborhood(HexPosition center, int distance)
    {
        HexPosition current = center;
        // Walk around the center in circles (counter clockwise).
        for(int radius = 1; radius <= distance; radius++)
        {
            current = current.N;
            for (int i = 0; i < radius; i++)
                yield return current = current.SW;
            for (int i = 0; i < radius; i++)
                yield return current = current.S;
            for (int i = 0; i < radius; i++)
                yield return current = current.SE;
            for (int i = 0; i < radius; i++)
                yield return current = current.NE;
            for (int i = 0; i < radius; i++)
                yield return current = current.N;
            for (int i = 0; i < radius; i++)
                yield return current = current.NW;
        }
    }
}
