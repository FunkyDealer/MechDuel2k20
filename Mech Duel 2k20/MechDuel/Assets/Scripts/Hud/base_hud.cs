using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class base_hud : MonoBehaviour
{
    public string text;
    protected Text textDisplay;

    protected virtual void Awake()
    {
        textDisplay = GetComponent<Text>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
