using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;

public class AppsFlyerObjectScript : MonoBehaviour , IAppsFlyerConversionData{
    void Start()
    {
        AppsFlyer.initSDK("BDREFvBLEZQKVYEhZafc85", "appID", this);
        AppsFlyer.startSDK();
    }

    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
        Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("onConversionDataFail", error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution: This method was replaced by UDL. This is a fake call.", attributionData);
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure: This method was replaced by UDL. This is a fake call.", error);
    }
}