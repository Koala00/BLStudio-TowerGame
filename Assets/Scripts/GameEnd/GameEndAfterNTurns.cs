using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

class GameEndAfterNTurns : ICheckGameEnd
{
    public int MaxTurns = 20;

    public Text ProgressLabel { get; set; }

    public GameEndAfterNTurns()
    {
    }

    public bool IsGameEnd()
    {
        ProgressLabel.text = String.Format("{0} / {1} turns to end", Player.Turns, MaxTurns);
        return Player.Turns >= MaxTurns;
    }
}