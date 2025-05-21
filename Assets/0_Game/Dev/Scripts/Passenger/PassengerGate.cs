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
        private TrainCarMovementController _lastTriggeredTrainCarMovementController;
        private MeshRenderer _passageGateMeshRenderer;

        public void Init(List<PassengerController> passengers, GameObject passageGateModel)
        {
            controllers = passengers;
            GameEventsManager.Instance.InputEvents.OnMouseUp += OnMouseUp;
            _passageGateMeshRenderer = passageGateModel.GetComponent<MeshRenderer>();
            ChangeGateColor(passengers[0].Color);
        }

        private void OnDestroy()
        {
            GameEventsManager.Instance.InputEvents.OnMouseUp -= OnMouseUp;
        }

        private void OnMouseUp()
        {
            if (_lastTriggeredTrainCarMovementController)
            {
                StartCoroutine(JumpPassenger(_lastTriggeredTrainCarMovementController));
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out TrainCarMovementController trainCarMovementController))
            {
                _lastTriggeredTrainCarMovementController = trainCarMovementController;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _lastTriggeredTrainCarMovementController = null;
        }

        // private void OnTriggerStay(Collider other)
        // {
        //     if (other.TryGetComponent(out TrainCarMovementController trainCarMovementController) && !_isPassengerMoving)
        //     {
        //         if (trainCarMovementController.IsMoving()) return;
        //
        //         var manager = trainCarMovementController.GetComponentInParent<TrainManager>();
        //         _isPassengerMoving = true;
        //         StartCoroutine(JumpPassenger(trainCarMovementController, manager));
        //     }
        // }
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
            while (!trainCarMovementController.IsMoving() && !isFinished)
            {
                var passenger = controllers[0];
                if (manager.trainColor != passenger.Color)
                {
                    print("not match color ");
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
                else
                {
                    print("no seat");
                    manager.StartFullCapacityProcess();
                    yield break;
                }
            }

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