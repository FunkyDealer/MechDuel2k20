using MechDuelCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotsManager : MonoBehaviour
{
    public GameObject laser;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnLaser(Vector3 position, Vector3 direction, int damage, Entity shooter)
    {   
        GameObject a = Instantiate(laser, position, Quaternion.identity);
        Laser l = a.GetComponent<Laser>();
        l.start = position;
        l.direction = direction;
        l.damage = damage;

        l.shooter = shooter;
    }
}
