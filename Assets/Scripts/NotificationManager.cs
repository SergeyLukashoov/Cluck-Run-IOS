using UnityEngine;
using Firebase.Messaging;
using System;
using UnityEngine.UI;

public class NotificationRequest : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject notificationPopup;
    [SerializeField] private Button allowButton;
    [SerializeField] private Button skipButton;

    private const string NOTIFICATION_ASKED_KEY = "NotificationAsked";
    private const string SKIP_DATE_KEY = "NotificationSkipDate";

    private bool firebaseInitialized = false;

    void Start()
    {
        allowButton.onClick.AddListener(OnAllowClicked);
        skipButton.onClick.AddListener(OnSkipClicked);
        CheckNotificationStatus();
    }

    private void CheckNotificationStatus()
    {
        if (PlayerPrefs.GetInt(NOTIFICATION_ASKED_KEY, 0) == 1)
        {
            notificationPopup.SetActive(false);
            InitializeFirebaseIfNeeded();
            return;
        }

        if (PlayerPrefs.HasKey(SKIP_DATE_KEY))
        {
            string skipDateString = PlayerPrefs.GetString(SKIP_DATE_KEY);
            if (DateTime.TryParse(skipDateString, out DateTime skipDate))
            {
                if (DateTime.Now < skipDate.AddDays(3))
                {
                    notificationPopup.SetActive(false);
                    return;
                }
            }
        }

        notificationPopup.SetActive(true);
    }

    private void OnAllowClicked()
    {
        PlayerPrefs.SetInt(NOTIFICATION_ASKED_KEY, 1);
        PlayerPrefs.DeleteKey(SKIP_DATE_KEY);

        #if UNITY_ANDROID
        if (GetAndroidVersion() >= 33)
        {
            UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
        #endif

        InitializeFirebaseMessaging();
        notificationPopup.SetActive(false);
    }

    private void OnSkipClicked()
    {
        PlayerPrefs.SetString(SKIP_DATE_KEY, DateTime.Now.ToString());
        PlayerPrefs.SetInt(NOTIFICATION_ASKED_KEY, 0);
        notificationPopup.SetActive(false);
    }

    private void InitializeFirebaseIfNeeded()
    {
        if (!firebaseInitialized)
        {
            InitializeFirebaseMessaging();
        }
    }

    private void InitializeFirebaseMessaging()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => 
        {
            if (task.Result == Firebase.DependencyStatus.Available)
            {
                FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
                FirebaseMessaging.SubscribeAsync("all");
                
                // Правильная подписка на события
                FirebaseMessaging.MessageReceived += OnMessageReceived;
                FirebaseMessaging.TokenReceived += OnTokenReceived;
                
                firebaseInitialized = true;
                Debug.Log("Firebase Messaging initialized");
            }
            else
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Result);
            }
        });
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Received notification: " + e.Message.Notification.Body);
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("FCM Token: " + token.Token);
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

    void OnDestroy()
    {
        allowButton.onClick.RemoveListener(OnAllowClicked);
        skipButton.onClick.RemoveListener(OnSkipClicked);
        
        // Отписываемся от событий только если Firebase был инициализирован
        if (firebaseInitialized)
        {
            FirebaseMessaging.MessageReceived -= OnMessageReceived;
            FirebaseMessaging.TokenReceived -= OnTokenReceived;
        }
    }
}