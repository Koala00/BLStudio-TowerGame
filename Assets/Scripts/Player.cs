using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

static class Player
{
    public const int Count = 2;
    public static int Current = 0;
    public const int NoPlayer = -1;

    public static Color GetColor(int player)
    {
        return ConfigurationElements.PlayerColors[player % ConfigurationElements.PlayerColors.Length];
    }

    public static Color GetTileColor(int player)
    {
        return Color.Lerp(Color.white, GetColor(player), 0.5f);
    }

    public static void NextPlayer()
    {
        Current = (Current + 1) % Count;
    }
}
