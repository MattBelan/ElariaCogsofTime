using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractibleScript : MonoBehaviour {

	// Use this for initialization
	public virtual void Start () {
		
	}
	
	// Update is called once per frame
	public virtual void Update () {
		
	}

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Interact();
        }
    }

    public virtual void Interact()
    {
        //Overwritten in child classes for specific cases
    }
}
