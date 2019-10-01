using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackPanel : MonoBehaviour
{
    private UIFader faderScript;
    public Camera cam;
    public GameObject background;
    private RectTransform backgroundTransf;

    void Start()
    {
        GameObject camObj = GameObject.FindGameObjectWithTag("Main Camera");
        cam = camObj.GetComponent<Camera>();
        this.GetComponent<Canvas>().worldCamera = cam;
        faderScript = this.GetComponent<UIFader>();
        backgroundTransf = background.GetComponent<RectTransform>();
    }

    void Update()
    {
        // adjust background values
        RectTransform thisTransf = this.gameObject.GetComponent<RectTransform>();
        backgroundTransf.localScale = thisTransf.localScale;
    }

    public void FadeIn()
    {
        faderScript.FadeIn();
    }

    public void FadeOut()
    {
        faderScript.FadeOut();
    }
}
