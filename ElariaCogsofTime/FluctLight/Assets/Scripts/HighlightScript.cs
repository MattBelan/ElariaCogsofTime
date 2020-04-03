using UnityEngine;

public class HighlightScript : MonoBehaviour {

	private SpriteRenderer sr;
	private Color clr;
	public float incrementVal;
	public float startAlpha = 0.25f;
	public float minAlpha = 0.05f;
	public float maxAlpha = 0.45f;

	// Use this for initialization
	void Start () 
	{
		sr = this.GetComponent<SpriteRenderer>();
		clr = Color.white;
		clr.a = startAlpha;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (clr.a <= minAlpha || clr.a >= maxAlpha) {
			incrementVal *= -1;
		}
		clr.a += incrementVal;
		sr.color = clr;
	}

	// Set values
	public void SetPulsateValues(float pStartAlpha = 0.25f, float pMinAlpha = 0.05f, float pMaxAlpha = 0.45f)
	{
		startAlpha = pStartAlpha;
		minAlpha = pMinAlpha;
		maxAlpha = pMaxAlpha;
	}
}
