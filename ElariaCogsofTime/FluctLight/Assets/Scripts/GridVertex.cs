using System.Collections.Generic;
using UnityEngine;

/*
 * ----- Grid Vertex Class -----
 *
 * Stores vertex data values for calculating ideal movement path for
 * entities during combat. Contains important values such as vertex
 * position, adjacent vertices, whether or not the vertex was visited,
 * and heuristic values.
 *
 */

public class GridVertex : MonoBehaviour
{
    public List<GridVertex> adjVertices;
    [HideInInspector]
    public Vector3 vertPos;
    [HideInInspector]
    public bool visited;
    public float heuristic;

    void Awake ()
    {
        vertPos = transform.position;
        visited = false;
        adjVertices = new List<GridVertex>();
    }
}
