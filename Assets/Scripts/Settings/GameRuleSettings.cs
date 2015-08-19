using UnityEngine;
using System.Collections;
using System;

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

    public void SaveInPlayerPrefs()
    {
        PlayerPrefsSerializer<GameRuleSettings>.Create().Save(this);
    }

    public void LoadFromPlayerPrefs()
    {
        PlayerPrefsSerializer<GameRuleSettings>.Create().Load(this);
    }

    public void ClearFromPlayerPrefs()
    {
        PlayerPrefsSerializer<GameRuleSettings>.Create().Clear();
    }
    

    public static readonly Color[] players_color = { Color.red, Color.blue, Color.cyan, Color.green };
    public static Color[] players_lerpedColor; // Will be initialized later based on PlayerColors.

    // initialisation section
    static GameRuleSettings()
    {
        players_lerpedColor = new Color[players_color.Length];
        for (int player = 0; player < players_color.Length; player++)
            players_lerpedColor[player] = Color.Lerp(Color.white, players_color[player], 0.5f);
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

