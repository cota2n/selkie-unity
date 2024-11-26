using UnityEngine;

namespace Selkie.Scripts.UI
{
    [ExecuteInEditMode]
    public class Gauge : MonoBehaviour
    {
        [SerializeField] private RectTransform parent;
        [SerializeField] private RectTransform fill;
        [SerializeField, Range(0f, 1f)] private float fillValue;
        [SerializeField, Min(0.001f)] private float lag = 0.2f;

        public float Value
        {
            get => fillValue;
            private set => fillValue = value;
        }

        private float InternalValue { get; set; }

        private void Update()
        {
            InternalValue = Mathf.Lerp(InternalValue, Value, (1 / lag) * Time.deltaTime);
            fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parent.sizeDelta.x * InternalValue);
        }

        public void SetValue(float value, bool isSnap)
        {
            value = Mathf.Clamp01(value);
            Value = value;
            if (isSnap)
            {
                InternalValue = value;
            }
        }
    }
}
