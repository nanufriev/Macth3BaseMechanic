using Core.AnimationSystem.Interfaces;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.AnimationSystem.AnimationTasks.TweenAnimationTasks
{
    public abstract class TweenAnimationTaskBase : IAnimationTask
    {
        public bool WaitForCompletion { get; protected set; }
        public Transform Target { get; protected set; }
        public Vector3 StartPosition { get; protected set; }
        public Vector3 Destination { get; protected set; }
        public float Duration { get; protected set; }
        public Ease EaseType { get; protected set; }

        protected TweenAnimationTaskBase(
            Vector3 startPosition, 
            Vector3 destination, 
            float duration,
            bool waitForCompletion = false, 
            Ease easeType = Ease.Linear)
        {
            StartPosition = startPosition;
            Destination = destination;
            Duration = duration;
            EaseType = easeType;
            WaitForCompletion = waitForCompletion;
        }

        public abstract UniTask Execute();
    }
}