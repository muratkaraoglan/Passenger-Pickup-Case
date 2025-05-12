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

        public GameObject cellPrefab;
        [SerializeField] private GameObject obstaclePrefab;

        public Cell GetCell(Vector2Int coord)
        {
            if (coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height)
                return cells[coord.y * width + coord.x];
            return null;
        }

        public void InitializeGrid(Transform parent)
        {
            var startXOffset = -width / 2;
            var startYOffset = -height / 2;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = cells[y * width + x];
                    if (cell.type == CellType.NotAvailable) continue;
                    var instance = Instantiate(cell.type == CellType.Empty ? cellPrefab : obstaclePrefab, parent);

                    instance.transform.localPosition = new Vector3(x + startXOffset, 0, y + startYOffset);
                }
            }
        }
    }
}