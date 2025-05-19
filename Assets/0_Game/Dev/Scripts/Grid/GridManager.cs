using System;
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
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float padding = .5f;
        [SerializeField] private Transform environmentGradientTop;
        [SerializeField] private Transform environmentGradientBottom;
        private Dictionary<Vector3, NodeBase> _grid;
        private int _gridXOffset;
        private int _gridZOffset;
        private int _gridMinX;
        private int _gridMaxX;
        private int _gridMinZ;
        private int _gridMaxZ;

        private void Start()
        {
            _gridXOffset = -levelConfig.width / 2;
            _gridZOffset = -levelConfig.height / 2;

            _gridMinX = _gridXOffset;
            _gridMaxX = _gridXOffset + levelConfig.width - 1;

            _gridMinZ = _gridZOffset;
            _gridMaxZ = _gridZOffset + levelConfig.height - 1;

            _grid = levelConfig.InitializeGrid(transform);

            foreach (var nodeBase in _grid.Values)
            {
                nodeBase.CacheNeighbors();
            }

            trainSpawner.SpawnTrains(levelConfig.trains, levelConfig.width, levelConfig.height);
            AdjustCameraToGrıd();
        }

        private void AdjustCameraToGrıd()
        {
            float screenAspectRatio = (float)Screen.width / (float)Screen.height;

            float totalWidth = levelConfig.width;
            float totalHeight = levelConfig.height;

            var sizeBasedOnWidth = (totalWidth / screenAspectRatio) / 2f;

            var sizeBaseOnHeight = totalHeight / 2f;

            var finalSize = Mathf.Max(sizeBasedOnWidth, sizeBaseOnHeight) + padding;

            mainCamera.orthographicSize = finalSize;
            var cameraX = (Mathf.Abs(_gridMaxX) - Mathf.Abs(_gridMinX)) / 2f;
            var cameraPosition = mainCamera.transform.position;
            cameraPosition.x = cameraX;
            mainCamera.transform.position = cameraPosition;
            AdjustEnvironmentPosition();
        }

        private void AdjustEnvironmentPosition()
        {
            Vector3 topCenter = mainCamera.ViewportToWorldPoint(new Vector3(.5f, 1f, mainCamera.nearClipPlane));
            Vector3 bottomCenter = mainCamera.ViewportToWorldPoint(new Vector3(.5f, 0f, mainCamera.nearClipPlane));
            var fwd = mainCamera.transform.forward;

            Adjust(topCenter, environmentGradientTop);
            Adjust(bottomCenter, environmentGradientBottom);
            return;

            void Adjust(Vector3 center, Transform environment)
            {
                if (Physics.Raycast(center, fwd, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
                {
                    var pos = hit.point;
                    pos.y = 0;
                    environment.position = pos;
                }
            }
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

        public bool PointInGrid(Vector3 point) => _grid.ContainsKey(point);

        public void SetNodeState(Vector3 pos, bool isEmpty)
        {
            var node = GetNoeAtPosition(pos);
            if (node != null)
            {
                node.IsEmpty = isEmpty;
                node.ChangeSpriteColor();
            }
        }

        public Vector3 ClampPosition(Vector3 pos)
        {
            return new Vector3(Mathf.Clamp(pos.x, _gridMinX, _gridMaxX), 0, Mathf.Clamp(pos.z, _gridMinZ, _gridMaxZ));
        }
    }
}