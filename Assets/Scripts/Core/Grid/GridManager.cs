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
        private IAnimationBatch _fallBatch;
        private IAnimationTask _animationTask;
        private float _fallDuration;
        private List<UniTask> _spawningTilesTask;

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
            if (_isBoardProcessing)
                return;

            if (_selectedTile == null || _selectedTile.Equals(tile))
                _selectedTile = tile;
            else
                SwapTiles(_selectedTile, tile).Forget();
        }

        private void SwipeTile(TileElementMono tile, Vector3 direction)
        {
            if (_isBoardProcessing)
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

                if (Mathf.Abs(tile1.PositionX - tile2.PositionX) + Mathf.Abs(tile1.PositionY - tile2.PositionY) == 1)
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
                    }
                    else
                    {
                        SwapTilePositions(tile1, tile2);
                    }

                    await _currentAnimationSequence.Execute();
                }
            }
            finally
            {
                _selectedTile = null;
                _isBoardProcessing = false;
            }
        }

        private void HandleMatches()
        {
            foreach (var matchedTile in _matchedTiles)
            {
                _tiles[matchedTile.PositionX, matchedTile.PositionY] = null;

                if (_animationConfig.IsAnimationsEnabled)
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
            if (_animationConfig.IsAnimationsEnabled)
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
                                if (_animationConfig.IsAnimationsEnabled)
                                {
                                    _fallDuration = (j - y) / _animationConfig.TilesFallSpeed;
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

            if (_animationConfig.IsAnimationsEnabled)
                _currentAnimationSequence.AddAnimationElement(_fallBatch);
        }

        private async UniTask SpawnNewTiles()
        {
            if (_animationConfig.IsAnimationsEnabled)
                _fallBatch = new AnimationBatch(true);
            else
                _spawningTilesTask = new List<UniTask>();

            for (var y = _rowsToUpdate.Min; y < _config.GridHeight; y++)
            {
                foreach (var x in _columnsToUpdate)
                {
                    if (_tiles[x, y] == null)
                    {
                        _fallDuration = CalculateFallDuration(y);
                        
                        if (_animationConfig.IsAnimationsEnabled)
                        {
                            _fallBatch.AddAnimationElement(_animationManager.CreateTweenAnimationTaskWithLazyTarget(
                                SpawnTile(x, y, x, _config.GridHeight), new Vector3(x, _config.GridHeight),
                                new Vector3(x, y), _fallDuration, false, Ease.InOutQuad));
                        }
                        else
                        {
                            _spawningTilesTask.Add(SpawnTile(x, y, x, y));
                        }
                    }
                }
                
                if (_animationConfig.IsAnimationsEnabled && !_fallBatch.IsEmpty())
                {
                    _currentAnimationSequence.AddAnimationElement(_fallBatch);
                    _fallBatch.AddAnimationElement(
                        _animationManager.CreateAnimationDelay(1 / _animationConfig.TilesFallSpeed));
                    _fallBatch = new AnimationBatch(true);
                }
            }

            if (!_animationConfig.IsAnimationsEnabled)
                await UniTask.WhenAll(_spawningTilesTask);
        }

        private void SwapTilePositions(TileElementMono tile1, TileElementMono tile2)
        {
            if (_animationConfig.IsAnimationsEnabled)
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