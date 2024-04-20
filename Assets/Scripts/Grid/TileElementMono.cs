using System;
using Match3BaseMechanic.Pooling;
using UnityEngine;

namespace Match3BaseMechanic.Grid
{
    public class TileElementMono : MonoBehaviour, IPoolElement
    {
        private const float SWIPE_THRESHOLD = 0.1f;
        
        public event Action<TileElementMono> OnTileClick;
        public event Action<TileElementMono, Vector3> OnTileSwap;
        public int PositionX { get; private set; } = -1;
        public int PositionY { get; private set; } = -1;
        public Color Color { get; private set; }

        private SpriteRenderer _spriteRenderer;
        private Camera _mainCamera;
        private Vector3 _initialPosition;

        public void Init(int x, int y, Color color, Camera mainCamera) 
        {
            PositionX = x;
            PositionY = y;
            Color = color;
            _mainCamera = mainCamera;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (_spriteRenderer == null)
                throw new Exception("Tile element doesn't have SpriteRenderer component!");

            _spriteRenderer.color = Color;
        }

        public void SetNewPositionIndex(int x, int y)
        {
            PositionX = x;
            PositionY = y;
        }
    
        private void OnMouseDown() 
        {
            //TODO Need to have Input manager and handle input only from pc, but from mobile devices too
            _initialPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        private void OnMouseUp() 
        {
            var finalPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var direction = finalPosition - _initialPosition;

            if (direction.magnitude < SWIPE_THRESHOLD)
            {
                OnTileClick?.Invoke(this);
            }
            else
            {
                direction.Normalize();
                OnTileSwap?.Invoke(this, direction);
            }
        }

        public void Dispose()
        {
            PositionX = -1;
            PositionY = -1;
        }
        
        public bool Equals(TileElementMono other)
        {
            return PositionX == other.PositionX && PositionY == other.PositionY;
        }

        public override bool Equals(object obj)
        {
            return obj is TileElementMono other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PositionX, PositionY);
        }
    }
}