using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [Header("SFX Parameters")]
    [SerializeField] AudioSource uiSource;
    [SerializeField] AudioClip buttonClick;
    [SerializeField] AudioClip buttonClickUI;
    [SerializeField] float sfxVolume = 1f;

    [Header("Music Parameters")]
    [SerializeField] AudioClip[] musicTracks;
    [SerializeField] AudioSource musicSource;
    [SerializeField] float musicVolume = 1f;
    [SerializeField] float fadeDuration = 2f; // fade time in seconds
    private int currentTrackIndex = 0;

    public static SoundManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(PlayMusicTracksWithFade());
    }

    IEnumerator PlayMusicTracksWithFade()
    {
        while (true)
        {
            AudioClip nextTrack = musicTracks[currentTrackIndex];
            yield return StartCoroutine(FadeInMusic(nextTrack));

            // wait for track to finish before switching
            yield return new WaitForSeconds(nextTrack.length - fadeDuration);

            yield return StartCoroutine(FadeOutMusic());

            // switch to next track
            currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
        }
    }

    IEnumerator FadeInMusic(AudioClip track)
    {
        musicSource.clip = track;
        musicSource.volume = 0f;
        musicSource.Play();

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / fadeDuration);
            yield return null;
        }
        musicSource.volume = musicVolume;
    }

    IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume;
    }

    public void PlaySoundButton()
    {
        uiSource.PlayOneShot(buttonClick, sfxVolume);
    }

    public void PlaySoundUI()
    {
        uiSource.PlayOneShot(buttonClickUI, sfxVolume);
    }

    // Attach click listeners automatically for all buttons in the scene
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Find all buttons in the new scene
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => PlaySoundUI());
        }
    }
}
