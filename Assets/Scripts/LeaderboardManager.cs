using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private int entriesCount = 100; // Количество записей
    private List<LeaderboardEntry> allEntries = new List<LeaderboardEntry>();
    private string currentPlayerName = "Player"; // Имя игрока

    void Awake()
    {
        GenerateFakeLeaderboard();
        // Добавляем игрока в лидерборд с начальным счетом 0
        AddOrUpdatePlayerEntry(currentPlayerName, 0); 
    }

    // Генерация случайных имен и очков
    private void GenerateFakeLeaderboard()
    {
        allEntries.Clear();

        // Массивы для генерации правдоподобных ников
        string[] namePrefixes = { "Dark", "Shadow", "Blood", "Iron", "Storm", "Night", "Wolf", "Dragon", "Fire", "Ice", "Epic", "Super", "Mega", "Ultra", "Alpha", "Beta", "Gamma", "Captain", "Lord", "Master" };
        string[] nameSuffixes = { "Slayer", "Hunter", "Warrior", "Killer", "Guardian", "Walker", "Rider", "Breaker", "Creator", "Destroyer", "Player", "Hacker", "Pro", "Newbie", "Gamer", "Legend", "Hero", "Villain", "Star", "King" };

        for (int i = 0; i < entriesCount - 1; i++) // -1 чтобы место для игрока
        {
            // Генерация случайного ника
            string randomName = namePrefixes[Random.Range(0, namePrefixes.Length)] + 
                               nameSuffixes[Random.Range(0, nameSuffixes.Length)] + 
                               Random.Range(1, 1000).ToString();

            // Генерация случайного счета (правдоподобное распределение: топ-10 высокие, остальные пониже)
            int randomScore;
            if (i < 10) { randomScore = Random.Range(5000, 100001); }     // Топ-10: 5000-10000
            else if (i < 30) { randomScore = Random.Range(3000, 50000); } // Топ-30: 3000-5000
            else if (i < 60) { randomScore = Random.Range(1000, 30000); } // Топ-60: 1000-3000
            else { randomScore = Random.Range(100, 1000); }               // Остальные: 100-1000

            allEntries.Add(new LeaderboardEntry(randomName, randomScore, 0));
            
        }

        SortLeaderboard(); // Первоначальная сортировка
    }

    // Метод для сортировки лидерборда по очкам (от большего к меньшему)
    private void SortLeaderboard()
    {
        // Используем LINQ для сортировки :cite[4]
        allEntries = allEntries.OrderByDescending(entry => entry.score).ToList();
        
        // Обновляем ранги после сортировки
        for (int i = 0; i < allEntries.Count; i++)
        {
            allEntries[i].rank = i + 1;
        }
    }

    // Добавление или обновление записи игрока
    public void AddOrUpdatePlayerEntry(string playerName, int newScore)
    {
        // Ищем существующую запись игрока
        LeaderboardEntry existingEntry = allEntries.FirstOrDefault(entry => entry.playerName == playerName);

        if (existingEntry != null)
        {
            // Обновляем счет существующей записи
            existingEntry.score = newScore;
        }
        else
        {
            // Создаем новую запись, если игрок еще не в лидерборде
            allEntries.Add(new LeaderboardEntry(playerName, newScore, 0));
        }

        // Пересортировываем весь лидерборд
        SortLeaderboard();
    }

    // Получение текущего ранга игрока
    public int GetPlayerRank(string playerName)
    {
        LeaderboardEntry playerEntry = allEntries.FirstOrDefault(entry => entry.playerName == playerName);
        return (playerEntry != null) ? playerEntry.rank : -1;
    }

    // Получение TOP N записей (для отображения)
    public List<LeaderboardEntry> GetTopEntries(int count)
    {
        return allEntries.Take(count).ToList();
    }

    // Получение всех записей (для прокрутки)
    public List<LeaderboardEntry> GetAllEntries()
    {
        return allEntries;
    }
}