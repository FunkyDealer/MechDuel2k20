using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    protected int maxAmmo;
    [SerializeField]
    protected int baseDamage;
    public int GetMaxAmmo() => maxAmmo;

    //Rate of Fire
    [SerializeField]
    protected float fireDelay;
    protected float fireDelayTimer;
    protected bool canFirePrimary;

    [SerializeField]
    protected GameObject projectile;

    protected WeaponManager manager;

    protected List<Transform> ShootPlaces;

    protected Entity owner;

    void onAwake()
    {

    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        ShootPlaces = new List<Transform>();
        FindShootPlaces();
        canFirePrimary = true;

    }

    public virtual void PrimaryFireStart(WeaponManager weaponManager)
    {
        this.manager = weaponManager;
        owner = manager.GetPlayer;
    }

    public virtual void PrimaryFireEnd()
    {

    }

    public virtual void SecondaryFireStart(WeaponManager weaponManager)
    {

    }

    public virtual void SecondaryFireEnd()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!canFirePrimary && fireDelayTimer < fireDelay) fireDelayTimer += Time.deltaTime;
        else { canFirePrimary = true; }
    }

    protected void FindShootPlaces()
    {
        int nrOfChildren = transform.childCount;
        for (int i = 0; i < nrOfChildren; i++)
        {
            Transform c = transform.GetChild(i);
            if (c.gameObject.name == "ShootPlace") ShootPlaces.Add(c);
        }
    }
}
