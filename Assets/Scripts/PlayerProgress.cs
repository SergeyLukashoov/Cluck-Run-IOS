using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    private int currentScore = 0;
    private LeaderboardUI leaderboardUI;

    void Start()
    {
        leaderboardUI = FindObjectOfType<LeaderboardUI>();
        // Загружаем сохраненный счет, если нужно
        currentScore = PlayerPrefs.GetInt("PlayerScore", 0);
        UpdateLeaderboard();
    }

    // Вызывайте этот метод, когда игрок зарабатывает очки (убийство врага, сбор монеты и т.д.)
    public void AddScore(int pointsToAdd)
    {
        currentScore += pointsToAdd;
        UpdateLeaderboard();
    }

    private void UpdateLeaderboard()
    {
        if (leaderboardUI != null)
        {
            leaderboardUI.UpdatePlayerScore(currentScore);
            Debug.Log("Score Added: " + currentScore);
        }
        // Сохраняем счет игрока
    }

    // Для тестирования из редактора
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddScore(Random.Range(10, 100)); // Добавляем случайные очки по нажатию пробела
        }
    }
}