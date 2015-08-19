using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Takes care of the fields controlled by each tower.
/// </summary>
[RequireComponent(typeof(HexBoard))]
public class GridPositionElements : MonoBehaviour
{
    //public static GameObject sampleCellColored;
    private HexBoard HexBoard;

    public void Start()
    {
        HexBoard = GetComponent<HexBoard>();
    }

    private Dictionary<HexCoord, PositionControl> PositionControls = new Dictionary<HexCoord, PositionControl>();

    /// <summary>
    /// Keeps track of which player controls the position.
    /// When a tower is placed that affects the position, its control of the position is increased.
    /// </summary>
    public class PositionControl
    {
        private int[] ControlOfPlayer = new int[Player.Count];
        public int TowerOfPlayer = Player.NoPlayer; // If occupied by a tower of a player, this contains the player number.
        //public GameObject cellColoring;
        private HexCoord Position;
        private HexBoard HexBoard;

        public PositionControl (HexCoord position, HexBoard hexBoard)
        {
            //cellColoring = (GameObject) Instantiate(sampleCellColored, new Vector3(position.x, 0.01f , position.z), sampleCellColored.transform.rotation);
            // TODO: show controlled field
            Position = position;
            HexBoard = hexBoard;
        }

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

        public void UpdateColor()
        {
            int player = TowerOfPlayer == Player.NoPlayer
                ? GetPlayerInControl()
                : TowerOfPlayer;
            HexBoard.SetTilePlayerColor(Position, player);
            ShowDebugNumbers();
        }

        private void ShowDebugNumbers()
        {
            string textName = Position.ToString();
            TextMesh text = HexBoard.GetComponentsInChildren<TextMesh>().FirstOrDefault(c => c.name == textName);
            if (text == null)
            {
                var textObj = new GameObject();
                textObj.transform.SetParent(HexBoard.transform);
                textObj.transform.localPosition = Position.Position3d() - new Vector3(0.2f, 0, 0.0f);
                text = textObj.AddComponent<TextMesh>();
                textObj.transform.Rotate(new Vector3(90, 0, 0));
                text.fontSize = 30;
                text.characterSize = 0.1f;
                text.color = Color.black;
                textObj.name = textName;
                text.name = textName;
            }
            text.text = Position.OddRToOffset().ToString("0") + "\n"+ String.Join(", ", ControlOfPlayer.Select(c => c.ToString()).ToArray());
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

    public PositionControl GetPositionControlAt(HexCoord position)
    {
        PositionControl positionControl;
        PositionControls.TryGetValue(position, out positionControl);
        return positionControl;
    }

    public bool IsTowerAt(HexCoord position)
    {
        var control = GetPositionControlAt(position);
        return control != null && control.TowerOfPlayer != Player.NoPlayer;
    }

    public void UpdateColors()
    {
        foreach (var positionControl in PositionControls.Values)
            positionControl.UpdateColor();
    }

    public int[] GetNumberOfControlledPositionsPerPlayer()
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

    public void IncreasePositionControl(HexCoord position, int player)
    {
        // add a PositionControl for the now occupied position, if missing.
        PositionControl control;
        if (!PositionControls.TryGetValue(position, out control))
            PositionControls.Add(position, control = new PositionControl(position, HexBoard));
        control.TowerOfPlayer = player;

        // recalculate position controls
        var settings = GlobalSettings.Instance.gameRuleSettings.Tower;
        foreach (var neighbor in ControlledPositionsAround(position, settings.ControlDistance))
        {
            neighbor.IncreaseControl(player);
        }
    }

    public void DecreasePositionControl(HexCoord position, int player)
    {
        // Remove the mark of the current player from the position.
        PositionControl control;
        if (PositionControls.TryGetValue(position, out control))
            control.TowerOfPlayer = Player.NoPlayer;

        // recalculate position controls
        var settings = GlobalSettings.Instance.gameRuleSettings.Tower;
        foreach (var neighbor in ControlledPositionsAround(position, settings.ControlDistance))
        {
            neighbor.DecreaseControl(player);
        }
    }

    /// <summary>
    /// Gets infos about who controls the positions around a given position.
    /// </summary>
    public IEnumerable<PositionControl> ControlledPositionsAround(HexCoord position, int distance)
    {
        foreach(HexCoord neighbor in NeighborhoodRespectingBorders(position, distance))
        {
            PositionControl control;
            if (!PositionControls.TryGetValue(neighbor, out control))
            {
                control = new PositionControl(neighbor, HexBoard);
                PositionControls.Add(neighbor, control);
            }
            yield return control;
        }
    }

    private IEnumerable<HexCoord> NeighborhoodRespectingBorders(HexCoord position, int distance)
    {
        foreach (HexCoord neighbor in Neighborhood(position, distance))
            if (HexBoard.IsPositionOnBoard(neighbor))
                yield return neighbor;
    }

    /// <summary>
    /// Returns all valid positions around the given center within a given distance.
    /// </summary>
    private static IEnumerable<HexCoord> Neighborhood(HexCoord center, int distance)
    {
        HexCoord current = center;
        // Walk around the center in circles.
        for(int radius = 1; radius <= distance; radius++)
        {
            current = current.Neighbor(0);
            for (int i = 0; i < 6; i++)
            {
                int direction = (i + 2) % 6;
                for (int j = 0; j < radius; j++)
                    yield return current = current.Neighbor(direction);
            }            
        }
    }
}
