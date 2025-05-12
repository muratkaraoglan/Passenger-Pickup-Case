using System;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Grid
{
    [Serializable]
    public class Cell
    {
        public CellType type;
        public Vector2Int coord;
        public bool isOccupied;
    }

    public enum CellType
    {
        Empty,
        Obstacle,
        NotAvailable,
    }
}