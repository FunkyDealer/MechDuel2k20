using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hud_health : base_hud
{
    int Health;


    protected override void Awake()
    {
        base.Awake();
        Player.onHealthUpdate += GetHealth;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void OnDestroy()
    {
        Player.onHealthUpdate -= GetHealth;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GetHealth(int H, int maxHealth)
    {
        Health = H;
        textDisplay.text = $"{text}: {Health}";
    }
}
