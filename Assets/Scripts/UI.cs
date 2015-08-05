using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The UI controls display of current user, scores etc that are shown using the Canvas.
/// </summary>
/// Use messaging to update the UI.
/// See also: http://docs.unity3d.com/Manual/MessagingSystem.html
class UI : MonoBehaviour, IUpdateUi
{
    public GameObject PlayerNameLabel;
    public GameObject ScorePanel;

    private Text[] PlayerScoreTexts;

    // Use this for initialization
    void Start()
    {
        SetCurrentPlayer();
        InitScorePanel();
    }

    private void InitScorePanel()
    {
        if (PlayerScoreTexts != null)
            return;
        PlayerScoreTexts = new Text[Player.Count];
        var scoreTemplate = ScorePanel.transform.GetChild(0).gameObject;
        var scoreTemplateRect = scoreTemplate.GetComponent<RectTransform>().rect;
        for (int player = 0; player < Player.Count; player++)
        {
            GameObject label = scoreTemplate;
            if (player > 0)
            {
                label = GameObject.Instantiate(scoreTemplate);
                label.transform.SetParent(ScorePanel.transform, false);
                label.transform.Translate(new Vector3(0, -scoreTemplateRect.height * player));
            }
            var labelText = label.GetComponent<Text>();
            labelText.color = Player.GetColor(player);
            PlayerScoreTexts[player] = labelText;
            SetPlayerScore(player, 0);
        }
        // Set new panel hight.
        var panelRect = ScorePanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, scoreTemplateRect.height * (Player.Count + 2 /* for borders */));
    }

    private void SetPlayerScore(int player, int score)
    {
        PlayerScoreTexts[player].text = "Player " + (player + 1) + ": " + score;
    }

    // Update is called once per frame
    void Update()
    {
        // UI updates are triggered by events, not here.
    }

    #region Implementation of IUpdateUi

    public void SetCurrentPlayer()
    {
        var label = PlayerNameLabel.GetComponent<Text>();
        label.text = "Player " + (Player.Current + 1);
        label.color = Player.GetColor(Player.Current);
    }

    public void SetScores(int[] scores)
    {
        InitScorePanel();
        for (int player = 0; player < scores.Length; player++)
        {
            SetPlayerScore(player, scores[player]);
        }
    }

    #endregion
}
