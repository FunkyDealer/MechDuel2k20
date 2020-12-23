using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementManager : MonoBehaviour
{
    CharacterController controller;
    Camera cam;
    [SerializeField]
    float runSpeed;
    [SerializeField]
    float walkSpeed;
    [SerializeField]
    float sprintSpeed;
    float currentSpeed;
    float jumpPower = 2;
    MainPlayer player;
    Quaternion originalRotation;
    Vector3 dir;
    bool jumping; //Jumping

    [SerializeField]
    float gravityScale = 0.8f; //Gravity's Scale

    //sprint
    float EnergySpendingTimer;
    [SerializeField]
    readonly float EnergySpendingTime = 0.05f;

    Vector3 oldPos;

    // awake is the first thing to be called
    void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        controller = GetComponent<CharacterController>();
        player = GetComponent<MainPlayer>();
        jumping = false;       
        EnergySpendingTimer = 0;
        oldPos = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        originalRotation = transform.localRotation;
        currentSpeed = runSpeed;
        dir = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.isAlive() && player.inControl)
        {
            modifiers(); //Run, walk and sprint speed
            EnergyManagement(); //energy management when sprinting
            Movement(); //Main direction Calculations
            moveCharacter(dir); //Movement Calculations

        }
        else
        {
            dir.x = 0; dir.z = 0;
        }
    }

    void EnergyManagement()
    {
        if (currentSpeed == sprintSpeed && (dir.x > 0 || dir.z > 0))
        {
            if (player.currentEnergy <= 0)
            {
                currentSpeed = walkSpeed;
                EnergySpendingTimer = 0;
            }
            else
            {
                if (EnergySpendingTimer < EnergySpendingTime) EnergySpendingTimer += Time.deltaTime;
                else
                {
                    player.spendEnergy(player.currentEnergy - 1);
                    EnergySpendingTimer = 0;
                }
            }
        }
    }

    void Movement()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input = Vector2.ClampMagnitude(input, 1);
        float dirY = dir.y;
        dir = transform.right * input.x + transform.forward * input.y;
        dir.y = dirY;

        if (controller.isGrounded) //Check if playing is on the ground
        {
            jumping = false;
            dir.y = -1;
            if (Input.GetButtonDown("Jump")) //Jumping when space is pressed
            {
                Jump();
            }
        }
        else
        {
            
            dir.y += (Physics.gravity.y * gravityScale * Time.deltaTime); //Gravity is applied if not grounded
        }
    }

    //Run, Walk and Sprint Modifiers
    void modifiers()
    {
        if (Input.GetButton("Sprint")) currentSpeed = sprintSpeed;
        else if (Input.GetButton("Walk") && controller.isGrounded) currentSpeed = walkSpeed;
        else currentSpeed = runSpeed;
    }


    //Movement Calculations
    void moveCharacter(Vector3 d)
    {
        oldPos = transform.position;

        controller.Move(new Vector3(
        d.x * currentSpeed * Time.deltaTime,
        0,
        d.z * currentSpeed * Time.deltaTime
        ));

        { //Gravity
            controller.Move(new Vector3(
            0,
            d.y * runSpeed * Time.deltaTime,
            0));
        }        
    }

    //Jumping
    void Jump()
    {
        dir.y = jumpPower;
        jumping = true;
    }

    public void rotatePlayer(Quaternion xQuaternion)
    {
        transform.localRotation = originalRotation * xQuaternion;
    }

    

}
