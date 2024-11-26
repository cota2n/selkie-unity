using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Selkie.Scripts.Common
{
    [RequireComponent(typeof(Animator))]
    public class TransitionAnimation : MonoBehaviour
    {
        [SerializeField] private AnimationClip inAnimation;
        [SerializeField] private AnimationClip outAnimation;
        [SerializeField] private bool autoDisable;
        [SerializeField] private Animator animator;

        private PlayableGraph _playableGraph;

        public IEnumerator In()
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }

            _playableGraph = PlayableGraph.Create();

            var playableClip = AnimationClipPlayable.Create(_playableGraph, inAnimation);
            var output = AnimationPlayableOutput.Create(_playableGraph, "Animation", animator);
            output.SetSourcePlayable(playableClip);

            _playableGraph.Play();
            gameObject.SetActive(true);

            yield return new WaitWhile(() => playableClip.GetTime() < playableClip.GetAnimationClip().length);
        }

        public IEnumerator Out()
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }

            _playableGraph = PlayableGraph.Create();

            var playableClip = AnimationClipPlayable.Create(_playableGraph, outAnimation);
            var output = AnimationPlayableOutput.Create(_playableGraph, "Animation", animator);
            output.SetSourcePlayable(playableClip);

            _playableGraph.Play();

            yield return new WaitWhile(() => playableClip.GetTime() < playableClip.GetAnimationClip().length);
            gameObject.SetActive(!autoDisable);
        }

        private void Awake()
        {
            gameObject.SetActive(!autoDisable);
        }

        private void OnValidate()
        {
            animator = GetComponent<Animator>();
        }

        private void OnDestroy()
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }
        }
    }
}
