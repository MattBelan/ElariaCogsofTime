using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDisplay : MonoBehaviour
{
    public CombatManager cm;
    Camera cam;
    public GameObject playerCanvas;
    List<Text> enemyHealth;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        foreach(EnemyScript enemy in cm.enemies)
        {
            Text text = playerCanvas.AddComponent<Text>();
            enemyHealth.Add(text);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < cm.enemies.Count; i++)
        {
            enemyHealth[i].text = cm.enemies[i].Health + "/10";
            Vector3 screenPos = cam.WorldToScreenPoint(cm.enemies[i].transform.position);
            RectTransform textPos = enemyHealth[i].GetComponent<RectTransform>();
            textPos.anchoredPosition = screenPos;
        }
    }
}
