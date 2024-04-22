using System;
using System.Text;
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

    [Header("UI")] 
    [SerializeField] 
    private UIManager _uiManager;
    
    private GridManager _gridManager;
    private CameraManager _cameraManager;
    private Camera _mainCamera;
    private int _correctSwipeAmount;
    private int _inCorrectSwipeAmount;
    private int _shuffleAmount;

    private async UniTaskVoid Start()
    {
        _mainCamera = Camera.main;
        
        if (_mainCamera == null)
            throw new Exception("Couldn't find main camera!");

        _cameraManager = new CameraManager(_mainCamera);
        _gridManager = new GridManager(_gameConfig, _tileElementMonoPrefab, _tilePoolParent, 
            _tileGridParent, _baseGridParent, _baseGridElement, _mainCamera, _animationConfig);
        
        _uiManager.Init();
        Subscribe();
        _gridManager.TurnOffInput();
        await _gridManager.Init();
        _cameraManager.AdjustCameraToGrid(_gameConfig.GridWidth, _gameConfig.GridHeight);
    }

    private void Subscribe()
    {
        _uiManager.OnStartSimulation += StartSimulation;
        _uiManager.OnStartCasualGame += OnStartCasualGame;
        _uiManager.OnGoToMenu += _gridManager.TurnOffInput;
        _gridManager.OnCorrectSwipe += () => _correctSwipeAmount++;
        _gridManager.OnIncorrectSwipe += () => _inCorrectSwipeAmount++;
        _gridManager.OnShuffle += () => _shuffleAmount++;
    }

    private void OnStartCasualGame()
    {
        if (_animationConfig.IsAnimationsEnabled)
            _gridManager.TurnOnAnimations();
        else
            _gridManager.TurnOffAnimations();
        
        _gridManager.TurnOnInput();
    }
    
    private void StartSimulation(int simulationAmount)
    {
        _gridManager.TurnOffAnimations();
        _gridManager.TurnOnInput();
        StartSimulationAsync(simulationAmount).Forget();
    }
    
    private async UniTask StartSimulationAsync(int simulationAmount)
    {
        _correctSwipeAmount = 0;
        _inCorrectSwipeAmount = 0;
        _shuffleAmount = 0;

        await UniTask.NextFrame();
        
        while (simulationAmount > 0)
        {
            await _gridManager.TrySwapRandomTiles();
            simulationAmount -= 1;
        }
        
        var result = BuildSimulationResult();
        _uiManager.SetSimulationResults(result);
    }

    private string BuildSimulationResult()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Simulation Results");
        sb.AppendLine($"Correct swipe amount: {_correctSwipeAmount}");
        sb.AppendLine($"Incorrect swipe amount: {_inCorrectSwipeAmount}");
        sb.AppendLine($"Shuffle amount: {_shuffleAmount}");
        return sb.ToString();
    }
}
