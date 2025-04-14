using System;
using System.Collections;
using Selkie.Scripts.Common;
using UnityEngine;

namespace Selkie.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Fade : MonoBehaviour
    {
        [SerializeField] private bool autoActive = true;
        [SerializeField] float defaultDuration = 0.1f;
        
        public bool IsShowing { get; private set; }
        
        CanvasGroup canvasGroup;
        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            if (autoActive)
            {
                canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
                IsShowing = false;
            }
            else
            {
                // activeの状態見て設定
                var isActive = gameObject.activeSelf;
                IsShowing = isActive;
                canvasGroup.alpha = isActive ? 1f : 0f;
            }
        }

        public void SnapIn()
        {
            canvasGroup.alpha = 1f;
            gameObject.SetActive(true);
            IsShowing = true;
        }

        public void SnapOut()
        {
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            IsShowing = false;
        }

        public IEnumerator FadeIn()
        {
            return FadeIn(defaultDuration);
        }

        public IEnumerator FadeIn(float duration)
        {
            IsShowing = true;
            yield return new LerpTo(canvasGroup.alpha, 1.0f, duration, x => canvasGroup.alpha = x);
            if (autoActive)
            {
                gameObject.SetActive(true);
            }
        }

        public IEnumerator FadeOut()
        {
            return FadeOut(defaultDuration);
        }

        public IEnumerator FadeOut(float duration)
        {
            IsShowing = false;
            yield return new LerpTo(canvasGroup.alpha, 0.0f, duration, x => canvasGroup.alpha = x);
            if (autoActive)
            {
                gameObject.SetActive(false);
            }
        }
    }
}