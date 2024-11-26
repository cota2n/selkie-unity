using System.Collections;
using UnityEngine;

namespace Selkie.Scripts.StateMachine
{
    public abstract class CoroutineMonoBehavior : MonoBehaviour, IBehavior
    {
        public StateMachine Machine { get; set; }
        public Context RegionContext { get; set; }

        private Coroutine _currentCoroutine;
        private bool _doneCoroutine;
        protected abstract string FinishTrigger { get; }

        protected abstract IEnumerator Coroutine();
        protected abstract void OnTerminate();

        public void Enter()
        {
            _currentCoroutine = StartCoroutine(InnerCoroutine());
            IEnumerator InnerCoroutine()
            {
                yield return Coroutine();
                _doneCoroutine = true;
            }
        }

        // ReSharper disable once Unity.IncorrectMethodSignature
        public void UpdateBehavior(float deltaTime, float elapsedTime)
        {
            if (_doneCoroutine)
            {
                Machine.DoTrigger(FinishTrigger);
            }
        }

        public void Exit()
        {
            if (!_doneCoroutine)
            {
                StopCoroutine(_currentCoroutine);
                OnTerminate();
            }
        }
    }
}
