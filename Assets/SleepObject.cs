using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SleepObject : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene(1);
    }
}
