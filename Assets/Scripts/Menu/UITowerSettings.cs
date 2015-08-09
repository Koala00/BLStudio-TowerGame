using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

/// <summary>
/// Controls the option menu group "Game End".
/// </summary>
public class UITowerSettings : MonoBehaviour, IOptionsGroup
{
    public Text ShootingRadiusText;
    public Text HitpointsText;
    public Slider ShootingRadiusSlider;
    public Slider HitpointsSlider;

    private string InitialShootingRadiusText;
    private string InitialHitpointsText;

    public void OnEnable()
    {
        // Remember the original text as defined at design time.
        InitialShootingRadiusText = ShootingRadiusText.text;
        InitialHitpointsText = HitpointsText.text;
    }

    public void Load()
    {
        var towerSettings = GameRuleSettings.Instance.Tower;
        ShootingRadiusSlider.value = towerSettings.ShootingDistance;
        HitpointsSlider.value = towerSettings.Hitpoints;
        SetShootingRadiusText(ShootingRadiusSlider.value);
        SetHitpointsText(HitpointsSlider.value);
    }

    public void Save()
    {
        var towerSettings = GameRuleSettings.Instance.Tower;
        towerSettings.ShootingDistance = Convert.ToInt32(ShootingRadiusSlider.value);
        towerSettings.Hitpoints = Convert.ToInt32(HitpointsSlider.value);
    }


    public void SetShootingRadiusText(float value)
    {
        ShootingRadiusText.text = InitialShootingRadiusText + " " + ShootingRadiusSlider.value;
    }

    public void SetHitpointsText(float value)
    {
        HitpointsText.text = InitialHitpointsText + " " + HitpointsSlider.value;
    }
}
