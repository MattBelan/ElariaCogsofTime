using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackPanelScript : UIFader
{
    private UIFader fadeEffect;
    public GameObject[] sprites;

    public bool lateStart = true;
    public bool dismissPanel = false;
    public bool displayPanel = false;

    void Start()
    {
        uiElement = this.gameObject.GetComponent<CanvasGroup>();
        uiElement.alpha = 0;
    }

    void Update()
    {
        // Start
        if (lateStart)
        {
            FadeIn();
            lateStart = false;
        }

        // Directly apply alpha value to sprites
        foreach (GameObject sprite in sprites) 
        {
            SpriteRenderer renderer = sprite.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = uiElement.alpha;
                renderer.color = color;
            }
        }

        // Dismiss window
        if (dismissPanel)
        {
            FadeOut();
            dismissPanel = false;
        }
        // Display the window
        if (displayPanel)
        {
            Animator anim = this.gameObject.GetComponent<Animator>();
            anim.Play("Attk_Panel_Enter");
            FadeIn();
            displayPanel = false;
        }
    }
}
