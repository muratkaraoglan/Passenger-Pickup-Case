using System;
using System.Collections.Generic;
using _0_Game.Dev.Scripts.Grid;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Train
{
    public class TrainInputManager : MonoBehaviour
    {
        [SerializeField] private float gridSize = 1f;
        [SerializeField] private float inputThresholdSqr = 100f;
        [SerializeField] private float pathPreviewFrequency = 0.2f; // How often to update path preview while dragging

        private Vector3 _direction;
        private Vector3 _dragStartPosition;
        private Vector3 _dragCurrentPosition;
        private TrainCarMovementController _selectedTrainCar;
        private Camera _mainCamera;
        private bool _isDragging;
        private float _lastPathUpdateTime;
        private List<NodeBase> _currentPath = new List<NodeBase>();

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
                        _selectedTrainCar.EnqueueTrainMovement(node.Coord.Position, Quaternion.identity);
                    }
                }

                _selectedTrainCar = null;
            }
        }
        
        private Vector3 SnapToGrid(Vector3 position)
        {
            return new Vector3(
                Mathf.Round(position.x / gridSize) * gridSize,
                position.y,
                Mathf.Round(position.z / gridSize) * gridSize
            );
        }

        private Quaternion CalculateRotation(Vector3 from, Vector3 to)
        {
            if ((to - from).sqrMagnitude < 0.001f)
                return Quaternion.identity;

            Vector3 direction = (to - from).normalized;
            return Quaternion.LookRotation(direction,Vector3.up);
        }
    }
}