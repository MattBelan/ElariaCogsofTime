using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Round and Turn Enums;
/// <summary> The current turn within a round of combat </summary>
public enum RoundState { Player, Enemy, Reset }
/// <summary> Current action in the turn </summary>
public enum TurnState { Character, Action, Display, Result }
public enum ActionIntent { Attack, Move, Pass, Deciding }

public class CombatManager : MonoBehaviour 
{
    public RoundState Round { get; set; }
    public bool IsPlaying;

    public TurnState TurnPhase;

    private ErrorHandler Error;

    public Canvas mainOverlayCanvas;
    public Canvas attackPanelCanvas;
    public Canvas combatLogCanvas;

    public Text playerPanelHealth;
    public Text enemyPanelHealth;

    public Button[] buttons;
    public GameObject tempArrow;
    public Canvas pauseMenu;

    public SaveScript saving;
    public PlayerScript player;
    public MovementGridScript gridScript;
    public AnimationFunctions anims;
    public List<PlayerScript> playerCharacters;
    public List<PlayerScript> deadPlayerCharacters;
    public int curPlayerIndex;
    

    public List<EnemyScript> enemies;
    public List<EnemyScript> deadEnemies;
    public EnemyScript targetEnemy;
    int curEnemyIndex;

    bool playerPassTurn;
    bool enemyPassTurn;

    bool playing;
    bool isDisplayingAttack;
    bool displayStart;
    bool enemiesMoved;
    bool enemiesMoving;
    bool enemiesAttacking;
    float attackDisplayTime;
    public CameraScript cam;
    bool enemiesAttacked;

    public float delayTimer;

    public List<GameObject> UIMovementPreview;
    private MovementArrows movementArrow;

    [HideInInspector]
    public GameObject activeSelector;
    
    // Prefabs
    // UI Assets
    [HideInInspector]
    public GameObject[] selectorPrefabs, pingPrefabs, tileHighlightPrefabs;
    
    // Game Logic Prefabs
    [HideInInspector]
    public GameObject tilePrefabs;


    void Awake()
    {
        // Get necessary assets from resources folder
        LoadResources();

        // Set up grid
        gridScript.combatManager = this;
        gridScript.Setup();
        
        attackPanelCanvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

    }

	// Use this for initialization
	void Start () 
    {
        // Play state
        delayTimer = 0.5f;
        // - player
        playing = playerPassTurn = false;
        curPlayerIndex = playerCharacters.IndexOf(player);
        // - enemy
        enemiesMoved = enemiesMoving = false;
        enemiesAttacking = enemiesAttacked = false;
        curEnemyIndex = 0;

        // Panel Display
        isDisplayingAttack = false;
        displayStart = false;
        attackDisplayTime = 0;
        attackPanelCanvas.enabled = false;

        // Stats
        player.Health = 20;
        
        // Camera
        cam.SetTarget(player.gameObject);

        // Round & Turn States
        Round = RoundState.Player;
        TurnPhase = TurnState.Character;

        // Attack Panel Display Canvas
        
        attackPanelCanvas.sortingLayerName = "UI";
        attackPanelCanvas.enabled = false;

        Text[] tempArr = attackPanelCanvas.GetComponentsInChildren<Text>();
        playerPanelHealth = tempArr[0];
        enemyPanelHealth = tempArr[0];

        

        

        foreach (Text obj in tempArr)
        {
            if (obj.gameObject.name == "PlayerHealth") {
                playerPanelHealth = obj;
            }
            else if (obj.gameObject.name == "EnemyHealth") {
                enemyPanelHealth = obj;
            }
        }
	}
	
	// Update is called once per frame
	void Update() 
    {
        // Check if there are any surviving player characters
        if (playerCharacters.Count == 0)
        {
            SceneManager.LoadScene("MainMenu");
        } 
        else if (curPlayerIndex < playerCharacters.Count)
        {
            player = playerCharacters[curPlayerIndex];
        }

        if (Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape))
        {
            TogglePause();
        }

        //Clear all Enemies Failsafe
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            enemies.Clear();
        }

        //checking if level is complete
        if (enemies.Count <= 0) {
            if(SceneManager.GetActiveScene().name == "Dungeon_Level1") {
                SceneManager.LoadScene("Dungeon_Level2");
            }
            else if (SceneManager.GetActiveScene().name == "Dungeon_Level2")
            {
                SceneManager.LoadScene("Dungeon_Level3");
            }
            else {
                SceneManager.LoadScene("MainMenu");
            }
        }

        //swapping players and handling camera
        //cam.SetTarget(player.gameObject);

        // Allow a short amount of time between turns
        delayTimer-= Time.deltaTime;
        if (delayTimer > 0) {
            return;
        }

        if (Input.GetButtonDown("Swap Characters") && Round == RoundState.Player)
        {
            SelectNextPC();
            //curPlayerIndex = playerCharacters.IndexOf(player);
        }

        //player.playerHealth.text = "Player Health: " + player.Health;
        //player.playerMoves.text = "Player Moves: " + (player.moveTotal - player.currentMove - 1);

        // -- ROUND STATES
        switch (Round) 
        {
            case RoundState.Player:
                // -- PLAYER TURN STATES
                switch (TurnPhase) 
                {
                    case TurnState.Character: {
                        // Room for pre-action state logic here
                        cam.SetTarget(player.gameObject);
                        player = playerCharacters[curPlayerIndex];
                        displayStart = true;
                        TurnPhase = TurnState.Action;
                        // Debug.Log("Health Values:");
                        // foreach (CombatEntity player in playerCharacters) 
                        // {   
                        //     Debug.Log(player.id + "-" + player.Health + "/" + player.MaxHealth);
                        // }               
                        // foreach (CombatEntity enemy in enemies) 
                        // {   
                        //     Debug.Log(enemy.id + "-" + enemy.Health + "/" + enemy.MaxHealth);
                        // }               
                        break;
                    }
                    case TurnState.Action: {
                        
                        if (player.IsTurnTaken) {
                            TurnPhase = TurnState.Result;
                        }
                        break;
                    }
                    case TurnState.Display: {

                        DisplayPanelStart(player, targetEnemy, true);
                        attackPanelCanvas.sortingOrder = 10;
                        
                        // Decrement timer
                        attackDisplayTime -= Time.deltaTime;

                        if (attackDisplayTime <= 2.0f + attackPanelCanvas.GetComponentInChildren<AttackPanelScript>().fadeOutTime) {
                            // Update Panel Health Display
                            if (Time.frameCount % 2 == 0) UpdatePanelHealth(player, targetEnemy, true);
                        }

                        if (attackDisplayTime <= 0.0f) {
                            EnemyScript target = player.Attack(targetEnemy);
                            if (target != null && target.Health <= 0) {
                                deadEnemies.Add(target);
                                enemies.Remove(target);
                            }
                            TurnPhase = TurnState.Action;
                            attackPanelCanvas.sortingOrder = -10;
                        }
                        
                        break;
                    }
                    case TurnState.Result: {
                        // See if all player characters have taken a turn
                        bool playerTurnsFinished = true;
                        foreach (PlayerScript character in playerCharacters) 
                        {
                            if (!character.IsTurnTaken)
                                playerTurnsFinished = false;
                        }

                        // If so, then enemies may go. Otherwise, auto switch to the next player character
                        if (playerTurnsFinished) {
                            Round = RoundState.Enemy;
                        } 
                        else {
                            SelectNextPC();
                        }
                        delayTimer = 1.0f;
                        TurnPhase = TurnState.Character;
                        break;
                    }
                }
                break;

            case RoundState.Enemy:
                // -- ENEMY TURN STATES
                switch (TurnPhase) 
                {
                    case TurnState.Character: {
                            // Space for any preliminary turn logic we want to add
                            if (curEnemyIndex < enemies.Count) 
                            {
                                if (!enemies[curEnemyIndex].IsAlive)
                                {
                                    deadEnemies.Add(enemies[curEnemyIndex]);
                                    enemies.Remove(enemies[curEnemyIndex]);
                                }

                                    cam.SetTarget(enemies[curEnemyIndex].gameObject);
                                // Debug.Log("Enemy " + curEnemyIndex + " deciding");
                                targetEnemy = enemies[curEnemyIndex];
                                displayStart = true;
                                TurnPhase = TurnState.Action;
                            }
                            else
                            {
                                TurnPhase = TurnState.Result;
                            }
                            
                        break;
                    }
                    case TurnState.Action: {

                        targetEnemy.AIMove();
                        if (!targetEnemy.IsLerping) {
                            TurnPhase = (targetEnemy.IsWithinRange(targetEnemy.target, targetEnemy.movementRangeStat))
                                ? TurnState.Display 
                                : TurnState.Result;
                        }
                        break;
                    }
                    case TurnState.Display: {
                        // Setup Panel (boolean determines whether or not we are showing a player attack or enemy attack)
                        DisplayPanelStart(targetEnemy.target, targetEnemy, false); 
                        attackPanelCanvas.sortingOrder = 10;
                        
                        // Decrement timer
                        attackDisplayTime -= Time.deltaTime;

                        if (attackDisplayTime <= 2.0f + attackPanelCanvas.GetComponentInChildren<AttackPanelScript>().fadeOutTime) {
                            // Update Panel Health Display
                            if (Time.frameCount % 2 == 0) UpdatePanelHealth(player, targetEnemy, false);
                        }

                        if (attackDisplayTime <= 0.0f) {
                            PlayerScript target = enemies[curEnemyIndex].AIAttack();
                            if (target != null && target.Health <= 0) {
                                deadPlayerCharacters.Add(target);
                                playerCharacters.Remove(target);
                                curPlayerIndex = playerCharacters.Count > 0 ? curPlayerIndex - 1 : 0;
                            }
                            TurnPhase = TurnState.Result;
                            attackPanelCanvas.sortingOrder = -10;
                        }
                        break;
                    }
                    case TurnState.Result: {

                        curEnemyIndex++;
                        if (curEnemyIndex >= enemies.Count) {
                            Round = RoundState.Reset;
                            break;
                        }
                        delayTimer = 1.0f;
                        TurnPhase = TurnState.Character;
                        break;
                    }
                };  
                break;

            case RoundState.Reset:
                // Reset Player Values
                foreach (PlayerScript p in playerCharacters)
                {
                    p.IsMoving = false;
                    p.UsedAttack = false;
                    p.IsTurnTaken = false;
                    p.CurrentStepsTaken = 0;
                    //p.dodge = p.startDodge;
                    p.abilityCooldown -= 1;
                    curPlayerIndex = 0;
                }
                // Reset Enemy Values
                curEnemyIndex = 0;
                enemiesMoved = false;
                enemiesMoving = false;
                enemiesAttacking = true;

                Round = RoundState.Player;
                break;
        }
        
        // Disabling/Enabling Action Buttons
        // Move
        if (player.movementRangeStat - player.CurrentStepsTaken - 1 == 0) {
            buttons[0].interactable = false;
        }
        else {
            buttons[0].interactable = true;
        }
        // Attack
        if (player.UsedAttack) {
            buttons[1].interactable = buttons[3].interactable = false;
        }
        else {
            buttons[1].interactable = buttons[3].interactable = true;
        }
        // Pass
        if (TurnPhase == TurnState.Character) {
            buttons[2].interactable = false;
        }
        else {
            buttons[2].interactable = true;
        }
        //Ability
        if (false)
        {
            buttons[4].interactable = false;
        }
        else
        {
            buttons[4].interactable = true;
        }
	}

    public void DisplayPanelStart(CombatEntity pPlayer, CombatEntity pEnemy, bool pPlayerAttacking)
    {
        if (displayStart) // Do this once
        {    
            // Enable Canvas & Get Script
            attackPanelCanvas.enabled = true;
            AttackPanelScript panelScript = attackPanelCanvas.GetComponentInChildren<AttackPanelScript>();

            // Set appropriate sprites
            SetSprites(pPlayer, pEnemy, pPlayerAttacking);

            // Set health values
            SetPanelHealth(panelScript, pPlayer, pEnemy, pPlayerAttacking);                           

            // Fade in and fade out attributes
            panelScript.playFadeIn = true;
            panelScript.fadeOutTime = 0.2f;
            panelScript.dismissAfterDelay = true;

            attackDisplayTime = panelScript.fadeInTime + panelScript.fadeOutTime 
                            + GetDisplayDuration(RoundState.Player, pPlayer.intendedAction);
                        
            panelScript.dismissTimer = attackDisplayTime - 0.75f;

            displayStart = false;
        }
    }

    public float GetDisplayDuration(RoundState round, ActionIntent intendAction)
    {
        float calculatedTime = // If this animation is present, get its duration
            anims.animationLibrary.ContainsKey("Attk_Panel")
                ? anims.animationLibrary["Attk_Panel"].duration
                : 0.0f;

        if (round == RoundState.Player) 
        {
            calculatedTime += // If this animation is present, get its duration
                anims.animationLibrary.ContainsKey(playerCharacters[curPlayerIndex].name + "_Attack_Basic")
                    ? anims.animationLibrary["Elaria_Attack_Basic"].duration
                    : 0.0f;
        } 
        else if (round == RoundState.Enemy) 
        {
            calculatedTime += // If this animation is present, get its duration
                anims.animationLibrary.ContainsKey("Bandit_Attack_Basic")
                    ? anims.animationLibrary["Bandit_Attack_Basic"].duration
                    : 0.0f;
        }

        return calculatedTime + 3.25f; // extra delay
    }
    
    public void SetSprites(CombatEntity pPlayer, CombatEntity pEnemy, bool pPlayerAttacking)
    {
        // Get appropriate animators
        List<Animator> animators = new List<Animator>();
        Animator[] tempArr = attackPanelCanvas.GetComponentsInChildren<Animator>();
        foreach (Animator anim in tempArr)
        {
            if (!anim.gameObject.name.Contains("AttackPanel")) {
                animators.Add(anim);
            }
        }

        // Identify player and enemy child objects from panel
        GameObject playerSpriteObj = 
            animators[0].gameObject.name == "PlayerSprite"
                ? animators[0].gameObject
                : animators[1].gameObject;
        GameObject enemySpriteObj = 
            animators[0].gameObject.name == "EnemySprite"
                ? animators[0].gameObject
                : animators[1].gameObject;

        // Set animators
        Animator playerSpriteAnim = playerSpriteObj.GetComponent<Animator>();
        Animator enemySpriteAnim = enemySpriteObj.GetComponent<Animator>();
        playerSpriteAnim.runtimeAnimatorController = pPlayer.GetComponent<Animator>().runtimeAnimatorController;
        enemySpriteAnim.runtimeAnimatorController = pEnemy.GetComponent<Animator>().runtimeAnimatorController;

        // Play animations
        if (pPlayerAttacking)
            StartCoroutine(EntityAttack(1.5f, playerSpriteAnim, enemySpriteAnim, pPlayer.id, pEnemy.id, "_Attack_Basic"));
        else 
            StartCoroutine(EntityAttack(1.5f, enemySpriteAnim, playerSpriteAnim, pEnemy.id, pPlayer.id, "_Attack_Basic"));
    }
    
    IEnumerator EntityAttack(float pWaitTime, Animator pAttackerAnim, Animator pTargetAnim, string pAttackerName, string pTargetName, string pAction)
    {
        // Start w/ idle anims
        pAttackerAnim.Play(pAttackerName + "_Idle");
        pTargetAnim.Play(pTargetName + "_Idle");

        // Set attack anims
        if ("_Attack_Basic" == pAction) {
            yield return new WaitForSeconds(pWaitTime);
            pAttackerAnim.Play(pAttackerName + pAction);
            if (anims.animationLibrary[pAttackerName + pAction] != null) {
                yield return new WaitForSeconds(anims.animationLibrary[pAttackerName + pAction].duration);
            }
            //pTargetAnim.Play(pTargetName + "_Damaged"); // Animation DNE yet
            //yield return new WaitForSeconds(anims.animationLibrary[pTargetName + "_Damaged].duration); // Animation DNE yet
            yield break;
        }
        yield break;
    }

    public void SetPanelHealth(AttackPanelScript pPanel, CombatEntity pPlayer, CombatEntity pEnemy, bool pPlayerAttacking)
    {
        playerPanelHealth.text = FormatHealthValue((int) pPlayer.Health) + " / " + FormatHealthValue((int) pPlayer.MaxHealth);
        enemyPanelHealth.text = FormatHealthValue((int) pEnemy.Health) + " / " + FormatHealthValue((int) pEnemy.MaxHealth);

        pPanel.dynamicHealthVal = pPlayerAttacking
            ? (int) pEnemy.Health
            : (int) pPlayer.Health;
    }

    public void UpdatePanelHealth(CombatEntity pPlayer, CombatEntity pEnemy, bool pPlayerAttacking)
    {
        int dynamicVal = attackPanelCanvas.GetComponentInChildren<AttackPanelScript>().dynamicHealthVal;
        if (pPlayerAttacking) {
            enemyPanelHealth.text = 
                FormatHealthValue(dynamicVal) + 
                " / " + 
                FormatHealthValue((int) pEnemy.MaxHealth);

            if (dynamicVal > pEnemy.Health - pPlayer.damageStat) {
                attackPanelCanvas.GetComponentInChildren<AttackPanelScript>().dynamicHealthVal--;
            } 
        }
        else {
            playerPanelHealth.text = 
                FormatHealthValue(dynamicVal) + 
                " / " + 
                FormatHealthValue((int) pPlayer.MaxHealth);

            if (dynamicVal > pPlayer.Health - pEnemy.damageStat) {
                attackPanelCanvas.GetComponentInChildren<AttackPanelScript>().dynamicHealthVal--;
            }
        }
    }

    public string FormatHealthValue(int value)
    {
        if (value < 10) {
            return "0" + (value > 0 ? value : 0);
        }
        else if (value < 100) {
            return "0" + (value > 0 ? value : 0);
        }
        return value.ToString();
    }

    public void ResetMovementPreview()
    {
        Destroy(activeSelector);
        activeSelector = null;

        int size = UIMovementPreview.Count;
        for (int i = size - 1; i >= 0; i--) {
            if (UIMovementPreview[i]) {
                GameObject tempRef = UIMovementPreview[i];
                UIMovementPreview.RemoveAt(i);
                Destroy(tempRef);
            }
        }
    }

    public void GenerateArrows(List<GridVertex> list)
    {
        // Exit, if the list has no value or is empty
        if (list == null || list.Count == 0) 
        {
            Error.MovePreviewEmptyError();
            return;
        }

        // Loop through list and show appropriate the appropriate arrow
        for (int i = 0; i < list.Count - 1; i++) 
        {
            // Smart Values
            Vector3 prevPos = list[(i >= 1) ? i - 1 : i].vertPos;
            Vector3 thisPos = list[i].vertPos;
            Vector3 nextPos = list[(i < list.Count - 1) ? i + 1 : i].vertPos;

            // Instantiate arrow object
            GameObject arrow = new GameObject("dirArrow");
            arrow.transform.position = thisPos;

            // Add components
            SpriteRenderer renderer = arrow.AddComponent<SpriteRenderer>();
            Color color = renderer.color;
            color.a = 0.9f;

            renderer.color = color;
            renderer.sortingLayerName = "Arrows";
            renderer.sortingOrder = 10;
            UIMovementPreview.Add(arrow); //Can do this here because we still have the ref
            
            // Set empty sprite to appropriate arrow sprite
            if (list.Count == 2) 
            {
                // Small arrow 'nubs'
                if (i == 0) 
                {
                    if (thisPos.x > prevPos.x) 
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_1"]; //Pointing Right
                    else if (thisPos.x < prevPos.x)
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_3"]; //Pointing Left
                    else if (thisPos.y > prevPos.y)
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_0"]; //Pointing Up
                    else if (thisPos.y < prevPos.y)
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_2"]; //Pointing Down
                }
            }
            else if (list.Count > 2)
            {
                // 'Nub' start
                if (i == 0)
                {
                    if (thisPos.x > prevPos.x) 
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_5"]; //Pointing Right
                    else if (thisPos.x < prevPos.x)
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_7"]; //Pointing Left
                    else if (thisPos.y > prevPos.y)
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_4"]; //Pointing Up
                    else if (thisPos.y < prevPos.y)
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_6"]; //Pointing Down
                }

                // Last positions (finishes)
                if (i == list.Count - 2) 
                {
                    // Straight finishes (i + 1 is NextPos and i - 1 is PrevPos)
                    if (prevPos.x == nextPos.x && prevPos.y > nextPos.y)
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_10"]; //Pointing Down 
                    else if (prevPos.x == nextPos.x && prevPos.y < nextPos.y)
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_8"]; //Pointing Up
                    else if (prevPos.y == nextPos.y && prevPos.x > nextPos.x)
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_11"]; //Pointing Left
                    else if (prevPos.y == nextPos.y && prevPos.x < nextPos.x)
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_9"]; //Pointing Right

                    // Corner finishes (when NextPos and PrevPos don't share an axis)
                    else if (prevPos.x == thisPos.x)
                    {
                        if (prevPos.x < nextPos.x && prevPos.y < nextPos.y) 
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_18"]; //Up-Right
                        else if (prevPos.x > nextPos.x && prevPos.y < nextPos.y)
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_14"]; //Up-Left
                        else if (prevPos.x < nextPos.x && prevPos.y > nextPos.y) 
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_12"]; //Down-Right
                        else if (prevPos.x > nextPos.x && prevPos.y > nextPos.y)
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_16"]; //Down-Left
                    }
                    else if (prevPos.y == thisPos.y)
                    {
                        if (prevPos.x < nextPos.x && prevPos.y < nextPos.y) 
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_19"]; //Right-Down
                        else if (prevPos.x > nextPos.x && prevPos.y < nextPos.y)
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_13"]; //Left-Down
                        else if (prevPos.x < nextPos.x && prevPos.y > nextPos.y) 
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_15"]; //Right-Up
                        else if (prevPos.x > nextPos.x && prevPos.y > nextPos.y)
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_17"]; //Left-Up
                    }
                }
                // Anything between start and end
                else
                {
                    // Straights
                    if (prevPos.x == thisPos.x && thisPos.x == nextPos.x)
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_25"]; //Vertical straight
                    else if (prevPos.y == thisPos.y && thisPos.y == nextPos.y)
                        renderer.sprite = movementArrow.Arrow["ArrowSheet_24"]; //Horiz straight

                    // Corners
                    else if (prevPos.x == thisPos.x)
                    {
                        if (prevPos.x < nextPos.x && prevPos.y < nextPos.y) 
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_21"]; //Up-Right
                        else if (prevPos.x > nextPos.x && prevPos.y < nextPos.y)
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_22"]; //Up-Left
                        else if (prevPos.x < nextPos.x && prevPos.y > nextPos.y) 
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_20"]; //Down-Right
                        else if (prevPos.x > nextPos.x && prevPos.y > nextPos.y)
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_23"]; //Down-Left
                    }
                    else if (prevPos.y == thisPos.y)
                    {
                        if (prevPos.x < nextPos.x && prevPos.y < nextPos.y) 
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_20"]; //Right-Down
                        else if (prevPos.x > nextPos.x && prevPos.y < nextPos.y)
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_23"]; //Left-Down
                        else if (prevPos.x < nextPos.x && prevPos.y > nextPos.y) 
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_21"]; //Right-Up
                        else if (prevPos.x > nextPos.x && prevPos.y > nextPos.y)
                            renderer.sprite = movementArrow.Arrow["ArrowSheet_22"]; //Left-Up
                    }
                }
            }
        }
    }

    public void TargetHover(EnemyScript target)
    {
        switch(player.intendedAction) 
        {
            case ActionIntent.Attack:

                if (target.IsAlive) 
                {
                    if (Input.GetMouseButtonDown(0) && !player.UsedAttack && Vector3.Distance(player.transform.position, target.transform.position) <= player.movementRangeStat) // on mouse press
                    {
                        player.intendedAction = ActionIntent.Deciding;
                        player.IsAttacking = true;
                        targetEnemy = target;
                        TurnPhase = TurnState.Display;
                    }
                }
                break;

            default:
                break;
        }
    }

    public void GenerateAccessibleTiles(GameObject pTarget)
    {
        //
        // set target as active in combat manager
        List<CombatEntity> entities = new List<CombatEntity>();
        entities.AddRange(playerCharacters);
        entities.AddRange(enemies);
        
        foreach (CombatEntity entity in entities)
        {
            if (entity.highlightTiles == null || entity.highlightTiles.Count == 0)
                continue;

            foreach (GameObject obj in entity.highlightTiles)
                GameObject.Destroy(obj);

            entity.highlightTiles.Clear();
        }

        //
        gridScript.CreateSelectableTiles(pTarget);
    }

    public void TogglePlayerMovement()
    {
        if (!player.IsLerping)
        {
            player.IsMoving = !player.IsMoving;

            player.intendedAction = player.intendedAction == ActionIntent.Move
                    ? ActionIntent.Deciding
                    : ActionIntent.Move;
                    
            // if (player.intendedAction == ActionIntent.Move)
            // {
            //     player.comLog[2].text = player.comLog[1].text;
            //     player.comLog[1].text = player.comLog[0].text;
            //     player.comLog[0].text = player.id + " is ready to move.";
            // }
            // else if (player.intendedAction == ActionIntent.Deciding)
            // {
            //     player.comLog[2].text = player.comLog[1].text;
            //     player.comLog[1].text = player.comLog[0].text;
            //     player.comLog[0].text = player.id + " is deciding her action.";
            // }
        }
    }
    public void TogglePlayerPass()
    {
        if (!player.IsLerping)
        {
            player.IsTurnTaken = true;

            // player.intendedAction = player.intendedAction == ActionIntent.Move
            //     ? ActionIntent.Deciding
            //     : ActionIntent.Move;
        }
    }

    public void TogglePlayerAttacking()
    {
        if (!player.IsLerping)
        {
            player.IsAttacking = !player.IsAttacking;

            player.intendedAction = player.intendedAction == ActionIntent.Attack
                ? ActionIntent.Deciding
                : ActionIntent.Attack;

            // if (player.intendedAction == ActionIntent.Attack)
            // {
            //     player.comLog[2].text = player.comLog[1].text;
            //     player.comLog[1].text = player.comLog[0].text;
            //     player.comLog[0].text = player.id + " is ready to attack.";
            // }
            // else if (player.intendedAction == ActionIntent.Deciding)
            // {
            //     player.comLog[2].text = player.comLog[1].text;
            //     player.comLog[1].text = player.comLog[0].text;
            //     player.comLog[0].text = player.id + " is choosing an action.";
            // }
        }
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void TogglePause()
    {
        playing = !playing;
        if (pauseMenu.enabled)
        {
            pauseMenu.enabled = false;
        }
        else
        {
            pauseMenu.enabled = true;
        }
    }

    public void UseAbility()
    {
        if (!player.IsLerping)
        {
            player.UseAbility();
        }
    }

    public void SelectNextPC()
    {
        curPlayerIndex = playerCharacters.IndexOf(player);
        if (curPlayerIndex < playerCharacters.Count - 1) {
            curPlayerIndex++;
        }
        else {
            curPlayerIndex = 0;
        }
        cam.SetTarget(playerCharacters[curPlayerIndex].gameObject);
    }

    public void Dodge()
    {
        player.Dodge();
    }

    // Function for storing imperative resources locally (in this script). It provides a reusuable and reliable
    // alternative to the drag and drop method in the Unity Inspector
    private void LoadResources ()
    {
        // Tile Object Prefab
        tilePrefabs = (GameObject) Resources.Load("Prefabs/TileObjects/TileVert");

        if (tilePrefabs == null)
            Error.ResourceLoadError("Tile Object Prefab");
        //

        // Arrows
        movementArrow = gameObject.AddComponent<MovementArrows>();
        // Check that the arrows were loaded properly
        if (movementArrow.Arrow == null)
            Error.ResourceLoadError("MovementArrows");
        //

        // Selectors
        selectorPrefabs = new GameObject[4];
        // Movement Selector
        selectorPrefabs[0] = (GameObject) Resources.Load("Prefabs/UI/Indication/Selector_Movement");
        selectorPrefabs[2] = (GameObject) Resources.Load("Prefabs/UI/Indication/Selector_Movement_Invalid");
        // Attack Selector
        selectorPrefabs[1] = (GameObject) Resources.Load("Prefabs/UI/Indication/Selector_Attack");
        selectorPrefabs[3] = (GameObject) Resources.Load("Prefabs/UI/Indication/Selector_Attack_Invalid");

        foreach (GameObject selector in selectorPrefabs) 
        {
            if (selector == null)
                Error.ResourceLoadError("UI Selectors");
        }
        //

        // Entity Pings
        pingPrefabs = new GameObject[6];
        // Player
        pingPrefabs[0] = (GameObject) Resources.Load("Prefabs/UI/Indication/Ping_Player");
        pingPrefabs[1] = (GameObject) Resources.Load("Prefabs/UI/Indication/Ping_Player_Inactive");
        // Neutral
        pingPrefabs[4] = (GameObject) Resources.Load("Prefabs/UI/Indication/Ping_Neutral");
        pingPrefabs[5] = (GameObject) Resources.Load("Prefabs/UI/Indication/Ping_Neutral_Inactive");
        // Enemy
        pingPrefabs[2] = (GameObject) Resources.Load("Prefabs/UI/Indication/Ping_Enemy");
        pingPrefabs[3] = (GameObject) Resources.Load("Prefabs/UI/Indication/Ping_Enemy_Inactive");

        foreach (GameObject ping in pingPrefabs) 
        {
            if (ping == null)
                Error.ResourceLoadError("Entity Pings");
        }
        //

        // Tile Highlights
        tileHighlightPrefabs = new GameObject[2];
        // Movement
        tileHighlightPrefabs[0] = (GameObject) Resources.Load("Prefabs/UI/Indication/TileHighlight_Movement");
        // Attack
        tileHighlightPrefabs[1] = (GameObject) Resources.Load("Prefabs/UI/Indication/TileHighlight_Attack");

        foreach (GameObject tileHighlight in tileHighlightPrefabs) 
        {
            if (tileHighlight == null)
                Error.ResourceLoadError("Tile Highlight");
        }
        //
    }
}
