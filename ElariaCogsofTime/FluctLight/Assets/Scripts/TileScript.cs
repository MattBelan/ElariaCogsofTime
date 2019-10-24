using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileScript : MonoBehaviour {

    GameObject player;
    PlayerScript ps;
    public GameObject onTile;
    CombatManager cm;

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        ps = player.GetComponent<PlayerScript>();
        cm = GameObject.FindGameObjectWithTag("CombatManager").GetComponent<CombatManager>();
    }

	// Use this for initialization
	void Start () {
        Destroy(this.GetComponent<MeshFilter>());
        Destroy(this.GetComponent<MeshRenderer>());
    }
	
	// Update is called once per frame
	void Update () {
        //highlighted = false;
        ps = cm.player;
        player = cm.player.gameObject;
		
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
                ps.MovePlayerTo(this);
            }
        }
    }
}
