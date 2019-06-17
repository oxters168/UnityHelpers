using UnityEngine;
using GoogleMobileAds.Api;

public class AdMobController : MonoBehaviour
{
    private static AdMobController admobControllerInScene;

    public string appId;
    public bool testAds = true;

    public AdUnit[] adUnits;

    private void Awake()
    {
        admobControllerInScene = this;
    }
    private void Start()
    {
        string currentAppId;
        if (testAds)
        {
            #if UNITY_ANDROID
                currentAppId = "ca-app-pub-3940256099942544~3347511713";
            #elif UNITY_IPHONE
                currentAppId = "ca-app-pub-3940256099942544~1458002511";
            #else
                currentAppId = "unexpected_platform";
            #endif
        }
        else
            currentAppId = appId;

        MobileAds.Initialize(currentAppId);

        foreach (AdUnit adUnit in adUnits)
            adUnit.Initialize(testAds);
    }

    public static void ShowAd(int index)
    {
        admobControllerInScene.adUnits[index].Show();
    }
    public static void HideAd(int index)
    {
        admobControllerInScene.adUnits[index].Hide();
    }
}
