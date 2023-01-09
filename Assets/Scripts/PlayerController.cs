using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("GameObjects")]
    public GameObject bullet;
    public Transform gunTip;

    [Header("Stats")]
    public float health = 10f;
    public float maxHealth = 10f;
    public float moveSpeed = 5f;
    public float sprintMult = 1.8f;
    public int shielded = 0;

    [Header("KeyBinds")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode fireKey = KeyCode.Mouse0;

    [Header("VFXs")]
    public ParticleSystem muzzleflashVFX;
    public GameObject death;
    public Material shieldMat;
    Material playerMat;
    MeshRenderer meshRenderer;


    NetworkTransform nT;

    bool isSprinting = false;

    Vector3 movement;

    CharacterController characterController;

    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        nT = GetComponent<NetworkTransform>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        playerMat = meshRenderer.material;
        if (!nT.isOwner) return;
        characterController = GetComponent<CharacterController>();
        cam = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        playerMat.color = Color.Lerp(Color.red, Color.green, health / maxHealth);

        if (!nT.isOwner) return;
        GetInputs();
        Move();

        if (Input.GetKeyDown(fireKey))
        {
            Shoot();
        }
    }

    //private void OnParticleCollision(GameObject other)
    //{
    //    Debug.Log(health);
    //    TakeDamge(1);
    //}

    private void Shoot()
    {
        muzzleflashVFX.Play();
        NetworkServer.Instantiate("Bullet", gunTip.position, gunTip.rotation);
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

        movement = new Vector3(movement.x, -1, movement.z);

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

    public void UpdateHealth(float dmg)
    {
        if (0 < shielded)
        {
            return;
        }
        health -= dmg;

        if (health <= 0)
        {

            GameObject obj = Instantiate(death, transform.position, transform.rotation);

            foreach (var v in obj.GetComponentsInChildren<Rigidbody>())
            {
                v.AddExplosionForce(50, transform.position + Vector3.up, 1, 1, ForceMode.Impulse);
            }

            Destroy(gameObject);
        }
    }

    public void TakeDamge(int dmg)
    {
        NetworkServer.DamagePlayer(new Packet(PacketType.Health, NetworkServer.playerId, "Server", nT.id+"|" +dmg.ToString()));
    }

    public void AddHealth(float health)
    {
        this.health = Mathf.Min(health + this.health, maxHealth);
    }

    public void Shield(float time)
    {
        StartCoroutine(ShieldWaiter(time));
    }

    IEnumerator ShieldWaiter(float time)
    {
        shielded++;
        meshRenderer.material = shieldMat;
        yield return new WaitForSeconds(time);
        shielded--;
        if (shielded == 0)
        {
            meshRenderer.material = playerMat;
        }
    }
}
