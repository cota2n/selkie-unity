using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Selkie.Scripts.Common
{
    public class WaitAll : IEnumerator
    {
        private class Inner : IEnumerator
        {
            private Coroutine _coroutine;
            private readonly IEnumerator _origin;

            public Inner(IEnumerator origin)
            {
                _origin = origin.Flatten();
            }
            
            bool IEnumerator.MoveNext()
            {
                if (_coroutine != null)
                {
                    return true;
                }

                var next = _origin.MoveNext();
                if (next && _origin.Current is YieldInstruction instruction)
                {
                    _coroutine = GlobalCoroutine.StartTask(InnerCoroutine());

                    IEnumerator InnerCoroutine()
                    {
                        yield return instruction;
                        _coroutine = null;
                    }

                    return true;
                }

                return next;
            }

            void IEnumerator.Reset()
            {
                _origin.Reset();
            }

            object IEnumerator.Current => _origin.Current;
        }

        private readonly IEnumerator[] _coroutines;
        public WaitAll(IEnumerable<IEnumerator> coroutines)
        {
            _coroutines = coroutines.Select(x => new Inner(x)).ToArray<IEnumerator>();
        }
        
        public WaitAll(params IEnumerator[] coroutines)
        {
            _coroutines = coroutines.Select(x => new Inner(x)).ToArray<IEnumerator>();
        }
        
        bool IEnumerator.MoveNext()
        {
            var ret = false;
            for (var i = 0; i < _coroutines.Length; i++)
            {
                ret |= _coroutines[i].MoveNext();
            }

            return ret;
        }

        void IEnumerator.Reset()
        {
            foreach (var t in _coroutines)
            {
                t.Reset();
            }
        }

        object IEnumerator.Current => new System.NotSupportedException();
    }
}
