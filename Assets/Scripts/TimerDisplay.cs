using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TimerController timerController;

    private void Update()
    {
        timerText.text = timerController.GetFormattedTime();
    }
}