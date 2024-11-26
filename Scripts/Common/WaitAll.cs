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
            
            public bool MoveNext()
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

            public void Reset()
            {
                throw new System.NotImplementedException();
            }

            public object Current => new System.NotImplementedException();
        }
        
        private readonly Inner[] _coroutines;
        public WaitAll(IEnumerable<IEnumerator> coroutines)
        {
            _coroutines = coroutines.Select(x => new Inner(x)).ToArray();
        }
        
        public WaitAll(params IEnumerator[] coroutines)
        {
            _coroutines = coroutines.Select(x => new Inner(x)).ToArray();
        }
        
        public bool MoveNext()
        {
            var ret = false;
            for (var i = 0; i < _coroutines.Length; i++)
            {
                ret |= _coroutines[i].MoveNext();
            }

            return ret;
        }

        public void Reset()
        {
            for (var i = 0; i < _coroutines.Length; i++)
            {
                _coroutines[i].Reset();
            }
        }

        public object Current => null;
    }
}
