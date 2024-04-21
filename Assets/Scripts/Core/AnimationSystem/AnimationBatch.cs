using System.Collections.Generic;
using Core.AnimationSystem.Interfaces;
using Cysharp.Threading.Tasks;

namespace Core.AnimationSystem
{
    public class AnimationBatch : IAnimationBatch
    {
        public bool WaitForCompletion { get; }
   
        private readonly Queue<IExecutableAnimation> _animationTasks = new();

        public AnimationBatch(bool waitForCompletion)
        {
            WaitForCompletion = waitForCompletion;
        }
        
        public void AddAnimationElement(IExecutableAnimation task)
        {
            _animationTasks.Enqueue(task);
        }

        public void Clear()
        {
            _animationTasks.Clear();
        }

        public bool IsEmpty()
        {
            return _animationTasks.Count == 0;
        }

        public async UniTask Execute()
        {
            if (WaitForCompletion)
            {
                foreach (var animationTask in _animationTasks)
                {
                    if (animationTask.WaitForCompletion)
                        await animationTask.Execute();
                    else
                        animationTask.Execute().Forget();
                }
            }
            else
            {
                UniTask.WhenAll(_animationTasks.Select(x => x.Execute())).Forget();
            }

            Clear();
        }
    }
}
