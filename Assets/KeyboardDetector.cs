using UnityEngine;
using System.Collections;

public class KeyboardDetector : MonoBehaviour
{
    private bool isKeyboardVisible = false;

    // Добавьте эти события для внешних подписчиков
    public event System.Action<UniWebView> OnKeyboardOpened;
    public event System.Action<UniWebView> OnKeyboardClosed;

    [SerializeField] public UniWebView targetWebView;

    void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        InitKeyboardListener();
        #endif
    }

    private void InitKeyboardListener()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activityRootView = unityClass.GetStatic<AndroidJavaObject>("currentActivity")
                .Call<AndroidJavaObject>("getWindow")
                .Call<AndroidJavaObject>("getDecorView")
                .Call<AndroidJavaObject>("getRootView");

            AndroidJavaObject observer = activityRootView.Call<AndroidJavaObject>("getViewTreeObserver");

            observer.Call("addOnGlobalLayoutListener", new GlobalLayoutListener(() =>
            {
                DetectKeyboardState();
            }));
        }
        #endif
    }

    private void DetectKeyboardState()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            
            AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect");
            AndroidJavaObject view = activity.Call<AndroidJavaObject>("getWindow")
                .Call<AndroidJavaObject>("getDecorView")
                .Call<AndroidJavaObject>("getRootView");
            
            view.Call("getWindowVisibleDisplayFrame", rect);
            
            AndroidJavaObject display = activity.Call<AndroidJavaObject>("getWindowManager")
                .Call<AndroidJavaObject>("getDefaultDisplay");
            
            AndroidJavaObject size = new AndroidJavaObject("android.graphics.Point");
            display.Call("getSize", size);
            
            int screenHeight = size.Get<int>("y");
            int visibleHeight = rect.Get<int>("bottom") - rect.Get<int>("top");
            int heightDiff = screenHeight - visibleHeight;

            bool newKeyboardState = heightDiff > screenHeight / 3;

            if (newKeyboardState != isKeyboardVisible)
            {
                isKeyboardVisible = newKeyboardState;
                
                if (isKeyboardVisible)
                {
                    OnKeyboardOpened?.Invoke(targetWebView);
                }
                else
                {
                    OnKeyboardClosed?.Invoke(targetWebView);
                }
            }
        }
        #endif
    }

    // Вложенный класс для обработки событий
    class GlobalLayoutListener : AndroidJavaProxy
    {
        private System.Action callback;

        public GlobalLayoutListener(System.Action callback) 
            : base("android.view.ViewTreeObserver$OnGlobalLayoutListener")
        {
            this.callback = callback;
        }

        void onGlobalLayout()
        {
            callback?.Invoke();
        }
    }

    void OnDestroy()
    {
        // Очистка ресурсов
        OnKeyboardOpened = null;
        OnKeyboardClosed = null;
    }
}