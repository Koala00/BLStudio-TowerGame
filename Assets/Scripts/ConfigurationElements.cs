using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ConfigurationElements
{
    public static int board_size_x = 29;
    public static int board_size_z = 29;

    public static int towers_reachDistance = 3;
    public static int towers_ControlDistance = 3;

    public static readonly Color[] PlayerColors = { Color.red, Color.blue, Color.cyan, Color.green };

    public static Color[] PlayersLerpedColors; // Will be initialized later based on PlayerColors.

    static ConfigurationElements()
    {
        PlayersLerpedColors = new Color[PlayerColors.Length];
        for (int player = 0; player < PlayerColors.Length; player++)
            PlayersLerpedColors[player] = Color.Lerp(Color.white, PlayerColors[player], 0.5f);
    }
}
