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
        public Vector2Int gridPosition;
        public PassengerSide side; 
        public int order;
    }
}