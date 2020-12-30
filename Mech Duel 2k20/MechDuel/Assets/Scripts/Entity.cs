using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public string nickName;
    public Guid id;
    protected int currentHealth;
    [SerializeField]
    protected int maxHealth;

    protected int currentArmour; //Armor for damage mitigation
    [SerializeField]
    protected int maxArmour;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;

  
    }

    public int Health() => currentHealth;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }


    public virtual void getDamage(int damage, Entity entity)
    {
        currentHealth -= damage;
        checkHealth();
    }

    protected virtual void checkHealth()
    {
        if (currentHealth <= 0) Die();
    }

    public virtual void Die()
    {

    }
}
