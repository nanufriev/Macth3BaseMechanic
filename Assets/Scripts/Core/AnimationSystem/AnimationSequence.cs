using System.Collections.Generic;
using Core.AnimationSystem.Interfaces;
using Cysharp.Threading.Tasks;

namespace Core.AnimationSystem
{
    public class AnimationSequence : IAnimationSequence
    {
        private readonly Queue<IExecutableAnimation> _animationElements = new();
        public bool WaitForCompletion { get; private set; }

        public AnimationSequence()
        {
            WaitForCompletion = false;
        }

        public AnimationSequence(bool waitForCompletion)
        {
            WaitForCompletion = waitForCompletion;
        }
        
        public void AddAnimationElement(IExecutableAnimation batch)
        {
            _animationElements.Enqueue(batch);
        }

        public void Clear()
        {
            _animationElements.Clear();
        }

        public async UniTask Execute()
        {
            IExecutableAnimation element;
            
            while (_animationElements.Count > 0)
            {
                element = _animationElements.Dequeue();
                
                if (element.WaitForCompletion)
                    await element.Execute();
                else 
                    element.Execute().Forget();
            }
        }
    }
}
