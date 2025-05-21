using System.Collections;
using System.Collections.Generic;
using _0_Game.Dev.Scripts.Helper;
using DG.Tweening;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Train
{
    public class TrainManager : MonoBehaviour
    {
        public TrainColor trainColor;
        private Queue<Transform> _seatPoints = new Queue<Transform>();

        public void AddSeatPoint(Transform seatPoint)
        {
            _seatPoints.Enqueue(seatPoint);
        }

        public Transform GetSeatPoint()
        {
            if (_seatPoints.Count == 0)
            {
                print("No seat points");
                StopInteraction();
                return null;
            }

            return _seatPoints.Dequeue();
        }

        public bool HasEmptySeatPoint() => _seatPoints.Count > 0;

        private void StopInteraction()
        {
            var controllers = GetComponentsInChildren<TrainCarMovementController>();
            foreach (var controller in controllers)
            {
                controller.enabled = false;
            }
        }

        public void StartFullCapacityProcess()
        {
            StartCoroutine(DestroyTrain());
        }

        IEnumerator DestroyTrain()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                child.DOScale(Vector3.zero, .1f).SetEase(Ease.InBack);
                yield return Extension.GetWaitForSeconds(.1f);
            }

            gameObject.SetActive(false);
        }
    }
}