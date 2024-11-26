using UnityEngine;

namespace Selkie.Scripts.UI
{
    public class FitScreen : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Transform parent;

        void Start()
        {
            rectTransform = (RectTransform)transform;
            rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
            parent = GetComponentInParent<Canvas>().transform;
        }

        void LateUpdate()
        {
            rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
            rectTransform.position = parent.position;
        }
    }
}