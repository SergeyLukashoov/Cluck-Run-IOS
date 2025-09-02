using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIManager : MonoBehaviour
{
   [SerializeField] private GameObject bestScoreWindow;
   [SerializeField] private TMP_Text bestScoreText;

   private void Start()
   {
      bestScoreWindow.SetActive(false);
   }

   public void StartGame()
   {
      SceneManager.LoadScene(1);
   }

   public void CloseGame()
   {
      Application.Quit();
   }

   public void OpenBestScoreWindow()
   {
      bestScoreWindow.SetActive(true);
      bestScoreText.text = "Your best score:" + "\n" + "\n" + PlayerPrefs.GetInt("points", 0).ToString();
   }

   public void CloseBestScoreWindow()
   {
      bestScoreWindow.SetActive(false);
   }
}
