﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [SerializeField]
    string scene;
    // Update is called once per frame
    void Update()
    {
    }

    public void Change() 
    {
        SceneManager.LoadScene(scene);
    }
}
