using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

/// <summary>
/// Controls the option menu group "Game End".
/// </summary>
public class UIGameEnd : MonoBehaviour, IOptionsGroup
{
    public Toggle AfterNTurnsToggle;
    public Toggle AfterNPercentToggle;
    public Slider AfterNTurnsSlider;
    public Slider AfterNPercentSlider;

    private string AfterNTurnsText;
    private string AfterNPercentText;

    public void OnEnable()
    {
        // Remember the original text as defined at design time.
        var text = AfterNTurnsToggle.GetComponentInChildren<Text>();
        AfterNTurnsText = text.text;
        text = AfterNPercentToggle.GetComponentInChildren<Text>();
        AfterNPercentText = text.text;
    }

    public void Load()
    {
        var gameEndType = GameRuleSettings.Instance.GameEnd.Type;
        AfterNTurnsToggle.isOn = gameEndType == GameEndType.AfterNTurns;
        AfterNPercentToggle.isOn = gameEndType == GameEndType.AfterPercentageCovered;
        AfterNTurnsSlider.value = GameRuleSettings.Instance.GameEnd.Turns;
        AfterNPercentSlider.value = GameRuleSettings.Instance.GameEnd.Percentage;
        PlayerPrefsSerializer<GameEndSettings>.Create().Load();
    }

    public void Save()
    {
        var gameEnd = GameRuleSettings.Instance.GameEnd;
        gameEnd.Type =
            AfterNTurnsToggle.isOn
            ? GameEndType.AfterNTurns
            : GameEndType.AfterPercentageCovered;
        gameEnd.Turns = Convert.ToInt32(AfterNTurnsSlider.value);
        gameEnd.Percentage = Convert.ToInt32(AfterNPercentSlider.value);
    }
      

    public void SetTurnsText(float value)
    {
        var text = AfterNTurnsToggle.GetComponentInChildren<Text>();
        text.text = AfterNTurnsText.Replace(" N ", String.Format(" {0} ", value));
    }

    public void SetPercentageText(float value)
    {
        var text = AfterNPercentToggle.GetComponentInChildren<Text>();
        text.text = AfterNPercentText.Replace(" N ", String.Format(" {0} ", value));
    }
}
