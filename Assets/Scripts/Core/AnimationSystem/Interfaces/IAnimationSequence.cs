namespace Core.AnimationSystem.Interfaces
{
    public interface IAnimationSequence : IExecutableAnimation
    {
        void AddAnimationElement(IExecutableAnimation batch);
        void Clear();
    }
}
