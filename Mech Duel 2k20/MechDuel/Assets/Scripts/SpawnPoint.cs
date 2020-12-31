using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    bool occupied;
    int inPlayers;

    public Vector3 spawnPos;

    void Awake()
    {
        occupied = false;
        inPlayers = 0;
        spawnPos = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public bool CanSpawn()
    {
        return !occupied;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            occupied = true;
            inPlayers++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            inPlayers--;
            if (inPlayers == 0) occupied = false;
        }
    }
}
