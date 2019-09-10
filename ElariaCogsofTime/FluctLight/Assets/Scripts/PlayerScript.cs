using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]

public class PlayerScript : MonoBehaviour {

    public GameObject currentTile;
    public GameObject highlight;
    public GameObject selector;
    public GameObject damagePrefab;
    public bool Moving { get; set; }
    public float Health { get; set; }
    public bool Attacking { get; set; }

    //Lerp Variables and Properties
    public bool IsLerping { get; set; }

    public bool LerpStart { get; set; }

    float lerpSpeed;
    float lerpLength;
    float startTime;

    Vector3 lerpTo;

    //Variables for combat
    public float moveTotal;
    public float currentMove;
    public float attackDist;
    public float damage;
    public bool usedAttack;

    //UI Elements
    public Text playerHealth;
    public Text playerMoves;

    //Data for saving
    public SaveData data;
    public float curLevel;

    //Animator
    public Animator animator;
    float animFloat;

    //Pausing
    public bool playing;

    // Use this for initialization
    void Start () {
        currentMove = 0;
        Health = 20;
        Moving = false;
        Attacking = false;
        usedAttack = false;

        data = new SaveData();

        IsLerping = false;
        LerpStart = false;
        lerpSpeed = 1.0f;
        animFloat = 0;
        playing = true;
    }
	
	// Update is called once per frame
	void Update () {
        playerHealth.text = "Player Health: " + Health;
        playerMoves.text = "Player Moves: " + (moveTotal - currentMove-1);

        data.health = Health;
        data.x = transform.position.x;
        data.y = transform.position.y;
        data.z = transform.position.z;
        data.currentMove = currentMove;

        if (playing)
        {
            if (LerpStart)
            {
                LerpStart = false;
                IsLerping = true;
                startTime = Time.time;
                lerpLength = Vector3.Distance(transform.position, lerpTo);

                animFloat = AnimationFloat(transform.position, lerpTo);
            }
            if (IsLerping)
            {
                float distCovered = (Time.time - startTime) * lerpSpeed;

                float fracLength = distCovered / lerpLength;

                transform.position = Vector3.Lerp(transform.position, lerpTo, fracLength);

                if (transform.position == lerpTo)
                {
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
        Health -= dam;
        Instantiate(damagePrefab, gameObject.transform.position, gameObject.transform.rotation);
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

                            lerpTo = newPos;
                            LerpStart = true;

                            currentTile.GetComponent<TileScript>().onTile = null;
                            tile.onTile = gameObject;
                            currentTile = tile.gameObject;
                            Debug.Log(currentMove);
                            currentMove += Mathf.Floor(dist);

                            Moving = false;
                        }
                    }
                }
            }
        }      
    }

    public void Attack(EnemyScript enemy)
    {
        if (playing)
        {
            if (enemy != null)
            {
                if (Attacking && !usedAttack)
                {
                    float dist = Vector3.Distance(transform.position, enemy.transform.position);

                    if (dist <= attackDist)
                    {
                        enemy.TakeDamage(damage);
                        usedAttack = true;

                        Attacking = false;
                        Instantiate(damagePrefab, enemy.transform.position, enemy.transform.rotation);
                    }
                }
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

    public void FindPath(Vector3 start, Vector3 end)
    {

    }
}
