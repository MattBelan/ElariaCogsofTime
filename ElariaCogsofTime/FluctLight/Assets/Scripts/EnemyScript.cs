using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyScript : CombatEntity {

    public bool IsAlive { get; set; }

    //MAKE THIS A LIST
    public List<PlayerScript> players;
    public PlayerScript target;
    public List<Equipment> potentialDrops;

    // Use this for initialization
    public override void Start () 
    {
        base.Start();
        Health = 10;
        MaxHealth = Health;
        displayHealth = Health;
        currentMove = 0;
        moveTotal = 5;
        Moving = false;
        IsAlive = true;
        IsCursorOver = false;

        data = new SaveData();

        IsLerping = false;
        LerpStart = false;
        lerpSpeed = 1.0f;
        animFloat = 0;

        combatManager = GameObject.FindGameObjectWithTag("CombatManager").GetComponent<CombatManager>();

        players = combatManager.playerCharacters;
        target = null;

        // Call after health values are set
        CreateHeathBar();
    }
	
	// Update is called once per frame
	public override void Update () 
    {
        base.Update();

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

    void OnMouseOver()
    {
        combatManager.TargetHover(this);
        IsCursorOver = true;
    }
    void OnMouseExit()
    {
        IsCursorOver = false;
    }

    bool PlayerVisible()
    {
        target = null;
        float closestDist = 100000f;

        foreach (PlayerScript player in players)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distToPlayer <= 8)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, player.transform.position - transform.position, out hit, 100.0f))
                {
                    if (hit.collider.gameObject == player.gameObject)
                    {
                        if (distToPlayer < closestDist)
                        {
                            target = player;
                            closestDist = distToPlayer;
                        }
                    }
                }
            }
        }

        if (target != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AIMove()
    {
        if (PlayerVisible())
        {
            // Debug.Log("(Movement)" + id + "is targeting " + target.id);
            float distToPlayer = Vector3.Distance(transform.position, target.transform.position);
            if (range < distToPlayer)
            {
                Vector3 direction = (target.transform.position - transform.position) / distToPlayer;

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
                                lerpTo.z = -1;
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

    public PlayerScript AIAttack()
    {
        // Debug.Log("(Attack)" + id + "is targeting " + target.id);
        if (PlayerVisible())
        {
            if (IsWithinRange(target, range)) {
                target.TakeDamage(damage);
                return target;
            }
        }
        return null;
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

    public Equipment LootDrop()
    {
        if(UnityEngine.Random.Range(0.0f, 100.0f) < 25)
        {
            return potentialDrops[(int)UnityEngine.Random.Range(0.0f, potentialDrops.Count - 1)];
        }
        else
        {
            return null;
        }
    }
}
