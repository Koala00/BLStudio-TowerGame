using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class GameRuleSettings
{
    public BoardSettings Board;
    public TowerSettings Tower;
    public GameEndSettings GameEnd;
    public PlayerSettings Player;

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
public class PlayerSettings
{
    public Color[] colors = { Color.red, Color.blue, Color.cyan, Color.green };
    public Color[] lerpedColors; // Will be initialized later based on PlayerColors.

    // initialisation section
    public PlayerSettings()
    {
        lerpedColors = new Color[colors.Length];
        for (int player = 0; player < colors.Length; player++)
            lerpedColors[player] = Color.Lerp(Color.white, colors[player], 0.5f);
    }
}

[System.Serializable]
public enum GameEndType
{
    AfterNTurns,
    AfterPercentageCovered
}