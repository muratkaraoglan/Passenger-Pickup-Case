using System;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Grid
{
    [Serializable]
    public class Cell
    {
        public CellType type;
        public Vector2Int position;
    }

    public enum CellType
    {
        Empty,
        Obstacle,
        NotAvailable,
    }
}