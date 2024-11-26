using UnityEngine;

namespace Selkie.Scripts.Common
{
    public abstract class SingletonObject<T> : SingletonObjectOrigin where T : MonoBehaviour
    {
        private static T _instance;
        // ReSharper disable once StaticMemberInGenericType
        private static bool _instanced;

        public static T Instance
        {
            get
            {
                if (!_instanced)
                {
                    _instance = OriginGameObject.AddComponent(typeof(T)) as T;
                    _instanced = true;
                }

                return _instance;
            }
        }

    }
}
