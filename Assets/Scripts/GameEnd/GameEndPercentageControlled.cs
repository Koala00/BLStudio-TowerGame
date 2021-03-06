﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

class GameEndPercentageControlled : ICheckGameEnd
{
    public int Percentage = 80;

    private GridPositionElements GridPositionElements;

    public GameEndPercentageControlled(GridPositionElements gridPositionElements)
    {
        GridPositionElements = gridPositionElements;
    }

    public Text ProgressLabel { get; set; }

    public bool IsGameEnd()
    {
        int[] scores = GridPositionElements.GetNumberOfControlledPositionsPerPlayer();
        int totalPositions = GameRuleSettings.Instance.Board.Width * GameRuleSettings.Instance.Board.Height;
        int maxScore = totalPositions * Percentage / 100;
        int score = scores.Max();
        ProgressLabel.text = String.Format("Highest score: {0} - Game end score: {1}", score, maxScore);
        return score >= maxScore;
    }
}
