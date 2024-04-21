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
        [SerializeField] [Tooltip("Tiles per second")]
        private float _tilesFallSpeed;

        public float TilesFallSpeed => _tilesFallSpeed;
        public float SwapAnimDuration => _swapAnimDuration;
        public bool IsAnimationsEnabled => _isAnimationsEnabled;
    }
}
