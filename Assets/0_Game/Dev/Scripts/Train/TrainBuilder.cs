using System.Collections.Generic;
using _0_Game.Dev.Scripts.Managers;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Train
{
    public class TrainBuilder
    {
        private readonly int _startXOffset;
        private readonly int _startYOffset;

        public TrainBuilder(int width, int height)
        {
            _startXOffset = -width / 2;
            _startYOffset = -height / 2;
        }

        public void BuildTrains(List<TrainHolder> trains)
        {
            foreach (var trainHolder in trains)
            {
                Build(trainHolder);
            }
        }

        private void Build(TrainHolder trainHolder)
        {
            var parent = new GameObject("Train " + trainHolder.trainColor);
            parent.AddComponent<TrainManager>().trainColor = trainHolder.trainColor;
            var trainVariation =
                TrainAndCharacterVariationManager.Instance.GetTrainAndCharacterVariationByTrainColor(trainHolder
                    .trainColor);
            var count = trainHolder.wagons.Count;

            // spawn head
            var head = Object.Instantiate(trainVariation.trainHeadPrefab, parent.transform);
            ApplyPositionAndRotation(head.transform, trainHolder.wagons[0].coord, trainHolder.headDirection);

            var currentMovementController = head.GetComponent<TrainCarMovementController>();
            currentMovementController.canInteractWithInput = true;
            //spawn bodies
            for (int i = 1; i < count - 1; i++)
            {
                var wagon = Object.Instantiate(trainVariation.trainBodyPrefab, parent.transform);
                SetPositionAndRotation(trainHolder, i, wagon);
                currentMovementController = SetConnection(currentMovementController, wagon);
            }

            //spawn tail
            var tail = Object.Instantiate(trainVariation.trainTailPrefab, parent.transform);
            SetPositionAndRotation(trainHolder, count - 1, tail);
            SetConnection(currentMovementController, tail);
            tail.GetComponent<TrainCarMovementController>().canInteractWithInput = true;
        }

        private void SetPositionAndRotation(TrainHolder trainHolder, int i, GameObject wagon)
        {
            var currentCoord = trainHolder.wagons[i].coord;
            var lastWagonDir = LookDirectionVector2(trainHolder.wagons[i - 1].coord, currentCoord);
            var direction = VectorToDirection(lastWagonDir);
            ApplyPositionAndRotation(wagon.transform, currentCoord, direction);
        }

        private Vector2Int LookDirectionVector2(Vector2Int previousCoord, Vector2Int currentCoord)
        {
            return previousCoord - currentCoord;
        }

        private void ApplyPositionAndRotation(Transform transform, Vector2 coord, Direction direction)
        {
            var position = new Vector3(_startXOffset + coord.x, 0, _startYOffset + coord.y);
            var rotation = DirectionToRotation(direction);
            transform.SetPositionAndRotation(position, rotation);
        }

        private TrainCarMovementController SetConnection(TrainCarMovementController current, GameObject wagon)
        {
            var wagonMovementController = wagon.GetComponent<TrainCarMovementController>();
            current.SetPreviousTrainCarMovementController(wagonMovementController);
            wagonMovementController.SetNextTrainCarMovementController(current);
            return wagonMovementController;
        }

        private Quaternion DirectionToRotation(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Quaternion.Euler(0, 0, 0);
                case Direction.Right: return Quaternion.Euler(0, 90, 0);
                case Direction.Down: return Quaternion.Euler(0, 180, 0);
                case Direction.Left: return Quaternion.Euler(0, 270, 0);
                default: return Quaternion.identity;
            }
        }

        private Direction VectorToDirection(Vector2Int dir)
        {
            if (dir == Vector2Int.down) return Direction.Down;
            if (dir == Vector2Int.left) return Direction.Left;
            if (dir == Vector2Int.right) return Direction.Right;
            return Direction.Up;
        }
    }
}