using System.Collections;
using UnityEngine;

namespace Selkie.Scripts.Common
{
    public class GlobalCoroutine : SingletonObject<GlobalCoroutine>
    {
        public static Coroutine StartTask(IEnumerator coroutine)
        {
            return Instance.StartCoroutine(coroutine);
        }

        public static void StopTask(Coroutine coroutine)
        {
            Instance.StopCoroutine(coroutine);
        }
    }

}
