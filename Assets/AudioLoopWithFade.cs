using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLoopWithFade : MonoBehaviour
{
   
    public List<AudioClip> audioClips;

    private AudioSource audioSource;
    public float fadeDuration = 1.0f;

    public float minDelay = 1.0f;

    public float maxDelay = 5.0f;
    private int currentClipIndex = 0;
    private Coroutine fadeCoroutine;

   
    void Awake()
    {
      
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            
            audioSource = gameObject.AddComponent<AudioSource>();
        }

       
        if (audioClips == null || audioClips.Count == 0)
        {
            Debug.LogError("No audio clips assigned to the RandomLoopWithFade script on " + gameObject.name);
            this.enabled = false;
            return;
        }
        audioSource.loop = false;
        fadeCoroutine = StartCoroutine(FadeInAndStartLoop());
    }

[ContextMenu("StartAudio")]
   
    IEnumerator FadeInAndStartLoop()
    {
        audioSource.volume = 0f; 
        PlayRandomClip();
        float timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(0f, 0.5f, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = 0.5f;
        StartCoroutine(StartLoop());
    }

    IEnumerator StartLoop()
    {
        while (true)
        {
           
            if (audioSource.isPlaying)
                yield return null;


            float delay = audioSource.clip.length;
            yield return new WaitForSeconds(delay);

           
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            
            fadeCoroutine = StartCoroutine(FadeOutAndSwitchClip());

            yield return null;
        }
    }

    IEnumerator FadeOutAndSwitchClip()
    {
        float timeElapsed = 0f;
        float startVolume = audioSource.volume;

        
        while (timeElapsed < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = 0f; 
        PlayRandomClip();

        timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(0f, 0.5f, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = 0.5f;
    }

 
    void PlayRandomClip()
    {
        if (audioClips.Count == 0) return;

      
        int previousClipIndex = currentClipIndex;
        while (currentClipIndex == previousClipIndex && audioClips.Count > 1) 
        {
            currentClipIndex = Random.Range(0, audioClips.Count);
        }

        
        audioSource.clip = audioClips[currentClipIndex];
        audioSource.Play();
    }

    
    private void OnDisable()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}
