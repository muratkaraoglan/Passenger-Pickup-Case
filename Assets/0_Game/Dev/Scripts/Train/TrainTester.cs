using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _0_Game.Dev.Scripts.Train
{
    public class TrainTester : MonoBehaviour
    {
        [FormerlySerializedAs("testCar")] public TrainCarMovementController testCarMovementController;
        public Vector3 direction;
        public Vector3 initialMouseInput;
        public float inputThresholdSqr = 100f;
        [SerializeField] private float gridSize = 1f;
        private bool _move;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                initialMouseInput = Input.mousePosition;
            }

            if (Input.GetMouseButton(0))
            {
                var newMousePosition = Input.mousePosition;
                var delta = newMousePosition - initialMouseInput;
                if (delta.sqrMagnitude < inputThresholdSqr) return;

                direction = Vector3.zero;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    direction.x = delta.x > 0 ? gridSize : -gridSize;
                }
                else
                {
                    direction.z = delta.y > 0 ? gridSize : -gridSize;
                }

                initialMouseInput = newMousePosition;
                _move = true;
            }

            if (_move)
            {
                _move = false;
                Vector3 newPosition = testCarMovementController.transform.position + direction;

                testCarMovementController.EnqueueTrainMovement(newPosition,
                    Quaternion.LookRotation(direction, Vector3.up), true);
            }
        }
    }
}