using System;
using System.Collections;
using System.Collections.Generic;
using _0_Game.Dev.Scripts.Events;
using _0_Game.Dev.Scripts.Helper;
using _0_Game.Dev.Scripts.Managers;
using _0_Game.Dev.Scripts.Train;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

namespace _0_Game.Dev.Scripts.Passenger
{
    public class PassengerGate : MonoBehaviour
    {
        [SerializeField] private TrainColor currentGateColor;
        [SerializeField] private List<PassengerController> controllers;
        [SerializeField] private TrainCarMovementController _lastTriggeredTrainCarMovementController;
        private MeshRenderer _passageGateMeshRenderer;
        private bool _isJumpStarted;

        public void Init(List<PassengerController> passengers, GameObject passageGateModel)
        {
            controllers = passengers;
            _passageGateMeshRenderer = passageGateModel.GetComponent<MeshRenderer>();
            ChangeGateColor(passengers[0].Color);
        }

        private void OnTriggerStay(Collider other)
        {
            if(_isJumpStarted) return; 
            if (other.TryGetComponent(out _lastTriggeredTrainCarMovementController))
            {
                if (!_lastTriggeredTrainCarMovementController.IsMoving())
                {
                    StartCoroutine(JumpPassenger(_lastTriggeredTrainCarMovementController));
                }
            }
        }

        private void ChangeGateColor(TrainColor color)
        {
            currentGateColor = color;
            var modelVariation = ModelVariationManager.Instance.GetModelVariation(currentGateColor);
            var materials = _passageGateMeshRenderer.materials;
            materials[1] = modelVariation.material;
            _passageGateMeshRenderer.materials = materials;
        }

        private IEnumerator JumpPassenger(TrainCarMovementController trainCarMovementController)
        {
            var manager = trainCarMovementController.GetComponentInParent<TrainManager>();
            if (!manager.HasEmptySeatPoint()) yield break;

            bool isFinished = false;
            _isJumpStarted = true;
            while (!trainCarMovementController.IsMoving() && !isFinished)
            {
                var passenger = controllers[0];
                if (manager.trainColor != passenger.Color)
                {
                    print("not match color ");
                    _isJumpStarted = false;
                    yield break;
                }

                var seatPoint = manager.GetSeatPoint();
                if (seatPoint)
                {
                    var passengerPosition = passenger.transform.localPosition;
                    passenger.transform.SetParent(seatPoint);
                    controllers.RemoveAt(0);
                    passenger.transform.DOLocalJump(Vector3.zero, 1f, 1, .1f).SetEase(Ease.Linear);
                    passenger.transform.forward = seatPoint.forward;
                    passenger.PlaySittingAnimation();
                    MovePassengers(passengerPosition);
                    if (controllers.Count == 0)
                    {
                        isFinished = true;
                        print("all passengers are finished");
                    }
                    else
                    {
                        if (currentGateColor != controllers[0].Color)
                            ChangeGateColor(controllers[0].Color);
                    }

                    yield return Extension.GetWaitForSeconds(.1f);
                }

                if (!manager.HasEmptySeatPoint())
                {
                    print("no seat");
                    _isJumpStarted = false;
                    manager.StartFullCapacityProcess();
                    yield break;
                }
            }

            //_lastTriggeredTrainCarMovementController = null;
            _isJumpStarted = false;
            if (isFinished)
            {
                Destroy(gameObject);
            }
        }


        private void MovePassengers(Vector3 targetPosition)
        {
            if (controllers.Count == 0) return;
            foreach (var passenger in controllers)
            {
                var tempPos = passenger.transform.localPosition;
                passenger.transform.DOLocalMove(targetPosition, .1f);
                targetPosition = tempPos;
            }
        }
    }
}