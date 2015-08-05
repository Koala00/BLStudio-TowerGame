using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.EventSystems;

interface IUpdateUi : IEventSystemHandler
{
    void SetCurrentPlayer();
    void SetScores(int[] scores);
}

