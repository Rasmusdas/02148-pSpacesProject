using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public float health = 10f;
    public float moveSpeed = 5f;
    public float sprintMult = 1.8f;

    [Header("KeyBinds")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode fireKey = KeyCode.Mouse0;

    [Header("VFXs")]
    public ParticleSystem muzzleflashVFX;
    public ParticleSystem bulletVFX;

    bool isSprinting = false;

    Vector3 movement;

    CharacterController characterController;

    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        Move();

        if (Input.GetKey(fireKey))
        {
            muzzleflashVFX.Play();
            bulletVFX.Play();
        }
    }

    private void FixedUpdate()
    {
        Rotate();
    }

    private void Rotate()
    {
        Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLenght;

        if (groundPlane.Raycast(camRay, out rayLenght))
        {
            Vector3 lookAtPoint = camRay.GetPoint(rayLenght);
            lookAtPoint.y = transform.position.y;
            transform.LookAt(lookAtPoint);
        }
    }

    private void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        movement = new Vector3(1, 0, 0) * x + new Vector3(0, 0, 1) * y;
        movement = movement.normalized;

        if (movement != Vector3.zero)
        {

            if (isSprinting)
            {
                movement *= moveSpeed * sprintMult;

            }
            else
            {
                movement *= moveSpeed;
            }
        }
        movement *= Time.deltaTime;
        characterController.Move(movement);
    }

    private void GetInputs()
    {
        //Sprint Key check
        if (Input.GetKeyDown(sprintKey))
        {
            isSprinting = true;
        }
        if (Input.GetKeyUp(sprintKey))
        {
            isSprinting = false;
        }
    }

    public void TakeDamge(float dmg)
    {
        health -= dmg;

        if (health <= 0)
        {
            Debug.Log("U DEAD");
        }
    }
}
