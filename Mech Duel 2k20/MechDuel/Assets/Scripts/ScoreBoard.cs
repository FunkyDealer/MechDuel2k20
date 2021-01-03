using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    [SerializeField]
    Entity[] Players;
    [SerializeField]
    Text[] texts;
    [SerializeField]
    Text[] names;
    [SerializeField]
    GameObject Canvas;
    bool scoreBoard;
    private void Start()
    {
        Players = FindObjectsOfType<Entity>();
    }
    void Update()
    {
        int c = 0;
        foreach(var v in Players) 
        {
            names[c].text = v.nickName;
            texts[c].text =  Convert.ToString(v.score);
            c++;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreBoard = !scoreBoard;
        }
        Canvas.SetActive(scoreBoard);

    }
}
