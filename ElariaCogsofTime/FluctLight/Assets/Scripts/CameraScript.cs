using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public GameObject target;
    private GameObject prevTarget;
    private Camera cam;
    public float cameraHeight; //default -10
    public float newViewSize;
    public float transitionTime = 1.25f;
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
        target = GameObject.FindWithTag("Player");
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
        // if (zoomReset) {
        //     ResetCameraZoom();
        // } else if (attackZoom) {
        //     CameraZoom(newViewSize);
        // }
	}

    void FixedUpdate ()
    {
        Vector3 newPos = target.transform.position;
        newPos.z = cameraHeight;
        if (target) {
            Vector2 vec = Vector2.Lerp(transform.position, target.transform.position, transitionTime * Time.deltaTime * 4);
            transform.position = new Vector3(vec.x, vec.y, transform.position.z);
        }
    }

    public void SetTarget (GameObject pTarget)
    {
        prevTarget = target ? target : pTarget;
        target = pTarget;
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
