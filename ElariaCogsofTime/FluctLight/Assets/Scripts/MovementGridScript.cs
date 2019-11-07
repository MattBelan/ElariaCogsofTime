using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementGridScript : MonoBehaviour
{

    public List<GridVertex> verts;
    GridVertex startVert;
    GridVertex endVert;

    // Start is called before the first frame update
    void Start()
    {
        SetAdjacencies();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetAdjacencies()
    {
        foreach (GridVertex vert in verts)
        {
            foreach(GridVertex posAdj in verts)
            {
                if(Vector3.Distance(vert.vertPos,posAdj.vertPos)<=1 && vert != posAdj)
                {
                    vert.adjVertices.Add(posAdj);
                }
            }
        }
    }

    public List<GridVertex> FindPath(Vector3 start, Vector3 end, float moves)
    {
        List<GridVertex> path = new List<GridVertex>();
        start.z = 0;
        end.z = 0;

        for (int i = 0; i < verts.Count; i++)
        {
            float newH = Mathf.Sqrt(Mathf.Pow(end.x - verts[i].vertPos.x, 2) + Mathf.Pow(end.y - verts[i].vertPos.y, 2));
            verts[i].heuristic = newH;
        }

        //setting starting and ending vertices
        foreach(GridVertex vert in verts)
        {
            vert.visited = false;

            if(vert.vertPos == start)
            {
                startVert = vert;
                path.Add(startVert);
            }
            else if(vert.vertPos == end)
            {
                endVert = vert;
            }
        }

        int iteration = 0;
        while (path.Count>0)
        {
            GridVertex curVert = path[path.Count - 1];
            bool add = false;
            iteration++;
            //checking if path is complete
            if(curVert == endVert)
            {
                // Debug.Log(iteration + "st Round");
                break;
            }

            //checking nearby vertices
            if (curVert.adjVertices.Count > 0)
            {
                GridVertex best = curVert.adjVertices[0];

                foreach(GridVertex adj in curVert.adjVertices)
                {
                    if (!adj.visited && adj.heuristic<=best.heuristic)
                    {
                        best = adj;
                    }
                }

                if (!best.visited)
                {
                    best.visited = true;
                    path.Add(best);
                    add = true;
                }
            }

            if (!add)
            {
                path.RemoveAt(path.Count - 1);
            }
        }

        return path;
    }
}
