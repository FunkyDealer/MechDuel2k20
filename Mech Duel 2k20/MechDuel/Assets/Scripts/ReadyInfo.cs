using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyInfo : MonoBehaviour
{
    Text promptText;

    void Awake()
    {
        promptText = GetComponentInChildren<Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ChangeText(bool ready, bool gameStarted)
    {
        if (ready) promptText.text = "Ready";
        else promptText.text = "Not Ready";

        if (gameStarted) promptText.text = "Game Started";

    }
}
