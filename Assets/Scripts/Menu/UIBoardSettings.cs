using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

/// <summary>
/// Controls the option menu group "Game End".
/// </summary>
public class UIBoardSettings : MonoBehaviour, IOptionsGroup
{
    public Text WidthText;
    public Text HeightText;
    public Slider WidthSlider;
    public Slider HeightSlider;

    private string InitialWidthText;
    private string InitialHeightText;

    public void OnEnable()
    {
        // Remember the original text as defined at design time.
        InitialWidthText = WidthText.text;
        InitialHeightText = HeightText.text;
    }

    public void Load()
    {
        var boardSettings = GameRuleSettings.Instance.Board;
        WidthSlider.value = boardSettings.Width;
        HeightSlider.value = boardSettings.Height;
        SetWidthText(WidthSlider.value);
        SetHeightText(HeightSlider.value);
    }

    public void Save()
    {
        var boardSettings = GameRuleSettings.Instance.Board;
        boardSettings.Width = Convert.ToInt32(WidthSlider.value);
        boardSettings.Height = Convert.ToInt32(HeightSlider.value);
    }
      

    public void SetWidthText(float value)
    {
        WidthText.text = InitialWidthText + " " + WidthSlider.value;
    }

    public void SetHeightText(float value)
    {
        HeightText.text = InitialHeightText + " " + HeightSlider.value;
    }
}
