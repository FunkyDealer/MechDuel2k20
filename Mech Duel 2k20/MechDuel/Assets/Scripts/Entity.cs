using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected int currentHealth;
    [SerializeField]
    protected int maxHealth;

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


    public virtual void getDamage(int damage)
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
