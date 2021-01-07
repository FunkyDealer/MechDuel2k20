using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{

    TCPClientController Players;
    [SerializeField]
    Text[] texts;
    [SerializeField]
    Text[] names;
    [SerializeField]
    GameObject Canvas;
    bool scoreBoard;
    private void Start()
    {
        Players = GetComponent<TCPClientController>();
    }
    void Update()
    {
        int c = 0;
        foreach (GameObject v in Players.playersList.Values)
        {
            Entity e = v.GetComponent<Entity>();
            texts[c].text = Convert.ToString(e.score);
            names[c].text = e.nickName;
            c++;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreBoard = !scoreBoard;
        }
        Canvas.SetActive(scoreBoard);

    }
}
