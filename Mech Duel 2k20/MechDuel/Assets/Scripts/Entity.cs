using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public string nickName;
    public Guid id;
    public int score;
    protected int currentHealth;
    [SerializeField]
    protected int maxHealth;

    protected int currentArmour; //Armor for damage mitigation
    [SerializeField]
    protected int maxArmour;

    public bool ready;

    public bool alive;

    protected virtual void Awake()
    {
        ready = false;
        Spawn();
    }

    public virtual void Spawn()
    {
        currentHealth = maxHealth;       
        alive = true;
    }

    public int Health() => currentHealth;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    protected virtual void OnEnable()
    {
        Spawn();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }


    public virtual void getDamage(int damage, Entity shooter)
    {
        currentHealth -= damage;
        checkHealth(shooter);
    }

    protected virtual void checkHealth(Entity shooter)
    {
        if (currentHealth <= 0) Die(shooter);
    }

    public virtual void Die(Entity shooter)
    {

    }

    public virtual void Die()
    {

    }
}
