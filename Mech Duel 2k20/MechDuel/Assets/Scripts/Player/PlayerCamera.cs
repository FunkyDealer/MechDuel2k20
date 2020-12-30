using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float sensitivity = 10F;
    float rotationX = 0F;
    float rotationY = 0F;
    private float rotArrayX;
    float rotAverageX = 0F;
    private float rotArrayY;
    float rotAverageY = 0F;
    Quaternion originalRotation;
    PlayerMovementManager playerMM;
    MainPlayer player;

    // Awake is the first thing to run
    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.freezeRotation = true;
        playerMM = GetComponentInParent<PlayerMovementManager>();
        player = GetComponentInParent<MainPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalRotation = transform.localRotation;
    }


    // Update is called once per frame
    void Update()
    {
        if (player.isAlive() && player.inControl)
        {
            //Resets the average rotation
            rotAverageY = 0f;
            rotAverageX = 0f;

            //Gets rotational input from the mouse
            rotationY += (Input.GetAxis("Mouse Y") * sensitivity) * 100 * Time.deltaTime;
            rotationX += (Input.GetAxis("Mouse X") * sensitivity) * 100 * Time.deltaTime;

            rotationY = Mathf.Clamp(rotationY, -90, 90);

            //Adds the rotation values to their relative array
            rotArrayY = rotationY;
            rotArrayX = rotationX;

            //Debug.Log(rotArrayY);

            //if (rotArrayY > 89) rotArrayY = 89;
            //else if (rotArrayY < -89) rotArrayY = -89;


            //Adding up all the rotational input values from each array

            rotAverageY += rotArrayY;
            rotAverageX += rotArrayX;


            //Get the rotation you will be at next as a Quaternion
            Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

            //Rotate
            transform.localRotation = originalRotation * yQuaternion;
            playerMM.rotatePlayer(xQuaternion);

        }
    }
}
