using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip levelMusic;

    private void Awake()
    {
        // Duplicate (e.g. the one in GameScene): remove only this component and its
        // music source. Never Destroy(gameObject): other managers share that object.
        if (Instance != null && Instance != this)
        {
            if (musicSource != null) Destroy(musicSource);
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Only the surviving instance reaches here
        if (Instance != this) return;

        if (musicSource != null && levelMusic != null && !musicSource.isPlaying)
        {
            musicSource.clip = levelMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }
}