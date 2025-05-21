using System.Collections.Generic;
using System.Linq;
using _0_Game.Dev.Scripts.Helper;
using _0_Game.Dev.Scripts.Level;
using _0_Game.Dev.Scripts.Train;
using UnityEngine;
using UnityEngine.Serialization;

namespace _0_Game.Dev.Scripts.Managers
{
    public class ModelVariationManager : Singleton<ModelVariationManager>
    {
        [SerializeField] private List<ModelVariation> modelVariations;
        
        private Dictionary<TrainColor, ModelVariation> _modelVariationMap;

        protected override void Awake()
        {
            Configure(config =>
            {
                config.DestroyOthers = true;
                config.Persist = true;
                config.Lazy = true;
            });
            _modelVariationMap = modelVariations.ToDictionary(t => t.trainColor, t => t);
            base.Awake();
        }

        public ModelVariation GetModelVariation(TrainColor color)
        {
            if (!_modelVariationMap.TryGetValue(color, out var trainAndCharacterVariation))
            {
                Debug.LogError("No variation for color");
                return null;
            }

            return trainAndCharacterVariation;
        }
    }
}