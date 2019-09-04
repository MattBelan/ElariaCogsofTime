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
    public Button[] buttons;
    public Canvas pauseMenu;

    public SaveScript saving;
    
    public PlayerScript player;
    public List<EnemyScript> enemies;

    bool playerPassTurn;
    bool enemyPassTurn;

    bool playing;
    bool enemiesMoved;
    bool enemiesMoving;
    bool enemiesAttacking;

    void Awake()
    {
        playerPassTurn = false;
        playing = false;
        enemiesMoved = false;
        enemiesMoving = false;
        enemiesAttacking = false;
        player.Health = 20;
        if (PlayerPrefs.GetInt("Loading") > 0)
        {
            saving.LoadGame();
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
    }

	// Use this for initialization
	void Start () {
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
	void Update () {
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

        switch (State)
        {
            case RoundState.Player:

                switch (turnState)
                {
                    case TurnState.Character:
                        turnState = TurnState.Action;
                        break;

                    case TurnState.Action:


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
                
                break;

            case RoundState.Reset:

                player.currentMove = 0;
                playerPassTurn = false;
                player.Moving = false;
                player.usedAttack = false;
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
        }
        else {
            buttons[1].interactable = true;
        }
        // Pass
        if (turnState == TurnState.Character) {
            buttons[2].interactable = false;
        }
        else {
            buttons[2].interactable = true;
        }
	}

    public void TogglePlayerMovement()
    {
        if (!player.IsLerping)
        {
            player.Moving = !player.Moving;
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
}
