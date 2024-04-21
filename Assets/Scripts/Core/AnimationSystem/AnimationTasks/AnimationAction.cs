using System;
using Core.AnimationSystem.Interfaces;
using Cysharp.Threading.Tasks;

namespace Core.AnimationSystem.AnimationTasks
{
    public class AnimationAction : IExecutableAnimation
    {
        private readonly Action _action;
        public bool WaitForCompletion => true;

        public AnimationAction(Action action)
        {
            _action = action;
        }
        
        public UniTask Execute()
        {
            _action?.Invoke();
            return default;
        }
    }
}

