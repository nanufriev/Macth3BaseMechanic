using System;
using Core.AnimationSystem.Settings;
using Core.CameraManagement;
using Core.Grid;
using Core.Settings;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField]
    private GameConfig _gameConfig;
    [SerializeField]
    private AnimationConfig _animationConfig;
    
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
    private CameraManager _cameraManager;
    private Camera _mainCamera;
    
    private async UniTaskVoid Start()
    {
        _mainCamera = Camera.main;
        
        if (_mainCamera == null)
            throw new Exception("Couldn't find main camera!");

        _cameraManager = new CameraManager(_mainCamera);
        _gridManager = new GridManager(_gameConfig, _tileElementMonoPrefab, _tilePoolParent, 
            _tileGridParent, _baseGridParent, _baseGridElement, _mainCamera, _animationConfig);
        
        await _gridManager.Init();
        _cameraManager.AdjustCameraToGrid(_gameConfig.GridWidth, _gameConfig.GridHeight);
        
    }
}
