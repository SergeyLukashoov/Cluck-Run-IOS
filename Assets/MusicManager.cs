using System;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    public bool IsMusicOn = false;
    
    [SerializeField] private Sprite musicOn;
    [SerializeField] private Sprite musicOff;
    private Image musicImage;

    private void Start()
    {
        musicImage = GetComponent<Image>();
    }

    public void SwitchMusic()
    {
        if (IsMusicOn)
        {
            audioSource.volume = 0;
            IsMusicOn = false;
            musicImage.sprite = musicOff;
        }
        else
        {
            audioSource.volume = 0.5f;
            IsMusicOn = true;
            musicImage.sprite = musicOn;
        }
    }
}
