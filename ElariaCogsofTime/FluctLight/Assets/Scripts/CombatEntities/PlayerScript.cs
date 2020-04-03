using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ----- Player Script -----
 *
 * Child class that handles the functitonality for player characters. This 
 * is the parent of all specific playable character scripts.
 *
 */

public class PlayerScript : CombatEntity 
{
    // Combat Values
    // -----------------
    public bool IsAlive { get; set; }
    public bool IsAttacking { get; set; }

    public float abilityCooldown;

    // Equipment
    // -------------
    public List<Equipment> gear;


    // Set up player characters for first round of combat
    public override void Start () 
    {
        base.Start();

        intendedAction = ActionIntent.Deciding;

        CurrentStepsTaken = 0;
        Health = MaxHealth = displayHealth = 20; // Will get value from static stat script

        IsAlive = false;
        IsAttacking = false;
        UsedAttack = false;

        abilityCooldown = 0.0f;

        IsMoving = false;
        IsLerping = false;
        LerpStart = false;
        lerpSpeed = 1.0f;
        animFloat = 0;

        IsTurnTaken = false;
        IsCursorOver = false;

        data = new SaveData();
    }
	
	// Update is called once per frame
	public override void Update () 
    {
        base.Update();

        // Set data
        data.health = Health;
        data.x = transform.position.x;
        data.y = transform.position.y;
        data.z = transform.position.z;
        data.currentMove = CurrentStepsTaken;
        
        // if (abilityCooldown > 0)
        //     UsedAbility = true;
        // else
        //     UsedAbility = false;

        if (combatManager.IsPlaying)
        {
            if (LerpStart)
            {
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
                        progressInPath++;
                        if (progressInPath < path.Count)
                        {
                            lerpTo = path[progressInPath].vertPos;
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

    public override void TakeDamage(int pDamage)
    {
        if (UnityEngine.Random.Range(0.0f, 100.0f) >= dodgeStat)
        {
            Health -= pDamage;

            // comLog[2].text = comLog[1].text;
            // comLog[1].text = comLog[0].text;
            // comLog[0].text = id + " took " + dam + " damage.";
        }
        else
        {
            // comLog[2].text = comLog[1].text;
            // comLog[1].text = comLog[0].text;
            // comLog[0].text = id + " dodged an attack!";
        }
    }

    public List<GridVertex> PreviewMovement(TileScript tile)
    {
        if (tile != null)
        {
            if (IsMoving)
                return combatManager.gridScript.FindPath(transform.position, tile.transform.position);

        }

        return null;
    }

    public EnemyScript Attack(EnemyScript enemy)
    {
        if (enemy != null)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            if (dist <= movementRangeStat)
            {
                enemy.TakeDamage(damageStat);

                UsedAttack = true;
                IsAttacking = false;
            
                // comLog[2].text = comLog[1].text;
                // comLog[1].text = comLog[0].text;
                // comLog[0].text = id + " dealt " + damageStat + " damage.";

                return enemy;
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

    public virtual void UseAbility()
    {
        //To be overwritten
    }

    public void Dodge()
    {
        // dodge = 33f;
        UsedAttack = true;
    }

    // Mouse Functions
    public override void MouseHover()
    {
        base.MouseHover();

        if (Input.GetMouseButtonDown(0) && combatManager.Round == RoundState.Player)
        {
            combatManager.curPlayerIndex = combatManager.playerCharacters.IndexOf(this);

            // When a tile with this character is targeted
            if (combatManager.curPlayerIndex == combatManager.playerCharacters.IndexOf(this)) 
            {
                // Toggle Movement
                combatManager.TogglePlayerMovement();
            }
            else
            {
                // Set Target
                combatManager.cam.SetTarget(combatManager.playerCharacters[combatManager.curPlayerIndex].gameObject);
                combatManager.playerCharacters[combatManager.curPlayerIndex].intendedAction = ActionIntent.Move;
            }
        }
    }
}
