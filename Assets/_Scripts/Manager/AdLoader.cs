using UnityEngine;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class AdLoader : MonoBehaviour
{
    public static AdLoader Instance;

    private readonly string _bannerId = "ca-app-pub-1945244255127558/2361559821";
    private readonly string _interstitialId = "ca-app-pub-1945244255127558/9815160974";

    private BannerView _bannerView;
    private InterstitialAd _interstitialAd;
    private bool _isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        MobileAds.Initialize(initStatus => {
            _isInitialized = true;
            LoadBannerAd();
            LoadInterstitialAd();
        });
    }

    #region Banner Logic
    public void LoadBannerAd()
    {
        if (!_isInitialized) return;

        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
        }

        _bannerView = new BannerView(_bannerId, AdSize.Banner, AdPosition.Bottom);

        _bannerView.OnBannerAdLoaded += () =>
        {
            _bannerView.Show();
        };

        _bannerView.OnBannerAdLoadFailed += (error) =>
        {
            _bannerView = null;
            StartCoroutine(RetryLoad("banner", 15f));
        };

        _bannerView.LoadAd(new AdRequest());
    }

    public void ShowBannerAd()
    {
        _bannerView?.Show();
    }
    #endregion

    #region Interstitial Logic
    public void LoadInterstitialAd()
    {
        if (!_isInitialized) return;

        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        InterstitialAd.Load(_interstitialId, new AdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                StartCoroutine(RetryLoad("interstitial", 10f));
                return;
            }

            _interstitialAd = ad;

            _interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                LoadInterstitialAd();
            };
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

            StartCoroutine(ShowInterstitialWithDelay(1f));
        }
    }

    private IEnumerator ShowInterstitialWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _interstitialAd.Show();
        }
    }
    #endregion

    private IEnumerator RetryLoad(string type, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (type == "banner") LoadBannerAd();
        else LoadInterstitialAd();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            _bannerView?.Destroy();
            _interstitialAd?.Destroy();
        }
    }
}