using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ConfigurationElements
{
    // variables section
    public static int board_size_x = 29;
    public static int board_size_z = 29;

    public static int towers_reachDistance = 3;
    public static int towers_ControlDistance = 3;

    public static readonly Color[] players_color = { Color.red, Color.blue, Color.cyan, Color.green };
    public static Color[] players_lerpedColor; // Will be initialized later based on PlayerColors.


    
    // initialisation section
    static ConfigurationElements()
    {
        players_lerpedColor = new Color[players_color.Length];
        for (int player = 0; player < players_color.Length; player++)
            players_lerpedColor[player] = Color.Lerp(Color.white, players_color[player], 0.5f);
    }
}
