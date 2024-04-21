using Core.AnimationSystem.Interfaces;
using Cysharp.Threading.Tasks;

namespace Core.AnimationSystem.AnimationTasks
{
    public class AnimationDelay : IExecutableAnimation
    {
        private readonly float _delay;
        public bool WaitForCompletion => true;

        public AnimationDelay(float delay)
        {
            _delay = delay;
        }
        
        public async UniTask Execute()
        {
            await UniTask.WaitForSeconds(_delay);
        }
    }
}
