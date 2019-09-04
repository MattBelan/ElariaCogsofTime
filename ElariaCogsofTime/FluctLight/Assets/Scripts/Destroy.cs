using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour {

	public bool timed = true;
	public float timer = 2.0f;
	
	// Update is called once per frame
	void Update () {
		if (timed) {
			timer -= Time.deltaTime;
		}
		else {
			Destroy(gameObject);
		}

		if (timer <= 0)
		{
			Destroy(gameObject);
		}
	}
}
