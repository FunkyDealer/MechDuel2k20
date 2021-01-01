using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spectator : MonoBehaviour
{
    public float sensitivity = 10F;
    float rotationX = 0F;
    float rotationY = 0F;
    private float rotArrayX;
    float rotAverageX = 0F;
    private float rotArrayY;
    float rotAverageY = 0F;
    Quaternion originalRotation;

    public GameManager manager;

    // Awake is the first thing to run
    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.freezeRotation = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        originalRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        CameraRotation();

        CameraMovement();

        Controls();
    }

    void CameraRotation()
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

        //Adding up all the rotational input values from each array

        rotAverageY += rotArrayY;
        rotAverageX += rotArrayX;


        //Get the rotation you will be at next as a Quaternion
        Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
        Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

        //Rotate
        transform.localRotation = originalRotation * yQuaternion * xQuaternion;

        
    }

    void CameraMovement()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input = Vector2.ClampMagnitude(input, 1);

    }

    void Controls()
    {
        if (Input.GetButtonDown("Spawn")) {
            Debug.Log("SPAWNING PLAYER");
            manager.SpawnMainPlayer();
            Destroy(this.gameObject);
        }
    }

    public void MoveToLocation(Vector3 location, Vector3 forwardFacing)
    {
        transform.position = location;
        transform.forward = forwardFacing;
    }
}
