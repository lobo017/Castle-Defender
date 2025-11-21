using UnityEngine;

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public Difficulty SelectedDifficulty { get; private set; } = Difficulty.Normal;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetDifficulty(Difficulty difficulty)
    {
        SelectedDifficulty = difficulty;
    }
}
