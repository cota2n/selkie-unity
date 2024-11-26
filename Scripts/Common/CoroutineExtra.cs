using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Selkie.Scripts.Common
{
    public static class CoroutineExtra
    {
        public static IEnumerator Flatten(this IEnumerator root)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var coroutine = stack.Peek();
                var next = coroutine.MoveNext();

                if (next && coroutine.Current is YieldInstruction yieldInstruction)
                {
                    yield return yieldInstruction;
                }
                else if (next && coroutine.Current is IEnumerator nest)
                {
                    stack.Push(nest);
                    yield return null;
                }
                else if (next && coroutine.Current != null)
                {
                    yield return coroutine.Current;
                }
                else if (next)
                {
                    yield return null;
                }
                else
                {
                    stack.Pop();
                }
            }
        }
    }

    public class LerpTo : IEnumerator
    {
        private float From { get; set; }
        private float To { get; set; }
        private float Duration { get; set; }
        private float Elapsed { get; set; }
        private int LastFrameCount { get; set; } = -1;
        System.Action<float> Callback { get; set; }
        
        public LerpTo(float from, float to, float duration, System.Action<float> callback)
        {
            From = from;
            To = to;
            Duration = duration;
            Callback = callback;
        }

        bool IEnumerator.MoveNext()
        {
            var tmp = LastFrameCount;
            LastFrameCount = Time.frameCount;
            if (tmp == Time.frameCount)
            {
                return true;
            }

            var progress = Mathf.Clamp01(Elapsed / Duration);
            Callback.Invoke(Mathf.Lerp(From, To, progress));
            var next = Elapsed < Duration;
            Elapsed += Time.deltaTime;

            return next;
        }

        void IEnumerator.Reset()
        {
            throw new System.NotImplementedException();
        }

        object IEnumerator.Current => null;
    }

}
