using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFader : MonoBehaviour
{
    public CanvasGroup uiElement;

    public void FadeIn(float lerpTime = 0.5f)
    {
        StartCoroutine(FadeCanvasGroup(uiElement, uiElement.alpha, 1, lerpTime));
    }

    public void FadeOut(float lerpTime = 0.5f)
    {
        StartCoroutine(FadeCanvasGroup(uiElement, uiElement.alpha, 0, lerpTime));
    }

    public IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float start, float end, float lerpTime = 0.5f)
    {
        float _timeStartedLerping = Time.time;
        float timeSinceStarted = Time.time - _timeStartedLerping;
        float percentageComplete = timeSinceStarted / lerpTime;

        while (true) 
        {
            timeSinceStarted = Time.time - _timeStartedLerping;
            percentageComplete = timeSinceStarted / lerpTime;

            float currentVal = Mathf.Lerp(start, end, percentageComplete);

            canvasGroup.alpha = currentVal;

            if (percentageComplete >= 1) break;

            yield return new WaitForEndOfFrame();
        }
    }
}
