using System;
using System.Collections.Generic;

namespace _0_Game.Dev.Scripts.Train
{
    [Serializable]
    public class TrainHolder
    {
        public TrainColor trainColor;
        public Direction headDirection;
        public List<Wagon> wagons = new();
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}