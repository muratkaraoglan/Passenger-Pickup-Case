using System;
using System.Collections.Generic;
using _0_Game.Dev.Scripts.Train;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Passenger
{
    [Serializable]
    public class PassengerQueue
    {
        public List<Passenger> passengers = new List<Passenger>();
        public Vector2Int gridPosition;
        public PassengerSide side;

        public void AddPassenger(TrainColor color)
        {
            var passenger = new Passenger
            {
                color = color,
                gridPosition = gridPosition,
                side = side,
                order = passengers.Count
            };
            passengers.Add(passenger);
        }

        public void RemovePassenger()
        {
            if (passengers.Count > 0)
            {
                passengers.RemoveAt(0);
                // Update order for remaining passengers
                for (int i = 0; i < passengers.Count; i++)
                {
                    passengers[i].order = i;
                }
            }
        }

        public void RemovePassenger(int index)
        {
            passengers.RemoveAt(index);
            for (int i = 0; i < passengers.Count; i++)
            {
                passengers[i].order = i;
            }
        }
    }
}