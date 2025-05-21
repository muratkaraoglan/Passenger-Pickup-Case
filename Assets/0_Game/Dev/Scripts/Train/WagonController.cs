using System.Collections.Generic;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Train
{
    public class WagonController : MonoBehaviour
    {
        [SerializeField] private List<Transform> wagons;

        public List<Transform> Wagons => wagons;
    }
}