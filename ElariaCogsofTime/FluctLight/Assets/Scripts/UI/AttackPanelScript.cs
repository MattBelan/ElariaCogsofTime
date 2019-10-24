using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackPanelScript : UIFader
{
    // Object references
    public GameObject[] sprites;

    // Display booleans
    private bool display = true;
    public bool dismiss = false;
    public bool dismissAfterDelay = false;
    public bool destroy = false;
    public bool destroyAfterDelay = false;

    // Display timers
    public float dismissTimer = 1.0f;
    public float destroyTimer = 1.0f;

    // Health Display num
    public int dynamicHealthVal;

    protected override void Update()
    {
        base.Update(); // Child class update

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

        // Timers
        if (dismissAfterDelay)
        {
            dismissTimer -= Time.deltaTime;
        }
        if (destroyAfterDelay)
        {
            destroyTimer -= Time.deltaTime;
        }

        // Display the window
        if (display)
        {
            Animator anim = this.gameObject.GetComponent<Animator>();
            anim.Play("Attk_Panel_Enter");
            display = false;
        }
        // Dismiss window
        if (dismiss || dismissTimer <= 0)
        {
            dismiss = false;
            playFadeOut = true;
            dismissAfterDelay = false;
            dismissTimer = 1.0f;
        }
        // Destroy window
        if (destroy || destroyTimer <= 0)
        {
            Destroy(gameObject.GetComponentInParent<RectTransform>().gameObject);
        }
    }
}
