using System;
using Core.AnimationSystem.AnimationTasks;
using Core.AnimationSystem.Interfaces;

namespace Core.AnimationSystem.AnimationFactory
{
    public abstract class AnimationFactoryBase
    {
        public IAnimationSequence CreateSequence()
        {
            return new AnimationSequence();
        }
        
        public IAnimationBatch CreateBatch(bool waitForCompletion)
        {
            return new AnimationBatch(waitForCompletion);
        }

        public AnimationDelay CreateAnimationDelay(float delay)
        {
            return new AnimationDelay(delay);
        }
        
        public AnimationAction CreateAnimationAction(Action action)
        {
            return new AnimationAction(action);
        }
    }
}
