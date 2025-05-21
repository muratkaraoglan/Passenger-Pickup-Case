using _0_Game.Dev.Scripts.Passenger;
using _0_Game.Dev.Scripts.Train;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Level
{
    [CreateAssetMenu(fileName = "ModelVariation", menuName = "Create New Model Variation", order = 0)]
    public class ModelVariation : ScriptableObject
    {
        public TrainColor trainColor;
        public GameObject trainHeadPrefab;
        public GameObject trainBodyPrefab;
        public GameObject trainTailPrefab;
        public PassengerController characterPrefab;
        public Material material;
    }
}