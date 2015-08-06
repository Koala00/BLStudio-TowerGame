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
    public static int Turns { get; private set; }

    static Player()
    {
        Turns = 1;
    }

    public static void NextPlayer()
    {
        Current = (Current + 1) % Count;
        Turns += 1;
    }
}
