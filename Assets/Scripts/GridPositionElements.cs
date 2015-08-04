using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class GridPositionElements
{
    public enum actions {create, delete};
    private static List<PositionElement> positionsList = new List<PositionElement>();
        
    public class PositionElement
    {
        public int player1count;
        public int player2count;
    }

    public static void updateElements (Vector3 position, int player, actions updateAction)
    {

    }

    public static int calculateDistance(Vector3 position1, Vector3 position2)
    {
        var hexPos1 = new HexPosition(position1);
        var hexPos2 = new HexPosition(position2);
        return hexPos1.dist(hexPos2);
    }
}
