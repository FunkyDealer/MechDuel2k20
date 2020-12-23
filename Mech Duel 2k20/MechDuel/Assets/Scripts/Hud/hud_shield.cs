using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hud_shield : base_hud
{
    int Shield;

    protected override void Awake()
    {
        base.Awake();
        MainPlayer.onShieldUpdate += GetShield;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void OnDestroy()
    {
        MainPlayer.onEnergyUpdate -= GetShield;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GetShield(int S, int maxShield)
    {
        Shield = S;
        textDisplay.text = $"{text}: {Shield}";
    }
}
