using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible : MonoBehaviour
{
    public OverworldPlayer ps;
    public float distToInteract;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Interact()
    {
        //Override in child scripts
    }

    void OnMouseOver()
    {
        //highlight or outline indicator goes here

        if(Vector3.Distance(transform.position,ps.gameObject.transform.position) < distToInteract)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Interact();
            }
        }
    }
}
