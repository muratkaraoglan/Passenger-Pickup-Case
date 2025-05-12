using _0_Game.Dev.Scripts.Level;
using _0_Game.Dev.Scripts.Train;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Grid
{
    public class GridManager : MonoBehaviour
    {
        public LevelConfig levelConfig;
        [SerializeField] private TrainSpawner trainSpawner;
        private Cell[] _cells;

        private void Start()
        {
            levelConfig.InitializeGrid(transform);
            trainSpawner.SpawnTrains(levelConfig.trains, levelConfig.width, levelConfig.height);
        }
    }
}