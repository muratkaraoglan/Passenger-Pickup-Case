using System;
using System.Collections;
using System.Collections.Generic;
using _0_Game.Dev.Scripts.Grid;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace _0_Game.Dev.Scripts.Train
{
    public class TrainCarMovementController : MonoBehaviour
    {
        [HideInInspector] public bool isTail;
        [HideInInspector] public bool canInteractWithInput;

        [SerializeField] private TrainCarMovementController nextTrainCarMovementController;
        [SerializeField] private TrainCarMovementController previousTrainCarMovementController;
        [SerializeField] private float delayTime = .2f;
        [SerializeField] private float speed = 5f;

        private bool _isProcessingMovement;
        private readonly Queue<TrainMovement> _trainMovementQueue = new Queue<TrainMovement>();
        private TrainMovement _lastTrainMovement;

        private void Start()
        {
            _lastTrainMovement = new TrainMovement
            {
                Position = transform.position,
                Rotation = transform.rotation,
                IsForwardMovement = true
            };
            if (previousTrainCarMovementController == null) isTail = true;
        }

        private void OnDisable()
        {
            GridManager.Instance.SetNodeState(transform.position, true);
        }

        public void SetPreviousTrainCarMovementController(TrainCarMovementController preCarMovementController) =>
            previousTrainCarMovementController = preCarMovementController;

        public void SetNextTrainCarMovementController(TrainCarMovementController nextCarMovementController) =>
            nextTrainCarMovementController = nextCarMovementController;

        public void EnqueueTrainMovement(Vector3 position, Quaternion rotation, bool isForwardMovement)
        {
            var trainMovement = new TrainMovement(position, rotation, isForwardMovement);
            _trainMovementQueue.Enqueue(trainMovement);

            if (!_isProcessingMovement)
            {
                ProcessMovements().Forget();
            }

            var controller = isForwardMovement ? previousTrainCarMovementController : nextTrainCarMovementController;

            if (controller != null)
                controller.EnqueueTrainMovement(_lastTrainMovement.Position, _lastTrainMovement.Rotation,
                    isForwardMovement);

            _lastTrainMovement = trainMovement;
        }

        private Quaternion CalculateRotation(Vector3 from, Vector3 to)
        {
            if ((to - from).sqrMagnitude < 0.001f)
                return Quaternion.identity;

            Vector3 direction = (to - from).normalized;
            return Quaternion.LookRotation(direction, Vector3.up);
        }

        private async UniTask ProcessMovements()
        {
            _isProcessingMovement = true;
            bool lastMovementFromHead = _lastTrainMovement.IsForwardMovement;

            while (_trainMovementQueue.Count > 0)
            {
                GridManager.Instance.SetNodeState(transform.position, true);

                var current = _trainMovementQueue.Dequeue();

                var rotation = CalculateRotation(transform.position, current.Position);
                rotation = current.IsForwardMovement ? rotation : rotation * Quaternion.Euler(0, 180, 0);

                await PerformMovement(current.Position, rotation);

                lastMovementFromHead = current.IsForwardMovement;
                GridManager.Instance.SetNodeState(current.Position, false);
            }

            AdjustFinalRotation(lastMovementFromHead);

            _isProcessingMovement = false;
        }

        private async UniTask PerformMovement(Vector3 targetPosition, Quaternion targetRotation)
        {
            var distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            var journeyTime = distanceToTarget / speed;
            var elapsedTime = 0f;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            while (elapsedTime < journeyTime)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / journeyTime);

                transform.position = Vector3.Lerp(startPos, targetPosition, t);
                transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);

                await UniTask.Yield();
            }

            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }

        private void AdjustFinalRotation(bool lastMovementFromHead)
        {
            if (lastMovementFromHead)
            {
                if (!previousTrainCarMovementController)
                {
                    var tailRotation = CalculateRotation(transform.position,
                        nextTrainCarMovementController.transform.position);
                    transform.DORotateQuaternion(tailRotation, 0.05f);
                }
            }
            else
            {
                if (!nextTrainCarMovementController)
                {
                    var headRotation = CalculateRotation(transform.position,
                        previousTrainCarMovementController.transform.position);
                    headRotation *= Quaternion.Euler(0, 180, 0);
                    transform.DORotateQuaternion(headRotation, 0.05f);
                }
            }
        }

        public bool IsMoving() => _isProcessingMovement;
    }

    public struct TrainMovement
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public bool IsForwardMovement;

        public TrainMovement(Vector3 pos, Quaternion rot, bool isForwardMovement)
        {
            Position = pos;
            Rotation = rot;
            IsForwardMovement = isForwardMovement;
        }
    }

    public enum TrainColor
    {
        Blue,
        Green,
        Orange,
        Purple,
        Red,
    }
}