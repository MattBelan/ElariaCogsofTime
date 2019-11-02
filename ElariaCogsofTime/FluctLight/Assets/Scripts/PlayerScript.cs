using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : CombatEntity {

    //Tile Selectors, etc.
    public GameObject currentTile;
    public GameObject highlight;
    public GameObject selector;
    public GameObject damagePrefab;
    public bool Attacking { get; set; }

    //UI Elements
    public Text playerHealth;
    public Text playerMoves;
    public List<Text> comLog;

    //Saving
    public float curLevel;

    //Pausing
    public bool playing;
    public bool usedAbility;
    public int abilityCooldown;

    //Player Specific Turn Logic
    public bool turnTaken;

    // Use this for initialization
    public override void Start () 
    {
        currentMove = 0;
        Health = 20;
        MaxHealth = Health;
        Moving = false;
        Attacking = false;
        usedAttack = false;
        turnTaken = false;
        intendedAction = ActionIntent.Deciding;
        usedAbility = false;
        abilityCooldown = 0;

        data = new SaveData();

        IsLerping = false;
        LerpStart = false;
        lerpSpeed = 1.0f;
        animFloat = 0;
        playing = true;

        healthDisplay = Instantiate(healthPrefab, new Vector3(transform.position.x, transform.position.y + .65f, transform.position.z), Quaternion.identity);
        healthBar = healthDisplay.transform.GetChild(1).gameObject;
        healthDisplay.transform.SetParent(transform);
    }
	
	// Update is called once per frame
	public override void Update () 
    {
        //health bar
        Vector3 newScale = healthBar.transform.localScale;
        newScale.x = Health / 20;
        healthBar.transform.localScale = newScale;

        data.health = Health;
        data.x = transform.position.x;
        data.y = transform.position.y;
        data.z = transform.position.z;
        data.currentMove = currentMove;

        if (abilityCooldown > 0)
        {
            usedAbility = true;
        }
        else
        {
            usedAbility = false;
        }

        if (playing)
        {
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

                if(lerpTo != lerpEnd)
                {
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
                }

                transform.position = Vector3.Lerp(transform.position, lerpTo, fracLength);

                if (Vector3.Distance(transform.position,lerpEnd)<=.05)
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
    }

    public void setHighlightPos(Vector3 pos) 
    {
        if (playing) {
            highlight.transform.position = pos;
        } else {
            highlight.transform.position = new Vector3(200, 200, 0);
        }
    }

    public void moveSelector(Vector3 pos)
    {
        if (playing) {
            selector.transform.position = pos;
        }
    }

    public void resetSelector()
    {
        selector.transform.position = new Vector3(200, 200, 0);
    }

    public void TakeDamage(float dam)
    {
        if(UnityEngine.Random.Range(0.0f, 100.0f)>=dodge)
        {
            Health -= dam;
            Instantiate(damagePrefab, gameObject.transform.position, gameObject.transform.rotation);

            comLog[2].text = comLog[1].text;
            comLog[1].text = comLog[0].text;
            comLog[0].text = id + " took " + dam + " damage.";
        }
        else
        {
            comLog[2].text = comLog[1].text;
            comLog[1].text = comLog[0].text;
            comLog[0].text = id + " dodged an attack!";
        }
    }

    public void MovePlayerTo(TileScript tile)
    {
        if (playing)
        {
            if (tile != null)
            {
                if (Moving)
                {
                    float dist = Vector3.Distance(transform.position, tile.transform.position);
                    if (dist <= moveTotal && currentMove + dist <= moveTotal)
                    {
                        if (tile.onTile == null)
                        {
                            Vector3 newPos = tile.transform.position;
                            newPos.z = -1;

                            //Pathfinding testing
                            path = grid.FindPath(transform.position, newPos, moveTotal);

                            lerpTo = path[1].vertPos;
                            lerpTo.z = -1;
                            lerpEnd = newPos;
                            pathProgress = 1;
                            LerpStart = true;

                            currentTile.GetComponent<TileScript>().onTile = null;
                            tile.onTile = gameObject;
                            currentTile = tile.gameObject;
                            currentMove += Mathf.Floor(dist);

                            Moving = false;
                            intendedAction = ActionIntent.Deciding;
                        }
                    }
                }
            }
        }      
    }

    public EnemyScript Attack(EnemyScript enemy)
    {
        if (playing)
        {
            if (enemy != null)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);

                if (dist <= range)
                {
                    enemy.TakeDamage(damage);

                    usedAttack = true;
                    Attacking = false;
                
                    comLog[2].text = comLog[1].text;
                    comLog[1].text = comLog[0].text;
                    comLog[0].text = id + " dealt " + damage + " damage.";

                    return enemy;
                }
            }
        }
        return null;
    }

    IEnumerator Delay(int timeToWait) {
        yield return new WaitForSeconds(timeToWait);
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

    public void FindPath(Vector3 start, Vector3 end)
    {

    }

    public virtual void UseAbility()
    {
        //To be overwritten
    }

    public void Dodge()
    {
        dodge = 33f;
        usedAttack = true;
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            combatManager.player = this;
        }
    }
}
