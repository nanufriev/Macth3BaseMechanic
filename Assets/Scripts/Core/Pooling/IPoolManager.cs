using Cysharp.Threading.Tasks;

namespace Core.Pooling
{
    public interface IPoolManager<T>
    {
        UniTask<T> GetElementFromPool();
        void ReturnToPool(T element);
    }
}
