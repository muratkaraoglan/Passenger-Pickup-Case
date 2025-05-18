using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Grid
{
    [Serializable]
    public class SquareNode : NodeBase
    {
        private static readonly List<Vector3> Dirs = new List<Vector3>()
        {
            new Vector3(0, 0, 1), //forward
            new Vector3(-1, 0, 0), //left
            new Vector3(0, 0, -1), //back
            new Vector3(1, 0, 0) //right
        };

        public override void CacheNeighbors()
        {
            Neighbors = new List<NodeBase>();

            foreach (var node in Dirs.Select(dir => GridManager.Instance.GetNoeAtPosition(Coord.Position + dir))
                         .Where(node => node != null))
            {
                Neighbors.Add(node);
            }
        }
    }
    
    [Serializable]
    public struct SquareCoord : ICoord
    {
        public float GetDistance(ICoord other)
        {
            var dist = new Vector3Int(Mathf.Abs((int)Position.x - (int)other.Position.x),
                Mathf.Abs((int)Position.y - (int)other.Position.y),
                Mathf.Abs((int)Position.z - (int)other.Position.z));

            var lowest = Mathf.Min(dist.x, dist.y);
            var highest = Mathf.Max(dist.x, dist.y);

            var horizontalMovesRequired = highest - lowest;

            return lowest * 14 + horizontalMovesRequired * 10;
        }

        public Vector3 Position { get; set; }
    }
}