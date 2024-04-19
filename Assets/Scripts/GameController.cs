using Cysharp.Threading.Tasks;
using Match3BaseMechanic.Grid;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField]
    private GameConfig _config;
    
    [Header("Tile Pool")]
    [SerializeField]
    private TileElementMono _tileElementMonoPrefab;
    [SerializeField]
    private Transform _tilePoolParent;
    
    [Header("Grid parent")]
    [SerializeField]
    private Transform _tileGridParent;
    
    [Header("Base grid")]
    [SerializeField]
    private Transform _baseGridParent;
    [SerializeField]
    private GameObject _baseGridElement;

    private GridManager _gridManager;
    
    private async UniTaskVoid Start()
    {
        _gridManager = new GridManager(_config, _tileElementMonoPrefab, _tilePoolParent, 
            _tileGridParent, _baseGridParent, _baseGridElement);
        
        await _gridManager.Init();
    }
}
