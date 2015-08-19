using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

class GameEndPercentageControlled : ICheckGameEnd
{
    public int Percentage = 80;

    private GridPositionElements PositionElements;

    public GameEndPercentageControlled(GridPositionElements positionElements)
    {
        PositionElements = positionElements;
    }

    public Text ProgressLabel { get; set; }

    public bool IsGameEnd()
    {
        int[] scores = PositionElements.GetNumberOfControlledPositionsPerPlayer();
        var boardSettings = GlobalSettings.Instance.gameRuleSettings.Board;
        int totalPositions = boardSettings.Width * boardSettings.Height;
        int maxScore = totalPositions * Percentage / 100;
        int score = scores.Max();
        ProgressLabel.text = String.Format("Highest score: {0} - Game end score: {1}", score, maxScore);
        return score >= maxScore;
    }
}
