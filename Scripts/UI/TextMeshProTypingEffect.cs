using System.Collections;
using TMPro;
using UnityEngine;

namespace Selkie.Scripts.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextMeshProTypingEffect : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float waitSec = 0.1f;

        private bool _skipped;

        private void OnValidate()
        {
            if (text == null)
            {
                text = GetComponent<TextMeshProUGUI>();
            }
        }

        public bool IsTyping { get; private set; }

        public void Skip()
        {
            _skipped = true;
        }

        public IEnumerator StartEffect()
        {
            int count = 1;
            int maxCharacters = text.text.Length;
            _skipped = false;
            IsTyping = true;

            do
            {
                if (_skipped)
                {
                    break;
                }

                text.maxVisibleCharacters = count;
                yield return new WaitForSeconds(waitSec);
                ++count;
            } while (count < maxCharacters);

            text.maxVisibleCharacters = text.text.Length;

            IsTyping = false;
        }
    }
}
