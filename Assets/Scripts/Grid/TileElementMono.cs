using System;
using Match3BaseMechanic.Pooling;
using UnityEngine;

namespace Match3BaseMechanic.Grid
{
    public class TileElementMono : MonoBehaviour, IPoolElement
    {
        public event Action<TileElementMono> OnTileClick;
        public event Action<TileElementMono, Vector3> OnTileSwap;
        public int PositionX { get; private set; }
        public int PositionY { get; private set; }
        public Color Color { get; private set; }

        private SpriteRenderer _spriteRenderer;
        private Camera _mainCamera;
        private Vector3 _initialPosition;

        public void Init(int x, int y, Color color) 
        {
            PositionX = x;
            PositionY = y;
            Color = color;

            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
                throw new Exception("Tile element doesn't have SpriteRenderer component!");

            _spriteRenderer.color = Color;
            
            _mainCamera = Camera.main;
            if (_mainCamera == null)
                throw new Exception("Couldn't find main camera!");
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

            if (finalPosition == _initialPosition)
            {
                OnTileClick?.Invoke(this);
            }
            else
            {
                var direction = finalPosition - _initialPosition;
                OnTileSwap?.Invoke(this, direction);
            }
        }
    }
}