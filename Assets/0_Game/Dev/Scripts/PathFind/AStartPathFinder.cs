using System;
using System.Collections.Generic;
using System.Linq;
using _0_Game.Dev.Scripts.Grid;
using UnityEngine;

namespace _0_Game.Dev.Scripts.PathFind
{
    public static class AStarPathFinder
    {
        public static List<NodeBase> FindPath(NodeBase startNode, NodeBase targetNode)
        {
            var toSearch = new List<NodeBase>() { startNode };
            var processed = new List<NodeBase>();

            //if (startNode.IsEmpty) return null;

            while (toSearch.Any())
            {
                var current = toSearch[0];
                foreach (var t in toSearch)
                    if (t.F < current.F || Mathf.Approximately(t.F, current.F) && t.H < current.H)
                        current = t;

                processed.Add(current);
                toSearch.Remove(current);

                if (current == targetNode)
                {
                    var currentPathTile = targetNode;
                    var path = new List<NodeBase>();
                    var count = 100;

                    while (currentPathTile != startNode)
                    {
                        path.Add(currentPathTile);
                        currentPathTile = currentPathTile.Connection;
                        count--;
                        if (count < 0) throw new Exception();
                    }
                    path.Reverse();
                    Debug.Log("Path Count: " + path.Count);
                    return path;
                }

                foreach (var neighbor in current.Neighbors.Where(t => t.IsEmpty && !processed.Contains(t)))
                {
                    var inSearch = toSearch.Contains(neighbor);

                    var costToNeighbor = current.G + current.GetDistance(neighbor);

                    if (!inSearch || costToNeighbor < neighbor.G)
                    {
                        neighbor.SetG(costToNeighbor);
                        neighbor.SetConnection(current);

                        if (!inSearch)
                        {
                            neighbor.SetH(neighbor.GetDistance(targetNode));
                            toSearch.Add(neighbor);
                        }
                    }
                }
            }

            return new List<NodeBase>();
        }
    }
}