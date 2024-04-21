using Core.Helpers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.AnimationSystem.AnimationTasks.TweenAnimationTasks
{
    public class TweenAnimationTask : TweenAnimationTaskBase
    {
        public TweenAnimationTask(
            Transform target,
            Vector3 startPosition,
            Vector3 destination,
            float duration,
            bool waitForCompletion = false,
            Ease easeType = Ease.Linear) : base(startPosition, destination, duration, waitForCompletion, easeType)
        {
            Target = target;
        }

        public override async UniTask Execute()
        {
            Target.position = StartPosition;
            await Target.DOMove(Destination, Duration).SetEase(EaseType).ToUniTask();
        }
    }
}