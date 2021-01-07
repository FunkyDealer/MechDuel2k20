using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    [HideInInspector]
    public TCPClientController TcpController;

    [SerializeField]
    Text[] texts;
    [SerializeField]
    Text[] names;

    private void Start()
    {
        int c = 0;
        foreach (GameObject v in TcpController.playersList.Values)
        {
            Entity e = v.GetComponent<Entity>();
            texts[c].text = Convert.ToString(e.score);
            names[c].text = e.nickName;
            c++;
        }
    }

    void Update()
    {

    }

    public void UpdateScore()
    {
        int c = 0;
        foreach (GameObject v in TcpController.playersList.Values)
        {
            Entity e = v.GetComponent<Entity>();
            texts[c].text = Convert.ToString(e.score);
            names[c].text = e.nickName;
            c++;
        }
    }

}
