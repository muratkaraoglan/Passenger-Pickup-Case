using _0_Game.Dev.Scripts.Helper;
using UnityEngine;

namespace _0_Game.Dev.Scripts.Events
{
    public class GameEventsManager : Singleton<GameEventsManager>
    {
        public InputEvents InputEvents;

        protected override void Awake()
        {
            Configure(config =>
            {
                config.Lazy = true;
                config.DestroyOthers = true;
                config.Persist = true;
            });
            base.Awake();
            InputEvents = new InputEvents();
        }
    }
}