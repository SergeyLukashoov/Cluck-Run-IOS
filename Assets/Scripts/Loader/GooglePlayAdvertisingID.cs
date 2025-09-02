using System;
using System.Collections;
using UnityEngine;

public class GooglePlayAdvertisingID
{
    public static void GetAdvertisingID(Action<string> callback)
    {
        string advertisingID = "";
        try
        {
            AndroidJavaClass up = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject> ("currentActivity");
            AndroidJavaClass client = new AndroidJavaClass ("com.google.android.gms.ads.identifier.AdvertisingIdClient");
            AndroidJavaObject adInfo = client.CallStatic<AndroidJavaObject> ("getAdvertisingIdInfo", currentActivity);
    
            advertisingID = adInfo.Call<string> ("getId").ToString();  
        }
        catch (Exception)
        {
            Debug.Log("Can't get GAID");
        }
        callback?.Invoke(advertisingID);
    }
}