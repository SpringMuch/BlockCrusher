using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Toggle musicToggle;

    private void Start()
    {
        if (SoundManager.Instance != null)
        {
            if (soundToggle != null)
            {
                soundToggle.SetIsOnWithoutNotify(SoundManager.Instance.IsSoundOn);
            }

            if (musicToggle != null)
            {
                musicToggle.SetIsOnWithoutNotify(SoundManager.Instance.IsMusicOn);
            }
        }
    }

    public void OnReplayClick()
    {
        SoundManager.Instance.PlaySound(SoundType.Click);
        SceneManager.LoadScene("PlayScene");
    }

    public void OnMenuClick()
    {
        SoundManager.Instance.PlaySound(SoundType.Click);
        SceneManager.LoadScene("MenuScene");
    }

    public void OnStudyClick()
    {
        SoundManager.Instance.PlaySound(SoundType.Click);
        SceneManager.LoadScene("StudyScene");
    }

    public void OnAdventureClick()
    {
        SoundManager.Instance.PlaySound(SoundType.Click);
        SceneManager.LoadScene("AdventureScene");
    }

    public void OnSettingClick()
    {
        SoundManager.Instance.PlaySound(SoundType.Click);
        settingPanel.SetActive(true);
    }

    public void OnCloseClick()
    {
        SoundManager.Instance.PlaySound(SoundType.Click);
        settingPanel.SetActive(false);
    }

    public void OnToggleMusicClick()
    {
        SoundManager.Instance.ToggleMusicOnClick();
        SoundManager.Instance.PlaySound(SoundType.Click);
    }

    public void OnToggleSoundClick()
    {
        SoundManager.Instance.ToggleSoundOnClick();
        SoundManager.Instance.PlaySound(SoundType.Click);
    }
}