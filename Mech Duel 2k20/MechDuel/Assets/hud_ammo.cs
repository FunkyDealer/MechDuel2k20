using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hud_ammo : base_hud
{
    int currentAmmo;
    int maxAmmo;

    protected override void Awake()
    {
        base.Awake();
        WeaponManager.onAmmoUpdate += GetAmmo;
    }

    void OnDestroy()
    {
        WeaponManager.onAmmoUpdate -= GetAmmo;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void GetAmmo(int currentAmmo, int maxAmmo)
    {
        textDisplay.text = $"{text}: {currentAmmo}/{maxAmmo}";

    }
}
