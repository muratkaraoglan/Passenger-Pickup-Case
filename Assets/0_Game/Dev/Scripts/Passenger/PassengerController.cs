using _0_Game.Dev.Scripts.Helper;
using _0_Game.Dev.Scripts.Train;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Passenger
{
    public class PassengerController : MonoBehaviour
    {
        [field: SerializeField] public TrainColor Color { get; private set; }
        [SerializeField] private Animator animator;

        public void PlaySittingAnimation()
        {
            animator.Play(StringHelper.AnimationKeyHelper.SittingAnimationKey, -1, 0);
        }
    }
}