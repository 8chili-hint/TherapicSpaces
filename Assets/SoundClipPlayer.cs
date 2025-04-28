using UnityEngine;

public class SoundClipPlayer : MonoBehaviour
{
    public static SoundClipPlayer Instance;

    public AudioClip clickSound;
    public AudioClip selectSound;
    public AudioClip changeSceneSound;

    private AudioSource audioSource;

    void Awake()
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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    public void PlaySelectSound()
    {
        if (selectSound != null)
        {
            audioSource.PlayOneShot(selectSound);
        }
    }

    public void PlayChangeSceneSound()
    {
        if (changeSceneSound != null)
        {
            audioSource.PlayOneShot(changeSceneSound);
        }
    }
}
