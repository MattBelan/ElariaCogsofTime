using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDisplay : MonoBehaviour
{
    public CombatManager cm;
    public Camera cam;
    public GameObject playerCanvas;
    List<Text> enemyHealth;
    public Font textFont;

    // Start is called before the first frame update
    void Start()
    {
        enemyHealth = new List<Text>();
        foreach(EnemyScript enemy in cm.enemies)
        {
            GameObject textObj = new GameObject();
            textObj.transform.SetParent(playerCanvas.transform);
            Text text = textObj.AddComponent<Text>();
            enemyHealth.Add(text);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < cm.enemies.Count; i++)
        {
            if (cm.enemies[i].Health <= 0)
            {
                enemyHealth[i].text = "";
            }
            else
            {
                enemyHealth[i].text = cm.enemies[i].Health + "/10";
            }
            enemyHealth[i].font = textFont;
            Vector3 screenPos = cam.WorldToScreenPoint(cm.enemies[i].transform.position);
            screenPos.x -= Screen.width/2;
            screenPos.y -= Screen.height/2;
            RectTransform textPos = enemyHealth[i].GetComponent<RectTransform>();
            textPos.anchoredPosition = screenPos;
        }
    }
}
