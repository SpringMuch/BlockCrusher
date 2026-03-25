using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.SceneManagement;

public class AdLoader : MonoBehaviour
{
    public static AdLoader Instance;

    private BannerView _bannerView;
    private InterstitialAd _interstitialAd;
    private bool isAdMobInitialized = false;

#if UNITY_ANDROID
    private string _bannerId = "ca-app-pub-3940256099942544/6300978111";
    private string _interstitialId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IOS
    private string _bannerId = "ca-app-pub-3940256099942544/2934735716";
    private string _interstitialId = "ca-app-pub-3940256099942544/4411468910";
#else
    private string _bannerId = "unused";
    private string _interstitialId = "unused";
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        MobileAds.Initialize(initStatus => {
            isAdMobInitialized = true;
        });
    }
    void Update()
    {
        if (isAdMobInitialized)
        {
            isAdMobInitialized = false;
            LoadBannerAd();
            LoadInterstitialAd();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadBannerAd();
    }

    public void LoadBannerAd()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
        }

        _bannerView = new BannerView(_bannerId, AdSize.Banner, AdPosition.Bottom);

        AdRequest adRequest = new AdRequest();
        _bannerView.LoadAd(adRequest);
    }

    public void HideBannerAd()
    {
        if (_bannerView != null) _bannerView.Hide();
    }

    public void ShowBannerAd()
    {
        if (_bannerView != null) _bannerView.Show();
    }

    public void DestroyBannerAd()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
        }
    }

    public void LoadInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        AdRequest adRequest = new AdRequest();

        InterstitialAd.Load(_interstitialId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Interstitial ad failed to load with error: " + error);
                    return;
                }
                _interstitialAd = ad;

                RegisterEventHandlers(_interstitialAd);
            });
    }

    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _interstitialAd.Show();
        }
        else
        {
            LoadInterstitialAd();
        }
    }

    private void RegisterEventHandlers(InterstitialAd interstitialAd)
    {
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            LoadInterstitialAd();
        };

        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            LoadInterstitialAd();
        };
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        DestroyBannerAd();
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
        }
    }
}