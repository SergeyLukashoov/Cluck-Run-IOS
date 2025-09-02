using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LeaderboardManager leaderboardManager; // Ссылка на менеджер
    [SerializeField] private GameObject entryPrefab; // Префаб строки
    [SerializeField] private Transform entriesContainer; // Content объект ScrollView

    [Header("Settings")]
    [SerializeField] private int entriesToShow = 100; // Сколько записей показывать

    private string currentPlayerName = "Player";
    
    [SerializeField] private Sprite playerSprite;

    void Start()
    {
        // Находим менеджер, если не присвоен вручную
        if (leaderboardManager == null)
            leaderboardManager = FindObjectOfType<LeaderboardManager>();

        RefreshLeaderboardUI();
    }

    // Обновляет весь UI лидерборда
    public void RefreshLeaderboardUI()
    {
        ClearContainer();

        // Получаем все записи от менеджера
        List<LeaderboardEntry> entriesToDisplay = leaderboardManager.GetAllEntries();

        // Создаем элементы UI для каждой записи
        for (int i = 0; i < entriesToShow && i < entriesToDisplay.Count; i++)
        {
            LeaderboardEntry entry = entriesToDisplay[i];
            CreateEntryElement(entry, i);
        }
    }

    // Очищает контейнер от старых элементов
    private void ClearContainer()
    {
        foreach (Transform child in entriesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    // Создает один элемент UI для записи
    private void CreateEntryElement(LeaderboardEntry entry, int index)
    {
        GameObject newEntry = Instantiate(entryPrefab, entriesContainer);
        entryPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(650f, 100f);
        // Находим текстовые компоненты внутри префаба (подстройте имена под свои!)
        TMP_Text rankText = newEntry.transform.Find("RankText").GetComponent<TMP_Text>();
        TMP_Text nameText = newEntry.transform.Find("NameText").GetComponent<TMP_Text>();
        TMP_Text scoreText = newEntry.transform.Find("ScoreText").GetComponent<TMP_Text>();

        // Заполняем данные
        if (rankText != null) rankText.text = entry.rank.ToString();
        if (nameText != null) nameText.text = entry.playerName;
        if (scoreText != null) scoreText.text = entry.score.ToString();

        // Визуально выделяем запись текущего игрока
        if (entry.playerName == currentPlayerName)
        {
            Image entryBackground = newEntry.GetComponent<Image>();
            if (entryBackground != null)
            {
                entryBackground.sprite = playerSprite;
            }
            // Или можно сделать текст жирным
            // if (nameText != null) nameText.fontStyle = FontStyle.Bold;
        }
    }

    // Public метод для обновления счета игрока (будет вызван из другого скрипта)
    public void UpdatePlayerScore(int newScore)
    {
        leaderboardManager.AddOrUpdatePlayerEntry(currentPlayerName, newScore);
        RefreshLeaderboardUI(); // Перерисовываем UI после обновления

        // Опционально: можно проскроллить к позиции игрока
        // ScrollToPlayerPosition();
    }
}