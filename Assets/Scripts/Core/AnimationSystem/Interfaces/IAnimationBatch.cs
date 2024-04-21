namespace Core.AnimationSystem.Interfaces
{
    public interface IAnimationBatch : IExecutableAnimation
    {
        void AddAnimationElement(IExecutableAnimation task);
        void Clear();
        bool IsEmpty();
    }
}
