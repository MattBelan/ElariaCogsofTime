using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * ----- Game Manager Class -----
 * 
 * Not fully implemented yet, but was created with the intent to have an
 * overarching controller that will handle pause states, menus, and the 
 * most essential state functionality. Ideally we can have the most 
 * important game states lie here and other classes can just access the
 * necessary variables those public game states.
 *
 */
public class GameManager : MonoBehaviour
{
    #region Menus

    // Pause Menus
    [Header("Pause Menus")]
    public Canvas pauseMenu;
    public Canvas combatPauseMenu;
    public Canvas cutScenePauseMenu;

    #endregion

    #region External Scripts

    // Combat

    public CombatManager combatManager;

    // Player
    public PlayerScript playerScript;

    // NPC

    #endregion

    #region Miscellaneous

    // Game State
    [Header("Game State")]
    public bool isPlaying;
    public bool isCombatActive;
    public bool isCombatInitiated;

    #endregion


    void Awake () 
    {
        //pauseMenu = AssetDatabase.LoadAssetAtPath("path", typeof(GameObject)) as Canvas;
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        isPlaying = false;   
        isCombatActive = true;
        
    }

    // Start is called before the first frame update
    void Start ()
    {
        combatManager.IsPlaying = true;
    }

    // Special Update Handled through Game Manager
    void Update ()
    {
        if (Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape))
        {
            //TogglePause();
        }


        // Overworld/General Game Functions
        if (!isCombatActive && !isCombatInitiated) 
        {
            // Do Stuff
        }
        // Transition Functions
        else if (!isCombatActive && isCombatInitiated) 
        {
            // Only set as active after transition animations are complete
            if (true) 
            {
                isCombatActive = true;
            }
        } 
        // Combat Game Functions
        else if (isCombatActive) 
        {
            if (!isPlaying) 
            {
            } 
            else 
            {
            }
        }
    }

    public void TogglePause()
    {
        isPlaying = !isPlaying;

        if (isCombatActive) 
        {
            if (combatPauseMenu.enabled)
            {
                combatPauseMenu.enabled = false;
            }
            else
            {
                combatPauseMenu.enabled = true;
            }
        }
    }
}
