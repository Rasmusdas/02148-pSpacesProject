using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public float fireratePistol = 0.5f;
    Animator anim;

    [Header("KeyBinds")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    //public KeyCode fireKey = KeyCode.Mouse0;

    [Header("VFXs")]
    public ParticleSystem muzzleflashVFX;
    public GameObject death;
    public Material shieldMat;
    public Image privateHealthBar;
    public Image publicHealthBar;
    
    Material playerMat;
    MeshRenderer meshRenderer;

    NetworkTransform nT;

    bool isSprinting = false;
    bool canShoot = true;
    //private AudioSource gunShot;

    Vector3 movement;

    CharacterController characterController;

    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        nT = GetComponent<NetworkTransform>();
        meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
        playerMat = meshRenderer.material;
        TryGetComponent<Animator>(out anim);
        if (!nT.isOwner) return;
        characterController = GetComponent<CharacterController>();
        cam = Camera.main;
        cam.GetComponent<CamController>().player = gameObject;
        //gunShot = GetComponent<AudioSource>();
        
        publicHealthBar.transform.parent.parent.gameObject.SetActive(!nT.isOwner);

    }

    // Update is called once per frame
    void Update()
    {
        //playerMat.color = Color.Lerp(Color.red, Color.green, health / maxHealth);

        privateHealthBar.fillAmount = health / maxHealth;
        publicHealthBar.fillAmount = health / maxHealth;

        if (!nT.isOwner) return;
        GetInputs();
        Move();

        //if (Input.GetKeyDown(fireKey) && health > 0)
        //{
        //    Shoot();
        //}
    }

    //private void OnParticleCollision(GameObject other)
    //{
    //    Debug.Log(health);
    //    TakeDamge(1);
    //}

    private void Shoot()
    {
        if (canShoot)
        {
            muzzleflashVFX.Play();
            //gunShot.Play();
            NetworkServer.Instantiate("Bullet", gunTip.position, gunTip.rotation);
            StartCoroutine(Firerate(fireratePistol));
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
            gunTip.transform.LookAt(lookAtPoint + new Vector3(0,1,0));
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

            anim?.SetBool("Move", true);
            if (isSprinting)
            {
                movement *= moveSpeed * sprintMult;

            }
            else
            {
                movement *= moveSpeed;
            }
        }
        else
        {
            anim?.SetBool("Move", false);
        }

        movement = new Vector3(movement.x, -3, movement.z);

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
        
        if(health - dmg > maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health -= dmg;
        }

        if (health <= 0)
        {
            transform.position = new Vector3(0, health > 0 ? -10 : 0, 0);   
        }
    }

    public void TakeDamge(int dmg)
    {
        NetworkServer.DamagePlayer(new Packet(PacketType.Health, NetworkServer.playerId, "Server", nT.id+"|" +dmg.ToString()));
    }

    public void AddHealth(float health)
    {
        TakeDamge(-(int)health);
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

    IEnumerator Firerate(float firerate)
    {
        canShoot = false;
        yield return new WaitForSeconds(firerate);
        canShoot = true;
    }

}
