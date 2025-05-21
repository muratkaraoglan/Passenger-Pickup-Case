using System.Collections.Generic;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Passenger
{
    public class PassageSpawner : MonoBehaviour
    {
        [SerializeField] private PassengerGate passengerGatePrefab;
        [SerializeField] private GameObject passageModelPrefab;
        public void SpawnPassages(int gridXOffset,
            int gridZOffset, List<PassengerQueue> passengers)
        {
            PassageBuilder builder = new PassageBuilder(gridXOffset, gridZOffset, passengers, passengerGatePrefab,passageModelPrefab);
            builder.BuildPassages();
        }
    }
}