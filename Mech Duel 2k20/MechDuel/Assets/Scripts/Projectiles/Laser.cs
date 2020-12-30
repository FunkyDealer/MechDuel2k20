using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : HitScanProjectile
{
    [SerializeField]
    private int maxSize;
    private LineRenderer lr;

    [SerializeField]
    private float LaserSpeed;

    float laserLenght;
    

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        calculateLaser();
        lifeTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (lifeTimer < lifeTime) lifeTimer += Time.deltaTime;
        else Destroy(gameObject);
    }

    void LateUpdate()
    {
        Vector3 laserDir = end - start;
        laserDir.Normalize();
        start += laserDir * LaserSpeed * Time.deltaTime;
        laserLenght = laserDir.magnitude;
        //if (laserLenght < 1) Destroy(gameObject);
        lr.SetPosition(0, start);
    }


    void calculateLaser()
    {
        lr.SetPosition(0, start);

        // Bit shift the index of the layer (9) to get a bit mask
        int layerMask = 1 << 9;

        layerMask = ~layerMask;

        RaycastHit hit;
        if (Physics.Raycast(start, direction, out hit, (direction * maxSize).magnitude, layerMask))
        {
            if (hit.collider)
            {
                end = hit.point;
                calcEntity(hit);
            }
        }
        else
        {
            // Debug.Log($"End Position: {direction * 10}");
            end = start + direction * maxSize;
        }
        lr.SetPosition(1, end);
    }


    void calcEntity(RaycastHit hit)
    {
        hitEntity = hit.transform.gameObject.GetComponent<Entity>();
        if (hitEntity != null) damageEntity();

    }
}
