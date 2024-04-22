using System;
using System.Collections.Generic;
using Core.AnimationSystem;
using Core.AnimationSystem.Interfaces;
using Core.AnimationSystem.Settings;
using Core.Helpers;
using Core.Settings;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Grid
{
    public class GridManager
    {
        public event Action OnCorrectSwipe;
        public event Action OnIncorrectSwipe;
        public event Action OnShuffle;
        
        private readonly GameConfig _config;
        private readonly Transform _tileGridParent;
        private readonly TileElementMono _tileElementMonoPrefab;
        private readonly Transform _tilePoolParent;
        private readonly TileElementPoolManagerMono _tileElementPool;
        private readonly GameObject _baseGridElement;
        private readonly Transform _baseGridParent;
        private readonly Camera _mainCamera;
        private readonly AnimationConfig _animationConfig;
        private readonly AnimationManager _animationManager;

        private IAnimationSequence _currentAnimationSequence;
        private TileElementMono[,] _tiles;
        private TileElementMono _selectedTile;
        private HashSet<TileElementMono> _matchedTiles;
        private SortedSet<int> _columnsToUpdate;
        private SortedSet<int> _rowsToUpdate;
        private bool _isBoardProcessing;
        private bool _isInputAllowed;
        private IAnimationBatch _fallBatch;
        private IAnimationTask _animationTask;
        private float _fallDuration;
        private List<UniTask> _spawningTilesTask;
        private int _reshuffleAmount;
        private bool _isAnimationsEnabled;
        
        public GridManager(
            GameConfig config,
            TileElementMono tileElementMonoPrefab,
            Transform tilePoolParent,
            Transform tileGridParent,
            Transform baseGridParent,
            GameObject baseGridElement,
            Camera mainCamera,
            AnimationConfig animationConfig)
        {
            _config = config;
            _tileElementMonoPrefab = tileElementMonoPrefab;
            _tilePoolParent = tilePoolParent;
            _tileGridParent = tileGridParent;
            _baseGridParent = baseGridParent;
            _baseGridElement = baseGridElement;
            _mainCamera = mainCamera;
            _animationConfig = animationConfig;
            _animationManager = new AnimationManager(_animationConfig);

            _tileElementPool = new TileElementPoolManagerMono();
            _matchedTiles = new HashSet<TileElementMono>();
            _columnsToUpdate = new SortedSet<int>();
            _rowsToUpdate = new SortedSet<int>();
        }

        public async UniTask Init()
        {
            var poolSize = _config.GridWidth * _config.GridWidth + _config.GridWidth * 2;
            await _tileElementPool.InitPool(poolSize, _tilePoolParent, _tileElementMonoPrefab);
            await UniTask.WhenAll(CreateBaseGrid(), CreateGrid());
            CheckMoves();
        }

        public async UniTask TrySwapRandomTiles()
        {
            var x = Random.Range(0, _config.GridWidth);
            var y = Random.Range(0, _config.GridHeight);
            var tile1 = _tiles[x, y];
            var tile2 = GetRandomAdjacentTile(x, y);

            await SwapTiles(tile1, tile2);
        }
        
        public void TurnOnAnimations()
        {
            _isAnimationsEnabled = true;
        }
        
        public void TurnOffAnimations()
        {
            _isAnimationsEnabled = false;
        }
        
        public void TurnOnInput()
        {
            _isInputAllowed = true;
        }
        
        public void TurnOffInput()
        {
            _isInputAllowed = false;
        }
        
        private async UniTask CreateBaseGrid()
        {
            for (var x = 0; x < _config.GridWidth; x++)
            for (var y = 0; y < _config.GridHeight; y++)
            {
                var newTile = await SpawnHelper.SpawnElement(_baseGridElement, _baseGridParent);
                newTile.transform.position = new Vector2(x, y);
            }
        }

        private async UniTask CreateGrid()
        {
            _tiles = new TileElementMono[_config.GridWidth, _config.GridHeight];
            for (var x = 0; x < _config.GridWidth; x++)
            for (var y = 0; y < _config.GridHeight; y++)
            {
                await SpawnTile(x, y, x, y);
            }
        }

        private void SelectTile(TileElementMono tile)
        {
            if (_isBoardProcessing || !_isInputAllowed)
                return;

            if (_selectedTile == null || _selectedTile.Equals(tile))
            {
                _selectedTile = tile;
                _selectedTile.Select();
            }
            else if (!AreTilesAdjacent(_selectedTile, tile))
            {
                _selectedTile.Deselect();
                _selectedTile = tile;
                _selectedTile.Select();
            }
            else
            {
                SwapTiles(_selectedTile, tile).Forget();
            }
        }

        private void SwipeTile(TileElementMono tile, Vector3 direction)
        {
            if (_isBoardProcessing || !_isInputAllowed)
                return;

            var targetX = tile.PositionX + (int)direction.x;
            var targetY = tile.PositionY + (int)direction.y;
            if (targetX >= 0 && targetX < _config.GridWidth && targetY >= 0 && targetY < _config.GridHeight)
            {
                var targetTile = _tiles[targetX, targetY];
                if (targetTile != null)
                    SwapTiles(tile, targetTile).Forget();
            }
        }

        private async UniTask SwapTiles(TileElementMono tile1, TileElementMono tile2)
        {
            try
            {
                _isBoardProcessing = true;

                if (AreTilesAdjacent(tile1, tile2))
                {
                    _currentAnimationSequence = _animationManager.CreateSequence();
                    SwapTilePositions(tile1, tile2);

                    CheckAndCollectMatches(tile1);
                    CheckAndCollectMatches(tile2);

                    if (_matchedTiles.Count > 0)
                    {
                        HandleMatches();
                        await RefillBoard();
                        _matchedTiles.Clear();
                        _columnsToUpdate.Clear();
                        _rowsToUpdate.Clear();
                        OnCorrectSwipe?.Invoke();
                    }
                    else
                    {
                        SwapTilePositions(tile1, tile2);
                        OnIncorrectSwipe?.Invoke();
                    }

                    await _currentAnimationSequence.Execute();
                    CheckMoves();
                }
            }
            finally
            {
                if (_selectedTile != null)
                {
                    _selectedTile.Deselect();
                    _selectedTile = null;
                }

                _isBoardProcessing = false;
            }
        }

        private void HandleMatches()
        {
            foreach (var matchedTile in _matchedTiles)
            {
                _tiles[matchedTile.PositionX, matchedTile.PositionY] = null;

                if (_isAnimationsEnabled)
                    _currentAnimationSequence.AddAnimationElement(
                        _animationManager.CreateAnimationAction(() => RemoveTile(matchedTile)));
                else
                    RemoveTile(matchedTile);
            }
        }

        private async UniTask RefillBoard()
        {
            TilesFall();
            await SpawnNewTiles();
        }

        private void TilesFall()
        {
            if (_isAnimationsEnabled)
                _fallBatch = _animationManager.CreateBatch(true);

            for (var y = _rowsToUpdate.Min; y < _config.GridHeight; y++)
            {
                foreach (var x in _columnsToUpdate)
                {
                    if (_tiles[x, y] == null)
                    {
                        for (var j = y + 1; j < _config.GridHeight; j++)
                        {
                            if (_tiles[x, j] != null)
                            {
                                if (_isAnimationsEnabled)
                                {
                                    _fallDuration = CalculateFallDuration(y);
                                    _animationTask = _animationManager.CreateTweenAnimationTask(_tiles[x, j].transform,
                                        new Vector3(x, j), new Vector3(x, y), _fallDuration,
                                        false, Ease.InOutQuad);

                                    _fallBatch.AddAnimationElement(_animationTask);
                                }
                                else
                                {
                                    _tiles[x, j].transform.position = new Vector2(x, y);
                                }

                                _tiles[x, y] = _tiles[x, j];
                                _tiles[x, j] = null;
                                _tiles[x, y].SetNewPositionIndex(x, y);
                                break;
                            }
                        }
                    }
                }
            }

            if (_isAnimationsEnabled)
                _currentAnimationSequence.AddAnimationElement(_fallBatch);
        }

        private async UniTask SpawnNewTiles()
        {
            if (_isAnimationsEnabled)
                _fallBatch = new AnimationBatch(true);
            else
                _spawningTilesTask = new List<UniTask>();

            var maxFallDuration = 0f;
            for (var y = _rowsToUpdate.Min; y < _config.GridHeight; y++)
            {
                foreach (var x in _columnsToUpdate)
                {
                    if (_tiles[x, y] == null)
                    {
                        var spawnPosition = new Vector3(x, _config.GridHeight, 0);
                        var targetPosition = new Vector3(x, y, 0);
                        var fallDuration = CalculateFallDuration(_config.GridHeight - y);
                        maxFallDuration = Math.Max(fallDuration, maxFallDuration);
                        if (_isAnimationsEnabled)
                            _fallBatch.AddAnimationElement(_animationManager.CreateTweenAnimationTaskWithLazyTarget(
                                SpawnTile(x, y, x, _config.GridHeight), spawnPosition, targetPosition, fallDuration,
                                false, Ease.InOutQuad));
                        else
                            _spawningTilesTask.Add(SpawnTile(x, y, x, y));
                    }
                }

                if (_isAnimationsEnabled && !_fallBatch.IsEmpty())
                {
                    _currentAnimationSequence.AddAnimationElement(_fallBatch);
                    _fallBatch = new AnimationBatch(true);
                }
            }

            if (_isAnimationsEnabled)
                _currentAnimationSequence.AddAnimationElement(
                    _animationManager.CreateAnimationDelay(maxFallDuration + _animationConfig.DelayAfterRefill));
            else
                await UniTask.WhenAll(_spawningTilesTask);
        }


        private void SwapTilePositions(TileElementMono tile1, TileElementMono tile2)
        {
            if (_isAnimationsEnabled)
                _currentAnimationSequence.AddAnimationElement(
                    _animationManager.CreateTilesSwapBatch(tile1.transform, tile2.transform, tile1.PositionX,
                        tile1.PositionY, tile2.PositionX, tile2.PositionY));
            else
                (tile1.transform.position, tile2.transform.position) =
                    (tile2.transform.position, tile1.transform.position);

            _tiles[tile1.PositionX, tile1.PositionY] = tile2;
            _tiles[tile2.PositionX, tile2.PositionY] = tile1;

            var tempX = tile1.PositionX;
            var tempY = tile1.PositionY;
            tile1.SetNewPositionIndex(tile2.PositionX, tile2.PositionY);
            tile2.SetNewPositionIndex(tempX, tempY);
        }

        private void CheckAndCollectMatches(TileElementMono tile)
        {
            CheckLineMatch(tile, Vector2.right, Vector2.left);
            CheckLineMatch(tile, Vector2.up, Vector2.down);
        }

        private void CheckLineMatch(TileElementMono tile, Vector2 firstDirection, Vector2 secondDirection)
        {
            var lineMatches = new List<TileElementMono> { tile };
            var consecutiveCount = 1;

            CheckDirectionLineMatch(tile, firstDirection, ref lineMatches, ref consecutiveCount);
            CheckDirectionLineMatch(tile, secondDirection, ref lineMatches, ref consecutiveCount);

            if (consecutiveCount >= 3)
                foreach (var tileInLine in lineMatches)
                {
                    _matchedTiles.Add(tileInLine);
                    _columnsToUpdate.Add(tileInLine.PositionX);
                    _rowsToUpdate.Add(tileInLine.PositionY);
                }
        }

        private void CheckDirectionLineMatch(TileElementMono startTile, Vector2 direction,
            ref List<TileElementMono> lineMatches, ref int consecutiveCount)
        {
            var currentTile = startTile;
            var nextX = startTile.PositionX + (int)direction.x;
            var nextY = startTile.PositionY + (int)direction.y;

            while (nextX >= 0 && nextX < _config.GridWidth && nextY >= 0 && nextY < _config.GridHeight)
            {
                var nextTile = _tiles[nextX, nextY];
                if (nextTile != null && nextTile.Color == currentTile.Color)
                {
                    lineMatches.Add(nextTile);
                    consecutiveCount++;
                    nextX += (int)direction.x;
                    nextY += (int)direction.y;
                }
                else
                {
                    break;
                }
            }
        }

        private void CheckMoves()
        {
            _reshuffleAmount = 0;
            while (!IsMoveAvailable() && _reshuffleAmount < _config.MaximumReshuffleAmount)
            {
                ReshuffleBoard();
                _reshuffleAmount++;
                OnShuffle?.Invoke();
            }

            if (_reshuffleAmount >= _config.MaximumReshuffleAmount)
                throw new Exception("Maximum number of reshuffle attempts exceeded!");
        }

        private bool IsMoveAvailable()
        {
            for (var y = 0; y < _config.GridHeight; y++)
            {
                for (var x = 0; x < _config.GridWidth; x++)
                {
                    var currentColor = _tiles[x, y].Color;

                    if (x < _config.GridWidth - 2)
                    {
                        if (_tiles[x + 1, y].Color == currentColor && _tiles[x + 2, y].Color == currentColor)
                            return true;
                    }

                    if (x > 0 && x < _config.GridWidth - 1)
                    {
                        if (_tiles[x - 1, y].Color == currentColor && _tiles[x + 1, y].Color == currentColor)
                            return true;
                    }

                    if (y < _config.GridHeight - 2)
                    {
                        if (_tiles[x, y + 1].Color == currentColor && _tiles[x, y + 2].Color == currentColor)
                            return true;
                    }

                    if (y > 0 && y < _config.GridHeight - 1)
                    {
                        if (_tiles[x, y - 1].Color == currentColor && _tiles[x, y + 1].Color == currentColor)
                            return true;
                    }

                    if (x > 0 && x < _config.GridWidth - 2)
                    {
                        if (_tiles[x - 1, y].Color == currentColor && _tiles[x + 2, y].Color == currentColor)
                            return true;
                    }

                    if (y > 0 && y < _config.GridHeight - 2)
                    {
                        if (_tiles[x, y - 1].Color == currentColor && _tiles[x, y + 2].Color == currentColor)
                            return true;
                    }
                }
            }

            return false;
        }

        private void ReshuffleBoard()
        {
            var tiles = new List<TileElementMono>(_config.GridWidth * _config.GridHeight);
            for (int y = 0; y < _config.GridHeight; y++)
            {
                for (var x = 0; x < _config.GridWidth; x++)
                {
                    tiles.Add(_tiles[x, y]);
                }
            }

            ShuffleTiles(tiles);

            var index = 0;
            for (var y = 0; y < _config.GridHeight; y++)
            {
                for (var x = 0; x < _config.GridWidth; x++)
                {
                    _tiles[x, y] = tiles[index++];
                    _tiles[x, y].SetNewPositionIndex(x, y);
                    _tiles[x, y].transform.position = new Vector3(x, y);
                }
            }
        }

        private void ShuffleTiles(List<TileElementMono> tiles)
        {
            var rnd = new System.Random();
            var n = tiles.Count;
            while (n > 1)
            {
                n--;
                var k = rnd.Next(n + 1);
                (tiles[k], tiles[n]) = (tiles[n], tiles[k]);
            }
        }

        private async UniTask<Transform> SpawnTile(int x, int y, int spawnX, int spawnY)
        {
            var newTile = await _tileElementPool.GetElementFromPool();
            newTile.transform.position = new Vector2(spawnX, spawnY);
            newTile.Init(x, y, GetRandomColor(), _mainCamera);
            newTile.OnTileClick += SelectTile;
            newTile.OnTileSwap += SwipeTile;
            newTile.transform.parent = _tileGridParent;
            newTile.gameObject.SetActive(true);
            _tiles[x, y] = newTile;
            return newTile.transform;
        }

        private void RemoveTile(TileElementMono tile)
        {
            tile.OnTileClick -= SelectTile;
            tile.OnTileSwap -= SwipeTile;
            _tileElementPool.ReturnToPool(tile);
        }

        private float CalculateFallDuration(int y)
        {
            return (_config.GridHeight - y) / _animationConfig.TilesFallSpeed;
        }
        
        private TileElementMono GetRandomAdjacentTile(int x, int y)
        {
            var possiblePositions = new List<(int, int)>();

            if (x > 0)
                possiblePositions.Add((x - 1, y));
    
            if (x < _config.GridWidth - 1)
                possiblePositions.Add((x + 1, y));

            if (y > 0)
                possiblePositions.Add((x, y - 1));

            if (y < _config.GridHeight - 1)
                possiblePositions.Add((x, y + 1));

            if (possiblePositions.Count == 0)
                return null;

            var (selectedX, selectedY) = possiblePositions[Random.Range(0, possiblePositions.Count)];
            return _tiles[selectedX, selectedY];
        }

        private bool AreTilesAdjacent(TileElementMono tile1, TileElementMono tile2)
        {
            return Mathf.Abs(tile1.PositionX - tile2.PositionX) + Mathf.Abs(tile1.PositionY - tile2.PositionY) == 1;
        }

        private Color GetRandomColor()
        {
            if (_config.TileColors.Count == 0)
            {
                Debug.LogError("Colors list is empty.");
                return Color.black;
            }

            var randomIndex = Random.Range(0, _config.TileColors.Count);
            return _config.TileColors[randomIndex];
        }
    }
}