using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadGame()
    {
        PlayerPrefs.SetInt("Loading", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Dungeon_Level1");
    }
    public void NewGame()
    {
        PlayerPrefs.SetInt("Loading", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Overworld");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
