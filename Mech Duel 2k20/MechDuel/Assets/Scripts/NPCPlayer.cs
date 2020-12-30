using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPlayer : Entity
{
    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }

    public override void getDamage(int damage, Entity entity)
    {
       // base.getDamage(damage, entity);
    }

    public void ReceiveDamage(int healthDam, int shieldDam, Entity shooter)
    {
        if (currentArmour - shieldDam < 0) currentArmour = 0;
        else currentArmour = currentArmour - shieldDam;

        if (currentHealth - healthDam < 0) currentHealth = 0;
        else currentHealth = currentHealth - healthDam;

            ShowHit();
    }

    private void ShowHit()
    {
        Debug.Log($"Player {nickName}");
    }

}
