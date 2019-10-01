using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridVertex : MonoBehaviour
{

    public Vector3 vertPos;
    public bool visited;
    public List<GridVertex> adjVertices;
    public float heuristic;

    void Awake()
    {
        vertPos = transform.position;
        //vertPos.z = -1;
        visited = false;
        adjVertices = new List<GridVertex>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
