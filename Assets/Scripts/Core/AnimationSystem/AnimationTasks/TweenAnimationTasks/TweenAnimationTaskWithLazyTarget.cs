using System;
using Core.Helpers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.AnimationSystem.AnimationTasks.TweenAnimationTasks
{
    public class TweenAnimationTaskWithLazyTarget : TweenAnimationTaskBase
    {
        public UniTask<Transform> GetTransformTask { get; protected set; }
        
        public TweenAnimationTaskWithLazyTarget(
            UniTask<Transform> getTransformTask,
            Vector3 startPosition, 
            Vector3 destination, 
            float duration, 
            bool waitForCompletion = false,
            Ease easeType = Ease.Linear) : base(startPosition, destination, duration, waitForCompletion, easeType)
        {
            GetTransformTask = getTransformTask;
        }

        public override async UniTask Execute()
        {
            Target = await GetTransformTask;
            if (Target == null)
                throw new Exception("Something went wrong during try to get Target from Task");
            
            Target.position = StartPosition;
            await Target.DOMove(Destination, Duration).SetEase(EaseType).ToUniTask();
        }
    }
}
