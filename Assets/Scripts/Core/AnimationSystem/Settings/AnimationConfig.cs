using UnityEngine;

namespace Core.AnimationSystem.Settings
{
    [CreateAssetMenu(fileName = "AnimationConfig", menuName = "Settings/AnimationConfig")]
    public class AnimationConfig : ScriptableObject
    {
        [SerializeField] 
        private bool _isAnimationsEnabled;
        [SerializeField] 
        private float _swapAnimDuration;
        [SerializeField]
        private float _tilesFallSpeed;
        [SerializeField]
        private float _delayAfterRefill;
        
        public float TilesFallSpeed => _tilesFallSpeed;
        public float SwapAnimDuration => _swapAnimDuration;
        public bool IsAnimationsEnabled => _isAnimationsEnabled;
        public float DelayAfterRefill => _delayAfterRefill;
    }
}
