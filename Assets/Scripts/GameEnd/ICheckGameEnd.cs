using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

interface ICheckGameEnd
{
    Text ProgressLabel { get; set; }

    bool IsGameEnd();
}
