using System.Collections.Generic;
using _0_Game.Dev.Scripts.Level;
using _0_Game.Dev.Scripts.Managers;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Passenger
{
    public class PassageBuilder
    {
        private int _gridXOffset;
        private int _gridZOffset;
        private PassengerGate _passengerGatePrefab;
        private List<PassengerQueue> _passengerQueues;
        private GameObject _passageModelPrefab;

        public PassageBuilder(int gridXOffset,
            int gridZOffset, List<PassengerQueue> passengerQueues, PassengerGate passengerGatePrefab,
            GameObject passageModelPrefab)
        {
            _gridXOffset = gridXOffset;
            _gridZOffset = gridZOffset;
            _passengerGatePrefab = passengerGatePrefab;
            _passengerQueues = passengerQueues;
            _passageModelPrefab = passageModelPrefab;
        }

        public void BuildPassages()
        {
            foreach (var queue in _passengerQueues)
            {
                var firstPassenger = queue.passengers[0];
                var passagePosition = new Vector3(queue.gridPosition.x + _gridXOffset, 0,
                    queue.gridPosition.y + _gridZOffset);
                var passageRotation = PassengerSideToQuaternion(queue.side);
                var passengerGate = Object.Instantiate(_passengerGatePrefab,
                    passagePosition
                    , Quaternion.identity);
                //modelVariation = ModelVariationManager.Instance.GetModelVariation(firstPassenger.color);
                var passageModelInstance =
                    Object.Instantiate(_passageModelPrefab,
                        Vector3.zero + PassengerSideToPositionOffset(firstPassenger.side), passageRotation);
                // var passageMeshRenderer = passageModelInstance.GetComponent<MeshRenderer>();
                // var materials = passageMeshRenderer.materials;
                // materials[1] = modelVariation.material;
                // passageMeshRenderer.materials = materials;
                passageModelInstance.transform.SetParent(passengerGate.transform, false);
                var passengerControllers = new List<PassengerController>();
                foreach (var passenger in queue.passengers)
                {
                    var modelVariation = ModelVariationManager.Instance.GetModelVariation(passenger.color);
                    var position = passagePosition +
                                   PassengerSideToPositionOffset(passenger.side) * (passenger.order + 2);
                    PassengerController passengerInstance =
                        Object.Instantiate(modelVariation.characterPrefab, passengerGate.transform);
                    passengerInstance.transform.position = position;
                    passengerInstance.transform.localRotation = passageRotation;
                    passengerControllers.Add(passengerInstance);
                }

                passengerGate.Init(passengerControllers,passageModelInstance);
            }
        }

        private Quaternion PassengerSideToQuaternion(PassengerSide side)
        {
            switch (side)
            {
                case PassengerSide.Left:
                    return Quaternion.Euler(0, 90, 0);
                case PassengerSide.Top:
                    return Quaternion.Euler(0, 180, 0);
                case PassengerSide.Right:
                    return Quaternion.Euler(0, 270, 0);
                case PassengerSide.Bottom:
                    return Quaternion.identity;
                default: return Quaternion.identity;
            }
        }

        private Vector3 PassengerSideToPositionOffset(PassengerSide side)
        {
            switch (side)
            {
                case PassengerSide.Left:
                    return new Vector3(-0.6f, 0, 0);
                case PassengerSide.Top:
                    return new Vector3(0, 0, 0.6f);
                case PassengerSide.Right:
                    return new Vector3(0.6f, 0, 0);
                case PassengerSide.Bottom:
                    return new Vector3(0, 0, -0.6f);
                default: return Vector3.zero;
            }
        }
    }
}