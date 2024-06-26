using System.Collections.Generic;
using Core.Helpers;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Pooling
{
    public abstract class PoolManagerMonoBase<T> : IPoolManager<T> where T : MonoBehaviour, IPoolElement
    {
        private Transform _poolParent;
        private Queue<T> _poolElements;
        private T _poolElementPrefab;

        public async UniTask InitPool(int size, Transform parent, T poolElementPrefab)
        {
            _poolParent = parent;
            _poolElementPrefab = poolElementPrefab;
            _poolElements = new Queue<T>(size);
            
            T newElement;

            for (var i = 0; i < size; i++)
            {
                newElement = await SpawnHelper.SpawnElement(_poolElementPrefab, _poolParent);
                newElement.gameObject.SetActive(false);
                _poolElements.Enqueue(newElement);
            }
        }

        public async UniTask<T> GetElementFromPool()
        {
            if (!_poolElements.TryDequeue(out var elementToReturn))
            {
                elementToReturn = await SpawnHelper.SpawnElement(_poolElementPrefab, _poolParent);
                elementToReturn.gameObject.SetActive(false);
            }
            
            return elementToReturn;
        }

        public void ReturnToPool(T element)
        {
            element.gameObject.SetActive(false);
            element.Dispose();
            _poolElements.Enqueue(element);
        }
    }
}