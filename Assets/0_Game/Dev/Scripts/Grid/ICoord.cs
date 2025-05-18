using UnityEngine;

namespace _0_Game.Dev.Scripts.Grid
{
    public interface ICoord
    {
        public float GetDistance(ICoord other);
        public Vector3 Position { get; set; }
    }
}