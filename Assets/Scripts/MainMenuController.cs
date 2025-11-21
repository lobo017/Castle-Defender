using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Difficulty UI")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button backButton;

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

        // Initialize buttons
        if (easyButton != null) easyButton.onClick.AddListener(() => SetDifficulty(Difficulty.Easy));
        if (normalButton != null) normalButton.onClick.AddListener(() => SetDifficulty(Difficulty.Normal));
        if (hardButton != null) hardButton.onClick.AddListener(() => SetDifficulty(Difficulty.Hard));
        if (backButton != null) backButton.onClick.AddListener(HideDifficultyPanel);

        // Initialize visual state
        UpdateDifficultyVisuals();
        
        // Ensure panel is hidden at start
        if(difficultyPanel != null)
            difficultyPanel.SetActive(false);
    }

    public void StartNewGame()
    {
        LevelManager.Instance.LoadLevel(LevelManager.Instance.allLevels[0]);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowDifficultyPanel()
    {
        if (difficultyPanel != null)
        {
            difficultyPanel.SetActive(true);
            UpdateDifficultyVisuals();
        }
    }

    public void HideDifficultyPanel()
    {
        if (difficultyPanel != null)
            difficultyPanel.SetActive(false);
    }

    private void SetDifficulty(Difficulty difficulty)
    {
        DifficultyManager.Instance.SetDifficulty(difficulty);
        UpdateDifficultyVisuals();
        StartNewGame();
    }

    private void UpdateDifficultyVisuals()
    {
        Difficulty current = DifficultyManager.Instance.SelectedDifficulty;

        UpdateButtonVisual(easyButton, current == Difficulty.Easy);
        UpdateButtonVisual(normalButton, current == Difficulty.Normal);
        UpdateButtonVisual(hardButton, current == Difficulty.Hard);
    }

    private void UpdateButtonVisual(Button button, bool isSelected)
    {
        if (button == null) return;

        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = isSelected ? selectedColor : normalColor;
        }

        var text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.color = isSelected ? selectedTextColor : normalTextColor;
        }
    }
}
