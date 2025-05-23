using System;
using System.Collections.Generic;
using UnityEngine;
using _0_Game.Dev.Scripts.Grid;
using _0_Game.Dev.Scripts.Passenger;
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
        public List<PassengerQueue> passengerQueues = new List<PassengerQueue>();
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject cornerWallPrefab;
        [SerializeField] private float wallOffset = .6f;
        [SerializeField] private float innerEdgeOffset = .4f;

        public Cell GetCell(Vector2Int coord)
        {
            if (coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height)
                return cells[coord.y * width + coord.x];
            return null;
        }

        public Cell GetCell(int x, int y)
        {
            return GetCell(new Vector2Int(x, y));
        }
        
        public bool IsPassengerQueueAtPosition(Vector2Int gridPos, PassengerSide side)
        {
            return passengerQueues.Exists(q => q.gridPosition == gridPos && q.side == side);
        }

        public PassengerQueue GetPassengerQueueAtPosition(Vector2Int gridPos, PassengerSide side)
        {
            return passengerQueues.Find(q => q.gridPosition == gridPos && q.side == side);
        }

        public Dictionary<Vector3, NodeBase> InitializeGrid(Transform parent)
        {
            var grid = new Dictionary<Vector3, NodeBase>();
            GameObject wallParent = new GameObject("Walls");
            Transform wallParentTransform = wallParent.transform;
            var startXOffset = -width / 2;
            var startYOffset = -height / 2;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = cells[y * width + x];
                    var worldPosition = new Vector3(startXOffset + x, 0, startYOffset + y);
                    var squareNode = new SquareNode
                    {
                        IsEmpty = !cell.isOccupied,
                        Coord = new SquareCoord()
                    };
                    squareNode.Coord.Position = worldPosition;
                    grid.Add(worldPosition, squareNode);
                    var current = new Vector2Int(x, y);
                    if (cell.type == CellType.NotAvailable)
                    {
                        // TryPlaceCornerInner(current, Vector2Int.left, Vector2Int.down,
                        //     new Vector3(startXOffset - innerEdgeOffset, 0.022f, startYOffset - innerEdgeOffset),
                        //     Quaternion.Euler(0, -90, 0));     
                        // TryPlaceCornerInner(current, Vector2Int.left, Vector2Int.up,
                        //     new Vector3(startXOffset - innerEdgeOffset, 0, startYOffset + innerEdgeOffset),
                        //     Quaternion.identity);
                        continue;
                    }

                    var instance = Instantiate(cell.type == CellType.Empty ? cellPrefab : obstaclePrefab, parent);

                    instance.transform.localPosition = new Vector3(x + startXOffset, 0, y + startYOffset);

                    TryPlaceWall(current, Vector2Int.left, new Vector3(startXOffset - wallOffset, 0, startYOffset),
                        Quaternion.identity, wallParentTransform);
                    TryPlaceWall(current, Vector2Int.right, new Vector3(startXOffset + wallOffset, 0, startYOffset),
                        Quaternion.identity, wallParentTransform);
                    TryPlaceWall(current, Vector2Int.down, new Vector3(startXOffset, 0, startYOffset - wallOffset),
                        Quaternion.Euler(0, -90, 0), wallParentTransform);
                    TryPlaceWall(current, Vector2Int.up, new Vector3(startXOffset, 0, startYOffset + wallOffset),
                        Quaternion.Euler(0, -90, 0), wallParentTransform);

                    TryPlaceCorner(current, Vector2Int.left, Vector2Int.down,
                        new Vector3(startXOffset - wallOffset, 0, startYOffset - wallOffset),
                        Quaternion.Euler(0, -90, 0), wallParentTransform);

                    TryPlaceCorner(current, Vector2Int.left, Vector2Int.up,
                        new Vector3(startXOffset - wallOffset, 0, startYOffset + wallOffset),
                        Quaternion.identity, wallParentTransform);

                    TryPlaceCorner(current, Vector2Int.right, Vector2Int.down,
                        new Vector3(startXOffset + wallOffset, 0, startYOffset - wallOffset),
                        Quaternion.Euler(0, -180, 0), wallParentTransform);

                    TryPlaceCorner(current, Vector2Int.right, Vector2Int.up,
                        new Vector3(startXOffset + wallOffset, 0, startYOffset + wallOffset),
                        Quaternion.Euler(0, -270, 0), wallParentTransform);
                }
            }

            return grid;
        }

        private void TryPlaceWall(Vector2Int current, Vector2Int direction, Vector3 positionOffset, Quaternion rotation,
            Transform parent)
        {
            var neighbor = GetCell(current + direction);
            if (neighbor == null || neighbor.type == CellType.NotAvailable)
            {
                Vector3 pos = new Vector3(current.x, 0, current.y) + positionOffset;
                Instantiate(wallPrefab, pos, rotation, parent);
            }
        }

        private void TryPlaceCorner(Vector2Int current, Vector2Int dirA, Vector2Int dirB, Vector3 offset,
            Quaternion rotation, Transform parent)
        {
            var neighborA = GetCell(current + dirA);
            var neighborB = GetCell(current + dirB);

            bool isBlockedA = neighborA == null || neighborA.type == CellType.NotAvailable;
            bool isBlockedB = neighborB == null || neighborB.type == CellType.NotAvailable;

            if (isBlockedA && isBlockedB)
            {
                Vector3 pos = new Vector3(current.x, 0, current.y) + offset;
                Instantiate(cornerWallPrefab, pos, rotation, parent);
            }
        }

        private void TryPlaceCornerInner(Vector2Int current, Vector2Int dirA, Vector2Int dirB, Vector3 offset,
            Quaternion rotation)
        {
            var neighborA = GetCell(current + dirA);
            var neighborB = GetCell(current + dirB);

            bool isOccupiedA = neighborA != null && neighborA.type != CellType.NotAvailable;
            bool isOccupiedB = neighborB != null && neighborB.type != CellType.NotAvailable;

            if (isOccupiedA && isOccupiedB)
            {
                Vector3 pos = new Vector3(current.x, 0, current.y) + offset;
                Instantiate(cornerWallPrefab, pos, rotation);
            }
        }
    }
}