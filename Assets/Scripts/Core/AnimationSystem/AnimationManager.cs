using System;
using System.Collections.Generic;
using Core.AnimationSystem.AnimationFactory;
using Core.AnimationSystem.AnimationTasks;
using Core.AnimationSystem.AnimationTasks.TweenAnimationTasks;
using Core.AnimationSystem.Interfaces;
using Core.AnimationSystem.Settings;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.AnimationSystem
{
    public class AnimationManager : IExecutable
    {
        private readonly Queue<IAnimationSequence> _animationSequences = new();
        private readonly TweenAnimationFactory _tweenAnimationFactory = new();
        private readonly AnimationConfig _animationConfig;

        public AnimationManager(AnimationConfig animationConfig)
        {
            _animationConfig = animationConfig;
        }

        public void AddAnimationSequence(IAnimationSequence animationSequence)
        {
            _animationSequences.Enqueue(animationSequence);
        }

        public async UniTask Execute()
        {
            while (_animationSequences.Count > 0)
            {
                await _animationSequences.Dequeue().Execute();
            }
        }

        public IAnimationBatch CreateTilesSwapBatch(Transform tile1, Transform tile2, int pos1X, int pos1Y, int pos2X,
            int pos2Y)
        {
            var batch = _tweenAnimationFactory.CreateBatch(true);
            var anim1 = _tweenAnimationFactory.CreateTweenAnimationTask(
                tile1, new Vector3(pos1X, pos1Y), new Vector3(pos2X, pos2Y),
                _animationConfig.SwapAnimDuration, false, Ease.InOutQuad);
            var anim2 = _tweenAnimationFactory.CreateTweenAnimationTask(
                tile2, new Vector3(pos2X, pos2Y), new Vector3(pos1X, pos1Y),
                _animationConfig.SwapAnimDuration, true, Ease.InOutQuad);
            batch.AddAnimationElement(anim1);
            batch.AddAnimationElement(anim2);
            return batch;
        }

        public IAnimationSequence CreateSequence()
        {
            return _tweenAnimationFactory.CreateSequence();
        }

        public IAnimationBatch CreateBatch(bool waitForCompletion)
        {
            return _tweenAnimationFactory.CreateBatch(waitForCompletion);
        }

        public AnimationDelay CreateAnimationDelay(float delay)
        {
            return _tweenAnimationFactory.CreateAnimationDelay(delay);
        }
        
        public AnimationAction CreateAnimationAction(Action action)
        {
            return _tweenAnimationFactory.CreateAnimationAction(action);
        }

        public TweenAnimationTask CreateTweenAnimationTask(Transform target, Vector3 startPosition, Vector3 destination,
            float duration, bool waitForCompletion = false, Ease easeType = Ease.Linear)
        {
            return _tweenAnimationFactory.CreateTweenAnimationTask(
                target, startPosition, destination, duration, waitForCompletion, easeType);
        }

        public TweenAnimationTaskWithLazyTarget CreateTweenAnimationTaskWithLazyTarget(
            UniTask<Transform> getTransformTask, Vector3 startPosition, Vector3 destination, float duration,
            bool waitForCompletion = false, Ease easeType = Ease.Linear)
        {
            return _tweenAnimationFactory.CreateTweenAnimationTaskWithLazyTarget(
                getTransformTask, startPosition, destination, duration, waitForCompletion, easeType);
        }
    }
}