using MechDuelCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Weapon
{

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

    }

    public override void PrimaryFireStart(WeaponManager weaponManager)
    {
        base.PrimaryFireStart(weaponManager);
        PrimaryFire();
    }

    public override void PrimaryFireEnd()
    {

    }

    public override void SecondaryFireStart(WeaponManager weaponManager)
    {
        SecondaryFire();
    }

    public override void SecondaryFireEnd()
    {

    }

    private void PrimaryFire()
    {
        if (canFirePrimary)
        {
            // Debug.Log($"Primary Weapon Firing: Primary Fire");
            fireDelayTimer = 0;
            canFirePrimary = false;

            foreach (Transform s in ShootPlaces)
            {
                CreateProjectile(s);
            }
        }
    }

    private void CreateProjectile(Transform s)
    {
        SendProjectile(s);
        GameObject a = Instantiate(projectile, s.position, Quaternion.identity);
        Laser l = a.GetComponent<Laser>();
        l.start = s.position;
        l.direction = s.forward;
        l.damage = baseDamage;
        l.shooter = manager.GetPlayer;
    }

    private void SendProjectile(Transform s)
    {
        Message m = new Message();
        m.MessageType = MessageType.Shoot;
        Shot shot = new Shot();
        shot.shooter = manager.GetPlayer.tcpClient.player.Id;        
        shot.xStart = s.transform.position.x;
        shot.yStart = s.transform.position.y;
        shot.zStart = s.transform.position.z;
        shot.xDir = s.transform.forward.x;
        shot.yDir = s.transform.forward.y;
        shot.zDir = s.transform.forward.z;
        shot.damage = baseDamage;
        m.shot = shot;
        manager.GetPlayer.tcpClient.player.SendMessage(m);
        Debug.Log("sending shot");
    }

    private void SecondaryFire()
    {
        //Debug.Log($"Primary Weapon Firing: Secondary Fire");
    }

    private void ShootPrimary()
    {



    }
}
