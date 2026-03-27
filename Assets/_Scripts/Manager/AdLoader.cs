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

            SceneManager.sceneLoaded += OnSceneLoaded;
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

    // Mỗi khi chuyển sang Scene bất kỳ (Menu hay Play), hàm này sẽ chạy
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ShowBannerAd();
    }

    #region Banner Logic
    public void LoadBannerAd()
    {
        if (!_isInitialized || _bannerView != null) return;

        _bannerView = new BannerView(_bannerId, AdSize.Banner, AdPosition.Bottom);
        _bannerView.LoadAd(new AdRequest());

        _bannerView.OnBannerAdLoadFailed += (error) => {
            _bannerView = null;
            StartCoroutine(RetryLoad("banner", 15f));
        };
    }

    public void ShowBannerAd()
    {
        // Kiểm tra nếu banner tồn tại thì gọi hiện lại cho chắc chắn
        _bannerView?.Show();
    }
    #endregion

    #region Interstitial Logic
    public void LoadInterstitialAd()
    {
        if (!_isInitialized) return;
        if (_interstitialAd != null) _interstitialAd.Destroy();

        InterstitialAd.Load(_interstitialId, new AdRequest(), (ad, error) => {
            if (error != null)
            {
                StartCoroutine(RetryLoad("interstitial", 15f));
                return;
            }
            _interstitialAd = ad;
            _interstitialAd.OnAdFullScreenContentClosed += () => { LoadInterstitialAd(); };
        });
    }

    public void ShowInterstitialAd()
    {
        // Vẫn dùng cho logic Lose() trong Blocks.cs
        if (_interstitialAd != null && _interstitialAd.CanShowAd()) _interstitialAd.Show();
        else LoadInterstitialAd();
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
        // Chỉ hủy sự kiện và dọn dẹp nếu đây là Instance chính
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _bannerView?.Destroy();
            _interstitialAd?.Destroy();
        }
    }
}