using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSelector : MonoBehaviour
{
    private float val;
    private float distance = 0.3f;
    public float pulsateSpeed = 1.0f;
    public int vector;
    public List<GameObject> pointers;

    // Pulse is not working right

    void Start() 
    {
        val = 0f;
        vector = 1;    
    }

    void Update()
    {
        val += 0.5f * Time.deltaTime * pulsateSpeed * vector;

        if (val >= 1 || val <= 0)
        {
            vector *= -1;
        }

        foreach (GameObject ptr in pointers)
        {
            Vector3 v3Pos = ptr.transform.localPosition;
            
            v3Pos.x = (val * distance);
        }
    }
}
