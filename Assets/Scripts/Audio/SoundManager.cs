using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all game audio: BGM, SFX, and volume settings.
/// Singleton pattern – persists across scene loads.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip shopMusic;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip spinSFX;
    [SerializeField] private AudioClip reelStopSFX;
    [SerializeField] private AudioClip winSFX;
    [SerializeField] private AudioClip bigWinSFX;
    [SerializeField] private AudioClip jackpotSFX;
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip coinDropSFX;
    [SerializeField] private AudioClip bonusSFX;
    [SerializeField] private AudioClip errorSFX;

    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY   = "SFXVolume";

    private float musicVolume = 1f;
    private float sfxVolume   = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadVolumeSettings();
    }

    private void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
        sfxVolume   = PlayerPrefs.GetFloat(SFX_VOL_KEY,   1f);
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource   != null) sfxSource.volume   = sfxVolume;
    }

    // ── Music ────────────────────────────────────────────────────────────────

    public void PlayMenuMusic()   => PlayMusic(menuMusic);
    public void PlayGameMusic()   => PlayMusic(gameMusic);
    public void PlayShopMusic()   => PlayMusic(shopMusic);

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    // ── SFX ─────────────────────────────────────────────────────────────────

    public void PlaySpin()       => PlaySFX(spinSFX);
    public void PlayReelStop()   => PlaySFX(reelStopSFX);
    public void PlayWin()        => PlaySFX(winSFX);
    public void PlayBigWin()     => PlaySFX(bigWinSFX);
    public void PlayJackpot()    => PlaySFX(jackpotSFX);
    public void PlayButtonClick() => PlaySFX(buttonClickSFX);
    public void PlayCoinDrop()   => PlaySFX(coinDropSFX);
    public void PlayBonus()      => PlaySFX(bonusSFX);
    public void PlayError()      => PlaySFX(errorSFX);

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ── Volume API ───────────────────────────────────────────────────────────

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            if (musicSource != null) musicSource.volume = musicVolume;
            PlayerPrefs.SetFloat(MUSIC_VOL_KEY, musicVolume);
        }
    }

    public float SFXVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            if (sfxSource != null) sfxSource.volume = sfxVolume;
            PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxVolume);
        }
    }
}
