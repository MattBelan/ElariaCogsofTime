﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public bool isPlaying;
    public bool isCombatActive;
    public bool isCombatInitiated;

    #endregion


    void Awake() 
    {
        combatManager = this.gameObject.GetComponent<CombatManager>();
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        isPlaying = false;   
        isCombatActive = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Special Update Handled through Game Manager
    void Update()
    {
        playerScript.playing = isPlaying;

        if (Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape))
        {
            TogglePause();
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
                // Disable Combat Manager's Update
            } 
            else 
            {
                // Enable Combat Manager's Update
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
