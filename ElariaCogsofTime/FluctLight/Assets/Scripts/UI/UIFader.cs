using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFader : MonoBehaviour
{
    // Canvas elements
    public CanvasGroup uiElement;

    // MonoBehaviours
    private bool lateStart = true;
    
    // Animation Vars
    public bool playFadeIn = false;
    public float fadeInTime = 0.6f;
    public bool playFadeOut = false;
    public float fadeOutTime = 0.6f;

    // Lerp
    private float startTime;

    protected virtual void Update()
    {
        // Acts as start (for dynamically instantiated objects)
        if (lateStart) 
        {
            uiElement = this.gameObject.GetComponent<CanvasGroup>();
            uiElement.alpha = 0;
            startTime = 0;
            lateStart = false; // don't run again
        }

        // Opacity to 1
        if (playFadeIn)
        {
            bool finished = Fade(0.0f, 1.0f, fadeInTime);
            if (finished)
            {
                startTime = 0;
                playFadeIn = false;
            }
            else 
            {
                playFadeIn = true;
            }
        }
        // Opacity to 0
        else if (playFadeOut)
        {
            bool finished = Fade(1.0f, 0.0f, fadeOutTime);
            if (finished)
            {
                startTime = 0;
                playFadeOut = false;
            }
            else 
            {
                playFadeOut = true;
            }
        }
    }

    public bool Fade(float startVal, float endVal, float fadeTime)
    {
        startTime += Time.deltaTime / fadeTime;
        uiElement.alpha = Mathf.Lerp(startVal, endVal, startTime);

        return uiElement.alpha == endVal
                ? true
                : false;
    }
}
