using System;

namespace _0_Game.Dev.Scripts.Events
{
    public class InputEvents
    {
        public event Action OnMouseUp;

        public void RaiseMouseUp()
        {
            OnMouseUp?.Invoke();
        }
    }
}