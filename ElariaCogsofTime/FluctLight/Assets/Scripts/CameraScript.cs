using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public GameObject player;
    private Camera cam;
    public float cameraHeight; //default -10
    public float newViewSize;
    private float defaultViewSize;

    private bool attackZoom = false;
    public bool AttackZoom {
        set { attackZoom = value; }
    }
    private bool zoomReset = false;
    public bool ZoomReset {
        set { zoomReset = value; }
    }
    
    void Awake()
    {
        player = GameObject.FindWithTag("Player");
    }

	// Initialization
	void Start () 
    {
        cam = this.GetComponent<Camera>();
        defaultViewSize = cam.orthographicSize;
	}
	
	// Update is called once per frame
	void Update () 
    {
        Vector3 newPos = player.transform.position;
        newPos.z = cameraHeight;
        transform.position = newPos;

        if (zoomReset) {
            ResetCameraZoom();
        } else if (attackZoom) {
            CameraZoom(newViewSize);
        }
	}

    public void CameraZoom (float viewSize, float zoomTime = 0.5f) 
    {
        float _timeStartedLerping = Time.time;
        float timeSinceStarted = Time.time - _timeStartedLerping;
        float percentageComplete = timeSinceStarted / zoomTime;

        while (true) 
        {
            timeSinceStarted = Time.time - _timeStartedLerping;
            percentageComplete = timeSinceStarted / zoomTime;

            float currentVal = Mathf.Lerp(defaultViewSize, viewSize, percentageComplete);

            cam.orthographicSize = currentVal;

            if (percentageComplete >= 1) break;
        }
    }

    public void ResetCameraZoom (float zoomTime = 0.5f) 
    {
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, defaultViewSize, zoomTime);
    }
}
