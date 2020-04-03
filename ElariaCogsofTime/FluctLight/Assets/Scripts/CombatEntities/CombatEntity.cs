using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// ----- Combat Entity Class -----
///
/// Parent Class for all combat entities, including player characters 
/// and enemy characters.
///
/// </summary>
public class CombatEntity : Entity
{
    // Entity ID & Data
    public string id;
    public SaveData data;

    // External Refs
    // -----------------
    public List<GridVertex> path;
    public List<GameObject> highlightTiles;
    public Animator animator;
    public HealthBar healthDisplay;

    // Movement
    // ------------
    public bool IsMoving { get; set; }
    public bool IsLerping { get; set; }
    public bool LerpStart { get; set; }

    public Vector3 lerpTo;
    public Vector3 lerpEnd;
    public float lerpSpeed;
    public float lerpLength;
    // ~ Animation
    public float startTime;
    public float animFloat;
    public int progressInPath;

    // Combat Values
    // -----------------
    public bool UsedAttack { get; set; }
    public bool IsTurnTaken { get; set; }

    public ActionIntent intendedAction;

    // Stat Values
    // ---------------
    // ~ Volatile
    public int CurrentStepsTaken { get; set; }
    // ~ Static
    public int dodgeStat;
    public int damageStat;
    public int movementRangeStat;
    public int attackRangeStat;


    // Start is called before the first frame update
    public virtual void Start ()
    {   
        /*healthDisplay = Instantiate(
            cm.healthPrefab, 
            new Vector3(transform.position.x, 
            transform.position.y + .65f, 
            transform.position.z), 
            Quaternion.identity
        ).GetComponent<HealthBar>();*/
    }

    //
    public virtual void Update ()
    {

    }

    public virtual bool IsWithinRange(CombatEntity pTarget, float pRange)
    {
        if (pTarget) 
        {
            float distToPlayer = Vector3.Distance(transform.position, pTarget.transform.position);
            return (distToPlayer <= pRange) 
                ? true 
                : false;
        }
        else 
        {
            return false;
        }
    }

    public virtual void TakeDamage(int pDamage)
    {
        this.Health -= pDamage;
    }

    // Basic tile to tile movement
    public void MoveTo(TileScript pTile)
    {
        // Exit if there's no tile reference or if the tile is already occupied
        if (pTile == null || pTile.onTile == null)
            return;
        
        if (!IsMoving)
            return;

        // Set values
        Vector3 startPos = currentTile.transform.position;
        Vector3 newPos = pTile.transform.position;
        startPos.z = newPos.z = -1;
        
        // Get path
        path = combatManager.gridScript.FindPath(startPos, newPos);

        // Check that the entity is within range and has enough steps left
        if (path.Count <= 1 || path.Count > movementRangeStat || CurrentStepsTaken + path.Count > movementRangeStat)
            return;

        // Set values for animation/lerping 
        lerpTo = path[1].vertPos;
        lerpTo.z = -1;
        lerpEnd = newPos;
        LerpStart = true;
        progressInPath = 1;
        
        // Set tile, movement, and action values
        currentTile.GetComponent<TileScript>().onTile = null;
        pTile.onTile = this.gameObject;
        currentTile = pTile.gameObject;
        
        CurrentStepsTaken += path.Count;
        IsMoving = false;
        
        intendedAction = ActionIntent.Deciding;
    }

    public void RenderMovementHighlight ()
    {
        Debug.Log("Rendering highlights" + highlightTiles.Count);

        combatManager.GenerateAccessibleTiles(this.gameObject);
    }

    // Mouse functions
    public override void MouseHover ()
    {
        base.MouseHover();
        
        if (combatManager.IsPlaying)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Render tile highlight over accessible tiles 
                RenderMovementHighlight();
            }
        }
    }

    public override void MouseExit ()
    {
        base.MouseExit();
        return;
    }
}
