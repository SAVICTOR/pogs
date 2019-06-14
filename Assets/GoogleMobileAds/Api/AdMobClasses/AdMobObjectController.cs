using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class AdMobObjectController : MonoBehaviour
{

    private int showAttempts = 0;
    public bool loadedAdmob = false;

    private string AndroidCallerID = "android";
    private string IOSCallerID = "ios";
    private string WPCallerID = "wp";
    private WWW www_image = null;
    public string[] frames;
    public int[] adsShowed;

    private string APIUrl = "http://houseadsserver.com/ServerPlay/default.php?";

    private bool canPushButtons = true;

    public GameObject admobAdsObject;
    public GameObject adView;
    public GameObject loadingPanel;
    private string storeAppID;

    private int attempts = 0;
    private bool isVisible = false;
    // Use this for initialization
    void Start()
    {

        DontDestroyOnLoad(admobAdsObject.transform.gameObject);

        GameManager.Instance.adsScript = this;

        if (!AdMobObjectSingleton.Instance.houseAdDisplayed)
        {
            StartCoroutine(DownloadAdData());
        }

    }

    public void destroy()
    {
        if (admobAdsObject != null)
            DestroyImmediate(admobAdsObject);
    }

    IEnumerator DownloadAdData()
    {
        string os;
        string calling_app;// = AndroidCallerID;

#if UNITY_ANDROID
        os = "android";
        calling_app = AndroidCallerID;
#elif UNITY_IOS
        os = "ios";
        calling_app = IOSCallerID;
#else
        os = "wp";
        calling_app = WPCallerID;
#endif

        yield return new WaitForSeconds(2);

        WWW www = new WWW(APIUrl + "os=" + os + "&calling_app=" + calling_app);
        yield return www;

        if (www.error == null && www.text.Contains("API_DATA_BEGIN|"))
        {
            string image_url = www.text.Substring(www.text.IndexOf("API_DATA_BEGIN|") + "API_DATA_BEGIN|".Length, www.text.Length - "API_DATA_BEGIN|".Length - "|API_DATA_END".Length - 1);

            string[] splitted = image_url.Split(';');



            frames = splitted[1].Split('-');

            adsShowed = new int[frames.Length];

            for (int i = 0; i < frames.Length; i++)
            {
                adsShowed[i] = Int32.Parse(frames[i]);
            }

            image_url = splitted[0];

            storeAppID = image_url;
            //Debug.Log (www.text.Substring(www.text.IndexOf("API_DATA_BEGIN|") + "API_DATA_BEGIN|".Length, www.text.Length - "API_DATA_BEGIN|".Length - "|API_DATA_END".Length - 1));

            // Downloading image
            string img_url = "";
            if (Screen.orientation == ScreenOrientation.Landscape)
            {
#if UNITY_ANDROID
                img_url = "http://houseadsserver.com/ServerPlay/Android_PNG/Landscape/" + image_url + ".png";
#elif UNITY_IOS
                img_url = "http://houseadsserver.com/ServerPlay/iOS_PNG/Landscape/" + image_url + ".png";
#else
                img_url = "http://houseadsserver.com/ServerPlay/WP_PNG/Landscape/" + image_url + ".png";
#endif

            }
            else if (Screen.orientation == ScreenOrientation.Portrait)
            {
#if UNITY_ANDROID
                img_url = "http://houseadsserver.com/ServerPlay/Android_PNG/Portrait/" + image_url + ".png";
#elif UNITY_IOS
                img_url = "http://houseadsserver.com/ServerPlay/iOS_PNG/Portrait/" + image_url + ".png";
#else
                img_url = "http://houseadsserver.com/ServerPlay/WP_PNG/Portrait/" + image_url + ".png";
#endif
            }



            www_image = new WWW(img_url);





        }
    }

    public void ShowAd()
    {

        if (loadedAdmob)
        {

            showAttempts++;

            bool showH = false;

            for (int i = 0; i < adsShowed.Length; i++)
            {
                if (adsShowed[i] == showAttempts)
                {
                    showH = true;
                    break;
                }
            }

            if (showH)
            {
                if (www_image != null && www_image.error == null && www_image.texture.width != 8 && www_image.texture.height != 8)
                {


                    adView.GetComponent<Image>().sprite = Sprite.Create(www_image.texture, new Rect(0, 0, www_image.texture.width, www_image.texture.height), new Vector2(0.5f, 0.5f));

                    loadingPanel.SetActive(true);

                    AdMobObjectSingleton.Instance.houseAdDisplayed = true;
                    Invoke("enableButtons", 2);
                    Screen.fullScreen = false;
                    isVisible = true;

                }
                else
                {
                    GameManager.Instance.interstitialAds.showInterstitial();
                }
            }
            else
            {
                GameManager.Instance.interstitialAds.showInterstitial();
            }



        }


    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isVisible)
        {
            loadingPanel.SetActive(false);
            Screen.fullScreen = true;
            isVisible = false;
        }
    }


    public void openAppStore()
    {
        if (canPushButtons)
        {
            Debug.Log("Open store!!");

#if UNITY_ANDROID
            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "market://details?id=" + storeAppID);

            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject(
                            "android.content.Intent",
                            intentClass.GetStatic<string>("ACTION_VIEW"),
                            uriObject
            );

            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

            currentActivity.Call("startActivity", intentObject);
#elif UNITY_IOS
            Application.OpenURL("itms-apps://itunes.apple.com/app/id" + storeAppID);
#else
			Application.OpenURL("ms-windows-store:navigate?appid=" + storeAppID);
#endif


            //loadingPanel.SetActive(false);
        }
    }

    public void closeAd()
    {
        if (canPushButtons)
        {
            loadingPanel.SetActive(false);
            Screen.fullScreen = true;
            isVisible = false;
            //GetComponent<Animator>().Play("HouseAdsShowDownAnimation");
        }
    }

    private void enableButtons()
    {
        canPushButtons = true;
    }
}
