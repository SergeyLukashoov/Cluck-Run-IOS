using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelChoose : MonoBehaviour
{
    public int levelIndex; // Индекс уровня (начинается с 1)
    public GameObject lockIcon;
    public GameObject starsPanel;
    public Image[] starIcons; // 3 элемента для звезд

    private Button _button;

    void Start()
    {
        _button = GetComponent<Button>();
        UpdateButtonState();
    }

    void UpdateButtonState()
    {
        int lastPassed = PlayerPrefs.GetInt("Win", 0);
        bool isUnlocked = levelIndex <= lastPassed + 1;
        bool isCompleted = levelIndex <= lastPassed;

        // Блокировка/разблокировка
        lockIcon.SetActive(!isUnlocked);
        _button.interactable = isUnlocked;

        // Отображение звезд
        starsPanel.SetActive(isCompleted);
        if (isCompleted)
        {
            int stars = PlayerPrefs.GetInt($"Stars_{levelIndex}", 0);
            for (int i = 0; i < starIcons.Length; i++)
                starIcons[i].gameObject.SetActive(i < stars);
        }
    }

    public void LoadLevel()
    {
        if (_button.interactable)
            SceneManager.LoadScene($"Level_{levelIndex}");
    }
}