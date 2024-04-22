using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public event Action OnStartCasualGame;
    public event Action<int> OnStartSimulation;
    public event Action OnGoToMenu;
    
    [SerializeField]
    private GameObject _mainMenuWindow;
    [SerializeField]
    private GameObject _simulationSettingsWindow;
    [SerializeField]
    private GameObject _simulationResultsWindow;
    [SerializeField]
    private GameObject _loadingText;
    [SerializeField]
    private Image _backgroundPanel;
    [SerializeField]
    private Button _menuButton;
    [SerializeField]
    private Button _startCasualGameButton;
    [SerializeField]
    private Button _startSimulationSettingsButton;
    [SerializeField]
    private Button _startSimulationButton;
    [SerializeField]
    private TMP_Text _simulationResultsText;
    [SerializeField]
    private TMP_InputField _turnAmountInput;
    
    public void Init()
    {
        _menuButton.onClick.AddListener(ShowMainMenu);
        _startCasualGameButton.onClick.AddListener(StartCasualGame);
        _startSimulationSettingsButton.onClick.AddListener(ShowSimulationSettings);
        _startSimulationButton.onClick.AddListener(StartSimulation);

        ShowMainMenu();
    }

    public void SetSimulationResults(string results)
    {
        _loadingText.SetActive(false);
        _simulationResultsWindow.SetActive(true);
        _simulationResultsText.text = results;
    }
    
    private void StartCasualGame()
    {
        _simulationSettingsWindow.SetActive(false);
        _simulationResultsWindow.SetActive(false);
        _mainMenuWindow.SetActive(false);
        _loadingText.SetActive(false);
        _menuButton.gameObject.SetActive(true);
        _backgroundPanel.enabled = false;
        OnStartCasualGame?.Invoke();
    }

    private void ShowMainMenu()
    {
        _simulationSettingsWindow.SetActive(false);
        _simulationResultsWindow.SetActive(false);
        _mainMenuWindow.SetActive(true);
        _loadingText.SetActive(false);
        _menuButton.gameObject.SetActive(false);
        _backgroundPanel.enabled = true;
        OnGoToMenu?.Invoke();
    }

    private void ShowSimulationSettings()
    {
        _menuButton.gameObject.SetActive(true);
        _simulationSettingsWindow.SetActive(true);
        _simulationResultsWindow.SetActive(false);
        _mainMenuWindow.SetActive(false);
    }

    private void StartSimulation()
    {
        var turnAmount = _turnAmountInput.text;

        if (turnAmount.Equals(string.Empty))
        {
            Debug.LogError("Turn amount is empty! Need to write correct amount");
            return;
        }

        var amount = Convert.ToInt32(turnAmount);
        
        _simulationSettingsWindow.SetActive(false);
        _simulationResultsWindow.SetActive(false);
        _mainMenuWindow.SetActive(false);
        _loadingText.SetActive(true);
        OnStartSimulation?.Invoke(amount);
    }
    
    
}
