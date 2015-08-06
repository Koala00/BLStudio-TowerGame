// BallisticTrajectoriesUnityDemo. Copyright (c) 2015, Giliam de Carpentier (www.decarpentier.nl). All rights reserved. See the README.txt for the BSD-style license details.

using UnityEngine;

namespace Ballistics
{
    // The Misc class is meant to be a set of general-purpose static helper functions
    public abstract class Misc
    {
        // Return some (and potentially non-normalized) Vector3 that's perpendicular to 'along'
        public static Vector3 GetAnyVector3PerpendicularTo(Vector3 along)
        {
            if (along.x * along.x > along.y * along.y)
            {
                return new Vector3(-along.z, 0, along.x);
            }
            else
            {
                return new Vector3(0, -along.z, along.y);
            }
        }
    }
}
