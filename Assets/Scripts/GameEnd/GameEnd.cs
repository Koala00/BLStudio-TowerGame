using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

class GameEnd : MonoBehaviour, IHandleEndTurn
{
    public GameObject GameOverPanel;
    public GameObject GameProgressPanel;
    private ICheckGameEnd GameOverChecker;

    public static bool GameEnded { get; private set; }

    void Start()
    {
        //GameOverChecker = new GameEndAfterNTurns() { MaxTurns = 20 };
        GameOverChecker = new GameEndPercentageControlled() { Percentage = 80 };
        GameOverChecker.ProgressLabel = GameProgressPanel.GetComponentInChildren<Text>();
        ToggleVisibility(GameOverPanel, false);
    }

    private void ToggleVisibility(GameObject obj, bool visible)
    {
        obj.SetActive(visible);
        /*var renderers = obj.GetComponentsInChildren<CanvasRenderer>();
        foreach(var renderer in renderers)
          renderer.SetAlpha(visible ? 100 : 0);*/
    }

    public void EndTurn()
    {
        bool gameOver = GameOverChecker.IsGameEnd();
        if (gameOver)
        {
            ToggleVisibility(GameOverPanel, true);
            GameEnded = true;
        }
    }
}
