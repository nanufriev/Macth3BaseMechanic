using Core.AnimationSystem.AnimationTasks.TweenAnimationTasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.AnimationSystem.AnimationFactory
{
    public class TweenAnimationFactory : AnimationFactoryBase
    {
        public TweenAnimationTask CreateTweenAnimationTask(Transform target, Vector3 startPosition, Vector3 destination,
            float duration, bool waitForCompletion = false, Ease easeType = Ease.Linear)
        {
            return new TweenAnimationTask(target, startPosition, destination, duration, waitForCompletion, easeType);
        }

        public TweenAnimationTaskWithLazyTarget CreateTweenAnimationTaskWithLazyTarget(
            UniTask<Transform> getTransformTask, Vector3 startPosition, Vector3 destination, float duration,
            bool waitForCompletion = false, Ease easeType = Ease.Linear)
        {
            return new TweenAnimationTaskWithLazyTarget(
                getTransformTask, startPosition, destination, duration, waitForCompletion, easeType);
        }
    }
}
