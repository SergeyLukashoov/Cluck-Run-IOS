using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class InternetChecker : MonoBehaviour
{
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject levelChooseWindow;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject loadingScreenLand;
    [SerializeField] private GameObject noInternetUI;
    [SerializeField] private GameObject noInternetUILand;
    [SerializeField] private GameObject notificationWindow;
    [SerializeField] private GameObject notificationWindowLand;
    [SerializeField] private GameObject apsFlyerSDK;
    [SerializeField] private float checkTimeout = 3f;
    [SerializeField] private string testUrl = "https://www.google.com";
    [SerializeField] private string privacyURL = "https://chickenrunn.com/privacy-policy.html";
    
    public bool hasInternet = true;
    
    private bool isPortraitOrientation = false;

    private void Start()
    {
        // Сначала скрываем оба UI
        CloseAll();
        gameplayUI.SetActive(true);
        // Запускаем проверку интернета
        StartCoroutine(CheckInternetConnection());
    }

    private IEnumerator CheckInternetConnection()
    {
        UnityWebRequest request = UnityWebRequest.Get(testUrl);
        request.timeout = (int)checkTimeout;
        yield return request.SendWebRequest();
         if (request.result == UnityWebRequest.Result.Success)
        {
            hasInternet = true;
        }
        else
        {
            hasInternet = false;
        }
        SetUIState();
    }

    private void SetUIState()
    {
        gameplayUI.SetActive(true);
    }
    
    
    private void CloseAll()
    {
        noInternetUI.SetActive(false);
        noInternetUILand.SetActive(false);
        loadingScreen.SetActive(false);
        loadingScreenLand.SetActive(false);
        notificationWindow.SetActive(false);
        notificationWindowLand.SetActive(false);
        gameplayUI.SetActive(false);
        levelChooseWindow.SetActive(false);
    }

    public void OpenGameplayUI()
    {
        if (hasInternet)
        {
            if (loadingScreen != null) loadingScreen.SetActive(false);
            if (loadingScreenLand != null) loadingScreenLand.SetActive(false);
            if (gameplayUI != null) gameplayUI.SetActive(true);
            if (noInternetUI != null) noInternetUI.SetActive(false);
            if (noInternetUI != null) noInternetUILand.SetActive(false);
            if (notificationWindow != null) notificationWindow.SetActive(false);
            if (notificationWindowLand != null) notificationWindowLand.SetActive(false);
        }
    }

    private void Update()
    {
        if (Screen.width > Screen.height)
        {
            isPortraitOrientation = false;
        }
        else if (Screen.height > Screen.width)
        {
            isPortraitOrientation = true;
        }
        
        ChangeLoadingScreen();
        ChangeNoInternetScreen();
    }
    
    

    public void ChangeLoadingScreen()
    {
        if (loadingScreen.activeSelf || loadingScreenLand.activeSelf)
        {
            if (isPortraitOrientation)
            {
                loadingScreenLand.SetActive(false);
                loadingScreen.SetActive(true);
            }
            else
            {
                loadingScreenLand.SetActive(true);
                loadingScreen.SetActive(false);
            }
        }
    }

    public void ChangeNoInternetScreen()
    {
        if (noInternetUI.activeSelf || noInternetUILand.activeSelf)
        {
            if (isPortraitOrientation)
            {
                noInternetUILand.SetActive(false);
                noInternetUI.SetActive(true);
            }
            else
            {
                noInternetUILand.SetActive(true);
                noInternetUI.SetActive(false);
            }
        }
    }
    
    
    public void OpenPrivacyURL()
    {
        Application.OpenURL(privacyURL);
    }

    public void CloseOtherWindows()
    {
        if (loadingScreen != null) loadingScreen.SetActive(false);
        loadingScreenLand.SetActive(false);
        if (gameplayUI != null) gameplayUI.SetActive(false);
        if (noInternetUI != null) noInternetUI.SetActive(false);
    }

    public void RunLevel()
    {
        SceneManager.LoadScene(2);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenLevelChooseWindow()
    {
        gameplayUI.SetActive(false);
        levelChooseWindow.SetActive(true);
    }
    
    public void CloseLevelChooseWindow()
    {
        gameplayUI.SetActive(true);
        levelChooseWindow.SetActive(false);
    }

    public void OpenLevel(int i)
    {
        SceneManager.LoadScene(i);
    }
}