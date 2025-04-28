using System;
using System.Collections;
using UnityEngine;

public class CameraFade : MonoBehaviour
{
    // Public field for the CanvasGroup
    public CanvasGroup canvasGroup;

    // Public field for the duration of the fade
    public float duration = 1.0f;

    // Coroutine for fading
    private Coroutine fadeCoroutine;

    public static CameraFade Instance;


    public static Action FadeInComplete;
    // Make sure CanvasGroup is assigned
    void Awake()
    {
        Instance = this;
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("CanvasGroupFader: No CanvasGroup assigned and none found on GameObject.  Disabling script.");
                this.enabled = false;
            }
        }
        StartCoroutine(FadeInOutCoroutine(true));
    }
    // Public method to start the fade-in-out sequence
    public void FadeInOut()
    {
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // Start the fade coroutine
        fadeCoroutine = StartCoroutine(FadeInOutCoroutine(false));
    }

    // Coroutine for fading in and out
    IEnumerator FadeInOutCoroutine(bool FadeOutOnly)
    {
        float timeElapsed = 0f;
        // Fade in
        if (!FadeOutOnly)
        {
            
            while (timeElapsed < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f; // Ensure alpha is 1

            // Fade out
            FadeInComplete?.Invoke();
            timeElapsed = 0f;
            while (timeElapsed < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0f; // Ensure alpha is 0
        }
        else
        {
            while (timeElapsed < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0f; // Ensure alpha is 0
        }
    }

    // OnDisable to stop coroutine.
    private void OnDisable()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}
