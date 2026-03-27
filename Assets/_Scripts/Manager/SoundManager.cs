using UnityEngine;
using System.Collections.Generic;

public enum SoundType
{
    Break, FullBreak, Click, Place, Win, Lose
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Clips")]
    [SerializeField] private List<AudioClip> breakSounds;
    [SerializeField] private List<AudioClip> fullBreakSounds;
    [SerializeField] private List<AudioClip> clickSounds;
    [SerializeField] private List<AudioClip> placeSounds;
    [SerializeField] private List<AudioClip> winSounds;
    [SerializeField] private List<AudioClip> loseSounds;
    [SerializeField] private AudioClip backgroundMusic;

    private AudioSource audioSource;

    private bool isSoundOn;
    private bool isMusicOn;
    public bool IsSoundOn => isSoundOn;
    public bool IsMusicOn => isMusicOn;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        isSoundOn = PlayerPrefs.GetInt("SoundSetting", 1) == 1;
        isMusicOn = PlayerPrefs.GetInt("MusicSetting", 1) == 1;

        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
        }
    }

    private void Start()
    {
        if (isMusicOn)
        {
            PlayBackgroundMusic();
        }
    }

    public void PlaySound(SoundType soundType)
    {
        if (!isSoundOn) return;

        AudioClip clip = GetRandomClip(soundType);
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        audioSource.Stop();
    }

    public void ToggleSoundOnClick() => ToggleSound(!isSoundOn);

    public void ToggleMusicOnClick() => ToggleMusic(!isMusicOn);

    public void ToggleSound(bool on)
    {
        isSoundOn = on;
        PlayerPrefs.SetInt("SoundSetting", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleMusic(bool on)
    {
        isMusicOn = on;
        PlayerPrefs.SetInt("MusicSetting", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();

        if (isMusicOn)
        {
            PlayBackgroundMusic();
        }
        else
        {
            StopBackgroundMusic();
        }
    }

    private AudioClip GetRandomClip(SoundType soundType)
    {
        List<AudioClip> sounds = null;
        switch (soundType)
        {
            case SoundType.Break: sounds = breakSounds; break;
            case SoundType.FullBreak: sounds = fullBreakSounds; break;
            case SoundType.Click: sounds = clickSounds; break;
            case SoundType.Place: sounds = placeSounds; break;
            case SoundType.Win: sounds = winSounds; break;
            case SoundType.Lose: sounds = loseSounds; break;
        }

        if (sounds != null && sounds.Count > 0)
        {
            return sounds[Random.Range(0, sounds.Count)];
        }
        return null;
    }
}