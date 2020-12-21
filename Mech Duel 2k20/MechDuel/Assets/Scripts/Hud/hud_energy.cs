using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class hud_energy : base_hud
{
    int Energy;



    protected override void Awake()
    {
        base.Awake();
        Player.onEnergyUpdate += GetEnergy;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void OnDestroy()
    {
        Player.onEnergyUpdate -= GetEnergy;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GetEnergy(int E, int maxEnergy)
    {
        Energy = E;
        textDisplay.text = $"{text}: {Energy}";
    }



}
