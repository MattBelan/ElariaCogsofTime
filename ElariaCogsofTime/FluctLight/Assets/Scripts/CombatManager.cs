using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum RoundState { Player, Enemy, Reset }
enum TurnState { Character, Action, Result }

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

    public Canvas attackPanelCanvasPrefab;
    public Button[] buttons;
    public Canvas pauseMenu;

    public SaveScript saving;
    
    public PlayerScript player;
    
    public List<PlayerScript> playerCharacters;
    public int selectedPlayer;
    

    public List<EnemyScript> enemies;

    bool playerPassTurn;
    bool enemyPassTurn;

    bool playing;
    bool isDisplayingAttack;
    bool enemiesMoved;
    bool enemiesMoving;
    bool enemiesAttacking;

    public CameraScript cam;

    void Awake()
    {
        playerPassTurn = false;
        playing = false;
        isDisplayingAttack = false;
        enemiesMoved = false;
        enemiesMoving = false;
        enemiesAttacking = false;
        player.Health = 20;
        attackPanelCanvasPrefab.enabled = false;
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
        State = RoundState.Player;
        turnState = TurnState.Character;
        if(SceneManager.GetActiveScene().name == "Dungeon_Level1")
        {
            player.curLevel = 1;
        }
        else
        {
            player.curLevel = 2;
        }
	}
	
	// Update is called once per frame
	void Update () 
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

                        if (isDisplayingAttack)
                        {
                            break;
                        }

                        if (playerPassTurn)
                        {
                            turnState = TurnState.Result;
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
                        foreach (EnemyScript enemy in enemies)
                        {
                            if (enemy.IsAlive)
                            {
                                enemy.AIMove();
                            }
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

                            foreach (EnemyScript enemy in enemies)
                            {
                                if (enemy.IsAlive)
                                {
                                    enemy.AIAttack();
                                }
                            }
                            State = RoundState.Reset;
                        }
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

    public void PlayerAttack(EnemyScript enemy) // probably should make a base enemy/entity class for this parameter
    {
        if (player.Attacking) 
        {
            player.moveSelector(enemy.transform.position);
            player.setHighlightPos(enemy.transform.position);
        } 
        if (enemy.IsAlive)
        {

            if (Input.GetMouseButtonDown(0))
            {
                if (player.Attacking)
                {
                    player.setHighlightPos(new Vector3(200,200,0));
                    player.Attack(enemy);
                    Canvas displayCanvas = Instantiate(attackPanelCanvasPrefab);
                    displayCanvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                    displayCanvas.sortingLayerName = "Menus";
                    displayCanvas.enabled = true;
                }
            }
        }
    }

    public void TogglePlayerMovement()
    {
        if (!player.IsLerping)
        {
            player.Moving = !player.Moving;
            if (player.Moving)
            {
                player.comLog[2].text = player.comLog[1].text;
                player.comLog[1].text = player.comLog[0].text;
                player.comLog[0].text = "Elaria is ready to move.";
            }
            else
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
        }
    }

    public void TogglePlayerAttacking()
    {
        if (!player.IsLerping)
        {
            player.Attacking = !player.Attacking;
            if (player.Attacking)
            {
                player.comLog[2].text = player.comLog[1].text;
                player.comLog[1].text = player.comLog[0].text;
                player.comLog[0].text = "Elaria is ready to attack.";
            }
            else
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
