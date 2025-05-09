using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Train
{
    public class TrainCar : MonoBehaviour
    {
        [SerializeField] private TrainCar nextTrainCar;
        [SerializeField] private TrainCar previousTrainCar;
        [SerializeField] private float delayTime = .2f;
        [SerializeField] private float speed = 5f;

        private bool _isProcessingMovement;
        private readonly Queue<TrainMovement> _trainMovementQueue = new Queue<TrainMovement>();

        public void EnqueueTrainMovement(Vector3 position, Quaternion rotation)
        {
            _trainMovementQueue.Enqueue(new TrainMovement(position, rotation));

            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            if (!_isProcessingMovement)
            {
                StartCoroutine(ProcessMovements());
            }

            if (previousTrainCar)
            {
                //previousTrainCar.EnqueueTrainMovement( pos, rot);
                StartCoroutine(DelayedPassMovement(pos, rot));
            }
        }

        private IEnumerator DelayedPassMovement(Vector3 position, Quaternion rotation)
        {
            yield return new WaitForSeconds(delayTime);
            previousTrainCar.EnqueueTrainMovement(position, rotation);
        }

        private IEnumerator ProcessMovements()
        {
            _isProcessingMovement = true;

            while (_trainMovementQueue.Count > 0)
            {
                var movement = _trainMovementQueue.Dequeue();
                var rotation = movement.Rotation;
                // if (!previousTrainCar)
                // {
                //     rotation = nextTrainCar.transform.rotation;
                // }

                transform.DORotateQuaternion(rotation, 0.05f);

                while (Vector3.Distance(movement.Position, transform.position) > .01f)
                {
                    transform.position =
                        Vector3.MoveTowards(transform.position, movement.Position, Time.deltaTime * speed);

                    yield return null;
                }

                transform.position = movement.Position;
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
}