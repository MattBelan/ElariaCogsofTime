using UnityEngine;

/*
 * ----- Enemy Script -----
 *
 * Child class that handles the functitonality for enemy characters. This 
 * is the parent of all specific playable character scripts.
 *
 */

public class EnemyScript : CombatEntity 
{
    // Combat Values
    // -----------------
    public bool IsAlive { get; set; }
    public bool IsAttacking { get; set; }

    public PlayerScript target;
    // public List<Equipment> potentialDrops;
    

    // Set up for enemies 
    public override void Start () 
    {
        base.Start();

        CurrentStepsTaken = 0;
        Health = MaxHealth = displayHealth = 10;

        IsAlive = true;
        IsAttacking = false;
        UsedAttack = false;

        IsMoving = false;
        IsLerping = false;
        LerpStart = false;
        lerpSpeed = 1.0f;
        animFloat = 0;

        IsCursorOver = false;

        data = new SaveData();

        target = null;
    }
	
	// Update is called once per frame
	public override void Update () 
    {
        base.Update();

        data.health = Health;
        data.x = transform.position.x;
        data.y = transform.position.y;
        data.z = transform.position.z;
        data.currentMove = CurrentStepsTaken;

        if (Health <= 0)
        {
            healthDisplay.gameObject.SetActive(false);
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
                progressInPath++;
                if (progressInPath < path.Count)
                {
                    lerpTo = path[progressInPath].vertPos;
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

    public override void MouseHover()
    {
        base.MouseHover();
        combatManager.TargetHover(this);
    }

    bool PlayerVisible()
    {
        target = null;
        float closestDist = 100000f;

        foreach (PlayerScript player in combatManager.playerCharacters)
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

        return (target != null) ? true : false;
    }

    public void AIMove()
    {
        if (PlayerVisible())
        {
            // Debug.Log("(Movement)" + id + "is targeting " + target.id);
            float distToPlayer = Vector3.Distance(transform.position, target.transform.position);
            if (movementRangeStat < distToPlayer)
            {
                Vector3 direction = (target.transform.position - transform.position) / distToPlayer;

                float distToMove = 0;
                if (distToPlayer <= movementRangeStat)
                {
                    distToMove = distToPlayer - movementRangeStat;
                }
                else
                {
                    distToMove = movementRangeStat;
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

                                path = combatManager.gridScript.FindPath(transform.position, newPos);

                                lerpTo = path[1].vertPos;
                                lerpTo.z = -1;
                                lerpEnd = newPos;
                                progressInPath = 1;
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

                                path = combatManager.gridScript.FindPath(transform.position, newPos);

                                lerpTo = path[1].vertPos;
                                lerpTo.z = -1;
                                lerpEnd = newPos;
                                progressInPath = 1;
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
            if (IsWithinRange(target, movementRangeStat)) {
                target.TakeDamage(damageStat);
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
            return null;//potentialDrops[(int)UnityEngine.Random.Range(0.0f, potentialDrops.Count - 1)];
        }
        else
        {
            return null;
        }
    }
}
