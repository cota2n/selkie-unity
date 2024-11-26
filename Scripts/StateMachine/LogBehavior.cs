using UnityEngine;

namespace Selkie.Scripts.StateMachine
{
    public class LogBehavior : IBehavior
    {
        public StateMachine Machine { get; set; }
        public Context RegionContext { get; set; }

        public LogBehavior(string onEnter, string onExit = default)
        {
            OnEnter = onEnter;
            OnExit = onExit;
        }

        private string OnEnter { get; set; }
        private string OnExit { get; set; }

        public void Enter()
        {
            if (!string.IsNullOrEmpty(OnEnter))
            {
                Debug.Log($"On Enter: {OnEnter}");
            }
        }

        public void UpdateBehavior(float deltaTime, float elapsedTime)
        {
            
        }

        public void Exit()
        {
            if (!string.IsNullOrEmpty(OnExit))
            {
                Debug.Log($"On Enter: {OnExit}");
            }
        }
    }
}