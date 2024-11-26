using UnityEngine;

namespace Selkie.Scripts.Common
{
    public abstract class SingletonObjectOrigin : MonoBehaviour
    {
        private static GameObject _singletonGameObject;
        private static bool _instanced;

        protected static GameObject OriginGameObject
        {
            get
            {
                if (!_instanced)
                {
                    _singletonGameObject = new GameObject("Singleton");
                    _instanced = true;
                    DontDestroyOnLoad(_singletonGameObject);
                }

                return _singletonGameObject;
            }
        }
    }
}
