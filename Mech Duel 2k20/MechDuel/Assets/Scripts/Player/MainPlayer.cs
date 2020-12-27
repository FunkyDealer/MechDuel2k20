﻿using MechDuelCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlayer : Entity
{
    int currentArmour; //Armor for damage mitigation
    [SerializeField]
    int maxArmour;

    public int currentEnergy; //enemy for sprinting
    [SerializeField]
    int maxEnergy;

    bool alive;
    public bool inControl;

    public delegate void PlayerDeath();
    public static event PlayerDeath onDeath;

    public delegate void UpdateHealthEvent(int h, int maxH);
    public static event UpdateHealthEvent onHealthUpdate;

    public delegate void UpdateEnergyEvent(int e, int maxE);
    public static event UpdateEnergyEvent onEnergyUpdate;

    public delegate void UpdateShieldEvent(int s, int maxS);
    public static event UpdateShieldEvent onShieldUpdate;

    public delegate void UpdateNanopakEvent(int n);
    public static event UpdateNanopakEvent onNanopakUpdate;

    public delegate void PrimaryFire();
    public static event PrimaryFire onPrimaryFire;

    public delegate void UpdatePlayableAreaHud(float timer, bool outsideMap, bool alive);
    public static event UpdatePlayableAreaHud onAreaUpdate;

    //Energy
    float EnergyRecoverTimer;
    [SerializeField]
    readonly float EnergyRecoverTime = 0.025f; //Time Between Energy recovering
    bool canRecoverEnergy;
    [SerializeField]
    float EnergyRecoverDelay = 0.3f; //Time before energy starts recovering again
    float EnergyRecoverDelayTimer;

    PlayerMovementManager movManager;

    public bool isAlive() => alive;
    public int CurrentArmour() => currentArmour;
    public int MaxArmour() => maxArmour;

    [SerializeField]
    GameObject hud;

    Vector3 oldPosition;
    Vector3 oldForward;

    TCPClientController tcpClient;

    protected override void Awake()
    {
        alive = true;
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        inControl = true;
        EnergyRecoverTimer = 0;
        canRecoverEnergy = true;
        EnergyRecoverTimer = 0;
        currentArmour = 0;

        tcpClient = GameObject.Find("TCPClientController").GetComponent<TCPClientController>();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        Instantiate(hud, Vector3.zero, Quaternion.identity);

        onHealthUpdate(currentHealth, maxHealth);
        onEnergyUpdate(currentEnergy, maxEnergy);
        onShieldUpdate(currentArmour, maxArmour);

        SendPositionInformation();
        SendRotationInformation();
    }
       
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (alive && inControl)
        {
            recoverEnergy();            
            
        }
    }

    protected void FixedUpdate()
    {
        if (transform.position != oldPosition)
        {
            SendPositionInformation();
        }
    }

    void SendPositionInformation()
    {
            Message m = new Message();
            m.MessageType = MessageType.PlayerMovement;
            PlayerInfo info = new PlayerInfo();
            info.Id = tcpClient.player.Id;
            info.Name = tcpClient.player.Name;
            info.X = transform.position.x;
            info.Y = transform.position.y;
            info.Z = transform.position.z;
            info.rX = transform.forward.x;
            info.rY = transform.forward.y;
            info.rZ = transform.forward.z;
            m.PlayerInfo = info;
            tcpClient.player.SendMessage(m);
        
        oldPosition = transform.position;
    }

    void SendRotationInformation()
    {
        Message m = new Message();
        m.MessageType = MessageType.PlayerMovement;
        PlayerInfo info = new PlayerInfo();
        info.Id = tcpClient.player.Id;
        info.Name = tcpClient.player.Name;
        info.rX = transform.forward.x;
        info.rY = transform.forward.y;
        info.rZ = transform.forward.z;
        m.PlayerInfo = info;
        tcpClient.player.SendMessage(m);

        oldForward = transform.forward;
    }

    public void spendEnergy(int newEnergy)
    {
        currentEnergy = newEnergy;
        try
        {
            onEnergyUpdate(currentEnergy, maxEnergy);
        }
        catch (NullReferenceException)
        {

        }
        canRecoverEnergy = false;
        EnergyRecoverDelayTimer = 0;
    }

    private void recoverEnergy()
    {
        if (currentEnergy < maxEnergy)
        {
            if (canRecoverEnergy)
            {
                if (EnergyRecoverTimer <= EnergyRecoverTime) EnergyRecoverTimer += Time.deltaTime;
                else
                {
                    currentEnergy++;
                    onEnergyUpdate(currentEnergy, maxEnergy);
                    EnergyRecoverTimer = 0;
                }
            }
            else
            {
                if (EnergyRecoverDelayTimer <= EnergyRecoverDelay) EnergyRecoverDelayTimer += Time.deltaTime;
                else { canRecoverEnergy = true; }
            }
        }
    }

    public override void getDamage(int damage)
    {
        // base.getDamage(damage);

        if (currentArmour < 0)
        { //If player has shield
            int dmgHealth = damage / 5; //damage receive is 1/5
            currentHealth -= dmgHealth;
            int dmgShield = (damage - damage / 5) / 2; //shield receives 80% / 2 damage
            currentArmour -= dmgShield;
            if (currentArmour > 0) currentArmour = 0;
        }
        else
        {
            currentHealth -= damage;
        }

        checkHealth();

        onHealthUpdate(currentHealth, maxHealth);
        onShieldUpdate(currentArmour, maxArmour);

    }

    public override void Die()
    {
        currentHealth = 0;
        alive = false;
        inControl = false;
        Debug.Log($"Player Died");

    }

    public void increaseShield(int ammount)
    {
        if (currentArmour + ammount < maxArmour) currentArmour += ammount;
        else currentArmour = maxArmour;
        onShieldUpdate(currentArmour, maxArmour);
    }






}
