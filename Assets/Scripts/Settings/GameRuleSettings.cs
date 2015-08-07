using UnityEngine;
using System.Collections;

[System.Serializable]
public class GameRuleSettings
{
    public BoardSettings Board;
    public TowerSettings Tower;
    public GameEndSettings GameEnd;

    public static GameRuleSettings Instance
    {
        get { return GlobalSettings.Instance.gameRuleSettings; }
    }
}

[System.Serializable]
public class BoardSettings
{
    public int Width = 29;
    public int Height = 29;
}

[System.Serializable]
public class TowerSettings
{
    public int ShootingDistance = 3;
    public int ControlDistance = 3;
    public int Hitpoints = 10;
}

[System.Serializable]
public class GameEndSettings
{
    public GameEndType Type;
    public int Turns;
    public int Percentage;
}

[System.Serializable]
public enum GameEndType
{
    AfterNTurns,
    AfterPercentageCovered
}