using Cysharp.Threading.Tasks;
using Match3BaseMechanic.Helpers;
using UnityEngine;

namespace Match3BaseMechanic.Grid
{
    public class GridManager
    {
        private GameConfig _config;
        private Transform _tileGridParent;
        private TileElementMono _tileElementMonoPrefab;
        private Transform _tilePoolParent;
        private TileElementPoolManager _tileElementPool;
        private GameObject _baseGridElement;
        private Transform _baseGridParent;
        
        private TileElementMono[,] _tiles;
        private TileElementMono _selectedTile;

        public GridManager(
            GameConfig config, 
            TileElementMono tileElementMonoPrefab, 
            Transform tilePoolParent, 
            Transform tileGridParent,
            Transform baseGridParent,
            GameObject baseGridElement)
        {
            _config = config;
            _tileElementMonoPrefab = tileElementMonoPrefab;
            _tilePoolParent = tilePoolParent;
            _tileGridParent = tileGridParent;
            _baseGridParent = baseGridParent;
            _baseGridElement = baseGridElement;
            
            _tileElementPool = new TileElementPoolManager();
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
                var newTile = await _tileElementPool.GetElementFromPool();
                newTile.transform.position = new Vector2(x, y);
                newTile.Init(x, y, GetRandomColor());
                newTile.transform.parent = _tileGridParent;
                newTile.gameObject.SetActive(true);
                _tiles[x, y] = newTile;
            }
        }

        public void SelectTile(TileElementMono tile)
        {
            if (_selectedTile == null)
            {
                _selectedTile = tile;
            }
            else
            {
                SwapTiles(_selectedTile, tile);
                _selectedTile = null;
            }
        }

        public void SwapTiles(TileElementMono tile1, TileElementMono tile2)
        {
            if (Mathf.Abs(tile1.PositionX - tile2.PositionX) + Mathf.Abs(tile1.PositionY - tile2.PositionY) == 1)
            {
                (tile1.transform.position, tile2.transform.position) = (tile2.transform.position, tile1.transform.position);

                var tempX = tile1.PositionX;
                var tempY = tile1.PositionY;
                tile1.SetNewPositionIndex(tile2.PositionX, tile2.PositionY);
                tile2.SetNewPositionIndex(tempX, tempY);
                
                //CheckMatches
            }
            else
            {
                //Can't swap
            }
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