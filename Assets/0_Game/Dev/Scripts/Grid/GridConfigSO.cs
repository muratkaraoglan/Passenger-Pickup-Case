using UnityEngine;

namespace _0_Game.Dev.Scripts.Grid
{
    [CreateAssetMenu(fileName = "GridConfig", menuName = "Grid Config/New", order = 0)]
    public class GridConfigSO : ScriptableObject
    {
        public Vector2Int gridSize;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject obstaclePrefab;
        
        public void InitializeGrid()
        {
            
        }
    }
}