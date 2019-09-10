using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementGridScript : MonoBehaviour
{

    public List<GameObject> tiles;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    public void FindPath(GameObject start, GameObject end)
    {
        List<GameObject> path = new List<GameObject>();

        for (int i = 0; i < tiles.Count; i++)
        {
            float distX = Mathf.abs(start.position.x - tiles[i].position.x);
            float distY = Mathf.abs(start.position.y - tiles[i].position.y);

            if(distX<=1 && distY <= 1)
            {
                if(Vector3.Distance(tiles[i].position,end.position) < Vector3.Distance(start.position, end.position))
                {
                    path.Add(tiles[i]);
                }
            }
        }

        
    }
    */
}
