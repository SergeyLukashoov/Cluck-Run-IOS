using UnityEngine;
using DG.Tweening;

public class TimerController : MonoBehaviour
{
    public float gameDuration = 60f; // 1 минута
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float shrinkDuration = 2f; // Длительность уменьшения
    
    public float _currentTime;
    private bool _timerEnded;

    private void Start()
    {
        _currentTime = gameDuration;
        _timerEnded = false;
    }

    private void Update()
    {
        if (_timerEnded) return;
        
        _currentTime -= Time.deltaTime;
        
        if (_currentTime <= 0f)
        {
            _currentTime = 0f;
            _timerEnded = true;
            ShrinkPlayer();
        }
    }

    private void ShrinkPlayer()
    {
        if (playerTransform != null)
        {
            playerTransform.DOScale(Vector3.zero, shrinkDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    GetComponent<UIManager>().OpenWinWindow();
                });
        }
    }
    
    // Для отображения времени в UI (опционально)
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(_currentTime / 60f);
        int seconds = Mathf.FloorToInt(_currentTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}