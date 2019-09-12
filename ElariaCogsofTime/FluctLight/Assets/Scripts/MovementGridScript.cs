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

        while (path.Count>0)
        {
            GridVertex curVert = path[path.Count - 1];
            bool add = false;
            
            //checking if path is complete
            if(curVert = endVert)
            {
                break;
            }

            if (curVert.adjVertices.Count > 0)
            {
                foreach(GridVertex adj in curVert.adjVertices)
                {
                    if (!adj.visited)
                    {
                        adj.visited = true;
                        path.Add(adj);
                        add = true;
                    }
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
