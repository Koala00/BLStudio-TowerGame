﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.EventSystems;

interface IHandleMissleHit : IEventSystemHandler
{
    void HitByMissle();
}
