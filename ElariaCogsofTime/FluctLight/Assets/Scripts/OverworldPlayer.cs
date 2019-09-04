using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OverworldPlayer : MonoBehaviour {


    public List<GameObject> bounds;
    public GameObject toNextArea;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float horizonatlIn = Input.GetAxis("Horizontal");
        float verticalIn = Input.GetAxis("Vertical");

        Vector3 newPos = transform.position;
        newPos.x += horizonatlIn/10;
        newPos.y += verticalIn/10;

        bool inBounds = true;
        foreach(GameObject wall in bounds)
        {
            if (wall.GetComponent<BoxCollider>().bounds.Contains(newPos))
            {
                inBounds = false;
            }
        }

        if (inBounds)
        {
            transform.position = newPos;
        }

        if (toNextArea.GetComponent<BoxCollider>().bounds.Contains(transform.position))
        {
            SceneManager.LoadScene("Dungeon_Level1");
        }
	}
}
