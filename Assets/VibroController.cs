using System;
using TechJuego.HapticFeedback;
using UnityEngine;
using UnityEngine.UI;

public class VibroController : MonoBehaviour
{
    [SerializeField] private Sprite vibroOff;
    [SerializeField] private Sprite vibroOn;

    private Image vibroImage;

    private void Start()
    {
        vibroImage = GetComponent<Image>();
    }

    public void SwitchVibro()
    {
        if (CallHapticFeedback.Instance.VibrationEnabled)
        {
            vibroImage.sprite = vibroOff;
            CallHapticFeedback.Instance.VibrationEnabled = false;
        }
        else
        {
            vibroImage.sprite = vibroOn;
            CallHapticFeedback.Instance.VibrationEnabled = true;
            CallHapticFeedback.Instance.SoftHaptic();
        }
    }
}
