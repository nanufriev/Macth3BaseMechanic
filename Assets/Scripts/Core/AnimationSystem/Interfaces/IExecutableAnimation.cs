using Cysharp.Threading.Tasks;

namespace Core.AnimationSystem.Interfaces
{
    public interface IExecutableAnimation : IExecutable
    {
        bool WaitForCompletion { get; }
    }
}
