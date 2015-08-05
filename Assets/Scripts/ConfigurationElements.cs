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
    public static int ControlDistance = 3;

    public static readonly Color[] PlayerColors = { Color.red, Color.blue, Color.cyan, Color.green };

    public static Color[] playersColor = {
        Color.Lerp(Color.white, Color.red, 0.5f),
        Color.Lerp(Color.white, Color.blue, 0.5f),
    } ;
}
