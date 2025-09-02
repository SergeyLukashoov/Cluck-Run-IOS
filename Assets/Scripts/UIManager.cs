using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
   [SerializeField] private Button _addTimeButton;
   [SerializeField] private Button _addSizeButton;
   [SerializeField] public GameObject _popUpWindow;
   [SerializeField] public GameObject _pauseWindow;
   [SerializeField] public GameObject winWindow;
   [SerializeField] private TimerController _timer;
   [SerializeField] private PlayerController _player;
   [SerializeField] private TMP_Text scoreText;
   [SerializeField] private TMP_Text scorePauseText;
   [SerializeField] private TMP_Text scoreWinText;
   [SerializeField] private TMP_Text scoreBestText;
   private int points;

   private void Start()
   {
      _popUpWindow.SetActive(false);
      _pauseWindow.SetActive(false);
      winWindow.SetActive(false);
      points = 0;
      scoreText.text = points.ToString();
      
      PlayerPrefs.SetInt("CollectedPoints", points);
      PlayerPrefs.SetInt("PlayerScore", points);
   }

   public void OpenPopUpWindow()
   {
      _popUpWindow.SetActive(true);
      Time.timeScale = 0;
   }

   public void ClosePopUpWindow()
   {
      _popUpWindow.SetActive(false);
   }

   public void AddTime()
   {
      _timer._currentTime += 5f;
      Time.timeScale = 1f;
      ClosePopUpWindow();
   }

   public void AddSize()
   {
      Time.timeScale = 1f;
      _player.collectedObjects = 5;
      _player.GrowPlayer(0.5f);
      int growUp = PlayerPrefs.GetInt("GrowUp200Times");
      growUp++;
      PlayerPrefs.SetInt("GrowUp200Times", growUp);
      ClosePopUpWindow();
   }

   public void AddPoints(float size)
   {
      points += (int)(size * 1000);
      scoreText.text = points.ToString();
      
      PlayerPrefs.SetInt("PlayerScore", points);
      PlayerPrefs.SetInt("CollectedPoints", points);
   }

   public void ShowPauseWindow()
   {
      _pauseWindow.SetActive(true);
      Time.timeScale = 0;
      UpdatePauseScore();
   }

   public void ClosePauseWindow()
   {
      _pauseWindow.SetActive(false);
      Time.timeScale = 1;
   }

   public void UpdatePauseScore()
   {
      scorePauseText.text = "Your score:" + "\n" + points.ToString();
      PlayerPrefs.SetInt("points", points);
   }

   private void OnApplicationPause(bool pauseStatus)
   {
      PlayerPrefs.SetInt("points", points);
   }

   public void OpenWinWindow()
   {
      winWindow.SetActive(true);
      PlayerPrefs.SetInt("points", points);
      scoreWinText.text = "Your score:" + "\n" + points.ToString();
      scoreBestText.text = "Best score:" + "\n" + PlayerPrefs.GetInt("points", 0);
   }

   public void RestartGame()
   {
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
   }

   public void GoToMainMenu()
   {
      SceneManager.LoadScene(0);
   }
}
