using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    public Vector3 direction;

    [SerializeField]
    protected float lifeTime;
    protected float lifeTimer;

    protected Entity hitEntity;
    public Entity shooter;

    protected virtual void Awake()
    {
        hitEntity = null;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    protected void damageEntity()
    {
      hitEntity.getDamage(damage, shooter);

    }
}
