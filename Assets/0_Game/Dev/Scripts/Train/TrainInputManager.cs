using System;
using System.Collections.Generic;
using _0_Game.Dev.Scripts.Grid;
using UnityEngine;
using UnityEngine.Serialization;

namespace _0_Game.Dev.Scripts.Train
{
    public class TrainInputManager : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private float inputThresholdSqr = 100f;
        [SerializeField] private float pathPreviewFrequency = 0.2f; // How often to update path preview while dragging

        private Vector3 _direction;
        [SerializeField] private Vector3 _dragStartPosition;
        [SerializeField] private Vector3 _dragCurrentPosition;
        private TrainCarMovementController _selectedTrainCar;
        private Camera _mainCamera;
        private bool _isDragging;
        private float _lastPathUpdateTime;

        public bool Drag;
        private Plane _plane = new(Vector3.up, Vector3.zero);

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var trainCar = hit.transform.GetComponent<TrainCarMovementController>();
                    if (trainCar != null)
                    {
                        _selectedTrainCar = trainCar;
                        _dragStartPosition = trainCar.transform.position;
                        _dragStartPosition = SnapToGrid(_dragStartPosition);
                    }
                }
            }

            HandleDragMovement();

            if (Input.GetMouseButtonUp(0))
            {
                _selectedTrainCar = null;
                _dragStartPosition = _dragCurrentPosition;
            }
        }

        private void HandleDragMovement()
        {
            if (Input.GetMouseButton(0) && _selectedTrainCar)
            {
                var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

                if (!_plane.Raycast(ray, out var entry)) return;
                _dragCurrentPosition = ray.GetPoint(entry);
                _dragCurrentPosition = SnapToGrid(_dragCurrentPosition);

                var sqrDistance = (_dragStartPosition - _dragCurrentPosition).sqrMagnitude;

                var isValidInput = sqrDistance >= inputThresholdSqr;

                if (!isValidInput) return;

                _dragCurrentPosition = GridManager.Instance.ClampPosition(_dragCurrentPosition);

                if (_dragStartPosition == _dragCurrentPosition) return;

                var path = GridManager.Instance.TryFindPath(_dragStartPosition, _dragCurrentPosition);
                if (path.Count > 0)
                {
                    foreach (var node in path)
                    {
                        _selectedTrainCar.EnqueueTrainMovement(node.Coord.Position, Quaternion.identity,
                            !_selectedTrainCar.isTail);
                    }
                    
                    _dragStartPosition = _dragCurrentPosition;
                }
            }
        }

        private void MouseDownUpMovement()
        {
            if (Input.GetMouseButtonUp(0) && _selectedTrainCar)
            {
                var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

                if (!_plane.Raycast(ray, out var entry)) return;
                _dragCurrentPosition = ray.GetPoint(entry);
                _dragCurrentPosition = SnapToGrid(_dragCurrentPosition);

                var path = GridManager.Instance.TryFindPath(_dragStartPosition, _dragCurrentPosition);
                if (path.Count > 0)
                {
                    foreach (var node in path)
                    {
                        _selectedTrainCar.EnqueueTrainMovement(node.Coord.Position, Quaternion.identity,
                            !_selectedTrainCar.isTail);
                    }
                }

                _selectedTrainCar = null;
            }
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            return new Vector3(
                Mathf.Round(position.x / cellSize) * cellSize,
                0,
                Mathf.Round(position.z / cellSize) * cellSize
            );
        }
    }
}