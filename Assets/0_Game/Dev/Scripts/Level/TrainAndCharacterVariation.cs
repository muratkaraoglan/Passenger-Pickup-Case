using _0_Game.Dev.Scripts.Train;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Level
{
    [CreateAssetMenu(fileName = "TrainAndCharacterVariation", menuName = "Create New Train and Character Variation", order = 0)]
    public class TrainAndCharacterVariation : ScriptableObject
    {
        public TrainColor trainColor;
        public GameObject trainHeadPrefab;
        public GameObject trainBodyPrefab;
        public GameObject trainTailPrefab;
        public GameObject characterPrefab;
    }
}