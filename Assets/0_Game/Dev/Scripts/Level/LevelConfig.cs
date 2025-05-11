using System.Collections.Generic;
using UnityEngine;
using _0_Game.Dev.Scripts.Grid;
using _0_Game.Dev.Scripts.Train;

namespace _0_Game.Dev.Scripts.Level
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Level Config/New", order = 0)]
    public class LevelConfig : ScriptableObject
    {
        public int width;
        public int height;
        public Cell[] cells;
        public List<TrainHolder> trains;

        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject obstaclePrefab;

        public Cell GetCell(Vector2Int coord)
        {
            if (coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height)
                return cells[coord.y * width + coord.x];
            return null;
        }

        public void SetCell(int x, int y, Cell cell) => cells[y * width + x] = cell;

        public void InitializeGrid()
        {
        }
    }
}