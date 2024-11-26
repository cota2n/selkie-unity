using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Selkie.Scripts.UI
{
    [RequireComponent(typeof(FitScreen))]
    public class ScreenButton : Graphic, IPointerDownHandler
    {
        [SerializeField] UnityEvent onTap;
        public UnityEvent OnTap => onTap;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnTap.Invoke();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
