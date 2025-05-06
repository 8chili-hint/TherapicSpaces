using System.Collections;
using UnityEngine;

public class AmbientAudioClipManager : MonoBehaviour
{
    public AudioSource audioSource;
    public float AudioVolume;
    public static AmbientAudioClipManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayAudio(AudioClip clip)
    {
        StartCoroutine(AudioFadeOut());
        audioSource.Stop();
        audioSource.volume = 0;
        audioSource.clip = clip;
        audioSource.loop = true;
        StartCoroutine(AudioFadeIn());

    }
    IEnumerator AudioFadeOut()
    {
        float timeElapsed = 0f;
        float duration = 2;
        while (timeElapsed < duration)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, 0, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
    IEnumerator AudioFadeIn()
    {
        float timeElapsed = 0f;
        float duration = 2;
        while (timeElapsed < duration)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, AudioVolume, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}
