using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Frog SFX")]
    public AudioClip leapSound;
    public AudioClip normalLandingSound;
    public AudioClip frenzyLandingSound;
    public AudioClip drownSound;

    [Header("UI & Metagame SFX Clips")]
    public AudioClip comboPopSound;
    public AudioClip comboBreakSound;
    public AudioClip newHighScoreSound;

    [Header("Environmental SFX Clips")]
    public AudioClip padSinkWarningSound;

    [Header("Music Tracks")]
    public AudioClip mainMenuMusic;
    public AudioClip[] gameplayMusicPlaylist;
    private int lastPlayedTrackIndex = -1;

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

        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenuScene")
        {
            PlayMusic(mainMenuMusic, true);
        }
        else if (scene.name == "PondScene")
        {
            PlayNextGameplayTrack();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    public void PlaySFXWithPitch(AudioClip clip, float pitch)
    {
        if (clip != null)
        {
            sfxSource.pitch = Mathf.Clamp(pitch, 0.5f, 3f);
            sfxSource.PlayOneShot(clip);

            Invoke(nameof(ResetPitch), clip.length);
        }
    }

    private void ResetPitch()
    {
        sfxSource.pitch = 1f;
    }

    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicClip == null || musicSource.clip == musicClip) return;
        musicSource.clip = musicClip;
        musicSource.loop = loop;
        musicSource.volume = 0.35f;
        musicSource.Play();
    }

    public void PlayNextGameplayTrack()
    {
        if (gameplayMusicPlaylist == null || gameplayMusicPlaylist.Length == 0) return;

        if (gameplayMusicPlaylist.Length == 1)
        {
            PlayMusic(gameplayMusicPlaylist[0], true);
            return;
        }

        int randomIndex = lastPlayedTrackIndex;
        while (randomIndex == lastPlayedTrackIndex)
        {
            randomIndex = Random.Range(0, gameplayMusicPlaylist.Length);
        }

        lastPlayedTrackIndex = randomIndex;
        AudioClip nextTrack = gameplayMusicPlaylist[randomIndex];

        PlayMusic(nextTrack, false);

        CancelInvoke(nameof(OnTrackEnded));
        Invoke(nameof(OnTrackEnded), nextTrack.length);
    }

    private void OnTrackEnded()
    {
        PlayNextGameplayTrack();
    }
}
