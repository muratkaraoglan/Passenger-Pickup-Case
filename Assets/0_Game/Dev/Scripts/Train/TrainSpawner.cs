using System;
using System.Collections.Generic;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Train
{
    public class TrainSpawner : MonoBehaviour
    {
        public void SpawnTrains(List<TrainHolder> trains, int width, int height)
        {
            TrainBuilder builder = new TrainBuilder(width, height);
            builder.BuildTrains(trains);
        }
    }
}