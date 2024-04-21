using Cysharp.Threading.Tasks;

namespace Core.AnimationSystem.Interfaces
{
    public interface IExecutable
    {
        UniTask Execute();
    }
}
