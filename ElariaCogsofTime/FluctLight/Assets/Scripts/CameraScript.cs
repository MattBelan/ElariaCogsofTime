using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public float cameraHeight; //default -10
    GameObject player;

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 newPos = player.transform.position;
        newPos.z = cameraHeight;
        transform.position = newPos;
	}
}
