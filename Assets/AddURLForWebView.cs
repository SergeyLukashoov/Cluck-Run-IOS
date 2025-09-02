using System;
using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;
using Application = UnityEngine.Application;
using Screen = UnityEngine.Screen;

public class AddURLForWebView : MonoBehaviour
{
    private UniWebView webView;
    [SerializeField] private GameObject GameCanvas;

    private void Awake()
    {
        UniWebView.SetAllowAutoPlay(true);
        UniWebView.SetAllowInlinePlay(true);
        UniWebView.SetEnableKeyboardAvoidance(true);
        UniWebView.SetJavaScriptEnabled(true);
    }
    
    public void RunWebView()
    {
        if (webView == null)
        {
            UniWebView.SetEnableKeyboardAvoidance(true);
            webView = gameObject.AddComponent<UniWebView>();
        }
        GameCanvas.SetActive(false);
        if (webView == null) return;
        
        SetUpWebView();
        
        webView.SetUserAgent("Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.6723.86 Mobile Safari/537.36");
        webView.RegisterOnRequestMediaCapturePermission(permission => UniWebViewMediaCapturePermissionDecision.Grant); 
        
        webView.urlOnStart = PlayerPrefs.GetString("URLForWebWiev", "");
    }
    
    public void RunWebViewWithUrl(string specificUrl)
    {
        if (webView == null)
        {
            UniWebView.SetEnableKeyboardAvoidance(true);
            webView = gameObject.AddComponent<UniWebView>();
        }
    
        GameCanvas.SetActive(false);
        if (webView == null) return;
    
        SetUpWebView();
    
        webView.SetUserAgent("Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.6723.86 Mobile Safari/537.36");
        webView.RegisterOnRequestMediaCapturePermission(permission => UniWebViewMediaCapturePermissionDecision.Grant); 
    
        // Используем переданный URL или URL из конфига, если specificUrl пустой
        string urlToLoad = !string.IsNullOrEmpty(specificUrl) ? specificUrl : PlayerPrefs.GetString("URLForWebWiev", "");
    
        if (!string.IsNullOrEmpty(urlToLoad))
        {
            webView.Load(urlToLoad);
            webView.Show();
        }
        else
        {
            Debug.LogError("No valid URL provided for WebView");
            // Можно добавить fallback-логику здесь
        }
    }
    
    private void SetUpWebView()
    {
        webView.EmbeddedToolbar.Hide();
        webView.SetSupportMultipleWindows(true, true);
        webView.SetAllowFileAccess(true);
        webView.SetCalloutEnabled(true);
        webView.SetBackButtonEnabled(true);
        webView.SetAllowBackForwardNavigationGestures(true);
        webView.SetAcceptThirdPartyCookies(true);
        
        webView.OnShouldClose += webView => false;
        webView.OnPageFinished += (webView, code, url) => OnLoadFinished(webView);
        
        webView.AddUrlScheme("paytmmp");
        webView.AddUrlScheme("phonepe");
        webView.AddUrlScheme("bankid");
        webView.OnMessageReceived += (v, message) => Application.OpenURL(message.RawMessage);
        
        webView.OnPageStarted += (view, url) => 
        {
            if (!url.StartsWith("http")) 
                AppsFlyer.sendEvent("deeplink_load", new Dictionary<string, string> { {"url", url} });
        };
        
        webView.OnLoadingErrorReceived += OnLoadingError;
        
        webView.OnOrientationChanged += (view, orientation) =>
        {
            view.Frame = FlipRectY(Screen.safeArea);
        };
        webView.OnMultipleWindowOpened += (view, id) =>
        {
            view.Frame = FlipRectY(Screen.safeArea);
            view.ScrollTo(0, 0, false);
        };
    }

    private void OnLoadingError(UniWebView view, int code, string message, UniWebViewNativeResultPayload payload)
    {
        if (code is not (-1007 or -9 or 0)) return;
        if (payload.Extra != null && payload.Extra.TryGetValue(UniWebViewNativeResultPayload.ExtraFailingURLKey, out var value))
            view.Load((string)value);
    }
    
    private static Rect FlipRectY(Rect rect) => new(rect.x, Screen.height - rect.yMax, rect.width, rect.height);
    
    private void OnLoadFinished(UniWebView view) 
    {
        IEnumerator Show()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.3f);
            view.Show();
            yield return new WaitForEndOfFrame();
            view.Frame = FlipRectY(Screen.safeArea);
        }
        StartCoroutine(Show());
    }
}

// Кастомная реализация интерфейса OnGlobalLayoutListener
public class GlobalLayoutListener : AndroidJavaProxy
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