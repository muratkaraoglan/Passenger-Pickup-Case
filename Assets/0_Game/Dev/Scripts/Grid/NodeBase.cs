using System;
using System.Collections.Generic;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Grid
{
    [Serializable]
    public abstract class NodeBase
    {
       
        public ICoord Coord;
        public float GetDistance(NodeBase otherNode) => Coord.GetDistance(otherNode.Coord);
        [field: SerializeField] public bool IsEmpty { get; set; }
        public List<NodeBase> Neighbors { get; set; }
        public NodeBase Connection { get; set; }
        public float G { get; private set; }
        public float H { get; private set; }

        public float F => G + H;

        public abstract void CacheNeighbors();

        public void SetConnection(NodeBase connection) => Connection = connection;

        public void SetG(float g) => G = g;
        public void SetH(float h) => H = h;
    }
}