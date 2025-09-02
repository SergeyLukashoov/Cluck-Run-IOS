using System;
using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    private Animator animator;
    private bool pressed = false;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnButtonPressed()
    {
        pressed = !pressed;
        animator.SetBool("Button Pressed", pressed);
    }
}
