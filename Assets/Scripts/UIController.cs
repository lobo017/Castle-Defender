using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text resourcesText;
    [SerializeField] private TMP_Text warningText;

    [SerializeField] private GameObject towerPanel;
    [SerializeField] private GameObject towerCardPrefab;
    [SerializeField] private Transform cardsContainer;

    [SerializeField] private TowerData[] towers;
    private List<GameObject> activeCards = new List<GameObject>();

    private Platform _currentPlatform;

    [SerializeField] private Button speed1Button;
    [SerializeField] private Button speed2Button;
    [SerializeField] private Button speed3Button;

    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color selectedButtonColor = Color.blue;

    [SerializeField] private GameObject pausePanel;
    private bool _isGamePaused = false;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private GameObject missionCompletePanel;

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        GameManager.OnLivesChanged += UpdateLivesText;
        GameManager.OnResourcesChanged += UpdateResourcesText;
        Platform.OnPlatformClicked += HandlePlatformClicked;
        TowerCard.OnTowerSelected += HandleTowerSelected;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Spawner.OnMissionComplete += ShowMissionComplete; 
    }

    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        GameManager.OnLivesChanged -= UpdateLivesText;
        GameManager.OnResourcesChanged -= UpdateResourcesText;
        Platform.OnPlatformClicked -= HandlePlatformClicked;
        TowerCard.OnTowerSelected -= HandleTowerSelected;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Spawner.OnMissionComplete -= ShowMissionComplete;
    }

    [Header("Difficulty UI")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private GameObject pauseMenuButtonsContainer; // New container reference
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button difficultyMenuButton; // Button in pause menu to open difficulty

    [Header("Visuals")]
    [SerializeField] private Color selectedColor = Color.cyan;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color selectedTextColor = Color.black;

    private void Start()
    {
        // Ensure DifficultyManager exists
        if (DifficultyManager.Instance == null)
        {
            GameObject go = new GameObject("DifficultyManager");
            go.AddComponent<DifficultyManager>();
        }

        speed1Button.onClick.AddListener(() => SetGameSpeed(0.2f));
        speed2Button.onClick.AddListener(() => SetGameSpeed(1f));
        speed3Button.onClick.AddListener(() => SetGameSpeed(2f));

        HighlightSelectedSpeedButton(GameManager.Instance.GameSpeed);

        // Difficulty Buttons
        if (easyButton != null) easyButton.onClick.AddListener(() => SelectDifficultyAndRestart(Difficulty.Easy));
        if (normalButton != null) normalButton.onClick.AddListener(() => SelectDifficultyAndRestart(Difficulty.Normal));
        if (hardButton != null) hardButton.onClick.AddListener(() => SelectDifficultyAndRestart(Difficulty.Hard));
        if (backButton != null) backButton.onClick.AddListener(HideDifficultyPanel);
        if (difficultyMenuButton != null) difficultyMenuButton.onClick.AddListener(ShowDifficultyPanel);

        if (difficultyPanel != null) difficultyPanel.SetActive(false);
    }

    private void SelectDifficultyAndRestart(Difficulty difficulty)
    {
        SetDifficulty(difficulty);
        // Resume time before restarting to ensure clean state if needed, though LoadLevel usually handles it.
        GameManager.Instance.SetTimeScale(1f); 
        RestartLevel();
    }

    public void ShowDifficultyPanel()
    {
        if (difficultyPanel != null)
        {
            difficultyPanel.SetActive(true);
            UpdateDifficultyVisuals();
        }
        if (pauseMenuButtonsContainer != null)
        {
            pauseMenuButtonsContainer.SetActive(false);
        }
    }

    public void HideDifficultyPanel()
    {
        if (difficultyPanel != null)
            difficultyPanel.SetActive(false);
        
        if (pauseMenuButtonsContainer != null)
        {
            pauseMenuButtonsContainer.SetActive(true);
        }
    }

    private void SetDifficulty(Difficulty difficulty)
    {
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.SetDifficulty(difficulty);
            UpdateDifficultyVisuals();
        }
    }

    private void UpdateDifficultyVisuals()
    {
        if (DifficultyManager.Instance == null) return;

        Difficulty current = DifficultyManager.Instance.SelectedDifficulty;

        UpdateButtonVisual(easyButton, current == Difficulty.Easy);
        UpdateButtonVisual(normalButton, current == Difficulty.Normal);
        UpdateButtonVisual(hardButton, current == Difficulty.Hard);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void UpdateWaveText(int currentWave)
    {
        waveText.text = $"Wave: {currentWave + 1}";
    }

    private void UpdateLivesText(int currentLives)
    {
        livesText.text = $"Lives: {currentLives}";
        if (currentLives <= 0)
        {
            ShowGameOver();
        }
    }

    private void UpdateResourcesText(int currentResources)
    {
        resourcesText.text = $"Resources: {currentResources}";
    }

    private void HandlePlatformClicked(Platform platform)
    {
        _currentPlatform = platform;
        ShowTowerPanel();
    }

    private void ShowTowerPanel()
    {
        towerPanel.SetActive(true);
        Platform.towerPanelOpen = true;
        GameManager.Instance.SetTimeScale(0f);
        PopulateTowerCards();
    }

    public void HideTowerPanel()
    {
        towerPanel.SetActive(false);
        Platform.towerPanelOpen = false;
        GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);

    }

    private void PopulateTowerCards()
    {
        foreach (var card in activeCards)
        {
            Destroy(card);
        }
        activeCards.Clear();

        foreach (var data in towers)
        {
            GameObject cardGameObject = Instantiate(towerCardPrefab, cardsContainer);
            TowerCard card = cardGameObject.GetComponent<TowerCard>();
            card.Initialize(data);
            activeCards.Add(cardGameObject);
        }
    }

    private void HandleTowerSelected(TowerData towerData)
    {
        if (_currentPlatform.transform.childCount > 0)
        {
            HideTowerPanel();
            StartCoroutine(ShowWarningResourcesMessage("This platform already has a tower!"));
            return;
        }
        if (GameManager.Instance.Resources >= towerData.cost)
        {
            GameManager.Instance.SpendResources(towerData.cost);
            _currentPlatform.PlaceTower(towerData);
        } else
        {
            StartCoroutine(ShowWarningResourcesMessage("Not enough resources!"));
        }

        HideTowerPanel();
    }
    private IEnumerator ShowWarningResourcesMessage(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(3f);
        warningText.gameObject.SetActive(false);
    }

    private void SetGameSpeed(float timeScale)
    {
        HighlightSelectedSpeedButton(timeScale);
        GameManager.Instance.SetGameSpeed(timeScale);
    }
    private void UpdateButtonVisual(Button button, bool isSelected)
    {
        button.image.color = isSelected ? selectedButtonColor : normalButtonColor;
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.color = isSelected ? selectedTextColor : normalTextColor;
        }
    }

    private void HighlightSelectedSpeedButton(float selectedSpeed)
    {
        UpdateButtonVisual(speed1Button, selectedSpeed == 0.2f);
        UpdateButtonVisual(speed2Button, selectedSpeed == 1f);
        UpdateButtonVisual(speed3Button, selectedSpeed == 2f);
    }

    public void TogglePause()
    {
        if (towerPanel.activeSelf)
            return;

        if (_isGamePaused)
        {
            pausePanel.SetActive(false);
            _isGamePaused = false;
            GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
        }
        else
        {
            pausePanel.SetActive(true);
            _isGamePaused = true;
            GameManager.Instance.SetTimeScale(0f);

            // Reset menu state
            if (pauseMenuButtonsContainer != null) pauseMenuButtonsContainer.SetActive(true);
            if (difficultyPanel != null) difficultyPanel.SetActive(false);
        }
    }
    public void RestartLevel()
    {
       LevelManager.Instance.LoadLevel(LevelManager.Instance.CurrentLevel);
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        GameManager.Instance.SetTimeScale(1f);
        SceneManager.LoadScene("MainMenu");
    }

    private void ShowGameOver()
    {
        GameManager.Instance.SetTimeScale(0f);
        gameOverPanel.SetActive(true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ShowObjective());
    }
    private IEnumerator ShowObjective()
    {
        objectiveText.text = $"Survive {LevelManager.Instance.CurrentLevel.wavesToWin} waves!";
        objectiveText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        objectiveText.gameObject.SetActive(false);
    }

    private void ShowMissionComplete()
    {
        missionCompletePanel.SetActive(true);
        GameManager.Instance.SetTimeScale(0f);
    }

    public void EnterEndlessMode()
    {
        missionCompletePanel.SetActive(false);
        GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
        Spawner.Instance.EnableEndlessMode();
    }
}
