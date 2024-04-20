using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Helpers
{
    public static class SpawnHelper
    {
        //TODO Use addressables system for better performance, more suitable async operations
        //TODO Also for for real game we have to load resources of object too
        //TODO I'm convinced that using addressables for this test task is overhead
        public static async UniTask<T> SpawnElement<T>(T prefab, Transform parent) where T : Object
        {
            var operation = Object.InstantiateAsync(prefab, parent);
            await operation.ToUniTask();
            var newElement = operation.Result[0];
            if (newElement == null)
                throw new Exception($"Some error occured during spawn new element of type {typeof(T)}");
            return newElement;
        }
    }
}