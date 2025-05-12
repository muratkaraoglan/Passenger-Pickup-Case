using System.Collections.Generic;
using System.Linq;
using _0_Game.Dev.Scripts.Helper;
using _0_Game.Dev.Scripts.Level;
using _0_Game.Dev.Scripts.Train;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Managers
{
    public class TrainAndCharacterVariationManager : Singleton<TrainAndCharacterVariationManager>
    {
        [SerializeField] private List<TrainAndCharacterVariation> trainAndCharacterVariations;

        private Dictionary<TrainColor, TrainAndCharacterVariation> _trainAndCharacterVariationMap;

        protected override void Awake()
        {
            Configure(config =>
            {
                config.DestroyOthers = true;
                config.Persist = true;
                config.Lazy = true;
            });
            _trainAndCharacterVariationMap = trainAndCharacterVariations.ToDictionary(t => t.trainColor, t => t);
            base.Awake();
        }

        public TrainAndCharacterVariation GetTrainAndCharacterVariationByTrainColor(TrainColor color)
        {
            if (!_trainAndCharacterVariationMap.TryGetValue(color, out var trainAndCharacterVariation))
            {
                Debug.LogError("No variation for color");
                return null;
            }

            return trainAndCharacterVariation;
        }
    }
}