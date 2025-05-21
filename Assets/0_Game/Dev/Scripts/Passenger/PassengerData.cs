using System;
using _0_Game.Dev.Scripts.Train;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Passenger
{
    public enum PassengerSide
    {
        Top,
        Bottom,
        Left,
        Right
    }

    [System.Serializable]
    public class Passenger
    {
        public TrainColor color;
        public Vector2Int gridPosition; // Grid position adjacent to where passenger stands
        public PassengerSide side; // Side of the grid where passenger stands
        public int order; // Order in passenger queue (0 is first)
    }
}