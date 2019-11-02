using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum RoundState { Player, Enemy, Reset }
public enum TurnState { Character, Action, Display, Result }

public enum ActionIntent {
    Attack,
    Move,
    Pass,
    Deciding,
}

public class CombatManager : MonoBehaviour {

    /// <summary>
    /// The current state of combat
    /// </summary>
    public RoundState roundState { get; set; }

    /// <summary>
    /// Current action in the turn
    /// </summary>
    TurnState turnPhase;

    public Canvas playerCanvas;

    // Attack Panel Canvas
    public Canvas attackPanelCanvas;
    public Text playerPanelHealth;
    public Text enemyPanelHealth;

    public Button[] buttons;
    public Canvas pauseMenu;

    public SaveScript saving;
    
    public PlayerScript player;
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



    void Awake()
    {
        // Play state
        delayTimer = 0.5f;
        // - player
        playing = false;
        playerPassTurn = false;
        curPlayerIndex = playerCharacters.IndexOf(player);
        // - enemy
        enemiesMoved = false;
        enemiesMoving = false;
        enemiesAttacking = false;
        enemiesAttacked = false;
        curEnemyIndex = 0;

        // Panel Display
        isDisplayingAttack = false;
        displayStart = false;
        attackDisplayTime = 0;
        attackPanelCanvas.enabled = false;

        // Stats
        player.Health = 20;
        
        // Scene Management
        if (PlayerPrefs.GetInt("Loading") > 0) {
            //saving.LoadGame();
            if (player.curLevel == 1 && SceneManager.GetActiveScene().name != "Dungeon_Level1") {
                SceneManager.LoadScene("Dungeon_Level1");
            }
            else if (player.curLevel == 2 && SceneManager.GetActiveScene().name != "Dungeon_Level2") {
                SceneManager.LoadScene("Dungeon_Level2");
            }
            PlayerPrefs.SetInt("Loading", 0);
            PlayerPrefs.Save();
        }
    }

	// Use this for initialization
	void Start () 
    {
        // Scene Transitioning
        if (SceneManager.GetActiveScene().name == "Dungeon_Level1") {
            player.curLevel = 1;
        }
        else {
            player.curLevel = 2;
        }
        
        // Camera
        cam.SetTarget(player.gameObject);

        // Round & Turn States
        roundState = RoundState.Player;
        turnPhase = TurnState.Character;

        // Attack Panel Display Canvas
        attackPanelCanvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        attackPanelCanvas.sortingLayerName = "Menus";
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
        if (playerCharacters.Count == 0) {
            SceneManager.LoadScene("MainMenu");
        } 
        else if (curPlayerIndex < playerCharacters.Count) {
            player = playerCharacters[curPlayerIndex];
        }
        player.playing = playing;

        if (Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape))
        {
            TogglePause();
        }

        //checking if level is complete
        if (enemies.Count <= 0) {
            if(SceneManager.GetActiveScene().name == "Dungeon_Level1") {
                SceneManager.LoadScene("Dungeon_Level2");
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

        if (Input.GetButtonDown("Swap Characters") && roundState == RoundState.Player)
        {
            SelectNextPC();
            //curPlayerIndex = playerCharacters.IndexOf(player);
        }

        player.playerHealth.text = "Player Health: " + player.Health;
        player.playerMoves.text = "Player Moves: " + (player.moveTotal - player.currentMove - 1);

        // -- ROUND STATES
        switch (roundState) 
        {
            case RoundState.Player:
                // -- PLAYER TURN STATES
                switch (turnPhase) 
                {
                    case TurnState.Character: {
                        // Room for pre-action state logic here
                        cam.SetTarget(player.gameObject);
                        player = playerCharacters[curPlayerIndex];
                        displayStart = true;
                        turnPhase = TurnState.Action;
                        break;
                    }
                    case TurnState.Action: {
                        
                        if (player.turnTaken) {
                            turnPhase = TurnState.Result;
                        }
                        break;
                    }
                    case TurnState.Display: {

                        DisplayPanelStart(true);
                        
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
                            turnPhase = TurnState.Action;
                        }
                        break;
                    }
                    case TurnState.Result: {
                        // See if all player characters have taken a turn
                        bool playerTurnsFinished = true;
                        foreach (PlayerScript character in playerCharacters) 
                        {
                            if (!character.turnTaken)
                                playerTurnsFinished = false;
                        }

                        // If so, then enemies may go. Otherwise, auto switch to the next player character
                        if (playerTurnsFinished) {
                            roundState = RoundState.Enemy;
                            Debug.Log("Enemy");
                        } 
                        else {
                            Debug.Log("Next");
                            SelectNextPC();
                        }
                        delayTimer = 1.0f;
                        turnPhase = TurnState.Character;
                        break;
                    }
                }
                break;

            case RoundState.Enemy:
                // -- ENEMY TURN STATES
                switch (turnPhase) 
                {
                    case TurnState.Character: {
                        // Space for any preliminary turn logic we want to add
                        cam.SetTarget(enemies[curEnemyIndex].gameObject);
                        Debug.Log("Enemy " + curEnemyIndex + " deciding");
                        targetEnemy = enemies[curEnemyIndex];
                        displayStart = true;
                        turnPhase = TurnState.Action;
                        break;
                    }
                    case TurnState.Action: {

                        targetEnemy.AIMove();
                        if (!targetEnemy.IsLerping) {
                            turnPhase = (targetEnemy.IsWithinRange(targetEnemy.target, targetEnemy.range))
                                ? TurnState.Display 
                                : TurnState.Result;
                        }
                        break;
                    }
                    case TurnState.Display: {
                        // Setup Panel (boolean determines whether or not we are showing a player attack or enemy attack)
                        DisplayPanelStart(false); 
                        
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
                            turnPhase = TurnState.Result;
                        }
                        break;
                    }
                    case TurnState.Result: {

                        curEnemyIndex++;
                        if (curEnemyIndex >= enemies.Count) {
                            roundState = RoundState.Reset;
                            Debug.Log("Enemy Round Finished");
                            break;
                        }
                        delayTimer = 1.0f;
                        turnPhase = TurnState.Character;
                        break;
                    }
                };  
                break;

            case RoundState.Reset:
                // Reset Player Values
                foreach (PlayerScript p in playerCharacters)
                {
                    p.Moving = false;
                    p.usedAttack = false;
                    p.turnTaken = false;
                    p.currentMove = 0;
                    p.dodge = p.startDodge;
<<<<<<< HEAD
                    curPlayerIndex = 0;
=======
                    p.abilityCooldown -= 1;
>>>>>>> 64a2e983990d9f6e5dc2703a6251b8c3575aca13
                }
                // Reset Enemy Values
                curEnemyIndex = 0;
                enemiesMoved = false;
                enemiesMoving = false;
                enemiesAttacking = true;

                roundState = RoundState.Player;
                break;
        }
        
        // Disabling/Enabling Action Buttons
        // Move
        if (player.moveTotal - player.currentMove - 1 == 0) {
            buttons[0].interactable = false;
        }
        else {
            buttons[0].interactable = true;
        }
        // Attack
        if (player.usedAttack) {
            buttons[1].interactable = false;
            buttons[3].interactable = false;
        }
        else {
            buttons[1].interactable = true;
            buttons[3].interactable = true;
        }
        // Pass
        if (turnPhase == TurnState.Character) {
            buttons[2].interactable = false;
        }
        else {
            buttons[2].interactable = true;
        }
        //Ability
        if (player.usedAbility)
        {
            buttons[4].interactable = false;
        }
        else
        {
            buttons[4].interactable = true;
        }
	}

    public void DisplayPanelStart(bool pPlayerAttacking)
    {
        if (displayStart) // Do this once
        {    
            // Enable Canvas & Get Script
            attackPanelCanvas.enabled = true;
            AttackPanelScript panelScript = attackPanelCanvas.GetComponentInChildren<AttackPanelScript>();

            // Set appropriate sprites
            SetSprites(player, targetEnemy, pPlayerAttacking);

            // Set health values
            SetPanelHealth(panelScript, player, targetEnemy, pPlayerAttacking);                           

            // Fade in and fade out attributes
            panelScript.playFadeIn = true;
            panelScript.fadeOutTime = 0.2f;
            panelScript.dismissAfterDelay = true;

            attackDisplayTime = panelScript.fadeInTime + panelScript.fadeOutTime 
                            + GetDisplayDuration(RoundState.Player, player.intendedAction);
                        
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
                anims.animationLibrary.ContainsKey("Elaria_Attack_Basic")
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

    public void SetDisplayAnimations(GameObject pAttackPanel, string pAttackerAnimation, string pTargetAnimation)
    {
        Animator[] anims = pAttackPanel.GetComponentsInChildren<Animator>();
        if (anims.Length > 0) {
            foreach (Animator anim in anims) 
            {
                if ("PlayerSprite" == anim.gameObject.name) {
                    anim.SetBool("BasicAttack", true); // Actual value in animator component
                }
            }
            return;
        }
        Debug.Log("Combat Manager - Animations: Display Animation Error");
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
            StartCoroutine(EntityAttack(1.5f, playerSpriteAnim, enemySpriteAnim, /*pPlayer.id*/ "Elaria", pEnemy.id, "_Attack_Basic"));
        else 
            StartCoroutine(EntityAttack(1.5f, enemySpriteAnim, playerSpriteAnim, pEnemy.id, pPlayer.id, "_Attack_Basic"));
    }
    
    IEnumerator EntityAttack(float pWaitTime, Animator pAttackerAnim, Animator pTargetAnim, string pAttackerName, string pTargetName, string pAction)
    {
        if ("_Attack_Basic" == pAction) {
            yield return new WaitForSeconds(pWaitTime);
            pAttackerAnim.Play(pAttackerName + pAction);
            yield return new WaitForSeconds(anims.animationLibrary[pAttackerName + pAction].duration);
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

            if (dynamicVal > pEnemy.Health - pPlayer.damage) {
                attackPanelCanvas.GetComponentInChildren<AttackPanelScript>().dynamicHealthVal--;
            } 
        }
        else {
            playerPanelHealth.text = 
                FormatHealthValue(dynamicVal) + 
                " / " + 
                FormatHealthValue((int) pPlayer.MaxHealth);

            if (dynamicVal > pPlayer.Health - pEnemy.damage) {
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

    public void TargetHover(EnemyScript target)
    {
        switch(player.intendedAction) 
        {
            case ActionIntent.Attack:

                if (target.IsAlive) 
                {
                    player.moveSelector(target.transform.position);
                    player.setHighlightPos(target.transform.position);

                    if (Input.GetMouseButtonDown(0) && !player.usedAttack && Vector3.Distance(player.transform.position, target.transform.position) <= player.range) // on mouse press
                    {
                        player.setHighlightPos(new Vector3(200,200,0)); // make highlight live in Combatmanager
                        player.intendedAction = ActionIntent.Deciding;
                        player.Attacking = true;
                        targetEnemy = target;
                        turnPhase = TurnState.Display;
                    }
                }
                break;

            default:
                break;
        }
    }

    public void TogglePlayerMovement()
    {
        if (!player.IsLerping)
        {
            player.Moving = !player.Moving;

            player.intendedAction = player.intendedAction == ActionIntent.Move
                    ? ActionIntent.Deciding
                    : ActionIntent.Move;
                    
            if (player.intendedAction == ActionIntent.Move)
            {
                player.comLog[2].text = player.comLog[1].text;
                player.comLog[1].text = player.comLog[0].text;
                player.comLog[0].text = player.id + " is ready to move.";
            }
            else if (player.intendedAction == ActionIntent.Deciding)
            {
                player.comLog[2].text = player.comLog[1].text;
                player.comLog[1].text = player.comLog[0].text;
                player.comLog[0].text = player.id + " is deciding her action.";
            }
        }
    }
    public void TogglePlayerPass()
    {
        if (!player.IsLerping)
        {
            player.turnTaken = true;

            // player.intendedAction = player.intendedAction == ActionIntent.Move
            //     ? ActionIntent.Deciding
            //     : ActionIntent.Move;
        }
    }

    public void TogglePlayerAttacking()
    {
        if (!player.IsLerping)
        {
            player.Attacking = !player.Attacking;

            player.intendedAction = player.intendedAction == ActionIntent.Attack
                ? ActionIntent.Deciding
                : ActionIntent.Attack;

            if (player.intendedAction == ActionIntent.Attack)
            {
                player.comLog[2].text = player.comLog[1].text;
                player.comLog[1].text = player.comLog[0].text;
                player.comLog[0].text = player.id + " is ready to attack.";
            }
            else if (player.intendedAction == ActionIntent.Deciding)
            {
                player.comLog[2].text = player.comLog[1].text;
                player.comLog[1].text = player.comLog[0].text;
<<<<<<< HEAD
                player.comLog[0].text = player.id + " is choosing an action.";
=======
                player.comLog[0].text = player.id + " is deciding her action.";
>>>>>>> 64a2e983990d9f6e5dc2703a6251b8c3575aca13
            }
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
    
}
