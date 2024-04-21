using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Core.Helpers
{
    public static class DOTweenExtensions
    {
        public static UniTask ToUniTask(this Tween tween)
        {
            var completionSource = new UniTaskCompletionSource();
            tween.OnComplete(() => completionSource.TrySetResult());
            tween.OnKill(() => completionSource.TrySetCanceled());
            return completionSource.Task;
        }
    }
}
