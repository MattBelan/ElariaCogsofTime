using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileScript : MonoBehaviour {

    GameObject player;
    PlayerScript ps;
    public GameObject onTile;

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        ps = player.GetComponent<PlayerScript>();
    }

	// Use this for initialization
	void Start () {
        Destroy(this.GetComponent<MeshFilter>());
        Destroy(this.GetComponent<MeshRenderer>());
	}
	
	// Update is called once per frame
	void Update () {
        //highlighted = false;
        
		
	}

    void OnMouseOver()
    {
        ps.setHighlightPos(this.transform.position);
        ps.resetSelector();

        // Button press
        if (Input.GetMouseButtonDown(0))
        {
            if (ps.Moving)
            {
                Debug.Log("Moving");
                ps.MovePlayerTo(this);
            }
        }
    }
}
