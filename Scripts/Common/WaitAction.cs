using System;
using System.Collections;

namespace Selkie.Scripts.Common
{
    public class WaitAction : IEnumerator
    {
        public WaitAction(Action<Action> action)
        {
            action(Callback);
        }

        private void Callback()
        {
            called = true;
        }

        private bool called;

        public bool MoveNext()
        {
            return !called;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public object Current => throw new NotSupportedException();
    }
}