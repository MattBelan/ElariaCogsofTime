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
    public RoundState State { get; set; }

    /// <summary>
    /// Current action in the turn
    /// </summary>
    TurnState turnState;

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
    public int selectedPlayer;
    

    public List<EnemyScript> enemies;
    public EnemyScript targetEnemy;

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

    int currEnemy;

    void Awake()
    {
        playerPassTurn = false;
        playing = false;
        isDisplayingAttack = false;
        displayStart = false;
        attackDisplayTime = 0;
        enemiesMoved = false;
        enemiesMoving = false;
        enemiesAttacking = false;
        enemiesAttacked = false;
        currEnemy = 0;

        player.Health = 20;
        attackPanelCanvas.enabled = false;
        if (PlayerPrefs.GetInt("Loading") > 0)
        {
            //saving.LoadGame();
            if(player.curLevel == 1 && SceneManager.GetActiveScene().name != "Dungeon_Level1")
            {
                SceneManager.LoadScene("Dungeon_Level1");
            }
            else if(player.curLevel == 2 && SceneManager.GetActiveScene().name != "Dungeon_Level2")
            {
                SceneManager.LoadScene("Dungeon_Level2");
            }
            PlayerPrefs.SetInt("Loading", 0);
            PlayerPrefs.Save();
        }

        selectedPlayer = playerCharacters.IndexOf(player);
    }

	// Use this for initialization
	void Start () 
    {
        // Scene Transitioning
        if(SceneManager.GetActiveScene().name == "Dungeon_Level1")
        {
            player.curLevel = 1;
        }
        else
        {
            player.curLevel = 2;
        }

        // Round & Turn States
        State = RoundState.Player;
        turnState = TurnState.Character;

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
        player.playing = playing;
        if (Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape))
        {
            TogglePause();
        }

        //checking if level is complete
        int numEnemiesAlive = 0;
        foreach(EnemyScript enemy in enemies)
        {
            if (enemy.IsAlive)
            {
                numEnemiesAlive++;
            }
        }
        if (numEnemiesAlive <= 0)
        {
            if(SceneManager.GetActiveScene().name == "Dungeon_Level1")
            {
                SceneManager.LoadScene("Dungeon_Level2");
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        //swapping players and handling camera
        cam.player = player.gameObject;

        if(Input.GetButtonDown("Swap Characters"))
        {
            SelectNextPC();
        }
        selectedPlayer = playerCharacters.IndexOf(player);

        // -- ROUND STATES
        switch (State)
        {
            case RoundState.Player:

                // -- TURN STATES
                switch (turnState)
                {
                    case TurnState.Character:

                        if (player.Health <= 0)
                        {
                            SceneManager.LoadScene("MainMenu");
                        }
                        turnState = TurnState.Action;
                        break;

                    case TurnState.Action:

                        if (playerPassTurn)
                        {
                            turnState = TurnState.Result;
                        }
                        break;

                    case TurnState.Display:

                        if (displayStart) // Do this once
                        {    
                            // Enable Canvas & Get Script
                            attackPanelCanvas.enabled = true;
                            AttackPanelScript panelScript = attackPanelCanvas.GetComponentInChildren<AttackPanelScript>();

                            // Set appropriate sprites
                            SetSprites(player, targetEnemy, true);

                            // Set health values
                            SetPanelHealth(panelScript, player, targetEnemy, true);                           

                            // Fade in and fade out attributes
                            panelScript.playFadeIn = true;
                            panelScript.fadeOutTime = 0.2f;
                            panelScript.dismissAfterDelay = true;

                            attackDisplayTime = panelScript.fadeInTime + panelScript.fadeOutTime 
                                            + GetDisplayDuration(RoundState.Player, player.intendedAction);
                                        
                            panelScript.dismissTimer = attackDisplayTime - 1.0f;

                            displayStart = false;
                        }
                        
                        // Decrement timer
                        attackDisplayTime -= Time.deltaTime;

                        if (attackDisplayTime <= 1.75f + attackPanelCanvas.GetComponentInChildren<AttackPanelScript>().fadeOutTime) {
                            // Update Panel Health Display
                            if (Time.frameCount % 2 == 0) UpdatePanelHealth(player, targetEnemy, true);
                        }

                        if (attackDisplayTime <= 0.0f) 
                        {
                            player.Attack(targetEnemy);
                            turnState = TurnState.Action;
                        }
                        break;

                    case TurnState.Result:

                        turnState = TurnState.Character;
                        State = RoundState.Enemy;
                        break;
                }
                break;

            case RoundState.Enemy:
            
                if (!isDisplayingAttack) 
                {
                    if (!enemiesMoved)
                    {
                        if (enemies[currEnemy].IsAlive)
                        {
                            enemies[currEnemy].AIMove();
                        }
                        enemiesMoved = true;
                        enemiesMoving = true;
                    }
                    else if (enemiesMoving)
                    {
                        int numMoving = 0;
                        foreach (EnemyScript enemy in enemies)
                        {
                            if (enemy.IsLerping)
                            {
                                numMoving++;
                            }
                        }
                        if (numMoving <= 0)
                        {
                            enemiesMoving = false;
                            enemiesAttacking = true;
                            enemiesMoved = false;

                            if (enemies[currEnemy].IsAlive)
                            {
                                enemies[currEnemy].AIAttack();
                            }

                            currEnemy++;
                        }
                    }

                    if (currEnemy >= enemies.Count)
                    {
                        currEnemy = 0;
                        State = RoundState.Reset;
                    }
                }
                break;

            case RoundState.Reset:

                playerPassTurn = false;
                foreach(PlayerScript p in playerCharacters)
                {
                    p.Moving = false;
                    p.usedAttack = false;
                    p.currentMove = 0;
                    p.dodge = p.startDodge;
                }

                enemiesMoved = false;
                enemiesMoving = false;
                enemiesAttacking = true;

                State = RoundState.Player;
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
        if (turnState == TurnState.Character) {
            buttons[2].interactable = false;
        }
        else {
            buttons[2].interactable = true;
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
            if (intendAction == ActionIntent.Attack) 
            {
                calculatedTime += // If this animation is present, get its duration
                    anims.animationLibrary.ContainsKey("Elaria_Attack_Basic")
                        ? anims.animationLibrary["Elaria_Attack_Basic"].duration
                        : 0.0f;
            }
        } 
        else if (round == RoundState.Enemy) 
        {}

        return calculatedTime + 4.0f; // extra delay
    }

    public void SetDisplayAnimations(GameObject pAttackPanel, string pAttackerAnimation, string pTargetAnimation)
    {
        Animator[] anims = pAttackPanel.GetComponentsInChildren<Animator>();
        if (anims.Length > 0) {
            foreach (Animator anim in anims) 
            {
                if ("PlayerSprite" == anim.gameObject.name) {
                    anim.SetBool("BasicAttack", true);
                }
            }
            return;
        }
        
        Debug.Log("Combat Manager - Animations: Display Animation Error");
    }    
    
    public void SetSprites(PlayerScript pPlayer, EnemyScript pEnemy, bool pPlayerAttacking)
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
            StartCoroutine(PlayerAttack(1.5f, playerSpriteAnim, enemySpriteAnim));
        else 
            StartCoroutine(EnemyAttack(1.5f, playerSpriteAnim, enemySpriteAnim));
    }
    
    IEnumerator PlayerAttack(float pWaitTime, Animator pPlayerSpriteAnim, Animator pEnemySpriteAnim)
    {
        yield return new WaitForSeconds(pWaitTime);
        pPlayerSpriteAnim.Play("Elaria_Attack_Basic");
        yield return new WaitForSeconds(anims.animationLibrary["Elaria_Attack_Basic"].duration);
        //enemySpriteAnim.Play("Bandit_Attack_Damaged"); Not implemented yet
    }

    IEnumerator EnemyAttack(float pWaitTime, Animator pPlayerSpriteAnim, Animator pEnemySpriteAnim)
    {
        yield return new WaitForSeconds(pWaitTime);
        pEnemySpriteAnim.Play("Bandit_Attack_Basic");
        yield return new WaitForSeconds(anims.animationLibrary["Bandit_Attack_Basic"].duration);
        //pPlayerSpriteAnim.Play("Elaria_Attack_Damaged"); Not implemented yet
    }

    public void SetPanelHealth(AttackPanelScript pPanel, PlayerScript pPlayer, EnemyScript pEnemy, bool pPlayerAttacking)
    {
        playerPanelHealth.text = FormatHealthValue((int) pPlayer.Health) + " / " + FormatHealthValue((int) pPlayer.MaxHealth);
        enemyPanelHealth.text = FormatHealthValue((int) pEnemy.Health) + " / " + FormatHealthValue((int) pEnemy.MaxHealth);

        pPanel.dynamicHealthVal = pPlayerAttacking
            ? (int) pEnemy.Health
            : (int) pPlayer.Health;
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

    public void UpdatePanelHealth(PlayerScript pPlayer, EnemyScript pEnemy, bool pPlayerAttacking)
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
                        turnState = TurnState.Display;
                        displayStart = true;
                    }
                }
                break;

            default:
                break;
        }
    }
    // public void P(EnemyScript enemy) // probably should make a base enemy/entity class for this parameter
    // {
    //         if (player.Attacking) 
    //         {
    //             player.moveSelector(enemy.transform.position);
    //             player.setHighlightPos(enemy.transform.position);
    //         } 

    //         enemy.enemyHealth.text = "Enemy Health: " + enemy.Health;

    //         if (Input.GetMouseButtonDown(0))
    //         {
    //             if (player.Attacking && !player.usedAttack)
    //             {
    //                 player.setHighlightPos(new Vector3(200,200,0));
    //             }
    //         }
    // }

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
                player.comLog[0].text = "Elaria is ready to move.";
            }
            else if (player.intendedAction == ActionIntent.Deciding)
            {
                player.comLog[2].text = player.comLog[1].text;
                player.comLog[1].text = player.comLog[0].text;
                player.comLog[0].text = "Elaria is deciding her action.";
            }
        }
    }
    public void TogglePlayerPass()
    {
        if (!player.IsLerping)
        {
            playerPassTurn = !playerPassTurn;

            player.intendedAction = player.intendedAction == ActionIntent.Move
                ? ActionIntent.Deciding
                : ActionIntent.Move;
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
                player.comLog[0].text = "Elaria is ready to attack.";
            }
            else if (player.intendedAction == ActionIntent.Deciding)
            {
                player.comLog[2].text = player.comLog[1].text;
                player.comLog[1].text = player.comLog[0].text;
                player.comLog[0].text = "Elaria is deciding her action.";
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
        selectedPlayer++;
        if (selectedPlayer < playerCharacters.Count)
        {
            player = playerCharacters[selectedPlayer];
        }
        else
        {
            selectedPlayer = 0;
            player = playerCharacters[selectedPlayer];
        }
    }

    public void Dodge()
    {
        player.Dodge();
    }
    
}
