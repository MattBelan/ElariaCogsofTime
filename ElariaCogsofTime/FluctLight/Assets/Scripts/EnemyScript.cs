using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyScript : MonoBehaviour {

    public bool Moving { get; set; }
    public float Health { get; set; }
    public bool IsAlive { get; set; }

    public float moveTotal;
    public float currentMove;
    public float damage;
    public float range;

    public bool IsLerping { get; set; }

    public bool LerpStart { get; set; }

    float lerpSpeed;
    float lerpLength;
    float startTime;

    Vector3 lerpTo;

    public Text enemyHealth;

    public PlayerScript player;

    public SaveData data;

    public Animator animator;
    float animFloat;

    //Pathfinding
    public MovementGridScript grid;
    Vector3 currGridTarget;
    Vector3 endingTarget;
    List<GridVertex> path;
    int pathProgress;
    Vector3 lerpEnd;

    // Use this for initialization
    void Start () {
        Health = 10;
        currentMove = 0;
        moveTotal = 5;
        Moving = false;
        IsAlive = true;

        data = new SaveData();

        IsLerping = false;
        LerpStart = false;
        lerpSpeed = 1.0f;
        animFloat = 0;
    }
	
	// Update is called once per frame
	void Update () {
        data.health = Health;
        data.x = transform.position.x;
        data.y = transform.position.y;
        data.z = transform.position.z;
        data.currentMove = currentMove;

        if (Health <= 0)
        {
            IsAlive = false;
            gameObject.SetActive(false);
        }

        if (LerpStart)
        {
            LerpStart = false;
            IsLerping = true;
            startTime = Time.time;
            lerpLength = Vector3.Distance(transform.position, lerpEnd);

            animFloat = AnimationFloat(transform.position, lerpTo);
        }
        if (IsLerping)
        {
            float distCovered = (Time.time - startTime) * lerpSpeed;

            float fracLength = distCovered / lerpLength;

            if (Vector3.Distance(transform.position, lerpTo) < 0.5)
            {
                pathProgress++;
                if (pathProgress < path.Count)
                {
                    lerpTo = path[pathProgress].vertPos;
                    lerpTo.z = -1;
                    animFloat = AnimationFloat(transform.position, lerpTo);
                }
            }

            transform.position = Vector3.Lerp(transform.position, lerpTo, fracLength);

            if (Vector3.Distance(transform.position, lerpEnd) <= .1)
            {
                transform.position = lerpEnd;
                IsLerping = false;
            }

            //animator.SetFloat("Direction", animFloat);
            animator.SetInteger("Direction", (int)animFloat);
        }
        else
        {
            animator.SetInteger("Direction", 0);
        }
    }

    public void TakeDamage(float dam)
    {
        Health -= dam;
    }

    void OnMouseOver()
    {
        if (player.Attacking) {
            player.moveSelector(this.transform.position);
            player.setHighlightPos(Vector3.zero);
        } 
        if (IsAlive)
        {
            enemyHealth.text = "Enemy Health: " + Health;

            if (Input.GetMouseButtonDown(0))
            {
                if (player.Attacking)
                {
                    player.setHighlightPos(new Vector3(200,200,0));
                    player.Attack(this);
                }
            }
        }
    }

    bool PlayerVisible()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distToPlayer <= 8)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, player.transform.position - transform.position, out hit, 100.0f))
            {
                if(hit.collider.gameObject == player.gameObject)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void AIMove()
    {
        if (PlayerVisible())
        {
            float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (range < distToPlayer)
            {
                Vector3 direction = (player.transform.position - transform.position) / distToPlayer;

                float distToMove = 0;
                if (distToPlayer <= moveTotal)
                {
                    distToMove = distToPlayer - range;
                }
                else
                {
                    distToMove = moveTotal;
                }

                if (distToMove >= 1)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + distToMove * direction, new Vector3(0, 0, 1), out hit, 5.0f))
                    {
                        TileScript tile = hit.collider.gameObject.GetComponent<TileScript>();
                        if (tile != null)
                        {
                            if (tile.onTile == null)
                            {
                                Vector3 newPos = hit.collider.gameObject.transform.position;
                                newPos.z = -1;

                                path = grid.FindPath(transform.position, newPos, moveTotal);

                                lerpTo = path[1].vertPos;
                                lerpTo.z = -1;
                                lerpEnd = newPos;
                                pathProgress = 1;
                                LerpStart = true;
                            }
                        }
                    }
                }
                else
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + 1 * direction, new Vector3(0, 0, 1), out hit, 5.0f))
                    {
                        TileScript tile = hit.collider.gameObject.GetComponent<TileScript>();
                        if (tile != null)
                        {
                            if (tile.onTile == null)
                            {
                                Vector3 newPos = hit.collider.gameObject.transform.position;
                                newPos.z = -1;

                                path = grid.FindPath(transform.position, newPos, moveTotal);

                                lerpTo = path[1].vertPos;
                                lerpEnd = newPos;
                                pathProgress = 1;
                                LerpStart = true;
                            }
                        }
                    }
                }
            }
        }
    }

    public void AIAttack()
    {
        if (PlayerVisible())
        {
            float distToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distToPlayer <= range)
            {
                player.TakeDamage(damage);
            }
        }
    }

    float AnimationFloat(Vector3 start, Vector3 end)
    {
        float animation = 0;

        Vector3 direction = end - start;

        float angle = Vector3.Angle(direction, Vector3.right);
        float vert = end.y - start.y;

        if ((angle > 45 && angle < 135) && vert > 0)
        {
            animation = 1;
        }
        if ((angle > 45 && angle < 135) && vert < 0)
        {
            animation = 3;
        }
        if (angle <= 45)
        {
            animation = 2;
        }
        if (angle >= 135)
        {
            animation = 4;
        }

        return animation;
    }
}
