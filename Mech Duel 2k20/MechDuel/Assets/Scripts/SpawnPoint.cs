using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Vector3 spawnPos;

    List<GameObject> playersIn;

    void Awake()
    {
        spawnPos = transform.position;
        playersIn = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {                
                playersIn.Add(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
       // occupied = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playersIn.Remove(other.gameObject);
        }
    }

    public bool isOcupied()
    {
        bool ocupied = false;

        if (playersIn.Count > 0)
        {
            foreach (var p in playersIn)
            {
                if (p.activeSelf) ocupied = true;
            }
        }

        return !ocupied;
    }
}
