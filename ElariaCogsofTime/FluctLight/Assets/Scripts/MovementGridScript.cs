using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementGridScript : MonoBehaviour
{
    // External Refs
    // -----------------
    [HideInInspector]
    public CombatManager combatManager;

    // Grid Tiles & Vertices
    // -------------------------
    public Tilemap tileMap;
    public List<GridVertex> vertices;


    // 
    public void Setup ()
    {
        // Make invisible (Play w/ this in the future to see visible terrain)
        tileMap.color = Color.clear;
    
        CreateTiles();
        SetTileRefs();
        SetAdjacencies();
    }

    // 
    public void CreateTiles ()
    {
        BoundsInt bounds = tileMap.cellBounds;
        Vector3Int positionalOffset = new Vector3Int(1, 1, 1);
        
        // Loop through all tiles and create tileVerts from prefab
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (!tileMap.HasTile(position))
                continue;

            GameObject tileObj = Instantiate(combatManager.tilePrefabs, position, Quaternion.Euler(Vector3.zero));
            Vector3 objPos = tileObj.transform.position + positionalOffset;
            tileObj.transform.position = new Vector3(objPos.x, objPos.y, tileMap.transform.position.z);
            vertices.Add(tileObj.GetComponent<GridVertex>());
        }
    }

    // Set all the external references for each tile
    public void SetTileRefs ()
    {
        List<CombatEntity> occupants = new List<CombatEntity>();
        occupants.AddRange(combatManager.playerCharacters);
        occupants.AddRange(combatManager.enemies);
        
        foreach (GridVertex vert in vertices) 
        {
            TileScript tile = vert.gameObject.GetComponent<TileScript>();
            tile.combatManager = combatManager;

            // Loop through entities and see if they are on any tiles
            foreach (CombatEntity occupant in occupants) 
                if (tile.transform.position == occupant.transform.position)
                {
                    tile.onTile = occupant.gameObject;
                    occupant.currentTile = tile.gameObject;
                }
        }
    }

    // Set all adjacent vertice values for each vertex
    public void SetAdjacencies ()
    {
        // Loop through vertices and compare
        foreach (GridVertex vert in vertices)
        {
            vert.adjVertices.Clear();

            foreach (GridVertex potentialAdj in vertices)
            {
                // Don't execute if vert and potentialAdj are the same value
                if (vert == potentialAdj)
                    continue; 

                // Calculate distance between the two vertices
                float distance = Vector3.Distance(vert.vertPos, potentialAdj.vertPos);
                
                // If it meets the adjacency criteria, add it to the list
                if (distance <= 1)
                    vert.adjVertices.Add(potentialAdj);
            }
        }
    }

    // Reset vertices values
    public void ResetVertices ()
    {
        foreach (GridVertex vert in vertices)
        {
            vert.visited = false;
            vert.heuristic = 0;
        }
    }

    //
    public void CreateSelectableTiles(GameObject pTarget)
    {
        CombatEntity entity = pTarget.GetComponent<CombatEntity>();
        GridVertex start = entity.currentTile.GetComponent<GridVertex>();

        //
        List<GridVertex> startLayer = new List<GridVertex>();
        List<GridVertex> totalVertices = new List<GridVertex>();
        totalVertices.Add(start);
        startLayer.Add(start);
        List<GameObject> totalObjs = new List<GameObject>();

        // Create
        List<GameObject> list = 
            GetMovementandAttackTiles(
                totalObjs, 
                totalVertices,
                startLayer,
                entity.movementRangeStat, 
                entity.attackRangeStat
            );

        entity.highlightTiles.AddRange(list);
    }

    // Recursively calculate accessible tiles and generate appropriate tiles
    private List<GameObject> GetMovementandAttackTiles(List<GameObject> pTotalObjs, List<GridVertex> pAllVertices, List<GridVertex> pLayer, int pMoveRange, int pAttackRange)
    {
        List<GridVertex> nextLayer = new List<GridVertex>(); 

        //
        foreach (GridVertex vert in pLayer)
            foreach (GridVertex adjVert in vert.adjVertices)
            {
                if (pAllVertices.Contains(adjVert))
                    continue;

                TileScript tile = adjVert.gameObject.GetComponent<TileScript>();
                if (tile.onTile)
                    continue;
            
                nextLayer.Add(adjVert);
                pAllVertices.Add(adjVert);
            }

        // Create specific tile and decrement
        if (pMoveRange > 0) 
        {
            // Movement Iteration
            pMoveRange--;
            foreach (GridVertex vert in nextLayer)
                pTotalObjs.Add(Instantiate(combatManager.tileHighlightPrefabs[0], vert.transform.position, vert.transform.rotation));
        }
        else if (pAttackRange > 0)
        {
            // Attack Iteration
            pAttackRange--;
            foreach (GridVertex vert in nextLayer)
                pTotalObjs.Add(Instantiate(combatManager.tileHighlightPrefabs[1], vert.transform.position, vert.transform.rotation));
        }
        else
        { 
            return pTotalObjs;
        }

        GetMovementandAttackTiles(pTotalObjs, pAllVertices, nextLayer, pMoveRange, pAttackRange);
        return pTotalObjs;
    }
    
    // Default path finding method
    public List<GridVertex> FindPath(Vector2 pStart, Vector2 pEnd)
    {
        ResetVertices();

        // Prep values
        List<GridVertex> path = new List<GridVertex>();

        // Loop through vertices, recalculate heuristic values, and set starting vertex
        foreach (GridVertex vert in vertices)
        {    
            // X + Y value over Direct Distance value (Diagonal movement is illegal)
            int displacementX = Mathf.Abs(Mathf.RoundToInt(pEnd.x - vert.vertPos.x));
            int displacementY = Mathf.Abs(Mathf.RoundToInt(pEnd.y - vert.vertPos.y));
            
            int newHeuristic = (displacementX + displacementY);

            vert.heuristic = newHeuristic;
            vert.visited = false;

            // Find start vertex and add to path
            if (pStart == (Vector2) vert.vertPos)
                path.Add(vert);
        }

        // Generate path
        int iteration = 0;
        while (path.Count > 0)
        {
            GridVertex curVert = path[path.Count - 1];
            
            // Exit loop if path is complete
            if (pEnd == (Vector2) curVert.vertPos || iteration > 100)
                break;

            // Check adjacent vertices
            if (curVert.adjVertices.Count > 0)
            {
                GridVertex bestOption = curVert.adjVertices[0]; //Default
                List<GridVertex> pathOptions = new List<GridVertex>();

                // Loop through and choose the adjacent vertex closest to the end vertex
                foreach (GridVertex adjVert in curVert.adjVertices) 
                {   
                    TileScript adjTile = adjVert.gameObject.GetComponent<TileScript>(); 
                    // Add it to the list of potential options
                    if (adjVert.heuristic < curVert.heuristic && !adjVert.visited && adjTile.onTile == null)
                        pathOptions.Add(adjVert);
                }

                // If there is more than one ideal option, chose the one that favors straight paths
                // if (pathOptions.Count == 2)
                // {
                //     int prevVertPosX = Mathf.RoundToInt(path[path.Count - 2].vertPos.x);
                //     int prevVertPosY = Mathf.RoundToInt(path[path.Count - 2].vertPos.y);
                //     int adjVertPosX = Mathf.RoundToInt(pathOptions[0].vertPos.x);
                //     int adjVertPosY = Mathf.RoundToInt(pathOptions[0].vertPos.y);

                //     // Compare vertices and select the one that shares an axis value w/ the last path value 
                //     bestOption = (prevVertPosX == adjVertPosX || prevVertPosY == adjVertPosY)  
                //         ? pathOptions[0]
                //         : pathOptions[1];
                // }
                /**NOTE** THERE SHOULD NEVER BE MORE THAN 2 POTENTIAL OPTIONS **/

                foreach (GridVertex adj in curVert.adjVertices)
                {
                    if (!adj.visited && adj.heuristic < bestOption.heuristic)
                        bestOption = adj;
                }

                // Double check if the best option is valid
                if (!bestOption.visited)
                {
                    bestOption.visited = true;
                    path.Add(bestOption);
                }
            }
            iteration++;
        }

        return path;
    }
}
