using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveScript : MonoBehaviour {

    public SaveData playerData;
    public PlayerScript player;

    public List<SaveData> enemyData;
    public List<EnemyScript> enemies;

    string playerDataPath;
    List<string> enemyDataPaths = new List<string>();

    void Start()
    {
        enemyData = new List<SaveData>();
        
        playerDataPath = Path.Combine(Application.persistentDataPath, "PlayerData.txt");

        for (int i = 0; i < enemies.Count; i++)
        {
            enemyDataPaths.Add(Path.Combine(Application.persistentDataPath, "EnemyData" + i + ".txt"));
        }
    }

    void Update()
    {
        playerData = player.data;
        if (enemies.Count > 0)
        {
            enemyData.Clear();

            foreach(EnemyScript enemy in enemies)
            {
                enemyData.Add(enemy.data);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Save(playerData, playerDataPath);

            for (int i = 0; i < enemies.Count; i++)
            {
                Save(enemyData[i], enemyDataPaths[i]);
            }
        }
            
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPlayers();
            LoadEnemies();
        }
    }

    static void Save(SaveData data, string path)
    {
        string jsonString = JsonUtility.ToJson(data);

        using (StreamWriter streamWriter = File.CreateText(path))
        {
            streamWriter.Write(jsonString);
        }
    }

    static SaveData Load(string path)
    {
        using (StreamReader streamReader = File.OpenText(path))
        {
            string jsonString = streamReader.ReadToEnd();
            return JsonUtility.FromJson<SaveData>(jsonString);
        }
    }

    public void LoadPlayers()
    {
        playerData = Load(playerDataPath);

        Vector3 newPos = new Vector3();
        newPos.x = playerData.x;
        newPos.y = playerData.y;
        newPos.z = playerData.z;

        player.transform.position = newPos;

        player.Health = playerData.health;
        player.currentMove = playerData.currentMove;
    }

    public void LoadEnemies()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            enemyData[i] = Load(enemyDataPaths[i]);

            Vector3 newPos = new Vector3();
            newPos.x = enemyData[i].x;
            newPos.y = enemyData[i].y;
            newPos.z = enemyData[i].z;

            enemies[i].transform.position = newPos;
            enemies[i].Health = enemyData[i].health;
            enemies[i].currentMove = enemyData[i].currentMove;
        }
    }

    public void SaveGame()
    {
        Save(playerData, playerDataPath);

        for (int i = 0; i < enemies.Count; i++)
        {
            Save(enemyData[i], enemyDataPaths[i]);
        }
    }

    public void LoadGame()
    {
        LoadPlayers();
        LoadEnemies();
    }


}
