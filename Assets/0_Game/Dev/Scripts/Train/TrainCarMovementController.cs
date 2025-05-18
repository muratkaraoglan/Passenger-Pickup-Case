using System;
using System.Collections;
using System.Collections.Generic;
using _0_Game.Dev.Scripts.Grid;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace _0_Game.Dev.Scripts.Train
{
    public class TrainCarMovementController : MonoBehaviour
    {
        [SerializeField] private TrainCarMovementController nextTrainCarMovementController;
        [SerializeField] private TrainCarMovementController previousTrainCarMovementController;
        [SerializeField] private float delayTime = .2f;
        [SerializeField] private float speed = 5f;

        private bool _isProcessingMovement;
        private readonly Queue<TrainMovement> _trainMovementQueue = new Queue<TrainMovement>();
        private TrainMovement _lastTrainMovement;

        private void Start()
        {
            _lastTrainMovement = new TrainMovement();
            _lastTrainMovement.Position = transform.position;
            _lastTrainMovement.Rotation = transform.rotation;
        }

        public void SetPreviousTrainCarMovementController(TrainCarMovementController preCarMovementController) =>
            previousTrainCarMovementController = preCarMovementController;

        public void SetNextTrainCarMovementController(TrainCarMovementController nextCarMovementController) =>
            nextTrainCarMovementController = nextCarMovementController;

        public void EnqueueTrainMovement(Vector3 position, Quaternion rotation)
        {
            var trainMovement = new TrainMovement(position, rotation);
            _trainMovementQueue.Enqueue(trainMovement);

            if (!_isProcessingMovement)
            {
                StartCoroutine(ProcessMovements());
            }

            if (previousTrainCarMovementController)
            {
                previousTrainCarMovementController.EnqueueTrainMovement(_lastTrainMovement.Position,
                    _lastTrainMovement.Rotation);
                //StartCoroutine(DelayedPassMovement(_lastTrainMovement.Position, _lastTrainMovement.Rotation));
            }

            _lastTrainMovement = trainMovement;
        }

        private Quaternion CalculateRotation(Vector3 from, Vector3 to)
        {
            if ((to - from).sqrMagnitude < 0.001f)
                return Quaternion.identity;

            Vector3 direction = (to - from).normalized;
            return Quaternion.LookRotation(direction, Vector3.up);
        }

        private IEnumerator DelayedPassMovement(Vector3 position, Quaternion rotation)
        {
            //yield return new WaitForSeconds(delayTime);
            yield return null;
            previousTrainCarMovementController.EnqueueTrainMovement(position, rotation);
        }

        private IEnumerator ProcessMovements()
        {
            _isProcessingMovement = true;

            while (_trainMovementQueue.Count > 0)
            {
                GridManager.Instance.SetNodeState(transform.position, true);
                var current = _trainMovementQueue.Dequeue();
                var rotation =
                    CalculateRotation(transform.position, current.Position);
                // if (!previousTrainCar)
                // {
                //     rotation = nextTrainCar.transform.rotation;
                // }

                //transform.DORotateQuaternion(rotation, 0.05f);

                // while (Vector3.Distance(current.Position, transform.position) > .01f)
                // {
                //     transform.position =
                //         Vector3.MoveTowards(transform.position, current.Position, Time.deltaTime * speed);
                //     transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime * 100);
                //     yield return null;
                // }
                float distanceToTarget = Vector3.Distance(transform.position, current.Position);
                float journeyTime = distanceToTarget / speed;
                float elapsedTime = 0;
                Vector3 startPos = transform.position;
                Quaternion startRot = transform.rotation;

                while (elapsedTime < journeyTime)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsedTime / journeyTime);

                    transform.position = Vector3.Lerp(startPos, current.Position, t);
                    transform.rotation = Quaternion.Slerp(startRot, rotation, t);

                    yield return null;
                }

                transform.position = current.Position;
                transform.rotation = rotation;
                GridManager.Instance.SetNodeState(current.Position, false);
            }

            if (!previousTrainCarMovementController)
            {
                var tailRotation =
                    CalculateRotation(transform.position, nextTrainCarMovementController.transform.position);
                transform.DORotateQuaternion(tailRotation, 0.05f);
            }

            _isProcessingMovement = false;
        }
    }

    public struct TrainMovement
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public TrainMovement(Vector3 pos, Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
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