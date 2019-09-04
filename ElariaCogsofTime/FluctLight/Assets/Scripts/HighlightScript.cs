using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightScript : MonoBehaviour {

	private SpriteRenderer sr;
	private Color clr;
	private float dir;

	// Use this for initialization
	void Start () {
		sr = this.GetComponent<SpriteRenderer>();
		clr = Color.white;
		clr.a = 0.25f;
		dir = -0.01f; // Increment val
	}
	
	// Update is called once per frame
	void Update () {
		if (clr.a <= 0.05f || clr.a >= 0.45f) {
			dir *= -1;
		}
		clr.a += dir;
		sr.color = clr;
	}
}
