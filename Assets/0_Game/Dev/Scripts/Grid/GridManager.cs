using System.Collections.Generic;
using System.Linq;
using _0_Game.Dev.Scripts.Helper;
using _0_Game.Dev.Scripts.Level;
using _0_Game.Dev.Scripts.PathFind;
using _0_Game.Dev.Scripts.Train;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Grid
{
    public class GridManager : Singleton<GridManager>
    {
        public LevelConfig levelConfig;
        [SerializeField] private TrainSpawner trainSpawner;
        private Dictionary<Vector3, NodeBase> _grid;

        private void Start()
        {
            _grid = levelConfig.InitializeGrid(transform);

            foreach (var nodeBase in _grid.Values)
            {
                nodeBase.CacheNeighbors();
            }

            trainSpawner.SpawnTrains(levelConfig.trains, levelConfig.width, levelConfig.height);
        }

        public List<NodeBase> TryFindPath(Vector3 start, Vector3 end)
        {
            print(start + " " + end);
            var startNode = GetNoeAtPosition(start);
            var endNode = GetNoeAtPosition(end);

            print(startNode);
            print(endNode);

            if (startNode != null && endNode != null)
            {
                return AStarPathFinder.FindPath(startNode, endNode);
            }

            return new List<NodeBase>();
        }

        public NodeBase GetNoeAtPosition(Vector3 pos) => _grid.TryGetValue(pos, out NodeBase node) ? node : null;

        public void SetNodeState(Vector3 pos, bool isEmpty)
        {
            var node = GetNoeAtPosition(pos);
            if (node != null)
            {
                node.IsEmpty = isEmpty;
            }
        }
    }
}